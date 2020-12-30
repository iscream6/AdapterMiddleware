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
        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(false);

        public IAdapter TargetAdapter { get; set; }

        private INetwork MyTcpNetwork { get; set; }
        private INetwork MyTcpServer { get; set; }

        public bool IsRuning { get => isRun; }

        public void Dispose()
        {
            _pauseEvent.Set();
            shutdownEvent.Set();
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

            MyTcpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, tcpServerIp, tcpPort);
            MyTcpServer = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpServer, myport);
            //Alive Check
            if (SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckUse").Equals("Y"))
            {
                aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
                aliveCheckThread.Name = "Cmx thread for alive check";
                if (!TimeSpan.TryParse(SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckTime"), out waitForWork))
                {
                    //Default 50초
                    waitForWork = TimeSpan.FromSeconds(50);
                }
            }
            //Alive Check

            return true;
        }

        public bool StartAdapter()
        {
            try
            {
                MyTcpNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
                MyTcpServer.ReceiveFromPeer += MyTcpServer_ReceiveFromPeer;
                isRun = MyTcpServer.Run();

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
                MyTcpNetwork.ReceiveFromPeer -= MyTcpNetwork_ReceiveFromPeer;
                isRun = !MyTcpNetwork.Down();

                //Alive Check Thread pause
                _pauseEvent.Reset();
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

        public void SendMessage(IPayload payload)
        {

        }

        public void SendMessage(byte[] buffer, long offset, long size)
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
                    if (MyTcpNetwork.Run())
                    {
                        RequestPayload<HdnAlertInOutCarPayload> payload = new RequestPayload<HdnAlertInOutCarPayload>();
                        payload.Deserialize(jobj);

                        if (payload.data.dong == null || payload.data.ho == null || payload.data.dong == "" || payload.data.ho == "")
                        {
                            //PASS
                        }
                        else
                        {
                            //IN/OUT 설정
                            if (payload.command == CmdType.alert_incar)
                            {
                                payload.data.type = "IN";
                            }
                            else if (payload.command == CmdType.alert_outcar)
                            {
                                payload.data.type = "OUT";
                            }
                            byte[] networkMessage = payload.data.Serialize();
                            MyTcpNetwork.SendToPeer(networkMessage, 0, networkMessage.Length);
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

            if(bResponseSuccess == false) MyTcpNetwork.Down();
        }

        private void MyTcpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null)
        {
            //응답 처리...
            bResponseSuccess = true;
        }

        private void MyTcpServer_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null)
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

                UInt64 length = BitConverter.ToUInt64(bLength, 0);
                string sData = bData.ToString(SysConfig.Instance.HomeNet_Encoding, (size - 8));
                var dicData = sData.DoubleSplit('&', '=');

                SendToNexpa(dicData);
                
            }
        }

        private void SendToNexpa(Dictionary<string, string> pData)
        {
            switch (pData["INOUT"])
            {
                case "VISIT": //방문자 등록
                    RequestPayload<RequestVisitRegPayload> payload = new RequestPayload<RequestVisitRegPayload>();
                    payload.command = CmdType.visit_reg;

                    RequestVisitRegPayload data = new RequestVisitRegPayload();
                    data.dong = pData["DONG"];
                    data.ho = pData["HO"];
                    data.car_number = pData["CARNO"];
                    data.date = pData["DATETIME"].Substring(0, 8); //yyyyMMdd
                    data.term = pData["CARNO"];
                    payload.data = data;
                    break;
            }
        }

        private void AliveCheck()
        {
            do
            {
                if (shutdownEvent.IsSet) return;

                {
                    //Alive Check 서버로 전달....
                    Log.WriteLog(LogType.Info, $"CcmAdapter | AliveCheck", $"Alive Check~!");

                    try
                    {

                    }
                    catch (Exception)
                    {

                    }
                }

                shutdownEvent.Wait(waitForWork);
            }
            while (_pauseEvent.WaitOne());
        }
    }
}
