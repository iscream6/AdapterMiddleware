﻿using System;
using System.Collections.Generic;
using System.Text;

// [ 작성일  ]  [ Version ]   [             변경 내용             ]
// 2020-10-07   Cn0200930.1   [Nexpa_Config] -> Version 추가
//                            [Nexpa_Config] -> Options -> CmdLocSearchUse|1 추가
// 2020-10-13   Cn0201010.1   [SysConfig] -> TestOption 추가
// 2020-10-14   Cn0201014.1   [SysConfig] -> Options -> TestMode|Y^AutoStart|Y^UseLocationSearch|Y 추가
//              Cn0201014.2   [Nexpa_Config] -> Options 삭제, [SysConfig] -> TestOption 삭제
//              Cn0201014.3   [Nexpa_Config] -> Encoding 추가
// 2020-10-15   Cn0201015.1   [SysConfig] -> Options -> CmxAliveCheckTime|00:00:05 추가

namespace NexpaAdapterStandardLib
{
    public class SysConfig : Singleton<SysConfig>
    {
        #region Sections

        public readonly string ConfigVersion = "Cn0201005.1";
        private string SysSection { get => "SysConfig"; }
        private string NexpaSection { get => "Nexpa_Config"; }
        private string HomeNetSection { get => "HomeNet_Config"; }

        #endregion

        #region Config Values

        #region SysConfig

        /// <summary>
        /// 넥스파 Adapter 설정 1=Tcp, 2=Web, 3=Full
        /// </summary>
        public readonly string Sys_NexpaAdapter;
        /// <summary>
        /// 홈넷 Adapter 설정 1=SHT5800, 2=CommaxDaelim(Tcp, Web), 2-1=CommaxDaelim(Tcp), 2-2=CommaxDaelim(Web)
        /// </summary>
        public readonly string Sys_HomeNetAdapter;
        /// <summary>
        /// 옵션
        /// </summary>
        public readonly Dictionary<string, string> Sys_Option;
        /// <summary>
        /// Config 버전 일치 여부
        /// </summary>
        public readonly bool ValidateConfig;
        /// <summary>
        /// Program Version
        /// </summary>
        public readonly string Version;

        #endregion

        #region Nexpa_Config

        /// <summary>
        /// 넥스파 Tcp 포트
        /// </summary>
        public readonly string Nexpa_TcpPort;
        /// <summary>
        /// 넥스파 Web 포트
        /// </summary>
        public readonly string Nexpa_WebPort;
        /// <summary>
        /// 넥스파 유도 WebSocket IP or URL...
        /// </summary>
        public readonly string Nexpa_UWebIP;
        /// <summary>
        /// 넥스파 유도 WebSocket Port
        /// </summary>
        public readonly string Nexpa_UWebPort;
        /// <summary>
        /// 넥스파 통신 Encoding Type
        /// </summary>
        public readonly Encoding Nexpa_Encoding;

        #endregion

        #region HomeNet_SerialConfig

        /// <summary>
        /// HomeNet Serial BaudRate
        /// </summary>
        public readonly string HS_BaudRate;
        /// <summary>
        /// HomeNet Serial PortName
        /// </summary>
        public readonly string HS_PortName;
        /// <summary>
        /// HomeNet Serial Parity
        /// </summary>
        public readonly string HS_Parity;

        #endregion

        #region HomeNet_WebConfig

        /// <summary>
        /// HomeNet Web Port
        /// </summary>
        public readonly string HW_Port;

        #endregion

        #region HomeNet_TcpConfig

        /// <summary>
        /// HomeNet Tcp iP
        /// </summary>
        public readonly string HT_IP;
        /// <summary>
        /// HomeNet Tcp Port
        /// </summary>
        public readonly string HT_Port;

        #endregion

        #region HomeNet_CommonConfig

        public readonly Encoding HomeNet_Encoding;

        #endregion

        #endregion

        #region Constructor

        public SysConfig()
        {
            Sys_NexpaAdapter = ConfigManager.ReadConfig("config", SysSection, "NexpaAdapter");
            Sys_HomeNetAdapter = ConfigManager.ReadConfig("config", SysSection, "HomeNetAdapter");
            ReadOption(ConfigManager.ReadConfig("config", SysSection, "Options"), ref Sys_Option);
            ValidateConfig = ConfigVersion.Equals(ConfigManager.ReadConfig("config", SysSection, "Config_Version"));
            Version = ConfigManager.ReadConfig("config", SysSection, "PVersion");

            Nexpa_TcpPort = ConfigManager.ReadConfig("config", NexpaSection, "TcpPort");
            Nexpa_WebPort = ConfigManager.ReadConfig("config", NexpaSection, "WebPort");
            Nexpa_UWebIP = ConfigManager.ReadConfig("config", NexpaSection, "UWebIP");
            Nexpa_UWebPort = ConfigManager.ReadConfig("config", NexpaSection, "UWebPort");
            Nexpa_Encoding = GetEncoding(ConfigManager.ReadConfig("config", NexpaSection, "Encoding"));

            HS_BaudRate = ConfigManager.ReadConfig("config", HomeNetSection, "Serial_BaudRate");
            HS_PortName = ConfigManager.ReadConfig("config", HomeNetSection, "Serial_PortName");
            HS_Parity = ConfigManager.ReadConfig("config", HomeNetSection, "Serial_Parity");
            HW_Port = ConfigManager.ReadConfig("config", HomeNetSection, "Web_Port");
            HT_IP = ConfigManager.ReadConfig("config", HomeNetSection, "Tcp_IP");
            HT_Port = ConfigManager.ReadConfig("config", HomeNetSection, "Tcp_Port");
            HomeNet_Encoding = GetEncoding(ConfigManager.ReadConfig("config", HomeNetSection, "Encoding"));
            
        }

        #endregion

        #region Methods

        /// <summary>
        /// 옵션을가져온다.
        /// </summary>
        /// <param name="option">Format = [key]|[value] ^[key]|[value] ^ ...</param>
        private void ReadOption(string pOption, ref Dictionary<string, string> pDicOptions)
        {
            try
            {
                if (pDicOptions == null) pDicOptions = new Dictionary<string, string>();

                string[] optionArr = pOption.Split('^');
                if (optionArr != null && optionArr.Length > 0)
                {
                    foreach (var option in optionArr)
                    {
                        string[] tempOptionArr = option.Split('|');
                        if (tempOptionArr != null && tempOptionArr.Length > 1)
                        {
                            pDicOptions.Add(tempOptionArr[0], tempOptionArr[1]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SysConfig | ReadOption", $"옵션을 가져오는데 실패하였습니다. : {ex.Message}", LogAdpType.Nexpa);
            }
        }

        private Encoding GetEncoding(string type)
        {
            switch (type)
            {
                case "D":
                    return Encoding.Default;
                case "8":
                    return Encoding.UTF8;
                default:
                    return Encoding.UTF8;
            }
        }

        #endregion

    }
}
