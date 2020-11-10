using NexpaAdapterStandardLib;
using NpmAdapter.Adapter;
using System;

namespace NpmAdapter
{
    /// <summary>
    /// Peer to peer Pipe 역할
    /// </summary>
    public class NexPipe : AbstractPipe, IDisposable
    {
        private IAdapter nexpa, homenet;
        //====== Config =====
        //NexpaAdapter
        //1 => TcpServer
        //2 => WebServer
        //HomeNetAdapter
        //1. SHT_5800
        //2. ...
        //ConfigManager.ReadConfig("config", Section, "BaudRate");

        /// <summary>
        /// config.ini 파일에 설정된 Adapter를 생성한다.
        /// </summary>
        /// <returns></returns>
        public override bool GeneratePipe()
        {
            try
            {
                bool isSuccess = true;

                //Config를 Load 한다.
                string npFlag = SysConfig.Instance.Sys_NexpaAdapter;
                string hnFlag = SysConfig.Instance.Sys_HomeNetAdapter;

                //NexpaAdapter 생성
                switch (npFlag)
                {
                    case "1": //TcpServer
                        nexpa = new NexpaAdapter(NexpaAdapter.Status.TcpOnly);
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Flag={npFlag} :  Nexpa TcpAdapter 생성", LogAdpType.Nexpa);
                        break;
                    case "2": //WebServer
                        nexpa = new NexpaAdapter(NexpaAdapter.Status.WebOnly);
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Flag={npFlag} :  Nexpa WebAdapter 생성", LogAdpType.Nexpa);
                        break;
                    case "3":
                        nexpa = new NexpaAdapter(NexpaAdapter.Status.Full);
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Flag={npFlag} :  Nexpa Full Adapter 생성", LogAdpType.Nexpa);
                        break;
                    default:
                        nexpa = null;
                        break;
                }
                if(nexpa == null) Log.WriteLog(LogType.Error, $"AdapterPipe | GeneratePipe", $"Nexpa Adapter 생성 실패");
                else
                {
                    isSuccess &= nexpa.Initialize();
                }

                //HomeNetAdapter  생성
                switch (hnFlag)
                {
                    case "1":
                        if(homenet == null)
                        {
                            homenet = new SHT5800Adapter();
                            Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Flag={hnFlag} : SHT5800Adapter 생성", LogAdpType.HomeNet);
                        }
                        else
                        {
                            Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Flag={hnFlag} : Already created SHT5800Adapter instance...", LogAdpType.HomeNet);
                        }
                        break;
                    case "2": //대림코맥스(TCP, Web 가동)
                        homenet = new CmxDLAdapter(CmxDLAdapter.Status.Full);
                        break;
                    case "2-1": //대림코맥스(TCP만 가동)
                        homenet = new CmxDLAdapter(CmxDLAdapter.Status.TcpOnly);
                        break;
                    case "2-2": //대림코맥스(Web만 가동)
                        homenet = new CmxDLAdapter(CmxDLAdapter.Status.WebOnly);
                        break;
                    case "3": //코맥스 전용
                        homenet = new CmxAdapter();
                        break;
                    default:
                        homenet = null;
                        break;
                }
                if (homenet == null) Log.WriteLog(LogType.Error, $"AdapterPipe | GeneratePipe", $"Homenet Adapter 생성 실패");
                else
                {
                    isSuccess &= homenet.Initialize();
                }

                //두 Adpater를 연결
                nexpa.TargetAdapter = homenet;
                homenet.TargetAdapter = nexpa;

                return isSuccess;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "AdapterPipe | GeneratePipe", $"{ex.Message}");
                return false;
            }

        }

        public override bool StartAdapter(AdapterType type)
        {
            bool bResult = false;
            LogAdpType logType = LogAdpType.none;
            switch (type)
            {
                case AdapterType.nexpa:
                    if (nexpa == null) return false;
                    bResult = nexpa.StartAdapter();
                    logType = LogAdpType.Nexpa;
                    break;
                case AdapterType.homenet:
                    if (homenet == null) return false;
                    bResult = homenet.StartAdapter();
                    logType = LogAdpType.HomeNet;
                    break;
            }
            string resultMessage = bResult ? "성공" : "실패";
            Log.WriteLog(LogType.Info, $"AdapterPipe | StartAdapter", $"Type : {type}, Result : {resultMessage}", logType);
            return bResult;
        }

        public override bool StopAdapter(AdapterType type)
        {
            bool bResult = false;
            LogAdpType logType = LogAdpType.none;
            switch (type)
            {
                case AdapterType.nexpa:
                    if (nexpa == null) return false;
                    bResult = nexpa.StopAdapter();
                    logType = LogAdpType.Nexpa;
                    break;
                case AdapterType.homenet:
                    if (homenet == null) return false;
                    bResult = homenet.StopAdapter();
                    logType = LogAdpType.HomeNet;
                    break;
            }
            string resultMessage = bResult ? "성공" : "실패";
            Log.WriteLog(LogType.Info, $"AdapterPipe | StopAdapter", $"Type : {type}, Result : {resultMessage}", logType);
            return bResult;
        }

        public void Dispose()
        {
            nexpa?.Dispose();
            homenet?.Dispose();
        }

        #region Test

        public bool TestSendMessage(AdapterType type, byte[] data)
        {
            IAdapter adapter = null;
            if (type == AdapterType.nexpa) adapter = nexpa;
            else if (type == AdapterType.homenet) adapter = homenet;

            if (adapter == null) return false;
            else
            {
                try
                {
                    adapter.SendMessage(data, 0, data.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "AdapterPipe | TestSendMessage", $"{ex.Message}");
                    return false;
                }
            }
        }
        
        public bool TestReceiveMessage(AdapterType type, byte[] data)
        {
            IAdapter adapter = null;
            if (type == AdapterType.nexpa) adapter = nexpa;
            else if (type == AdapterType.homenet) adapter = homenet;

            if (adapter == null) return false;
            else
            {
                try
                {
                    adapter.TestReceive(data);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "AdapterPipe | TestSendMessage", $"{ex.Message}");
                    return false;
                }
            }
        }

        #endregion
    }
}
