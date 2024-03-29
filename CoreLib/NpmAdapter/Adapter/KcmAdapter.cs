﻿using Newtonsoft.Json.Linq;
using NpmAdapter.Payload;
using NpmCommon;
using NpmNetwork;
using System;
using System.Threading;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 코콤
    /// PARK_INFO -> PARK_INFO_ACK / ERROR_ACK
    /// CHECK_PARK_INFO_ACK <- CHECK_PARK_INFO : 세대 동, 호를 기준으로 요청 세대내의 모든 차량의 입출차 정보를 보낸다.
    /// ALIVE -> ALIVE_ACK : 50초마다 보낸다.
    /// </summary>
    class KcmAdapter : IAdapter
    {
        private bool isRun = false;

        public IAdapter TargetAdapter { get; set; }

        private INetwork MyTcpNetwork { get; set; }

        public bool IsRuning { get => isRun; }
        public string reqPid { get; set; }

        private NpmThread aliveCheckThread;

        public event IAdapter.ShowBallonTip ShowTip;

        private delegate void SafeCallDelegate();

        public void Dispose()
        {
            aliveCheckThread.Dispose();
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
            aliveCheckThread = new NpmThread("Cmx thread for alive check", TimeSpan.FromSeconds(50));
            aliveCheckThread.ThreadAction = AliveCheck;
            //Alive Check

            return true;
        }

        public bool StartAdapter()
        {
            try
            {
                MyTcpNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
                isRun = MyTcpNetwork.Run();

                aliveCheckThread.Start();
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
                aliveCheckThread.Stop();
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

        private void MyTcpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null, System.Net.EndPoint ep = null)
        {
            
        }

        private void AliveCheck()
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
