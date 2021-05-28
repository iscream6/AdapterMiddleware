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
    class KcmAdapter : IAdapter
    {
        private bool isRun = false;

        public IAdapter TargetAdapter { get; set; }

        private INetwork MyTcpNetwork { get; set; }

        public bool IsRuning { get => isRun; }

        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(false);

        public event IAdapter.ShowBallonTip ShowTip;

        private delegate void SafeCallDelegate();

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
                Log.WriteLog(LogType.Error, "KcmAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.Nexpa);
                return false;
            }
            
            MyTcpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, SysConfig.Instance.HT_IP, SysConfig.Instance.HT_Port);

            //Alive Check
            aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
            aliveCheckThread.Name = "Cmx thread for alive check";
            if (!TimeSpan.TryParse(SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckTime"), out waitForWork))
            {
                //Default 50초
                waitForWork = TimeSpan.FromSeconds(50);
            }
            //Alive Check

            return true;
        }

        public bool StartAdapter()
        {
            try
            {
                MyTcpNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
                isRun = MyTcpNetwork.Run();

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
                Log.WriteLog(LogType.Error, "KcmAdapter | StartAdapter", $"{ex.Message}", LogAdpType.HomeNet);
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
                Log.WriteLog(LogType.Error, "KcmAdapter | StopAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }

            return true;
        }

        public void TestReceive(byte[] buffer)
        {
            
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            var jobj = JObject.Parse(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));

            Log.WriteLog(LogType.Info, $"KcmxDLAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);

            JObject data = jobj["data"] as JObject;
            string cmd = jobj["command"].ToString();

            switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
            {
                case CmdType.alert_incar:
                case CmdType.alert_outcar:

                    break;
            }
        }

        private void MyTcpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null)
        {
            
        }

        private void AliveCheck()
        {
            do
            {
                if (shutdownEvent.IsSet) return;

                {
                    //Alive Check 서버로 전달....
                    Log.WriteLog(LogType.Info, $"KcmAdapter | AliveCheck", $"Alive Check~!");

                    try
                    {
                        int value = 0x12345678;
                    }
                    catch (Exception)
                    {

                    }
                }

                shutdownEvent.Wait(waitForWork);
            }
            while (_pauseEvent.WaitOne());
        }

        //private class Msg
        //{
        //    private const int HeaderKey = 0x12345678;
        //    private int msgType;
        //    private int msgLength;
        //    private int town = 0;
        //    private int dong;
        //    private int ho;
        //    private int reserved = 0;
        //    //public Header()
        //    //{
        //    //    byte[] ppp = new byte[16];
        //    //    Array.Clear(ppp, 0, ppp.Length);
        //    //}

        //    public byte[] Login(string id, string pw)
        //    {
        //        int homeVersion = 0; //0으로 초기화
        //        int nKing = 0; //0으로 초기화
        //        byte[] nVersion = new byte[16];
        //        Array.Clear(nVersion, 0, nVersion.Length); //0으로 초기화
        //        byte[] bId = Encoding.UTF8.GetBytes(id);
        //        byte[] bPw = Encoding.UTF8.GetBytes(pw);
        //        char[] sid = Array.
        //    }

        //    public byte[] AliveCheck()
        //    {

        //    }

        //}
    }
}
