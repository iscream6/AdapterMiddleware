using NetworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NexpaAdapterStandardLib.Network
{
    class NetworkTcpClient : TcpClient, INetwork
    {
        public Action OnConnectionAction { get; set; }
        public event SendToPeer ReceiveFromPeer;
        private bool _stop;
        private int _stopCnt = 0;
        
        public NetworkTcpClient(string address, int port) : base(address, port) { }

        public bool Down()
        {
            _stop = true;

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
            _stop = false;

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

        protected override void OnConnected()
        {
            Log.WriteLog(LogType.Info, "TcpClientNetwork| OnConnected", $"Chat TCP client connected a new session with Id {Id}");
            if(OnConnectionAction != null) OnConnectionAction();
        }



        protected override void OnDisconnected()
        {
            //서버에서 끊어짐....
            Log.WriteLog(LogType.Info, "TcpClientNetwork| OnDisconnected", $"client disconnected a session with Id {Id}");
            // Wait for a while...
            Thread.Sleep(500);
            //연결 재 시도...
            _stopCnt += 1;
            if (_stopCnt >= 3)
            {
                _stop = true;
            }

            if (!_stop)
            {
                Connect();
            }

            _stopCnt = 0;
        }
    }
}
