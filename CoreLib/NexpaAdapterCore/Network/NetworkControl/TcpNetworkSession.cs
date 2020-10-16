using NetworkCore;
using System;
using System.Net.Sockets;
using System.Text;

namespace NexpaAdapterStandardLib.Network
{
    public delegate void SessionDisconnected(Guid id);

    class TcpNetworkSession : TcpSession
    {
        public event ReceiveAction receiveAction;
        public event SessionDisconnected Session_Disconnected;
        
        public TcpNetworkSession(TcpServer server) : base(server) { }
        
        protected override void OnConnected()
        {
            Log.WriteLog(LogType.Info, $"TcpNetworkSession | OnConnected", $"TCP session with Id {Id} connected!");
        }

        protected override void OnDisconnected()
        {
            Session_Disconnected?.Invoke(Id);
            Log.WriteLog(LogType.Info, $"TcpNetworkSession | OnDisconnected", $"TCP session with Id {Id} disconnected!");
        }

        protected override void OnReceived(byte[] buffer, long offset, long size)
        {
            try
            {
                string message = Encoding.Default.GetString(buffer, (int)offset, (int)size);
                Log.WriteLog(LogType.Info, $"TcpNetworkSession | OnReceived", $"Incoming : {message}");
                receiveAction?.Invoke(buffer, offset, size);
            }
            catch (Exception ex)
            {
                Disconnect();
                Log.WriteLog(LogType.Error, $"TcpNetworkSession | OnReceived", $"Error : {ex.Message}");
            }
        }

        protected override void OnError(SocketError error)
        {
            Log.WriteLog(LogType.Error, $"TcpNetworkSession | OnError", $"TCP session caught an error with code {error}");
        }
    }
}
