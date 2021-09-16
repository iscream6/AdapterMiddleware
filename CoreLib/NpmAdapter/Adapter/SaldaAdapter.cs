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
        private IPayload parkInfoPayload;
        private IPayload unitInfoPayload;
        private Dictionary<string, string> dicHeader = new Dictionary<string, string>();

        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        private ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private delegate void SafeCallDelegate();

        public event IAdapter.ShowBallonTip ShowTip;

        public IAdapter TargetAdapter { get; set; }
        private INetwork MyHttpNetwork { get; set; }
        public bool IsRuning { get => isRun; }
        public string reqPid { get; set; }

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
            
            if(SysConfig.Instance.Sys_NexpaAdapter == "TCP")
            {
                //ParkInfo DB Load =========
                parkInfoPayload = SaldaPacade.Instance.GetZoneInfo(aptId);
                //ParkInfo DB Load 완료 =====

                //UnitInfo DB Load =========
                unitInfoPayload = SaldaPacade.Instance.GetDeviceInfo(aptId);
                //UnitInfo DB Load 완료 =====
            }

            webport = SysConfig.Instance.HW_Port;
            MyHttpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, webport);

#if (!DEBUG)
            aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
            aliveCheckThread.Name = "Salda thread for alive check";
            waitForWork = TimeSpan.FromMinutes(40); //40분마다 Alive Check~!
#endif

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
                IPayload contentPayload = null;

                string urlData = e.Request.Uri.LocalPath;
                string sMethod = e.Request.Method;

                string receiveEncryptMsg = SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]);
                json = JObject.Parse(Helper.ValidateJsonParseingData(AESEncrypt.Decrypt(receiveEncryptMsg, aeskey)));
                
                Dictionary<ColName, object> param = new Dictionary<ColName, object>();
                ModelResult mResult = new ModelResult();

                switch (urlData)
                {
                    case GET_PARKINGZONE: //4.1 Zone 정보 == 완료
                        {
                            contentPayload = parkInfoPayload;
                            bResponseSuccess = true;
                        }
                        break;
                    case GET_DEVICES: //4.2 장치 정보 == 완료
                        {
                            contentPayload = unitInfoPayload;
                            bResponseSuccess = true;
                        }
                        break;
                    case GET_STATUS: //4.3 주차 현황 -- 안됨... PASS
                        {
                            mResult.code = ModelCode.NotAcceptable;
                            mResult.Message = "지원하지 않는 기능입니다.(주차현황)";
                            bResponseSuccess = true;
                        }
                        break;
                    case GET_WHITE_LIST: //4.4 화이트리스트 조회 == 완료
                        {
                            var sResult = SaldaPacade.Instance.GetWhiteList(json);
                            mResult = sResult.result;
                            contentPayload = sResult.payload;
                            bResponseSuccess = true;
                        }
                        break;
                    case SET_REGISTRATION: //4.5 차량등록(정기/방문) === 완료
                        {
                            var sResult = SaldaPacade.Instance.RegistCar(json);
                            mResult = sResult.result;
                            contentPayload = sResult.payload;
                            bResponseSuccess = true;
                        }
                        break;
                    case SET_UPDATE: //4.6 화이트리스트 수정 PASS
                        mResult.code = ModelCode.NotAcceptable;
                        mResult.Message = "지원하지 않는 기능입니다.(화이트리스트 수정)";
                        bResponseSuccess = true;
                        break;
                    case DEL_REMOVE: //4.7 차량삭제(정기/방문) === 완료
                        {
                            var sResult = SaldaPacade.Instance.RemoveCar(json);
                            mResult = sResult.result;
                            bResponseSuccess = true;
                        }
                        break;
                    case GET_LIST_IOCAR: //4.8 입출차내역 조회
                        {
                            var sResult = SaldaPacade.Instance.GetIOList(json);
                            mResult = sResult.result;
                            contentPayload = sResult.payload;
                            bResponseSuccess = true;
                        }
                        break;
                }

                if (bResponseSuccess) //응답성공
                {
                    responseSdPayload.resultCode = mResult.code.ToString();
                    responseSdPayload.resultMsg = Helper.NVL(mResult.Message);
                    responseSdPayload.content = contentPayload ?? null;

                    switch (mResult.code)
                    {
                        case ModelCode.OK:
                            e.Response.Status = System.Net.HttpStatusCode.OK;
                            break;
                        case ModelCode.AlreadyRegisted:
                            e.Response.Status = System.Net.HttpStatusCode.Conflict;
                            break;
                        case ModelCode.Fail:
                        case ModelCode.Exception:
                            e.Response.Status = System.Net.HttpStatusCode.InternalServerError;
                            break;
                        case ModelCode.NotFound:
                            e.Response.Status = System.Net.HttpStatusCode.NotFound;
                            break;
                        case ModelCode.NotAcceptable:
                            e.Response.Status = System.Net.HttpStatusCode.NotAcceptable;
                            break;
                        default:
                            e.Response.Status = System.Net.HttpStatusCode.InternalServerError;
                            break;  
                    }

                    e.Response.Encoding = SysConfig.Instance.HomeNet_Encoding;
                    e.Response.ContentType = new ContentTypeHeader("application/json");

                    byte[] result = ParseDataEncoding(responseSdPayload.ToJson().ToString());

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
            try
            {
                //bResponseSuccess
                receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
                Thread.Sleep(10);
                receiveMessageBuffer.Clear();

                Log.WriteLog(LogType.Info, $"SaldaAdapter | SendMessage", $"넥스파에서 받은 메시지!!!! : {jobj}", LogAdpType.HomeNet);
                //명령 데이터
                string cmd = jobj["command"].ToString();
                //응답 데이터
                JObject data = jobj["data"] as JObject; 
                //결과 데이터
                JObject result = jobj["result"] as JObject; //응답 결과
                if (result != null && Helper.NVL(result["status"]) != "200")
                {
                    responseSdPayload.resultCode = $"Fail_{cmd}";
                    responseSdPayload.resultMsg = Helper.NVL(result["message"]);
                }
                else
                {
                    responseSdPayload.resultCode = "OK";
                    responseSdPayload.resultMsg = "";
                }
                
                switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
                {
                    case CmdType.hello:
                        string handShakeData = jobj["data"].ToString();
                        if (Helper.NVL(handShakeData) == "biz") reqPid = pid;
                        break;
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

                            if (payload.data.kind == "a")
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
                }
            }
            catch (Exception)
            {
                receiveMessageBuffer.Clear();
                throw;
            }
            
        }

        public bool StartAdapter()
        {
            try
            {
                MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
                isRun = MyHttpNetwork.Run();

#if (!DEBUG)
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
#endif

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
