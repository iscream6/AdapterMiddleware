using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using WebSocketSharp;
using System.Security.Authentication;
using NpmCommon;

namespace NpmNetwork
{
    class NetworkWSSClient : INetwork
    {
        private WebSocketSharp.WebSocket _clientWebSocket = null;

        public Action OnConnectionAction { get; set; }
        public NetStatus Status { get; set; }

        public event SendToPeer ReceiveFromPeer;

        public NetworkWSSClient(Uri uri)
        {
            try
            {
                _clientWebSocket = new WebSocketSharp.WebSocket(uri.AbsoluteUri);
                _clientWebSocket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
                _clientWebSocket.OnOpen += new EventHandler(Connected);
                _clientWebSocket.OnError += new EventHandler<ErrorEventArgs>(Error);
                _clientWebSocket.OnMessage += new EventHandler<MessageEventArgs>(RecevieData);
                _clientWebSocket.OnClose += new EventHandler<CloseEventArgs>(Close);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"NetworkWSSClient| Constructor", $"{ex.Message}");
            }
        }

        #region WebSocketSharp.WebSocket Event Handler

        private void Connected(object sender, EventArgs e)
        {
            
        }

        private void Error(object sender, ErrorEventArgs e)
        {
            
        }

        /// <summary>
        /// 데이터 수신
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RecevieData(object sender, MessageEventArgs e)
        {
            try
            {
                byte[] rcvData = e.RawData;
                ReceiveFromPeer?.Invoke(rcvData, 0, rcvData.Length);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"NetworkWSSClient| RecevieData", $"Received error : {ex.Message}");
            }
        }
        private void Close(object sender, CloseEventArgs e)
        {
            
        }

        #endregion


        public bool Down()
        {
            bool isConnected = false;

            try
            {
                if(_clientWebSocket != null && Status == NetStatus.Connected)
                {
                    _clientWebSocket.Close();
                    isConnected = _clientWebSocket.IsAlive;
                    Status = isConnected ? NetStatus.Connected : NetStatus.Disconnected;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"NetworkWSSClient| Down", $"{ex.Message}");
                return false;
            }

            return isConnected;
        }

        public bool Run()
        {
            bool isConnected = false;
            try
            {
                _clientWebSocket.Connect();
                isConnected = _clientWebSocket.IsAlive;
                Status = isConnected ? NetStatus.Connected : NetStatus.Disconnected;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"NetworkWSSClient| Run", $"{ex.Message}");
                return false;
            }

            return isConnected;
        }

        /// <summary>
        /// 연결된 WSS 서버로 데이터 전달
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="id"></param>
        /// <param name="ep"></param>
        public void SendToPeer(byte[] buffer, long offset, long size, string id = null, EndPoint ep = null)
        {
            try
            {
                _clientWebSocket.Send(buffer[..(int)size]);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"NetworkWSSClient| SendToPeer", $"{ex.Message}");
            }
        }
    }
}
