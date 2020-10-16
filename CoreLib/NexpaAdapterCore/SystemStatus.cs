using System;
using System.Collections.Generic;
using System.Text;

namespace NexpaAdapterStandardLib
{
    public enum LogAdpType
    {
        none,
        Nexpa,
        HomeNet
    }

    public delegate void StatusChange(LogAdpType adapterType, string statusMessage);
    public class SystemStatus : Singleton<SystemStatus>
    {
        public SystemStatus() { }

        public event StatusChange StatusChanged;

        public void SendEventMessage(LogAdpType adapterType, string message)
        {
            StatusChanged?.Invoke(adapterType, message);
        }
    }
}
