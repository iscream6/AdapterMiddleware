using NpmNetwork;
using System;
using System.Collections.Generic;
using System.Text;
using NpmCommon;

namespace NpmAdapter.Adapter
{
    class WonHoAdapter : IAdapter
    {
        private NpmThread aliveCheckThread { get; set; }
        private INetwork networkTCPServer { get; set; }

        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => true;

        public string reqPid { get; set; }

        public event IAdapter.ShowBallonTip ShowTip;

        public void Dispose()
        {
            aliveCheckThread.Dispose();
        }

        public bool Initialize()
        {
            aliveCheckThread = new NpmThread("AliveCheck", TimeSpan.FromSeconds(5));
            aliveCheckThread.ThreadAction = Actions;

            var myPort = SysConfig.Instance.HT_MyPort;
            networkTCPServer = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpServer, myPort);
            return true;
        }

        private void Actions()
        {
            //5초마다 Alivecheck를 한다.
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            //Adapter Protocol 에 맞게 변경작업
            networkTCPServer.SendToPeer(buffer, offset, size);
        }

        public bool StartAdapter()
        {
            networkTCPServer.ReceiveFromPeer += NetworkTCPServer_ReceiveFromPeer;
            aliveCheckThread.Start();
            return networkTCPServer.Run();
        }

        private void NetworkTCPServer_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null, System.Net.EndPoint ep = null)
        {
            
        }

        public bool StopAdapter()
        {
            aliveCheckThread.Stop();
            return networkTCPServer.Down();
        }

        public void TestReceive(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
