using NetworkCore;
using NpmCommon;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NpmNetwork
{
    class NetworkUdpServer : UdpServer, INetwork
    {
        public Action OnConnectionAction { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public NetStatus Status { get; set; }

        public event SendToPeer ReceiveFromPeer;

        public NetworkUdpServer(IPAddress address, int port) : base(address, port)
        {

        }

        public bool Down()
        {
            if (!IsStarted) return false;
            return Stop();
        }

        public bool Run()
        {
            try
            {
                if (IsStarted)
                {
                    SystemStatus.Instance.SendEventMessage(LogAdpType.Nexpa, "Already started Udp Server...");
                    return true;
                }
                Log.WriteLog(LogType.Info, $"NetworkUdpServer | Run", $"UDP Server 시작");
                return Start();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"NetworkUdpServer | Run", $"TCP Server 오류 : {ex.Message}");
                return false;
            }
        }

        public void SendToPeer(byte[] buffer, long offset, long size, string id = null, System.Net.EndPoint ep = null)
        {
            SendAsync(ep, buffer, offset, size);
        }

        protected override void OnStarted()
        {
            // Start receive datagrams
            ReceiveAsync();
        }

        protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
        {
            ReceiveFromPeer?.Invoke(buffer, offset, size, ep: endpoint);
        }

        protected override void OnSent(EndPoint endpoint, long sent)
        {
            // Continue receive datagrams
            ReceiveAsync();
        }
    }
}
