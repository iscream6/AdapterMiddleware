using HttpServer;
using System;
using System.Collections.Generic;
using System.Text;

namespace NexpaAdapterStandardLib
{
    public delegate void SendToPeer(byte[] buffer, long offset, long size, RequestEventArgs pEvent = null, string id = null);

    public interface INetwork
    {
        event SendToPeer ReceiveFromPeer;

        public Action OnConnectionAction { get; set; }

        void SendToPeer(byte[] buffer, long offset, long size, string id = null);

        bool Run();

        bool Down();
    }
}
