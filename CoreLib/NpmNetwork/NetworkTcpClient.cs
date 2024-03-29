﻿using NetworkCore;
using NpmCommon;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace NpmNetwork
{
    class NetworkTcpClient : NetworkCore.TcpClient, INetwork
    {
        public Action OnConnectionAction { get; set; }
        public NetStatus Status { get; set; }

        public event SendToPeer ReceiveFromPeer;
        private bool _stop;
        private int _stopCnt = 0;
        
        public NetworkTcpClient(string address, int port) : base(address, port) { }

        /// <summary>
        /// 연결을 종료 시킴.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DisconnectSession(string id)
        {
            return Down();
        }

        public bool Down()
        {
            _stop = true;

            if (IsConnected)
            {
                Status = NetStatus.Disconnected;
                return Disconnect();
            }
            else
            {
                Status = NetStatus.Disconnected;
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

            bool stt = Connect();
            if (stt) Status = NetStatus.Connected;
            else Status = NetStatus.Disconnected;
            return stt;
        }

        protected override void OnError(SocketError error)
        {
            base.OnError(error);
            Log.WriteLog(LogType.Info, "TcpClientNetwork| OnError", $"Error : {error.ToString()}");
        }

        /// <summary>
        /// Peer로 Data를 전달한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendToPeer(byte[] buffer, long offset, long size, string id = null, System.Net.EndPoint ep = null)
        {
            //Send(buffer);
            SendAsync(buffer, offset, size);
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
            Log.WriteLog(LogType.Info, "TcpClientNetwork| OnConnected", $"TCP client connected a new session with Id {Id}");
            if(OnConnectionAction != null) OnConnectionAction();
        }

        protected override void OnDisconnected()
        {
            //서버에서 끊어짐....
            Log.WriteLog(LogType.Info, "TcpClientNetwork| OnDisconnected", $"client disconnected a session with Id {Id}");
            Status = NetStatus.Disconnected;
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
                bool stt = Connect();
                if (stt) Status = NetStatus.Connected;
                else Status = NetStatus.Disconnected;
            }

            _stopCnt = 0;
        }
    }
}
