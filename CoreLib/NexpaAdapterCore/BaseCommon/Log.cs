using System;

namespace NexpaAdapterStandardLib
{
    public enum LogType
    {
        Info,
        Error
    }

    public class Log
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static void WriteLog(LogType type, string pathName, string message, LogAdpType adapterType = LogAdpType.none)
        {
            string preAdpType = string.Empty;
            if(adapterType != LogAdpType.none)
            {
                preAdpType = "[" + adapterType.ToString().Substring(0, 1) + "]";
            }

            switch(type)
            {
                case LogType.Info:
                    logger.Info(preAdpType + pathName + " | " + message);
                    break;
                case LogType.Error:
                    logger.Error(preAdpType + pathName + " | " + message);
                    break;
            }

            if(adapterType != LogAdpType.none || type == LogType.Error)
            {
                SystemStatus.Instance.SendEventMessage(adapterType, message);
            }
        }
    }
}
