﻿using NpmAdapter.Adapter;
using NpmCommon;
using System;

namespace NpmAdapter
{
    /// <summary>
    /// Peer to peer Pipe 역할
    /// </summary>
    public class NexPipe : AbstractPipe, IDisposable
    {
        public event IAdapter.ShowBallonTip ShowTip;

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
                NexpaAdapterType nxpAdapter = StdHelper.GetValueFromDescription<NexpaAdapterType>(SysConfig.Instance.Sys_NexpaAdapter);
                HomeNetAdapterType homAdapter = StdHelper.GetValueFromDescription<HomeNetAdapterType>(SysConfig.Instance.Sys_HomeNetAdapter);
                //NexpaAdapter 생성
                switch (nxpAdapter)
                {
                    case NexpaAdapterType.All:
                        nexpa = new NexpaAdapter(NexpaAdapter.Status.Full);
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Nexpa Full Adapter 생성", LogAdpType.Nexpa);
                        break;
                    case NexpaAdapterType.Tcp_Only:
                    case NexpaAdapterType.Tcp:
                        nexpa = new NexpaAdapter(NexpaAdapter.Status.TcpOnly);
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Nexpa TcpAdapter 생성", LogAdpType.Nexpa);
                        break;
                    case NexpaAdapterType.Web_Only:
                    case NexpaAdapterType.Web:
                        nexpa = new NexpaAdapter(NexpaAdapter.Status.WebOnly);
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Nexpa WebAdapter 생성", LogAdpType.Nexpa);
                        break;
                    case NexpaAdapterType.AutoBooth:
                    case NexpaAdapterType.NPAutoBooth:
                        nexpa = new NPAutoBoothAdapter();
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Nexpa WebAdapter 생성", LogAdpType.Nexpa);
                        break;
                    case NexpaAdapterType.Hipass:
                        nexpa = new NexpaHipassAdapter();
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Nexpa Local Adapter 생성", LogAdpType.Nexpa);
                        break;
                    default:
                        nexpa = null;
                        break;
                }

                if(nexpa != null)
                {
                    isSuccess &= nexpa.Initialize();
                    Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Nexpa Adapter({nxpAdapter}) 생성", LogAdpType.Nexpa);
                }
                else
                {
                    isSuccess = false;
                    Log.WriteLog(LogType.Error, $"AdapterPipe | GeneratePipe", $"Nexpa Adapter 생성 실패");
                }

                //HomeNetAdapter  생성
                switch (homAdapter)
                {
                    case HomeNetAdapterType.None:
                        homenet = null;
                        Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"HomeNet 생성안함", LogAdpType.HomeNet);
                        return true;
                    case HomeNetAdapterType.SHT5800:
                        if(homenet == null)
                        {
                            homenet = new SHT5800Adapter();
                            Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"SHT5800Adapter 생성", LogAdpType.HomeNet);
                        }
                        else
                        {
                            Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"Already created SHT5800Adapter instance...", LogAdpType.HomeNet);
                        }
                        break;
                    case HomeNetAdapterType.Commax_All: //대림코맥스(TCP, Web 가동)
                        homenet = new CmxDLAdapter(CmxDLAdapter.Status.Full);
                        break;
                    case HomeNetAdapterType.Commax_Tcp: //대림코맥스(TCP만 가동)
                        homenet = new CmxDLAdapter(CmxDLAdapter.Status.TcpOnly);
                        break;
                    case HomeNetAdapterType.Commax_Web: //대림코맥스(Web만 가동)
                        homenet = new CmxDLAdapter(CmxDLAdapter.Status.WebOnly);
                        break;
                    case HomeNetAdapterType.Commax_Only: //코맥스 전용
                        homenet = new CmxAdapter();
                        break;
                    case HomeNetAdapterType.Cocom: //코콤
                        homenet = new KcmAdapter();
                        break;
                    case HomeNetAdapterType.ApartStory: //아파트스토리
                        homenet = new AptStAdapter();
                        break;
                    case HomeNetAdapterType.SmartVillage: //스마트빌리지
                        homenet = new SmtvAdapter();
                        break;
                    case HomeNetAdapterType.Samul: //샘물
                        homenet = new SamulAdapter();
                        break;
                    case HomeNetAdapterType.ezVille: //이지빌
                        homenet = new EzvAdapter();
                        break;
                    case HomeNetAdapterType.KakaoMovil: //카카오모빌, 직방
                        homenet = new KakaoMovilAdapter();
                        break;
                    case HomeNetAdapterType.Ulsan: //울산시청 공공감면
                        homenet = new ULSNServerAdapter();
                        break;
                    case HomeNetAdapterType.UjungAir: //우정항공
                        homenet = new UJAirAdapter();
                        break;
                    case HomeNetAdapterType.GSNeoTech: //GS네오텍
                        homenet = new STIAdapter();
                        break;
                    case HomeNetAdapterType.Salda: //살다, 잘살아보세
                        homenet = new SaldaAdapter();
                        break;
                    case HomeNetAdapterType.Beyond: //비욘드
                        homenet = new BeyondAdapter();
                        break;
                    case HomeNetAdapterType.Aptner: //아파트너
                        homenet = new AptNrAdapter();
                        break;
                    case HomeNetAdapterType.HyundaeSanup: //현대산업개발
                        homenet = new ShiAdapter();
                        break;
                    case HomeNetAdapterType.ChamsleTech: //참슬테크
                        homenet = new ChamAdapter();
                        break;
                    case HomeNetAdapterType.ITronHighPass: //아이트론 하이패스
                        //homenet = new ITronHighPassAdapter();
                        break;
                    case HomeNetAdapterType.AptHdnMulti: //아파트너 현대통신 멀티
                        homenet = new AptHdnMultier();
                        break;
                    case HomeNetAdapterType.Militaray:
                        homenet = new MilitaryAdapter();
                        break;
                    default:
                        homenet = null;
                        break;
                }

                if (homenet != null)
                {
                    homenet.ShowTip += ShowTip;
                    isSuccess &= homenet.Initialize();
                    Log.WriteLog(LogType.Info, $"AdapterPipe | GeneratePipe", $"HomeNet Adapter({homAdapter}) 생성", LogAdpType.HomeNet);
                }
                else
                {
                    isSuccess = false;
                    Log.WriteLog(LogType.Error, $"AdapterPipe | GeneratePipe", $"Homenet Adapter 생성 실패");
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
