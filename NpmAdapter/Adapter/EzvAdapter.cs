﻿using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NLog.LayoutRenderers.Wrappers;
using NLog.Targets;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
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

        #endregion

        #region Properties

        private INetwork TcpClientNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning => isRun;

        #endregion

        #region Implements interface

        public void Dispose()
        {

        }

        public bool Initialize()
        {
            try
            {
                ezHeader = new ezHeaderPayload();
                ezHeader.Initialize();

                tcpServerIp = SysConfig.Instance.HT_IP;
                tcpport = SysConfig.Instance.HT_Port;

                Log.WriteLog(LogType.Info, $"EzvAdapter | Initialize", $"TpcNetwork IP :{tcpServerIp}, Port :{tcpport}", LogAdpType.HomeNet);

                TcpClientNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, tcpServerIp, tcpport);

                //Alive Check
                if (SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckUse").Equals("Y"))
                {
                    aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
                    aliveCheckThread.Name = "Ezv thread for alive check";
                    if (!TimeSpan.TryParse(SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckTime"), out waitForWork))
                    {
                        //Default 3분
                        waitForWork = TimeSpan.FromMinutes(3);
                    }
                }
                //Alive Check

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
                if (SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckUse").Equals("Y"))
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

                TcpClientNetwork.ReceiveFromPeer += TcpClientNetwork_ReceiveFromPeer;
                isRun = TcpClientNetwork.Run();
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
            AliveCheck();
        }

        /// <summary>
        /// 30분 단위로 AliveCheck 를 날린다.
        /// </summary>
        private void AliveCheck()
        {
            do
            {
                if (shutdownEvent.IsSet || !isRun) return;

                {
                    //Alive Check 서버로 전달....
                    Log.WriteLog(LogType.Info, $"EzvAdapter | AliveCheck", $"Alive Check~!");

                    try
                    {
                        //<start=0072&0>$version=3.0$cmd=10$copy=1-10$dongho=100&900$target=server
                        string message = GetResponseMessage("$version=3.0$cmd=10$copy=1-10$dongho=100&900$target=server");
                        Log.WriteLog(LogType.Info, $"EzvAdapter | AliveCheck", $"전송 : {message}", LogAdpType.HomeNet);
                        byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(message);
                        TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
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

                string receiveMsg = SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]);
                ezHeader.Initialize();
                ezHeader.BindData(receiveMsg);

                if(ezHeader.cmd == EZV_HEAD_CMD.조회요청)
                {
                    if (ezHeader.target == "gateway") //AliveCheck 요청
                    {
                        //살아있다고 리턴하자....
                        ResponseEzAliveCheckPayload responsePayload = new ResponseEzAliveCheckPayload();
                        responsePayload.dong = "100";
                        responsePayload.ho = "900";
                        responsePayload.ip = Helper.GetLocalIP();
                        responsePayload.status = "0";

                        string responseMsg = ezHeader.ResponseToString() + responsePayload.ToString();
                        byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
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
                        data.car_number = "";

                        sendPayload.data = data;

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
                            Log.WriteLog(LogType.Info, $"EzvAdapter | TcpClientNetwork_ReceiveFromPeer", $"응답실패", LogAdpType.HomeNet);
                        }
                    }
                    else if(ezHeader.target == "server") //미들웨어가 보낸 Alive Check 에 대한 응답..
                    {
                        Log.WriteLog(LogType.Info, $"EzvAdapter | AliveCheck", $"수신 : {receiveMsg}", LogAdpType.HomeNet);
                    }
                }
                else if(ezHeader.cmd == EZV_HEAD_CMD.제어요청)
                {
                    //주차 예약 요청(mode = 1), 주차 예약 리스트 삭제(mode = 2)
                    var mode = ezHeader.GetMode(receiveMsg);
                    if(mode == EZV_VISIT_MODE.입차예약)
                    {
                        RequestEzVisitRegPayload requestPayload = new RequestEzVisitRegPayload();
                        requestPayload.BindData(receiveMsg);

                        RequestPayload<RequestVisitReg2Payload> sendPayload = new RequestPayload<RequestVisitReg2Payload>();
                        sendPayload.command = CmdType.visit_reg2;

                        RequestVisitReg2Payload data = new RequestVisitReg2Payload();
                        data.car_number = requestPayload.carno;
                        data.dong = requestPayload.dong;
                        data.ho = requestPayload.ho;
                        if(requestPayload.inout == "0")
                        {
                            data.start_date_time = requestPayload.time;
                        }
                        else
                        {
                            data.end_date_tiem = requestPayload.time;
                        }

                        sendPayload.data = data;

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

                        }
                    }
                    else if(mode == EZV_VISIT_MODE.입차예약취소)
                    {
                        RequestEzVisitDelPayload requestPayload = new RequestEzVisitDelPayload();
                        requestPayload.BindData(receiveMsg);

                        RequestPayload<RequestVisitDel2Payload> sendPayload = new RequestPayload<RequestVisitDel2Payload>();
                        sendPayload.command = CmdType.visit_reg2;

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
            }
            //결과 Payload 생성완료 =======

            if (data == null || data.Count == 0) return;

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
                            byte[] results = responsePayload.Serialize();
                            TargetAdapter.SendMessage(results, 0, results.Length);
                            Log.WriteLog(LogType.Info, $"EzvAdapter | SendMessage", $"전송메시지 : {responsePayload.ToJson().ToString()}", LogAdpType.Nexpa);
                        }
                        else
                        {
                            ezHeaderPayload headerPayload = new ezHeaderPayload();
                            headerPayload.version = "3.0";
                            headerPayload.cmd = EZV_HEAD_CMD.이벤트전송;
                            headerPayload.copy = "1-10";
                            headerPayload.dongho = "100&900";
                            headerPayload.target = "parking";

                            RequestEzAlertIOCarPayload alertPayload = new RequestEzAlertIOCarPayload();
                            alertPayload.dong = payload.data.dong;
                            alertPayload.ho = payload.data.ho;
                            alertPayload.carno = payload.data.car_number;

                            if (payload.data.kind == "a") //아파트 주민
                            {
                                alertPayload.mode = EZV_VISIT_MODE.일반입차통보;
                            }
                            else if (payload.data.kind == "v")
                            {
                                alertPayload.mode = EZV_VISIT_MODE.입차예약;
                            }

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

                                foreach (JObject item in list)
                                {
                                    if (visitlst.list == null) visitlst.list = new List<ResponseEzVisitListPayload.CarInfo>();
                                    if (visitlst.dongho == null || visitlst.dongho == "")
                                    {
                                        visitlst.dongho = $"{Helper.NVL(item["dong"])}&{Helper.NVL(item["ho"])}";
                                    }
                                    // ezHeader
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
                            byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                            //<start=0000&0> 을 붙여서 보내야 함.
                            TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                        }
                    }
                    bResponseSuccess = true;
                    break;
                case CmdType.visit_reg2:
                    {
                        string sData = "#mode=1";
                        if (resultPayload != null)
                        {
                            sData += $"#err={resultPayload.message}";
                        }

                        string responseMsg = GetResponseMessage(ezHeader.ResponseToString() + sData);
                        byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                        //<start=0000&0> 을 붙여서 보내야 함.
                        TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                    }
                    bResponseSuccess = true;
                    break;
                case CmdType.visit_del2:
                    {
                        string sData = "#mode=2";
                        if (resultPayload != null)
                        {
                            sData += $"#err={resultPayload.message}";
                        }

                        string responseMsg = GetResponseMessage(ezHeader.ResponseToString() + sData);
                        byte[] responseData = SysConfig.Instance.HomeNet_Encoding.GetBytes(responseMsg);
                        //<start=0000&0> 을 붙여서 보내야 함.
                        TcpClientNetwork.SendToPeer(responseData, 0, responseData.Length);
                    }
                    bResponseSuccess = true;
                    break;
            }
        }

        #endregion

        private string GetResponseMessage(string msg)
        {
            //int msgLength = SysConfig.Instance.HomeNet_Encoding.GetBytes(msg).Length;
            //string sMsgLength = msgLength.ToString();
            //int totalLength = (sMsgLength.Length + 10) + msgLength;
            //string sGap = totalLength.ToString();
            //int gap = sGap.Length - sMsgLength.Length;
            //totalLength += gap;

            int msgLength = SysConfig.Instance.HomeNet_Encoding.GetBytes(msg).Length;
            int totalLength = msgLength + 14;

            //0000 형태로 만들기
            string fmt = "0000.##";
            string sResultLen = totalLength.ToString(fmt);
            
            return $"<start={sResultLen}&0>{msg}";
        }
    }
}