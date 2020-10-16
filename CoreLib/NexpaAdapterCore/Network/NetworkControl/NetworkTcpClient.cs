using NetworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexpaAdapterStandardLib.Network
{
    class NetworkTcpClient : TcpClient, INetwork
    {
        public event SendToPeer ReceiveFromPeer;

        public NetworkTcpClient(string address, int port) : base(address, port) { }

        public bool Down()
        {
            if (IsConnected)
            {
                return Disconnect();
            }
            else
            {
                return true;
            }
        }

        public bool Run()
        {
            if (IsConnected)
            {
                Disconnect();
            }
            return Connect();
        }

        /// <summary>
        /// Peer로 Data를 전달한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendToPeer(byte[] buffer, long offset, long size)
        {
            Send(buffer);
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            try
            {
                Log.WriteLog(LogType.Info, "TcpClientNetwork| OnReceived", $"Received Data");
                ReceiveFromPeer?.Invoke(buffer, offset, size);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"TcpClientNetwork| OnReceived", $"Received error : {ex.Message}");
            }
        }

        protected override void OnDisconnected()
        {
            //서버에서 끊어짐....
            Log.WriteLog(LogType.Info, "TcpClientNetwork| OnDisconnected", $"client disconnected a session with Id {Id}");
            //연결을 끊어준다.
            Disconnect();
        }
    }
}
