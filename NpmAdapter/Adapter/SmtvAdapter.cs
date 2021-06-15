using HttpServer;
using HttpServer.Headers;
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
    /// <summary>
    /// 스마트빌리지 아답터
    /// </summary>
    class SmtvAdapter : IAdapter
    {
        public event IAdapter.ShowBallonTip ShowTip;

        private const string REQ_POST_STATUS = "/nxmdl/cmx";
        private const string REQ_POST_EVENT = "/api/v1/events";
        private const string REQ_POST_ASSIGN = "/api/v1/assignments";

        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private JObject responseJson = null;
        private object lockObj = new object();
        private Dictionary<string, string> dicHeader;
        private bool isRun = false;
        private bool bResponseSuccess = true;
        private string carNo = "";
        private INetwork MyHttpNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning { get => isRun; }

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "SmtvAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.HomeNet);
                return false;
            }

            var webport = SysConfig.Instance.HW_Port;
            MyHttpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, webport);

            dicHeader = new Dictionary<string, string>();
            string strAuth = SysConfig.Instance.HC_Id + ":" + SysConfig.Instance.HC_Pw;
            dicHeader.Add("Authorization", $"Basic {strAuth.Base64Encode()}");
            Log.WriteLog(LogType.Info, "SmtvAdapter | Initialize", $"인증토큰 : Basic {strAuth.Base64Encode()}", LogAdpType.HomeNet);
            return true;
        }

        private void MyHttpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, RequestEventArgs e, string id, System.Net.EndPoint ep = null)
        {
            lock (lockObj)
            {
                bResponseSuccess = false;
                ResponseDataPayload responsePayload = new ResponseDataPayload();
                string urlData = e.Request.Uri.PathAndQuery;
                Log.WriteLog(LogType.Info, $"SmtvAdapter | MyHttpNetwork_ReceiveFromPeer", $"URL : {urlData}", LogAdpType.HomeNet);
                if (urlData != REQ_POST_STATUS)
                {
                    responseJson = new JObject();
                    responseJson["command"] = "visit_reg2";
                    JObject jData = new JObject();
                    jData["reg_no"] = "";
                    responseJson["data"] = jData;
                    JObject jResult = new JObject();
                    jResult["status"] = "-100";
                    jResult["message"] = "OK";
                    responseJson["result"] = jResult;
                    responsePayload.Deserialize(responseJson);
                    byte[] result = responsePayload.Serialize();

                    e.Response.Connection.Type = ConnectionType.Close;
                    e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                    e.Response.Status = System.Net.HttpStatusCode.BadRequest;
                    e.Response.Reason = "Bad Request";
                    e.Response.Body.Write(result, 0, result.Length);
                }
                else
                {
                    string receiveMsg = SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]);
                    Log.WriteLog(LogType.Info, "SmtvAdapter | MyHttpNetwork_ReceiveFromPeer", $"받은메시지 : {receiveMsg}", LogAdpType.HomeNet);

                    JObject json = JObject.Parse(Helper.ValidateJsonParseingData(receiveMsg));
                    if (Helper.NVL(json["command"]) == "visit_reg2")
                    {
                        RequestPayload<RequestVisitReg2Payload> payload = new RequestPayload<RequestVisitReg2Payload>();
                        payload.Deserialize(json);
                        //MainForm으로 메시지 이벤트 전송
                        ShowTip?.Invoke(1, "방문예약", $"{payload.data.dong}동 {payload.data.ho}호 {payload.data.car_number}");

                        carNo = payload.data.car_number;
                        byte[] sendMsg = payload.Serialize();
                        TargetAdapter.SendMessage(sendMsg, 0, sendMsg.Length);

                        int iSec = 3 * 100; //3초
                        while (iSec > 0 && !bResponseSuccess)
                        {
                            Thread.Sleep(10); //0.01초씩..쉰다...
                            iSec -= 1;
                        }

                        if (bResponseSuccess == false)
                        {
                            responseJson = new JObject();
                            responseJson["command"] = "visit_reg2";
                            JObject jData = new JObject();
                            jData["reg_no"] = "";
                            responseJson["data"] = jData;
                            JObject jResult = new JObject();
                            jResult["status"] = "-100";
                            jResult["message"] = "OK";
                            responseJson["result"] = jResult;
                        }

                        responsePayload.Deserialize(responseJson);

                        byte[] result = responsePayload.Serialize();
                        e.Response.Connection.Type = ConnectionType.Close;
                        e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                        e.Response.Reason = "OK";
                        e.Response.Body.Write(result, 0, result.Length);
                    }
                    else
                    {
                        e.Response.Connection.Type = ConnectionType.Close;
                        e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                        e.Response.Status = System.Net.HttpStatusCode.BadRequest;
                        e.Response.Reason = "Bad Request";
                        e.Response.Body.Write(null, 0, 0);
                    }
                }

                bResponseSuccess = true;
                responseJson = null;
                carNo = "";
            }
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            JObject data = jobj["data"] as JObject;
            string cmd = jobj["command"].ToString();

            Uri uri = null;
            JObject result = null;
            CmdType cmdType = (CmdType)Enum.Parse(typeof(CmdType), cmd);

            try
            {
                //SmtvLotID
                switch (cmdType)
                {
                    case CmdType.alert_incar:
                    case CmdType.alert_outcar:
                        {
                            uri = new Uri(SysConfig.Instance.HW_Domain + REQ_POST_EVENT);
                            //carNo
                            
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);

                            if (payload.data.kind.ToLower() == "n") return;

                            if (payload.command == CmdType.alert_incar && payload.data.kind.ToLower() == "v")
                            {
                                Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage", "<<< ShowTip >>>", LogAdpType.HomeNet);
                                string sTitle = "방문차량입차";
                                string sMsg = $"{payload.data.dong}동 {payload.data.ho}호 {payload.data.car_number}";

                                //MainForm으로 메시지 이벤트 전송
                                if (ShowTip == null) Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage", "<<< ShowTip is null >>>", LogAdpType.HomeNet);
                                ShowTip?.Invoke(1, sTitle, sMsg);
                                Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage", "<<< ShowTip End >>>", LogAdpType.HomeNet);
                            }

                            if (bResponseSuccess == false && carNo == payload.data.car_number)
                            {
                                responseJson = new JObject();
                                responseJson["command"] = "visit_reg2";
                                JObject jData = new JObject();
                                jData["reg_no"] = payload.data.reg_no;
                                responseJson["data"] = jData;

                                if (payload.data.lprID != "-1")
                                {
                                    Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage", "중간 예약", LogAdpType.HomeNet);
                                    //중간 예약
                                    JObject jResult = new JObject();
                                    jResult["status"] = "200";
                                    jResult["message"] = "OK";
                                    responseJson["result"] = jResult;
                                    bResponseSuccess = true;

                                    result = new JObject();
                                    result["parkingLotId"] = SysConfig.Instance.ParkId;

                                    if (cmdType == CmdType.alert_incar)
                                    {
                                        result["type"] = "COME_IN";
                                    }
                                    else
                                    {
                                        result["type"] = "GO_OUT";
                                    }

                                    result["carNo"] = payload.data.car_number;
                                    result["eventDt"] = payload.data.date_time.ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                }
                                else
                                {
                                    Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage", "사전 예약", LogAdpType.HomeNet);
                                    //사전 예약
                                    JObject jResult = new JObject();
                                    jResult["status"] = "000";
                                    jResult["message"] = "OK";
                                    responseJson["result"] = jResult;
                                    bResponseSuccess = true;
                                    //-1 은 보낼 통보가 없다는 뜻...
                                    return;
                                }
                            }
                            else
                            {
                                result = new JObject();
                                result["parkingLotId"] = SysConfig.Instance.ParkId;

                                if (cmdType == CmdType.alert_incar)
                                {
                                    result["type"] = "COME_IN";
                                }
                                else
                                {
                                    result["type"] = "GO_OUT";
                                }

                                result["carNo"] = payload.data.car_number;
                                result["eventDt"] = payload.data.date_time.ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                            }
                        }
                        break;
                    case CmdType.sync_assign:
                        {
                            uri = new Uri(SysConfig.Instance.HW_Domain + REQ_POST_ASSIGN);
                            
                            RequestPayload<AsyncAssignDataListPayload> payload = new RequestPayload<AsyncAssignDataListPayload>();
                            payload.Deserialize(jobj);

                            result = new JObject();
                            result["parkingLotId"] = SysConfig.Instance.ParkId;
                            JArray arr = new JArray();
                            foreach (var asyncAssignData in payload.data.list)
                            {
                                JObject assignmentList = new JObject();
                                assignmentList["parkNo"] = asyncAssignData.park_no;
                                assignmentList["dong"] = asyncAssignData.dong;
                                assignmentList["ho"] = asyncAssignData.ho;
                                assignmentList["type"] = asyncAssignData.type;
                                long initPoint = 0;
                                long.TryParse(asyncAssignData.enable_point, out initPoint);
                                long usedPoint = 0;
                                long.TryParse(asyncAssignData.used_point, out usedPoint);
                                long remaintPoint = initPoint - usedPoint;
                                assignmentList["initMount"] = initPoint;
                                assignmentList["remainMount"] = remaintPoint;
                                assignmentList["dcTime"] = initPoint;
                                assignmentList["dcUsedTime"] = usedPoint;
                                assignmentList["startDate"] = asyncAssignData.acp_date.ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                assignmentList["endDate"] = asyncAssignData.exp_date.ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                assignmentList["delYn"] = "N";

                                arr.Add(assignmentList);
                            }

                            result["assignmentList"] = arr;
                        }
                        break;
                    case CmdType.visit_reg2:
                        return;
                    default:
                        result = null;
                        break;
                }

                if (result != null)
                {
                    Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage | WebClientResponse", $"전송메시지 {result}", LogAdpType.HomeNet);
                    byte[] requestData = result.ToByteArray(SysConfig.Instance.HomeNet_Encoding);
                    string responseData = string.Empty;
                    string responseHeader = string.Empty;

                    if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.PUT, ContentType.Json, requestData, ref responseData, ref responseHeader, dicHeader))
                    {
                        ResponsePayload responsePayload = new ResponsePayload();
                        responsePayload.command = cmdType;
                        byte[] responseBuffer;

                        if (responseData.StartsWith("ERR"))
                        {
                            responsePayload.result = ResultType.server_error;
                            int startIdx = responseData.IndexOfChar(',', 3);
                            var resultJson = responseData.Substring(startIdx);
                            try
                            {
                                JObject jError = JObject.Parse(resultJson);
                                Log.WriteLog(LogType.Error, "SmtvAdapter | SendMessage | WebClientResponse",
                                    $"==Error_응답==\r\n[Result-Code] {Helper.NVL(jError["result-code"])}\r\n[Result-Message] {Helper.NVL(jError["result-message"])}\r\n", LogAdpType.HomeNet);
                            }
                            catch (Exception ex)
                            {
                                Log.WriteLog(LogType.Error, "SmtvAdapter | SendMessage | WebClientResponse",
                                    $"==Error_Exception_응답==\r\n{responseData}\r\n{ex.Message}", LogAdpType.HomeNet);
                            }
                        }
                        else
                        {
                            JObject jHeader = JObject.Parse(responseHeader);
                            string resultCode = Helper.NVL(jHeader["result-code"]);
                            string hexMessage = Helper.NVL(jHeader["result-message"]).Replace("%", ""); //%로 Hex 값을 구분하고 있다.
                            byte[] resultBytes = hexMessage.ConvertHexStringToByte();

                            Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage | WebClientResponse",
                                $"==응답==\r\n[Result-Code] {resultCode}\r\n[Result-Message] {Encoding.UTF8.GetString(resultBytes)}\r\n{responseData}", LogAdpType.HomeNet);
                            
                            responsePayload.result = ResultType.OK;
                        }

                        responseBuffer = responsePayload.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage", $"Exception : {ex.Message}", LogAdpType.HomeNet);

                ResponsePayload responsePayload = new ResponsePayload();
                byte[] responseBuffer;

                responsePayload.command = cmdType;
                responsePayload.result = ResultType.ExceptionERROR;
                responseBuffer = responsePayload.Serialize();

                TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
            }
        }

        public bool StartAdapter()
        {
            try
            {
                MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
                isRun = MyHttpNetwork.Run();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SmtvAdapter | StartAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }
            
            return isRun;
        }

        

        public bool StopAdapter()
        {
            bool bResult = false;

            try
            {
                MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
                bResult = MyHttpNetwork.Down();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SmtvAdapter | StopAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }
            isRun = !bResult;
            return bResult;
        }

        public void TestReceive(byte[] buffer)
        {
            string json = "{\"command\": \"visit_reg2\",\"data\": {\"end_date_time\" : \"20210604203000\"," +
                            "\"ho\" : \"1701\"," +
                            $"\"car_number\" : \"170어8166\"," +
                            "\"dong\" : \"0502\"," +
                            "\"start_date_time\" : \"20210531083000\"}}";
            string json2 = "{\"command\":\"sync_assign\",\"data\":{\"list\":[{\"park_no\":\"1\",\"dong\":\"9999\",\"ho\":\"1010\",\"type\":\"MINUTE\",\"enable_point\":\"10000\",\"used_point\":\"10\",\"acp_date\":\"20210501\",\"exp_date\":\"20210531\"}]}}";
            byte[] test = SysConfig.Instance.Nexpa_Encoding.GetBytes(json);
            SendMessage(test, 0, test.Length);
        }
    }
}
