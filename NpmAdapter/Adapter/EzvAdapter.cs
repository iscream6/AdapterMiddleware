using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NLog.LayoutRenderers.Wrappers;
using NLog.Targets;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class EzvAdapter : IAdapter
    {
        #region Fields

        private bool isRun = false;
        private bool bResponseSuccess = false;
        private string tcpServerIp = "0.0.0.0";
        private string tcpport = "29712";
        private object lockObj = new object();

        private ezHeaderPayload ezHeader;
        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private StringBuilder responseData = new StringBuilder();
        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private delegate void SafeCallDelegate();

        private const string ERR_SAVE_CARNUMBER = "#0001&차번저장오류";
        private const string ERR_FAIL_RESERVE = "#0002&예약실패";
        private const string ERR_FAIL_RESERVE_CANCEL = "#0003&예약취소실패";
        private StringBuilder ErrorMessage = new StringBuilder();
        private string AuthDong = "100";
        private string AuthHo = "900";

        private static Dictionary<CmdType, JObject> dicBuffer = new Dictionary<CmdType, JObject>();
        public event IAdapter.ShowBallonTip ShowTip;

        #endregion

        #region Properties

        private INetwork TcpClientNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning => isRun;

        #endregion

        #region Implements interface

        public void Dispose()
        {
            _pauseEvent.Set();
            shutdownEvent.Set();
            StopAdapter();
        }

        public bool Initialize()
        {
            try
            {
                ezHeader = new ezHeaderPayload();
                ezHeader.Initialize();

                AuthDong = SysConfig.Instance.HC_Id;
                AuthHo = SysConfig.Instance.HC_Pw;
                tcpServerIp = SysConfig.Instance.HT_IP;
                tcpport = SysConfig.Instance.HT_Port;

                Log.WriteLog(LogType.Info, $"EzvAdapter | Initialize", $"TpcNetwork IP :{tcpServerIp}, Port :{tcpport}", LogAdpType.HomeNet);

                TcpClientNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, tcpServerIp, tcpport);

                //이지빌은 Default AliveCheck함.
                aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
                aliveCheckThread.Name = "Ezv thread for alive check";
                waitForWork = TimeSpan.FromMinutes(5);

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "EzvAdapter | Initialize", ex.Message);
                return false;
            }
        }

        public bool StartAdapter()
        {
            try
            {
                isRun = TcpClientNetwork.Run();
                TcpClientNetwork.ReceiveFromPeer += TcpClientNetwork_ReceiveFromPeer;
                
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
                Log.WriteLog(LogType.Error, "EzvAdapter | StartAdapter", ex.Message);
                return false;
            }
        }

        public bool StopAdapter()
        {
            try
            {
                //Alive Check Thread pause
                _pauseEvent.Reset();

                TcpClientNetwork.ReceiveFromPeer -= TcpClientNetwork_ReceiveFromPeer;
                isRun = !TcpClientNetwork.Down();
                return !isRun;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "EzvAdapter | StopAdapter", ex.Message);
                return false;
            }
        }

        public void TestReceive(byte[] buffer)
        {
            string json = "{\"command\": \"alert_incar\",\"data\": {\"dong\" : \"101\"," +
                            "\"ho\" : \"501\"," +
                            $"\"car_number\" : \"46부5989\"," +
                            "\"date_time\" : \"20210312102525\"," +
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

        /// <summary>
        /// 30분 단위로 AliveCheck 를 날린다.
        /// </summary>
        private void AliveCheck()
        {
            do
            {
                if (shutdownEvent.IsSet) return;

                {
                    //Alive Check 서버로 전달....
                    Log.WriteLog(LogType.Info, $"EzvAdapter | AliveCheck", $"Alive Check~!");

                    try
                    {
                        if(TcpClientNetwork.Status == NetStatus.Disconnected)
                        {
                            isRun = TcpClientNetwork.Run();
                        }
                        else
                        {
                            //<start=0072&0>$version=3.0$cmd=10$copy=1-10$dongho=100&900$target=server
                            string message = GetResponseMessage($"$version=3.0$copy=0-0$dongho={AuthDong}&{AuthHo}$cmd=10$target=server");
                            Log.WriteLog(LogType.Info, $"EzvAdapter | AliveCheck", $"전송 : {message}", LogAdpType.HomeNet);
                            byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(message);
                            TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogType.Error, $"EzvAdapter | AliveCheck", $"{ex.Message}");
                    }
                }

                shutdownEvent.Wait(waitForWork);
            }
            while (_pauseEvent.WaitOne());
        }

        

        private void TcpClientNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null)
        {
            lock (lockObj)
            {
                bResponseSuccess = false;
                ErrorMessage.Clear();

                string receiveMsg = SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]);

                Log.WriteLog(LogType.Info, "EzvAdapter | From 자이S&D", $"받은메시지(원문) : {receiveMsg}", LogAdpType.HomeNet);
                //<start=0399&0>
                var dataLeng = int.Parse(receiveMsg.Substring(7, 4));
                receiveMsg = receiveMsg.GetSubStringByteLength(0, dataLeng);

                Log.WriteLog(LogType.Info, "EzvAdapter | From 자이S&D", $"받은메시지(재처리) : {receiveMsg}", LogAdpType.HomeNet);

                ezHeader.Initialize();
                ezHeader.BindData(receiveMsg);

                if (ezHeader.cmd == EZV_HEAD_CMD.조회요청)
                {
                    if (ezHeader.target == "gateway") //AliveCheck 요청
                    {
                        //살아있다고 리턴하자....
                        ResponseEzAliveCheckPayload responsePayload = new ResponseEzAliveCheckPayload();
                        responsePayload.dong = AuthDong;
                        responsePayload.ho = AuthHo;
                        List<string> ips = Helper.GetLocalIPs();
                        if(ips.Count > 0)
                        {
                            foreach (var ip in ips)
                            {
                                //마포프레스티지 자이현장
                                if(SysConfig.Instance.ParkName == "mapoprestge")
                                {
                                    if (ip.StartsWith("10."))
                                    {
                                        responsePayload.ip = ip;
                                        break;
                                    }
                                }
                                else
                                {
                                    responsePayload.ip = ip;
                                    break;
                                }
                            }
                        }
                        //responsePayload.ip = Helper.GetLocalIP();
                        responsePayload.status = "0";

                        string responseMsg = GetResponseMessage(ezHeader.ResponseToString() + responsePayload.ToString());
                        byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                        Log.WriteLog(LogType.Info, $"EzvAdapter | To   자이S&D", $"전송메시지 : {responseMsg}", LogAdpType.HomeNet);
                        TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                    }
                    else if (ezHeader.target == "parking") //주차예약 조회
                    {
                        RequestEzVisitListPayload requestPayload = new RequestEzVisitListPayload();
                        requestPayload.BindData(receiveMsg);

                        //Nexpa로 전달하자..
                        RequestPayload<RequestVisitList2Payload> sendPayload = new RequestPayload<RequestVisitList2Payload>();
                        sendPayload.command = CmdType.visit_list2;

                        RequestVisitList2Payload data = new RequestVisitList2Payload();
                        data.dong = requestPayload.dong;
                        data.ho = requestPayload.ho;
                        data.event_date_time = DateTime.Now.ToString("yyyyMMdd") + "000000";
                        data.car_number = "";

                        sendPayload.data = data;

                        if (dicBuffer.ContainsKey(sendPayload.command))
                        {
                            dicBuffer[sendPayload.command] = data.ToJson();
                        }
                        else
                        {
                            dicBuffer.Add(sendPayload.command, data.ToJson());
                        }

                        byte[] responseBuffer = sendPayload.Serialize();
                        Log.WriteLog(LogType.Info, $"EzvAdapter | To   자이S&D", $"전송메시지 : {sendPayload.ToJson()}", LogAdpType.HomeNet);
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);

                        int iSec = 3 * 100; //3초
                        while (iSec > 0 && !bResponseSuccess)
                        {
                            Thread.Sleep(10); //0.01초씩..쉰다...
                            iSec -= 1;
                        }

                        if (!bResponseSuccess) //응답 실패
                        {
                            Log.WriteLog(LogType.Error, $"EzvAdapter | TcpClientNetwork_ReceiveFromPeer", $"응답시간초과", LogAdpType.HomeNet);
                            string responseMsg = "";
                            if (ErrorMessage.Length > 0)
                            {
                                responseMsg = GetResponseMessage(ezHeader.ResponseToString() + ErrorMessage.ToString());
                                ErrorMessage.Clear();

                            }
                            else
                            {
                                responseMsg = GetResponseMessage(ezHeader.ResponseToString() + "#0004&주차관제응답없음");
                            }

                            Log.WriteLog(LogType.Info, $"EzvAdapter | To   자이S&D", $"전송메시지 : {responseMsg}", LogAdpType.HomeNet);
                            byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                            TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                        }
                    }
                    else if(ezHeader.target == "server") //미들웨어가 보낸 Alive Check 에 대한 응답..
                    {
                        Log.WriteLog(LogType.Info, "EzvAdapter | From 자이S&D", $"받은메시지 : {receiveMsg}", LogAdpType.HomeNet);
                    }
                }
                else if(ezHeader.cmd == EZV_HEAD_CMD.제어요청)
                {
                    //주차 예약 요청(mode = 1), 주차 예약 리스트 삭제(mode = 2)
                    var mode = ezHeader.GetMode(receiveMsg);
                    if(mode == EZV_VISIT_MODE.입차예약)
                    {
                        ErrorMessage.Append(ERR_FAIL_RESERVE);

                        RequestEzVisitRegPayload requestPayload = new RequestEzVisitRegPayload();
                        requestPayload.BindData(receiveMsg);

                        RequestPayload<RequestVisitReg2Payload> sendPayload = new RequestPayload<RequestVisitReg2Payload>();
                        sendPayload.command = CmdType.visit_reg2;

                        RequestVisitReg2Payload data = new RequestVisitReg2Payload();
                        data.car_number = requestPayload.carno;
                        data.dong = requestPayload.dong;
                        data.ho = requestPayload.ho;
                        DateTime date = DateTime.ParseExact(requestPayload.time, "yyyyMMddHHmmss", null); //20210312120000
                        if (requestPayload.inout == "0")
                        {
                            data.start_date_time = requestPayload.time;
                            data.end_date_time = date.ToString("yyyyMMdd") + "235959";
                        }
                        else
                        {
                            data.end_date_time = requestPayload.time;
                            data.start_date_time = date.ToString("yyyyMMdd") + "000000";
                        }

                        sendPayload.data = data;

                        if (dicBuffer.ContainsKey(sendPayload.command))
                        {
                            dicBuffer[sendPayload.command] = data.ToJson();
                        }
                        else
                        {
                            dicBuffer.Add(sendPayload.command, data.ToJson());
                        }

                        Log.WriteLog(LogType.Info, $"EzvAdapter | To Nexpa", $"전송메시지 : {sendPayload.ToJson()}", LogAdpType.HomeNet);
                        byte[] responseBuffer = sendPayload.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);

                        int iSec = 3 * 100; //3초
                        while (iSec > 0 && !bResponseSuccess)
                        {
                            Thread.Sleep(10); //0.01초씩..쉰다...
                            iSec -= 1;
                        }

                        if (!bResponseSuccess) //응답 실패
                        {
                            Log.WriteLog(LogType.Error, $"EzvAdapter | TcpClientNetwork_ReceiveFromPeer", $"응답시간초과", LogAdpType.HomeNet);
                            string responseMsg = "";
                            if (ErrorMessage.Length > 0)
                            {
                                responseMsg = GetResponseMessage(ezHeader.ResponseToString() + ErrorMessage.ToString());
                                ErrorMessage.Clear();

                            }
                            else
                            {
                                responseMsg = GetResponseMessage(ezHeader.ResponseToString() + "#0004&주차관제응답없음");
                            }

                            Log.WriteLog(LogType.Info, $"EzvAdapter | To   자이S&D", $"전송메시지 : {responseMsg}", LogAdpType.HomeNet);
                            byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                            TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                        }
                    }
                    else if(mode == EZV_VISIT_MODE.입차예약취소)
                    {
                        ErrorMessage.Append(ERR_FAIL_RESERVE_CANCEL);
                        RequestEzVisitDelPayload requestPayload = new RequestEzVisitDelPayload();
                        requestPayload.BindData(receiveMsg);

                        RequestPayload<RequestVisitDel2Payload> sendPayload = new RequestPayload<RequestVisitDel2Payload>();
                        sendPayload.command = CmdType.visit_del2;

                        RequestVisitDel2Payload data = new RequestVisitDel2Payload();
                        data.dong = requestPayload.dong;
                        data.ho = requestPayload.ho;
                        if (requestPayload.list != null && requestPayload.list.Count > 0)
                        {
                            StringBuilder carnums = new StringBuilder();
                            for (int i = 0; i < requestPayload.list.Count; i++)
                            {
                                carnums.Append(requestPayload.list[i].carno);
                                if (i < (requestPayload.list.Count - 1))
                                {
                                    carnums.Append("&");
                                }
                            }

                            data.car_number = carnums.ToString();
                        }

                        sendPayload.data = data;

                        if (dicBuffer.ContainsKey(sendPayload.command))
                        {
                            dicBuffer[sendPayload.command] = data.ToJson();
                        }
                        else
                        {
                            dicBuffer.Add(sendPayload.command, data.ToJson());
                        }

                        Log.WriteLog(LogType.Info, $"EzvAdapter | To Nexpa", $"전송한메시지 : {sendPayload.ToJson()}", LogAdpType.HomeNet);
                        byte[] responseBuffer = sendPayload.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);

                        int iSec = 3 * 100; //3초
                        while (iSec > 0 && !bResponseSuccess)
                        {
                            Thread.Sleep(10); //0.01초씩..쉰다...
                            iSec -= 1;
                        }

                        if (!bResponseSuccess) //응답 실패
                        {
                            Log.WriteLog(LogType.Error, $"EzvAdapter | TcpClientNetwork_ReceiveFromPeer", $"응답시간초과", LogAdpType.HomeNet);
                            string responseMsg = "";
                            if (ErrorMessage.Length > 0)
                            {
                                responseMsg = GetResponseMessage(ezHeader.ResponseToString() + ErrorMessage.ToString());
                                ErrorMessage.Clear();

                            }
                            else
                            {
                                responseMsg = GetResponseMessage(ezHeader.ResponseToString() + "#0004&주차관제응답없음");
                            }

                            Log.WriteLog(LogType.Info, $"EzvAdapter | To   자이S&D", $"전송메시지 : {responseMsg}", LogAdpType.HomeNet);
                            byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                            TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                            bResponseSuccess = true;
                        }
                    }
                }
            }
        }
        
        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            receiveMessageBuffer.Append(buffer[..(int)size].ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(receiveMessageBuffer.ToString());
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"EzvAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
            JObject data = jobj["data"] as JObject;

            //결과 Payload 생성 =======
            ResultPayload resultPayload = null;
            JObject result = jobj["result"] as JObject; //응답 결과

            if (result != null && Helper.NVL(result["status"]) != "200")
            {
                resultPayload = new ResultPayload();
                string sCode = "";

                if (Helper.NVL(result["status"]) == "204") sCode = "404";
                else sCode = Helper.NVL(result["status"]);

                resultPayload.code = sCode;
                resultPayload.message = Helper.NVL(result["message"]);

                string responseMsg = "";
                if (ErrorMessage.Length > 0)
                {
                    responseMsg = GetResponseMessage(ezHeader.ResponseToString() + ErrorMessage.ToString());
                    ErrorMessage.Clear();
                }
                else
                {
                    responseMsg = GetResponseMessage(ezHeader.ResponseToString() + $"#0{resultPayload.code}&{resultPayload.message}");
                }

                Log.WriteLog(LogType.Info, $"EzvAdapter | SendMessage", $"SendToPeer : {responseMsg}", LogAdpType.HomeNet);
                byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                bResponseSuccess = true;
                return;
            }
            //결과 Payload 생성완료 =======

            string cmd = jobj["command"].ToString();
            switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
            {
                //입/출차 통보
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    {
                        //Json값을 파싱하자.
                        RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                        payload.Deserialize(jobj);

                        //동호가 없으면 PASS시킨다.
                        if (payload.data == null || payload.data.dong == null || payload.data.ho == null || payload.data.dong == "" || payload.data.ho == "")
                        {
                            ResponsePayload responsePayload = new ResponsePayload();
                            responsePayload.command = payload.command;
                            responsePayload.result = ResultType.FailFormatError;

                            Log.WriteLog(LogType.Info, $"EzvAdapter | SendMessage", $"전송메시지 : {responsePayload.ToJson().ToString()}", LogAdpType.Nexpa);
                            byte[] results = responsePayload.Serialize();
                            TargetAdapter.SendMessage(results, 0, results.Length, pid);
                        }
                        else
                        {
                            ezHeaderPayload headerPayload = new ezHeaderPayload();
                            headerPayload.version = "3.0";
                            headerPayload.cmd = EZV_HEAD_CMD.이벤트전송;
                            headerPayload.copy = "1-10";
                            headerPayload.dongho = $"{AuthDong}&{AuthHo}";
                            headerPayload.target = "parking";

                            RequestEzAlertIOCarPayload alertPayload = new RequestEzAlertIOCarPayload();
                            alertPayload.dong = payload.data.dong;
                            alertPayload.ho = payload.data.ho;
                            alertPayload.carno = payload.data.car_number;
                            alertPayload.mode = EZV_VISIT_MODE.일반입차통보;

                            if ((CmdType)Enum.Parse(typeof(CmdType), cmd) == CmdType.alert_incar)
                            {
                                alertPayload.inout = "0";
                            }
                            else
                            {
                                alertPayload.inout = "1";
                            }

                            alertPayload.time = payload.data.date_time;
                            string responseMsg = GetResponseMessage(headerPayload.ToString() + alertPayload.ToString());

                            Log.WriteLog(LogType.Info, $"EzvAdapter | SendMessage", $"SendToPeer : {responseMsg}", LogAdpType.HomeNet);
                            byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                            TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                        }
                    }
                    break;
                case CmdType.visit_list2:
                    {
                        if (data != null && data.HasValues)
                        {
                            ResponseEzVisitListPayload visitlst = new ResponseEzVisitListPayload();
                            visitlst.mode = EZV_VISIT_MODE.입차예약;

                            JArray list = data["list"] as JArray;
                            if (list != null)
                            {
                                int iNo = 1;

                                visitlst.total = list.Count.ToString();
                                JObject sendedJson = dicBuffer.TryGetValue(CmdType.visit_list2);
                                dicBuffer.Remove(CmdType.visit_list2);

                                if (sendedJson != null && sendedJson.Count > 0)
                                {
                                    visitlst.dongho = $"{Helper.NVL(sendedJson["dong"])}&{Helper.NVL(sendedJson["ho"])}";
                                }

                                foreach (JObject item in list)
                                {
                                    if (visitlst.list == null) visitlst.list = new List<ResponseEzVisitListPayload.CarInfo>();
                                    
                                    ResponseEzVisitListPayload.CarInfo carInfo = new ResponseEzVisitListPayload.CarInfo();

                                    carInfo.no = iNo++.ToString();
                                    carInfo.carno = Helper.NVL(item["car_number"]);
                                    if (Helper.NVL(item["end_date_time"]) == "")
                                    {
                                        carInfo.inout = "0";
                                        carInfo.time = Helper.NVL(item["start_date_time"]);
                                    }
                                    else if (Helper.NVL(item["start_date_time"]) == "")
                                    {
                                        carInfo.inout = "1";
                                        carInfo.time = Helper.NVL(item["end_date_time"]);
                                    }
                                    else
                                    {
                                        carInfo.inout = "0";
                                        carInfo.time = Helper.NVL(item["start_date_time"]);
                                    }
                                    visitlst.list.Add(carInfo);
                                }
                            }
                            
                            //여기서 보낸다. 즉 data가 없다면 안보낸다는 뜻....
                            string responseMsg = GetResponseMessage(ezHeader.ResponseToString() + visitlst.ToString());

                            Log.WriteLog(LogType.Info, $"EzvAdapter | SendMessage", $"SendToPeer : {responseMsg}", LogAdpType.HomeNet);
                            byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                            TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                        }
                    }
                    bResponseSuccess = true;
                    break;
                case CmdType.visit_reg2:
                    {
                        string sData = "#mode=1";
                        JObject sendedJson = dicBuffer.TryGetValue(CmdType.visit_reg2);
                        dicBuffer.Remove(CmdType.visit_reg2);
                        if (sendedJson != null && sendedJson.Count > 0)
                        {
                            sData += $"#dongho={Helper.NVL(sendedJson["dong"])}&{Helper.NVL(sendedJson["ho"])}";
                        }

                        if (resultPayload != null)
                        {
                            sData += $"#err={resultPayload.message}";
                        }

                        string responseMsg = GetResponseMessage(ezHeader.ResponseToString() + sData);

                        Log.WriteLog(LogType.Info, $"EzvAdapter | SendMessage", $"SendToPeer : {responseMsg}", LogAdpType.HomeNet);
                        byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                        TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                    }
                    bResponseSuccess = true;
                    break;
                case CmdType.visit_del2:
                    {
                        string sData = "#mode=2";
                        JObject sendedJson = dicBuffer.TryGetValue(CmdType.visit_del2);
                        dicBuffer.Remove(CmdType.visit_del2);
                        if (sendedJson != null && sendedJson.Count > 0)
                        {
                            sData += $"#dongho={Helper.NVL(sendedJson["dong"])}&{Helper.NVL(sendedJson["ho"])}";
                        }

                        if (resultPayload != null)
                        {
                            sData += $"#err={resultPayload.message}";
                        }

                        string responseMsg = GetResponseMessage(ezHeader.ResponseToString() + sData);

                        Log.WriteLog(LogType.Info, $"EzvAdapter | SendMessage", $"SendToPeer : {responseMsg}", LogAdpType.HomeNet);
                        byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                        TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                    }
                    bResponseSuccess = true;
                    break;
            }
        }

        #endregion

        private string GetResponseMessage(string msg)
        {
            int msgLength = msg.GetByteLength();
            int totalLength = msgLength + 14;
            string fmt = "0000.##"; //0000 형태로 만들기
            string sResultLen = totalLength.ToString(fmt);
            return $"<start={sResultLen}&0>{msg}";
        }
    }
}
