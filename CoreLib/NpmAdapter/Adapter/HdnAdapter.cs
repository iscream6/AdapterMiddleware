﻿using Newtonsoft.Json.Linq;
using NpmAdapter.Payload;
using NpmCommon;
using NpmNetwork;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 현대통신
    /// </summary>
    class HdnAdapter : IAdapter
    {
        private delegate void SafeCallDelegate();
        
        private string tcpServerIp = "172.20.200.200";
        private string tcpPort = "29500";
        private string myport = "29500";

        /// <summary>
        /// 넥스파 요청에 대한 응답이 왔는지 여부
        /// </summary>
        private bool bResponseSuccess = false;
        private bool isRun = false;

        private object lockObj = new object();
        public event IAdapter.ShowBallonTip ShowTip;

        public IAdapter TargetAdapter { get; set; }

        private INetwork MyTcpClientNetwork { get; set; }
        private INetwork MyTcpServer { get; set; }

        public bool IsRuning { get => isRun; }
        public string reqPid { get; set; }

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            //Config Version Check~!
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "CcmAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.Nexpa);
                return false;
            }

            tcpServerIp = SysConfig.Instance.HT_IP;
            tcpPort = SysConfig.Instance.HT_Port;
            myport = SysConfig.Instance.HT_MyPort;

            MyTcpClientNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, tcpServerIp, tcpPort);
            MyTcpServer = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpServer, myport);

            return true;
        }

        public bool StartAdapter()
        {
            try
            {
                MyTcpClientNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
                MyTcpServer.ReceiveFromPeer += MyTcpServer_ReceiveFromPeer;
                isRun = MyTcpServer.Run();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "CcmAdapter | StartAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }

            return isRun;
        }

        public bool StopAdapter()
        {
            try
            {
                MyTcpClientNetwork.ReceiveFromPeer -= MyTcpNetwork_ReceiveFromPeer;
                isRun = !MyTcpClientNetwork.Down();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "CcmAdapter | StopAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }

            return true;
        }

        public void TestReceive(byte[] buffer)
        {

        }
        
        /// <summary>
        /// From Nexpa Adapter
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="pid"></param>
        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            bResponseSuccess = false;

            var jobj = JObject.Parse(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));

            Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);

            JObject data = jobj["data"] as JObject;
            string cmd = jobj["command"].ToString();

            switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
            {
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    if (MyTcpClientNetwork.Run())
                    {
                        RequestPayload<HdnAlertInOutCarPayload> payload = new RequestPayload<HdnAlertInOutCarPayload>();
                        payload.Deserialize(jobj);

                        if (payload.data.dong == null || payload.data.ho == null || payload.data.dong == "" || payload.data.ho == "")
                        {
                            //PASS
                        }
                        else
                        {
                            string visitValue = "";
                            if (Helper.NVL(data["kind"]) == "v") visitValue = "VISIT_";

                            //IN/OUT 설정
                            if (payload.command == CmdType.alert_incar)
                            {
                                payload.data.type = $"{visitValue}IN";
                            }
                            else if (payload.command == CmdType.alert_outcar)
                            {
                                payload.data.type = $"{visitValue}OUT";
                            }
                            byte[] networkMessage = payload.data.Serialize();
                            MyTcpClientNetwork.SendToPeer(networkMessage, 0, networkMessage.Length);
                        }
                    }
                    break;
            }

            int iSec = 100; //1초
            while (iSec > 0 && !bResponseSuccess)
            {
                Thread.Sleep(10); //0.01초씩..쉰다...
                iSec -= 1;
            }

            if(bResponseSuccess == false) MyTcpClientNetwork.Down();
        }

        /// <summary>
        /// 주차서버 -> 단지서버
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="pEvent"></param>
        /// <param name="id"></param>
        /// <param name="ep"></param>
        private void MyTcpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null, System.Net.EndPoint ep = null)
        {
            //응답 처리...
            bResponseSuccess = true;
        }

        /// <summary>
        /// 단지서버 -> 주차서버
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="pEvent"></param>
        /// <param name="id"></param>
        /// <param name="ep"></param>
        private void MyTcpServer_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null, System.Net.EndPoint ep = null)
        {
            //세대 방문자 리스트(단지서버 -> 주차서버)
            //세대 방문자 등록(단지서버 -> 주차서버)
            //세대 방문자 삭제(단지서버 -> 주차서버)
            //세대 방문자 전체 삭제(단지서버 -> 주차서버)
            //세대 방문자 포인트(시간) 조회(단지서버 -> 주차서버)

            lock (lockObj)
            {
                byte[] bLength = buffer[..8];
                byte[] bData = buffer[8..];

                UInt64 length = BitConverter.ToUInt64(bLength, 0);//Header
                string sData = bData.ToString(SysConfig.Instance.HomeNet_Encoding, (size - 8));
                var dicData = sData.DoubleSplit('&', '=');

                SendToNexpa(dicData);
            }
        }

        private void SendToNexpa(Dictionary<string, string> pData)
        {
            IPayload responsePayload = null;
            switch (pData["INOUT"])
            {
                case "VISIT_IN":
                case "IN":
                    {
                        ResponsePayload payload = new ResponsePayload();
                        payload.command = CmdType.alert_incar;
                        if (pData["RETURN"] == "ok") payload.result = ResultType.OK;
                        else payload.result = ResultType.FailInterface;
                        responsePayload = payload;
                    }
                    break;
                case "VISIT_OUT":
                case "OUT":
                    {
                        ResponsePayload payload = new ResponsePayload();
                        payload.command = CmdType.alert_outcar;
                        if (pData["RETURN"] == "ok") payload.result = ResultType.OK;
                        else payload.result = ResultType.FailInterface;
                        responsePayload = payload;
                    }
                    break;
                case "VISIT": //방문자 등록
                    {
                        RequestPayload<RequestVisitRegPayload> payload = new RequestPayload<RequestVisitRegPayload>();
                        payload.command = CmdType.visit_reg;

                        RequestVisitRegPayload data = new RequestVisitRegPayload();
                        data.dong = pData["DONG"];
                        data.ho = pData["HO"];
                        data.car_number = pData["CARNO"];
                        data.date = pData["DATETIME"].Substring(0, 8); //yyyyMMdd
                        data.term = pData["CARNO"];
                        payload.data = data;
                        responsePayload = payload;
                    }
                    break;
                case "VISIT_LIST":
                    {
                        RequestPayload<RequestVisitList2Payload> payload = new RequestPayload<RequestVisitList2Payload>();
                        RequestVisitList2Payload data = new RequestVisitList2Payload();
                        data.eventType = RequestVisitList2Payload.EventType.F;
                        data.event_date_time = DateTime.Now.ToString("yyyyMMddHHmmss");
                        data.car_number = pData["CARNO"];
                        data.dong = pData["DONG"];
                        data.ho = pData["HO"];
                        payload.data = data;
                        responsePayload = payload;
                    }
                    break;
                case "VISIT_LIST_HISTORY":
                    {
                        RequestPayload<RequestVisitList2Payload> payload = new RequestPayload<RequestVisitList2Payload>();
                        RequestVisitList2Payload data = new RequestVisitList2Payload();
                        data.eventType = RequestVisitList2Payload.EventType.H;
                        data.event_date_time = DateTime.Now.ToString("yyyyMMddHHmmss");
                        data.car_number = pData["CARNO"];
                        data.dong = pData["DONG"];
                        data.ho = pData["HO"];
                        payload.data = data;
                        responsePayload = payload;
                    }
                    break;
                case "VISIT_LIST_ALL":
                    {
                        RequestPayload<RequestVisitList2Payload> payload = new RequestPayload<RequestVisitList2Payload>();
                        RequestVisitList2Payload data = new RequestVisitList2Payload();
                        data.eventType = RequestVisitList2Payload.EventType.A;
                        data.event_date_time = DateTime.Now.ToString("yyyyMMddHHmmss");
                        data.car_number = pData["CARNO"];
                        data.dong = pData["DONG"];
                        data.ho = pData["HO"];
                        payload.data = data;
                        responsePayload = payload;
                    }
                    break;
                case "VISIT_POINT":
                    {
                        RequestPayload<RequestCarInfoPayload> payload = new RequestPayload<RequestCarInfoPayload>();
                        payload.command = CmdType.remain_point;

                        RequestCarInfoPayload data = new RequestCarInfoPayload();
                        data.dong = pData["DONG"];
                        data.ho = pData["HO"];
                        payload.data = data; 
                        responsePayload = payload;
                    }
                    break;
            }
            
            if(responsePayload != null)
            {
                byte[] sendBytes = responsePayload.Serialize();
                TargetAdapter.SendMessage(sendBytes, 0, sendBytes.Length);
            }
        }
    }
}
