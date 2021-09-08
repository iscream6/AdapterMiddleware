using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NpmAdapter.Model;
using NpmAdapter.Payload;
using NpmCommon;
using NpmNetwork;
using NpmStandardLib.IO.Encrypt;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class SaldaAdapter : IAdapter
    {
        private const string GET_PARKINGZONE = "/parking/zone"; //4.1 Zone정보(주차장 정보)
        private const string GET_DEVICES = "/parking/devices"; //4.2 장치 정보
        private const string GET_STATUS = "/parking/status"; //4.3 주차 현황
        private const string GET_WHITE_LIST = "/parking/whitelist/list"; //4.4 화이트리스트 조회
        private const string SET_REGISTRATION = "/parking/whitelist/registration"; //4.5 화이트리스트 등록
        private const string SET_UPDATE = "/parking/whitelist/update"; //4.6 화이트리스트 수정
        private const string DEL_REMOVE = "/parking/whitelist/remove"; //4.7 화이트리스트 삭제
        private const string CON_DEVICE = "/parking/devices/control"; //4.9 차단기 제어
        private const string SND_ALERT = "/ext/parking/event";
        private const string SND_PING = "/ext/parking/ping";
        private const string GET_LIST_IOCAR = "/parking/event"; //4.8 입출차내역 조회

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
        private IPayload currentReqPayload;

        public event IAdapter.ShowBallonTip ShowTip;

        public IAdapter TargetAdapter { get; set; }
        private INetwork MyHttpNetwork { get; set; }
        public bool IsRuning { get => isRun; }
        public string reqPid { get; set; }
        private string ParkNo { get; set; }

        #region Model

        private ParkInfoModel PrkInf = new ParkInfoModel();
        private DataTable parkInfoDt;

        private UnitModel UitInf = new UnitModel();
        private DataTable unitInfoDt;

        #endregion
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
            
            if(SysConfig.Instance.Sys_NexpaAdapter == "TCP")
            {
                parkInfoDt = PrkInf.GetParkInfo();
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

                    ParkNo = Helper.NVL(fRow["ParkNo"]?.ToString());
                }
                //ParkInfo DB Load 완료 =====

                //UnitInfo DB Load =========
                unitInfoDt = UitInf.GetLprInfo();
                //UnitInfo DB Load 완료 =====
            }

            webport = SysConfig.Instance.HW_Port;
            MyHttpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, webport);

            aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
            aliveCheckThread.Name = "Salda thread for alive check";
            waitForWork = TimeSpan.FromMinutes(40); //40분마다 Alive Check~!

            return true;
        }

        private byte[] ParseDataEncoding(string Data)
        {
            string strEncrypt = AESEncrypt.Encrypt(Data, aeskey);
            byte[] resultData = SysConfig.Instance.HomeNet_Encoding.GetBytes(strEncrypt);

            return resultData;
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
                        byte[] requestData = ParseDataEncoding(json.ToString());
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

        private Dictionary<string, IPayload> dicTempSearch = new Dictionary<string, IPayload>();

        private void MyHttpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs e = null, string id = null, System.Net.EndPoint ep = null)
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
                    case GET_STATUS: //4.3 주차 현황
                        {
                            JObject sttJson = new JObject();
                            JObject sttDataJson = new JObject();
                            sttJson["command"] = CmdType.guidance.ToString();
                            sttDataJson["park_no"] = Helper.NVL(json["zoneId"]);
                            sttJson["data"] = sttDataJson;
                            byte[] responseBuffer = sttJson.ToByteArray(SysConfig.Instance.Nexpa_Encoding);
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        break;
                    case GET_DEVICES: //4.2 장치 정보
                        {
                            RequestPayload<RequestLprInfoPayload> payload = new RequestPayload<RequestLprInfoPayload>();
                            payload.command = CmdType.div_list;

                            RequestLprInfoPayload data = new RequestLprInfoPayload();
                            data.park_no = Helper.NVL(json["zoneId"]);
                            
                            payload.data = data;
                            byte[] responseBuffer = payload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        break;
                    case GET_PARKINGZONE:
                        currentReqPayload = null;
                        responseSdPayload.resultCode = "OK";
                        responseSdPayload.content = parkInfoPayload;
                        bResponseSuccess = true;
                        break;
                    case GET_WHITE_LIST: //4.4 화이트리스트 조회
                        {
                            RequestPayload<RequestListPayload> payload = new RequestPayload<RequestListPayload>();
                            payload.command = CmdType.list;

                            RequestListPayload data = new RequestListPayload();
                            data.park_no = Helper.NVL(json["zoneId"]);
                            data.page = Helper.NVL(json["pageNumber"]);
                            data.count = Helper.NVL(json["pageSize"]);
                            data.reg_no = Helper.NVL(json["regId"]);

                            if (Helper.NVL(json["type"]) == "Regular") data.search_type = RequestListPayload.SearchType.cust;
                            else data.search_type = RequestListPayload.SearchType.visit;
                            
                            if (Helper.NVL(json["dateTimeFilterType"]) == "StartDateTime") data.filter_type = RequestListPayload.FilterType.start;
                            else data.filter_type = RequestListPayload.FilterType.end;

                            data.start_date = Helper.NVL(json["dateTimeFrom"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd");
                            data.end_date = Helper.NVL(json["dateTimeTo"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd");
                            data.lprID = Helper.NVL(json["deviceId"]);
                            data.car_number = Helper.NVL(json["carNo"]);
                            data.tel_number = Helper.NVL(json["mobileNo"]);
                            data.dong = Helper.NVL(json["metaData1"]);
                            data.ho = Helper.NVL(json["metaData2"]);
                            
                            payload.data = data;
                            byte[] responseBuffer = payload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        currentReqPayload = null;
                        break;
                    case SET_REGISTRATION: //4.5 차량등록(정기/방문)
                        if (Helper.NVL(json["type"]).ToLower() == "regular")
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
                            dataPayload.remark = Helper.NVL(json["memo"]);
                            RequestPayload<RequestVisitRegPayload> payload = new RequestPayload<RequestVisitRegPayload>();
                            payload.command = CmdType.visit_reg;
                            payload.data = dataPayload;
                            Log.WriteLog(LogType.Info, $"SaldaAdapter | HttpServer_ReceiveFromPeer", $"{SET_REGISTRATION}: {payload.ToJson()}", LogAdpType.HomeNet);

                            byte[] responseBuffer = payload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        break;
                    case SET_UPDATE: //4.6 화이트리스트 수정
                        //가능하면 지원.... 지원하지 말자..
                        break;
                    case DEL_REMOVE: //4.7 차량삭제(정기/방문)
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
                    case GET_LIST_IOCAR: //4.8 입출차내역 조회
                        {
                            RequestIOListPayload dataPayload = new RequestIOListPayload();
                            dataPayload.park_no = Helper.NVL(json["zoneId"]);
                            dataPayload.car_number = Helper.NVL(json["carNo"]);
                            dataPayload.tel_number = Helper.NVL(json["mobileNo"]);
                            dataPayload.dong = Helper.NVL(json["metaData1"]);
                            dataPayload.ho = Helper.NVL(json["metaData2"]);
                            dataPayload.page = Helper.NVL(json["pageNumber"]);
                            dataPayload.count = Helper.NVL(json["pageSize"]);
                            dataPayload.start_date = Helper.NVL(json["dateTimeFrom"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd");
                            dataPayload.end_date = Helper.NVL(json["dateTimeTo"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd");

                            RequestPayload<RequestIOListPayload> payload = new RequestPayload<RequestIOListPayload>();
                            payload.command = CmdType.io_list;
                            payload.data = dataPayload;

                            Log.WriteLog(LogType.Info, $"SaldaAdapter | HttpServer_ReceiveFromPeer", $"{GET_LIST_IOCAR}: {payload.ToJson()}", LogAdpType.HomeNet);

                            byte[] responseBuffer = payload.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                        break;
                }

                //Task waitTask = WaitTask(15);
                //await waitTask;
                //5초 대기 Task
                int iSec = 5 * 100; //5초
                while (iSec > 0 && !bResponseSuccess)
                {
                    Thread.Sleep(10); //0.01초씩..쉰다...
                    iSec -= 1;
                }

                if (bResponseSuccess) //응답성공
                {
                    byte[] result = ParseDataEncoding(responseSdPayload.ToJson().ToString());
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

        /// <summary>
        /// Nexpa 에서 보내온 메시지 처리
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="pid"></param>
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
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, pid);
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
