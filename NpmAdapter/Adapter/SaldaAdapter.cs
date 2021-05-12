using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using System;
using System.Collections.Generic;
using System.Text;
using NpmStandardLib.IO.Encrypt;
using NpmAdapter.Model;
using System.Data;
using NpmAdapter.Payload;
using Newtonsoft.Json.Linq;
using System.Threading;
using HttpServer.Headers;
using System.Diagnostics;
using System.Runtime;

namespace NpmAdapter.Adapter
{
    class SaldaAdapter : IAdapter
    {
        private const string GET_PARKINGZONE = "/parking/zone"; //주차장 정보
        private const string GET_DEVICES = "/parking/devices";
        private const string GET_STATUS = "/parking/status";
        private const string GET_WHITE_LIST = "/parking/whitelist/list";
        private const string GET_LIST_REGULAR = "/parking/whitelist/regular";
        private const string GET_LIST_VISITOR = "/parking/whitelist/visitor";
        private const string SET_REGISTRATION = "/parking/whitelist/registration";
        private const string SET_UPDATE = "/parking/whitelist/update";
        private const string DEL_REMOVE = "/parking/whitelist/remove";
        private const string CON_DEVICE = "/parking/device/control";
        private const string SND_ALERT = "/ext/parking/event";
        private const string SND_PING = "/ext/parking/ping";

        private bool isRun = false;
        private bool bResponseSuccess = false;
        private string aptId = "";
        private string webport = "42141";
        private string hostDomain = "";
        private string aeskey = "";
        private object lockObj = new object();
        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private ReponseSdPayload responseSdPayload;
        private ReponseSdParkInfoPayload parkInfoPayload;
        private Dictionary<string, string> dicHeader = new Dictionary<string, string>();

        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private delegate void SafeCallDelegate();

        public IAdapter TargetAdapter { get; set; }
        private INetwork MyHttpNetwork { get; set; }
        public bool IsRuning { get => isRun; }

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            aptId = SysConfig.Instance.ParkId;
            hostDomain = SysConfig.Instance.HW_Domain;
            aeskey = SysConfig.Instance.AuthToken;
            responseSdPayload = new ReponseSdPayload();
            responseSdPayload.Initialize();
            //ParkInfo DB Load =========
            parkInfoPayload = new ReponseSdParkInfoPayload();
            DataTable parkInfoDt;
            ParkInfoModel mdl = new ParkInfoModel();

            parkInfoDt = mdl.GetParkInfo();
            if (parkInfoDt == null || parkInfoDt.Rows.Count == 0)
            {
                Log.WriteLog(LogType.Error, "SaldaAdapter | Initialize", "ParkInfo 가져오기 오류");
                return false;
            }
            else
            {
                DataRow fRow = parkInfoDt.Rows[0];
                parkInfoPayload.companyName = Helper.NVL(fRow["Admin"]?.ToString());
                parkInfoPayload.licenseNo = Helper.NVL(fRow["RegNo"]?.ToString());
                parkInfoPayload.zoneAddress = Helper.NVL(fRow["ParkAddr"]?.ToString());
                parkInfoPayload.zoneName = Helper.NVL(fRow["ParkName"]?.ToString());
            }
            //ParkInfo DB Load 완료 =====

            webport = SysConfig.Instance.HW_Port;
            MyHttpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, webport);

            aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
            aliveCheckThread.Name = "Salda thread for alive check";
            waitForWork = TimeSpan.FromMinutes(40); //40분마다 Alive Check~!

            return true;
        }

        private void AliveCheck()
        {
            do
            {
                if (shutdownEvent.IsSet) return;
                {
                    //Alive Check 서버로 전달....
                    Log.WriteLog(LogType.Info, $"SaldaAdapter | AliveCheck", $"Alive Check~!");

                    try
                    {
                        Uri uri = new Uri(string.Concat(hostDomain, SND_PING));
                        
                        JObject json = new JObject();
                        json["zoneId"] = aptId;
                        json["ip"] = Helper.GetLocalIP();
                        string strRequest = AESEncrypt.Encrypt(json.ToString(), aeskey);
                        byte[] requestData = SysConfig.Instance.HomeNet_Encoding.GetBytes(strRequest);
                        string responseData = string.Empty;
                        string responseHeader = string.Empty;

                        Dictionary<string, string> dicReqHeader = new Dictionary<string, string>();
                        dicReqHeader.Add("partner-id", ""); //TODO : 모든 요청시엔 partner-id를 전송해야 함.

                        if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Json, requestData, ref responseData, ref responseHeader, header: dicReqHeader))
                        {
                            if (responseData.StartsWith("ERR"))
                            {
                                Log.WriteLog(LogType.Error, "SaldaAdapter | SendMessage | WebClientResponse", $"==응답== {responseData}", LogAdpType.HomeNet);
                            }
                            else
                            {
                                string decVal = AESEncrypt.Decrypt(responseData, aeskey);
                                Log.WriteLog(LogType.Error, "SaldaAdapter | SendMessage | WebClientResponse", $"==응답== {decVal}", LogAdpType.HomeNet);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogType.Error, $"SaldaAdapter | AliveCheck", $"{ex.Message}");
                    }
                }

                shutdownEvent.Wait(waitForWork);
            }
            while (_pauseEvent.WaitOne());
        }
        
        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            //bResponseSuccess
            receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"SaldaAdapter | SendMessage", $"넥스파에서 받은 메시지!!!! : {jobj}", LogAdpType.HomeNet);
            JObject data = jobj["data"] as JObject; //응답 데이터
                                                    //결과 Payload 생성 =======
            JObject result = jobj["result"] as JObject; //응답 결과
            if (result != null && Helper.NVL(result["status"]) != "200")
            {
                string sCode = "";

                if (Helper.NVL(result["status"]) == "204") sCode = "404";
                else sCode = Helper.NVL(result["status"]);

                responseSdPayload.resultCode = sCode;
                responseSdPayload.resultMsg = Helper.NVL(result["message"]);
            }
            else
            {
                responseSdPayload.resultCode = "OK";
            }
            //결과 Payload 생성완료 =======

            string cmd = jobj["command"].ToString();
            switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
            {
                //입/출차 통보
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    {
                        RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                        payload.Deserialize(jobj);

                        Uri uri = new Uri(string.Concat(hostDomain, SND_ALERT));
                        byte[] requestData;

                        RequestSdAlertCarPayload alertCarPayload = new RequestSdAlertCarPayload();
                        alertCarPayload.zoneId = aptId;
                        alertCarPayload.eventId = "0";
                        alertCarPayload.carNo = payload.data.car_number;
                        alertCarPayload.deviceId = payload.data.lprID;
                        
                        if (payload.command == CmdType.alert_incar)
                        {
                            alertCarPayload.eventType = "In";
                        }
                        else
                        {
                            alertCarPayload.eventType = "Out";
                        }

                        alertCarPayload.link = payload.data.car_image;

                        if(payload.data.kind == "a")
                        {
                            alertCarPayload.whitelistType = "Regular";
                        }
                        else if (payload.data.kind == "v")
                        {
                            alertCarPayload.whitelistType = "Visitor";
                        }
                        else
                        {
                            alertCarPayload.whitelistType = "Guest";
                        }

                        alertCarPayload.metaData1 = payload.data.dong;
                        alertCarPayload.metaData2 = payload.data.ho;

                        string strAlert = AESEncrypt.Encrypt(alertCarPayload.ToJson().ToString(), aeskey);
                        requestData = SysConfig.Instance.HomeNet_Encoding.GetBytes(strAlert);
                        
                        string responseData = string.Empty;
                        string responseHeader = string.Empty;
                        ResponsePayload responsePayload = new ResponsePayload();
                        byte[] responseBuffer;
                        

                        if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Json, requestData, ref responseData, ref responseHeader, header: dicHeader))
                        {
                            responsePayload.command = payload.command;

                            if (responseData.StartsWith("ERR"))
                            {
                                Log.WriteLog(LogType.Error, "SaldaAdapter | SendMessage | WebClientResponse", $"==응답== {responseData}", LogAdpType.HomeNet);
                                string[] resArr = responseData.Split(',');
                                responsePayload.result = (ResultType)int.Parse(resArr[1]);
                            }
                            else
                            {
                                string decVal = AESEncrypt.Decrypt(responseData, aeskey);
                                Log.WriteLog(LogType.Error, "SaldaAdapter | SendMessage | WebClientResponse", $"==응답== {decVal}", LogAdpType.HomeNet);
                                responsePayload.result = ResultType.OK;
                            }
                            responseBuffer = responsePayload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                    }
                    break;
                case CmdType.visit_reg:
                case CmdType.cust_reg:
                    {
                        if(currentReqPayload != null)
                        {
                            RequestSdReg reqPayload = (RequestSdReg)currentReqPayload;
                            reqPayload.regId = Helper.NVL(data["reg_no"]);
                            reqPayload.createdAt = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
                            responseSdPayload.content = reqPayload;
                        }
                        bResponseSuccess = true;
                    }
                    break;
                case CmdType.cust_del:
                    {
                        bResponseSuccess = true;
                    }
                    break;
                case CmdType.visit_del:
                    {
                        bResponseSuccess = true;
                    }
                    break;
            }
        }

        private IPayload currentReqPayload;

        private void MyHttpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs e = null, string id = null)
        {
            lock (lockObj)
            {
                JObject json = null;
                bResponseSuccess = false;
                responseSdPayload.Initialize();
                
                string urlData = e.Request.Uri.LocalPath;
                string sMethod = e.Request.Method;

                string receiveEncryptMsg = SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]);
                json = JObject.Parse(Helper.ValidateJsonParseingData(AESEncrypt.Decrypt(receiveEncryptMsg, aeskey)));

                switch (urlData)
                {
                    case GET_PARKINGZONE:
                        currentReqPayload = null;
                        responseSdPayload.resultCode = "OK";
                        responseSdPayload.content = parkInfoPayload;
                        bResponseSuccess = true;
                        break;
                    case GET_WHITE_LIST:
                        currentReqPayload = null;
                        break;
                    case GET_LIST_VISITOR: //방문차량 조회
                        currentReqPayload = null;
                        break;
                    case GET_LIST_REGULAR: //정기차량 조회
                        currentReqPayload = null;
                        break;
                    case SET_REGISTRATION: //차량등록(정기/방문)
                        if(Helper.NVL(json["type"]).ToLower() == "regular")
                        {
                            //정기차량등록
                            RequestSdReg reqPayload = new RequestSdReg();
                            reqPayload.Deserialize(json);
                            currentReqPayload = reqPayload;
                            
                            RequestCustRegPayload dataPayload = new RequestCustRegPayload();
                            dataPayload.car_number = Helper.NVL(json["carNo"]);
                            dataPayload.dong = Helper.NVL(json["metaData1"]);
                            dataPayload.ho = Helper.NVL(json["metaData2"]);
                            dataPayload.name = Helper.NVL(json["customerName"]);
                            dataPayload.start_date = Helper.NVL(json["startDateTime"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd");
                            dataPayload.end_date = Helper.NVL(json["endDateTime"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd");
                            dataPayload.tel_number = Helper.NVL(json["mobileNo"]);
                            dataPayload.remark = Helper.NVL(json["memo"]);

                            RequestPayload<RequestCustRegPayload> payload = new RequestPayload<RequestCustRegPayload>();
                            payload.command = CmdType.cust_reg;
                            payload.data = dataPayload;
                            Log.WriteLog(LogType.Info, $"SaldaAdapter | HttpServer_ReceiveFromPeer", $"{SET_REGISTRATION}: {payload.ToJson()}", LogAdpType.HomeNet);

                            byte[] responseBuffer = payload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        else if (Helper.NVL(json["type"]).ToLower() == "visitor")
                        {
                            //방문차량등록
                            RequestSdReg reqPayload = new RequestSdReg();
                            reqPayload.Deserialize(json);
                            currentReqPayload = reqPayload;
                            
                            RequestVisitRegPayload dataPayload = new RequestVisitRegPayload();
                            dataPayload.car_number = Helper.NVL(json["carNo"]);
                            dataPayload.dong = Helper.NVL(json["metaData1"]);
                            dataPayload.ho = Helper.NVL(json["metaData2"]);
                            dataPayload.date = Helper.NVL(json["startDateTime"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd");
                            DateTime startDate = Convert.ToDateTime(Helper.NVL(json["startDateTime"]));
                            DateTime endDate = Convert.ToDateTime(Helper.NVL(json["endDateTime"]));

                            TimeSpan dateDiff = endDate - startDate;
                            string sTerm;
                            if (dateDiff.Days == 0) sTerm = "1";
                            else sTerm = dateDiff.Days.ToString();
                            dataPayload.term = sTerm;
                            //TODO : Remark 추가해야함... 2021-01-14
                            RequestPayload<RequestVisitRegPayload> payload = new RequestPayload<RequestVisitRegPayload>();
                            payload.command = CmdType.visit_reg;
                            payload.data = dataPayload;
                            Log.WriteLog(LogType.Info, $"SaldaAdapter | HttpServer_ReceiveFromPeer", $"{SET_REGISTRATION}: {payload.ToJson()}", LogAdpType.HomeNet);

                            byte[] responseBuffer = payload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        break;
                    case DEL_REMOVE: //차량삭제(정기/방문)
                        currentReqPayload = null;
                        if (Helper.NVL(json["type"]).ToLower() == "regular")
                        {
                            RequestCustDelPayload dataPayload = new RequestCustDelPayload();
                            dataPayload.car_number = Helper.NVL(json["carNo"]);
                            dataPayload.dong = Helper.NVL(json["metaData1"]);
                            dataPayload.ho = Helper.NVL(json["metaData2"]);
                            dataPayload.reg_no = Helper.NVL(json["regId"]);

                            RequestPayload<RequestCustDelPayload> payload = new RequestPayload<RequestCustDelPayload>();
                            payload.command = CmdType.cust_del;
                            payload.data = dataPayload;
                            Log.WriteLog(LogType.Info, $"SaldaAdapter | HttpServer_ReceiveFromPeer", $"{DEL_REMOVE}: {payload.ToJson()}", LogAdpType.HomeNet);

                            byte[] responseBuffer = payload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        else if (Helper.NVL(json["type"]).ToLower() == "visitor")
                        {
                            RequestVisitDelPayload dataPayload = new RequestVisitDelPayload();
                            dataPayload.dong = Helper.NVL(json["metaData1"]);
                            dataPayload.ho = Helper.NVL(json["metaData2"]);
                            dataPayload.reg_no = Helper.NVL(json["regId"]);
                            dataPayload.car_number = Helper.NVL(json["carNo"]);

                            RequestPayload<RequestVisitDelPayload> payload = new RequestPayload<RequestVisitDelPayload>();
                            payload.command = CmdType.visit_del;
                            payload.data = dataPayload;
                            Log.WriteLog(LogType.Info, $"SaldaAdapter | HttpServer_ReceiveFromPeer", $"{DEL_REMOVE}: {payload.ToJson()}", LogAdpType.HomeNet);

                            byte[] responseBuffer = payload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        break;
                }

                //Task waitTask = WaitTask(15);
                //await waitTask;
                //5초 대기 Task
                int iSec = 5 * 100; //10초
                while (iSec > 0 && !bResponseSuccess)
                {
                    Thread.Sleep(10); //0.01초씩..쉰다...
                    iSec -= 1;
                }

                if (bResponseSuccess) //응답성공
                {
                    byte[] result = responseSdPayload.Serialize();
                    e.Response.Status = System.Net.HttpStatusCode.OK;
                    e.Response.Encoding = SysConfig.Instance.HomeNet_Encoding;
                    e.Response.ContentType = new ContentTypeHeader("application/json");
                    
                    Log.WriteLog(LogType.Info, $"SaldaAdapter | 응답(성공)", $"{responseSdPayload.ToJson()}", LogAdpType.HomeNet);
                    e.Response.Body.Write(result, 0, result.Length);
                }
                else
                {
                    MvlResponsePayload payload = new MvlResponsePayload();
                    payload.resultCode = MvlResponsePayload.SttCode.NotSupportedMethod;
                    payload.resultMessage = "주차 시스템으로 부터 응답이 없습니다";
                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    byte[] result = payload.Serialize();
                    e.Response.Encoding = SysConfig.Instance.HomeNet_Encoding;
                    e.Response.ContentType = new ContentTypeHeader("application/json");
                    
                    Log.WriteLog(LogType.Info, $"SaldaAdapter | 응답(TimeOut)", $"{responseSdPayload.ToJson()}", LogAdpType.HomeNet);
                    e.Response.Body.Write(result, 0, result.Length);
                }
            }
        }

        public bool StartAdapter()
        {
            try
            {
                MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
                isRun = MyHttpNetwork.Run();

                if (isRun)
                {
                    //Alive Check Thread 시작
                    if (aliveCheckThread.IsAlive)
                    {
                        _pauseEvent.Set();
                    }
                    else
                    {
                        aliveCheckThread.Start();
                        _pauseEvent.Set();
                    }
                }

                return isRun;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SaldaAdapter | StartAdapter", ex.Message);
                return false;
            }
            
        }

        public bool StopAdapter()
        {
            try
            {
                //Alive Check Thread pause
                _pauseEvent.Reset();

                bool bResult = false;
                MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
                bResult = MyHttpNetwork.Down();

                isRun = !bResult;
                return bResult;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SaldaAdapter | StopAdapter", ex.Message);
                return false;
            }
        }

        public void TestReceive(byte[] buffer)
        {
            string json = "{\"command\": \"alert_incar\",\"data\": {\"dong\" : \"101\"," +
                            "\"ho\" : \"101\"," +
                            $"\"car_number\" : \"46부5989\"," +
                            "\"date_time\" : \"20210305042525\"," +
                            "\"kind\" : \"v\"," +
                            "\"lprid\" : \"Lpr 식별 번호\"," +
                            "\"car_image\" : \"차량 이미지 경로\"," +
                            $"\"reg_no\" : \"111111\"," +
                            "\"visit_in_date_time\" : \"yyyyMMddHHmmss\"," + //방문시작일시, kind가 v 일 경우 외 빈값
                            "\"visit_out_date_time\" : \"yyyyMMddHHmmss\"" + //방문종료일시, kind가 v 일 경우 외 빈값
                            "}" +
                            "}";
            byte[] test = SysConfig.Instance.Nexpa_Encoding.GetBytes(json);
            SendMessage(test, 0, test.Length);
        }
    }
}
