using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class BeyondAdapter : IAdapter
    {
        private struct FailMsg : IEquatable<FailMsg>
        {
            public enum Type
            {
                None,
                In,
                Out,
                Both
            }

            public enum SuccessYon
            {
                None,
                Success,
                Fail
            }

            public Type type;
            public SuccessYon successType;
            public int Count;
            public string tkNo;
            public string carNo;
            public string enterOffsetDateTime;
            public string exitOffsetDateTime;

            public FailMsg(int cnt)
                : this()
            {
                type = Type.None;
                successType = SuccessYon.None;
                Count = cnt;
                tkNo = "";
                carNo = "";
                enterOffsetDateTime = "";
                exitOffsetDateTime = "";
            }

            public bool Equals(FailMsg other)
            {
                if (other.tkNo == tkNo) return true;
                else return false;
            }
        }

        /// <summary>
        /// 기기 상태 조회 URL
        /// </summary>
        private const string ALIVE_CHECK = "/car/instant-message";
        /// <summary>
        /// 입차 알림 URL
        /// </summary>
        private const string ALERT_INCAR = "/car/entrance";
        /// <summary>
        /// 출차 알림 URL
        /// </summary>
        private const string ALERT_OUTCAR = "/car/exit";
        /// <summary>
        /// 누락 데이터 Sync URL
        /// </summary>
        private const string SYNC_DATA = "/car/omissionDataSync";

        private bool isRun = false;
        private string _Domain = string.Empty;
        private Dictionary<string, string> dicHeader = new Dictionary<string, string>();
        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private List<FailMsg> failSendList = new List<FailMsg>();
        //==== Alive Check ====
        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private delegate void SafeCallDelegate();
        //==== Alive Check ====

        //==== Fail Process Thread ====
        private Thread failProcessThread;
        private TimeSpan waitForFailProcess;
        private ManualResetEventSlim shutdownProcessEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseFailProcessEvent = new ManualResetEvent(false);
        private delegate void ProcessSafeCallDelegate();
        //==== Fail Process Thread ====

        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => isRun;

        public void Dispose()
        {
            _pauseEvent.Set();
            shutdownEvent.Set();

            _pauseFailProcessEvent.Set();
            shutdownProcessEvent.Set();
        }

        public bool Initialize()
        {
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "BeyondAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.Nexpa);
                return false;
            }

            try
            {
                _Domain = SysConfig.Instance.HW_Domain;
                aliveCheckThread = new Thread(new ThreadStart(AliveCheckAction));
                aliveCheckThread.Name = "alive check";
                waitForWork = TimeSpan.FromSeconds(10);

                failProcessThread = new Thread(new ThreadStart(FailProcessAction));
                failProcessThread.Name = "process";
                waitForFailProcess = TimeSpan.FromSeconds(15); //1초
                Log.WriteLog(LogType.Info, $"BeyondAdapter | Initialize", $"==초기화==", LogAdpType.HomeNet);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"BeyondAdapter | Initialize", $"{ex.Message}", LogAdpType.Nexpa);
                return false;
            }

            return true;
        }

        public bool StartAdapter()
        {
            isRun = true;
            dicHeader.Add("Authorization", "bearer " + GetAccessToken(_Domain));
            if (aliveCheckThread.IsAlive)
            {
                _pauseEvent.Set();
            }
            else
            {
                aliveCheckThread.Start();
                _pauseEvent.Set();
            }

            if (failProcessThread.IsAlive)
            {
                _pauseFailProcessEvent.Set();
            }
            else
            {
                failProcessThread.Start();
                _pauseFailProcessEvent.Set();
            }

            return true;
        }

        public bool StopAdapter()
        {
            isRun = false;
            _pauseEvent.Reset();
            _pauseFailProcessEvent.Reset();
            return true;
        }

        Dictionary<string, FailMsg> dicMsgBuffer = new Dictionary<string, FailMsg>();

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            try
            {
                receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                var jobj = JObject.Parse(receiveMessageBuffer.ToString());
                Thread.Sleep(10);
                receiveMessageBuffer.Clear();

                Log.WriteLog(LogType.Info, $"BeyondAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
                JObject data = jobj["data"] as JObject; //응답 데이터

                string cmd = jobj["command"].ToString();
                switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
                {
                    case CmdType.alert_incar:
                    case CmdType.alert_outcar:
                        {
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);

                            RequestByInOutCarPayload ioPayload = new RequestByInOutCarPayload();

                            Uri uri = null;
                            byte[] requestData;

                            if (payload.command == CmdType.alert_incar)
                            {
                                ioPayload.ioType = RequestByInOutCarPayload.IOType.IN;
                                uri = new Uri(string.Concat(_Domain, ALERT_INCAR));
                                //===== 실패여부 처리 =====
                                FailMsg newMsg = new FailMsg(0);
                                newMsg.tkNo = payload.data.reg_no;
                                newMsg.type = FailMsg.Type.In;
                                newMsg.carNo = payload.data.car_number;
                                newMsg.enterOffsetDateTime = DateTime.ParseExact(payload.data.date_time, "yyyyMMddHHmmss", null).ToString("yyyy-MM-ddTHH:mm:sszzz");
                                dicMsgBuffer.Add(payload.data.reg_no, newMsg);
                                //===== 실패여부 처리 완료=====
                            }
                            else
                            {
                                ioPayload.ioType = RequestByInOutCarPayload.IOType.OUT;
                                uri = new Uri(string.Concat(_Domain, ALERT_OUTCAR));

                                //===== 실패여부 처리 =====
                                FailMsg newMsg;
                                if (dicMsgBuffer.ContainsKey(payload.data.reg_no))
                                {
                                    newMsg = dicMsgBuffer[payload.data.reg_no];
                                    dicMsgBuffer.Remove(payload.data.reg_no);
                                    newMsg.type = FailMsg.Type.Out;
                                    newMsg.exitOffsetDateTime = DateTime.ParseExact(payload.data.date_time, "yyyyMMddHHmmss", null).ToString("yyyy-MM-ddTHH:mm:sszzz");
                                    dicMsgBuffer.Add(payload.data.reg_no, newMsg);
                                }
                                //===== 실패여부 처리 완료=====
                            }

                            ioPayload.carNo = payload.data.car_number;
                            ioPayload.dateTime = DateTime.ParseExact(payload.data.date_time, "yyyyMMddHHmmss", null).ToString("yyyy-MM-ddTHH:mm:sszzz");

                            requestData = ioPayload.Serialize();

                            string responseData = string.Empty;
                            string responseHeader = string.Empty;

                            if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Json, requestData, ref responseData, ref responseHeader, header: dicHeader))
                            {
                                ResponsePayload responsePayload = new ResponsePayload();
                                byte[] responseBuffer = null;

                                responsePayload.command = payload.command;

                                try
                                {
                                    var responseJobj = JObject.Parse(responseData);
                                    if (responseJobj != null && Helper.NVL(responseJobj["RESULT_CODE"]) == "200")
                                    {
                                        responsePayload.result = ResultType.OK;
                                        responseBuffer = responsePayload.Serialize();
                                        //===== 실패여부 처리 =====
                                        if (dicMsgBuffer.ContainsKey(payload.data.reg_no))
                                        {
                                            switch (dicMsgBuffer[payload.data.reg_no].successType)
                                            {
                                                case FailMsg.SuccessYon.None:
                                                    {
                                                        var msg = dicMsgBuffer[payload.data.reg_no];
                                                        dicMsgBuffer.Remove(payload.data.reg_no);
                                                        if (msg.type == FailMsg.Type.In)
                                                        {
                                                            msg.successType = FailMsg.SuccessYon.Success;
                                                            dicMsgBuffer.Add(payload.data.reg_no, msg);
                                                        }
                                                        else
                                                        {
                                                            dicMsgBuffer.Remove(payload.data.reg_no);
                                                        }
                                                    }
                                                    break;
                                                case FailMsg.SuccessYon.Success:
                                                    //2번 Success 면 삭제한다.
                                                    dicMsgBuffer.Remove(payload.data.reg_no);
                                                    break;
                                            }
                                        }
                                        //===== 실패여부 처리 완료=====
                                    }
                                    else
                                    {
                                        //===== 실패여부 처리 =====
                                        //보냈으나 받아주지 않으면 삭제한다.
                                        dicMsgBuffer.Remove(payload.data.reg_no);
                                        //===== 실패여부 처리 완료=====
                                        responsePayload.result = ResultType.FailSendMessage;
                                        responseBuffer = responsePayload.Serialize();
                                    }

                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                }
                                catch (Exception ex)
                                {
                                    responsePayload.result = ResultType.ExceptionERROR;
                                    responseBuffer = responsePayload.Serialize();
                                    Log.WriteLog(LogType.Error, "BeyondAdapter | SendMessage", $"{ex.Message}", LogAdpType.HomeNet);
                                    //===== 실패여부 처리 =====
                                    if (dicMsgBuffer.ContainsKey(payload.data.reg_no))
                                    {
                                        var msg = dicMsgBuffer[payload.data.reg_no];
                                        dicMsgBuffer.Remove(payload.data.reg_no);
                                        msg.successType = FailMsg.SuccessYon.Fail;
                                        dicMsgBuffer.Add(payload.data.reg_no, msg);
                                    }
                                    //===== 실패여부 처리 완료=====
                                }
                                finally
                                {
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                }
                            }
                            else
                            {
                                //===== 실패여부 처리 =====
                                if (dicMsgBuffer.ContainsKey(payload.data.reg_no))
                                {
                                    var msg = dicMsgBuffer[payload.data.reg_no];
                                    dicMsgBuffer.Remove(payload.data.reg_no);
                                    msg.successType = FailMsg.SuccessYon.Fail;
                                    dicMsgBuffer.Add(payload.data.reg_no, msg);
                                }
                                //===== 실패여부 처리 완료=====
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "BeyondAdapter | SendMessage", $"JSON Pasing Error : {ex.Message}", LogAdpType.HomeNet);
            }
        }

        private void FailBufferAction(string carno, string indt, string outdt)
        {
            FailMsg failMsgCompare = new FailMsg();
            failMsgCompare.carNo = carno;
            if (failSendList.Contains(failMsgCompare))
            {
                var msg = failSendList.Find(x => x.Equals(failMsgCompare));
                if (indt != null)
                {
                    if (msg.enterOffsetDateTime == indt) return;
                }
            }
        }

        private string GetAccessToken(string domain)
        {
            string secret = SysConfig.Instance.AuthToken;
            Uri uri = new Uri($"{domain}/oauth/token");

            //Response Valiable
            string accessToken = string.Empty;
            string responseData = string.Empty;
            string responseHeader = string.Empty;

            try
            {
                //Request
                string postMessage = $"grant_type=client_credentials&client_id=NEXPA&client_secret={secret}";
                Dictionary<string, string> dicAccHeader = new Dictionary<string, string>();
                Log.WriteLog(LogType.Info, "BeyondAdapter | GetAccessToken", $"Access Token 발급", LogAdpType.HomeNet);

                if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.FormData, SysConfig.Instance.HomeNet_Encoding.GetBytes(postMessage), ref responseData, ref responseHeader, header: dicAccHeader))
                {
                    JObject json = JObject.Parse(responseData);
                    accessToken = Helper.NVL(json["access_token"]);
                }

            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "KakaoMovilAdapter | GetAccessToken", $"Exception Message : {ex.Message}", LogAdpType.HomeNet);
            }

            return accessToken;
        }

        private void FailProcessAction()
        {
            List<FailMsg> list = new List<FailMsg>();
            JObject json = new JObject();
            json["siteId"] = SysConfig.Instance.HC_Id;
            JArray array = new JArray();
            foreach (var tkNo in dicMsgBuffer.Keys)
            {
                if (dicMsgBuffer[tkNo].successType == FailMsg.SuccessYon.Fail)
                {
                    var msg = dicMsgBuffer[tkNo];
                    msg.Count += 1;
                    JObject subJson = new JObject();
                    subJson["carNO"] = msg.carNo;
                    subJson["enterOffsetDateTime"] = msg.enterOffsetDateTime;
                    subJson["carNO"] = msg.exitOffsetDateTime;
                    array.Add(subJson);
                    list.Add(msg);
                }
            }

            json["omissionDataList"] = array;

            if (list.Count > 0)
            {
                //버퍼에서 Fail Message를 모두 삭제
                foreach (var fail in list)
                {
                    dicMsgBuffer.Remove(fail.tkNo);
                }

                string responseData = string.Empty;
                string responseHeader = string.Empty;
                Uri uri = new Uri(string.Concat(_Domain, SYNC_DATA));
                byte[] reqData = json.ToByteArray(SysConfig.Instance.HomeNet_Encoding);
                if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.FormData, reqData, ref responseData, ref responseHeader, header: dicHeader))
                {
                    List<FailMsg> reFailList = new List<FailMsg>();
                    JObject resjson = JObject.Parse(responseData);
                    JArray resArray = resjson["omissionDataList"] as JArray;
                    foreach (var item in resArray)
                    {
                        //응답받은 항목만 추출한다.
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].carNo == Helper.NVL(item["carNo"])
                                && list[i].enterOffsetDateTime == Helper.NVL(item["enterOffsetDateTime"])
                                && list[i].exitOffsetDateTime == Helper.NVL(item["exitOffsetDateTime"]))
                            {
                                dicMsgBuffer.Add(list[i].tkNo, list[i]);
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void AliveCheckAction()
        {
            string responseData = string.Empty;
            string responseHeader = string.Empty;

            RequestByAliveCheckPayload aliveCheckPayload = new RequestByAliveCheckPayload();
            aliveCheckPayload.requestType = "TEST_ALIVE";

            Uri uri = new Uri($"{_Domain}{ALIVE_CHECK}");
            if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Json, aliveCheckPayload.Serialize(), ref responseData, ref responseHeader, header: dicHeader))
            {
                JObject responseJson = JObject.Parse(responseData);

                Log.WriteLog(LogType.Info, "BeyondAdapter | AliveCheckAction", $"{responseJson}", LogAdpType.HomeNet);
            }
        }

        public void TestReceive(byte[] buffer)
        {

        }
    }
}
