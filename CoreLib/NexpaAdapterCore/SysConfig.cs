using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

// [ 작성일  ]  [ Version ]   [             변경 내용             ]
// 2020-10-07   Cn0200930.1   [Nexpa_Config] -> Version 추가
//                            [Nexpa_Config] -> Options -> CmdLocSearchUse|1 추가
// 2020-10-13   Cn0201010.1   [SysConfig] -> TestOption 추가
// 2020-10-14   Cn0201014.1   [SysConfig] -> Options -> TestMode|Y^AutoStart|Y^UseLocationSearch|Y 추가
//              Cn0201014.2   [Nexpa_Config] -> Options 삭제, [SysConfig] -> TestOption 삭제
//              Cn0201014.3   [Nexpa_Config] -> Encoding 추가
// 2020-10-15   Cn0201015.1   [SysConfig] -> Options -> CmxAliveCheckTime|00:00:05 추가
// 2020-10-20   Cn0201020.1   [HomeNet_Config] -> MyPort 추가
// 2020-11-19   Cn0201119.1   [HomeNet_Config] -> Web_Domain, HId, HPw 추가
// 2020-11-24   Cn0201124.1   [Etc_Config] 추가, AptId 추가
//              Cn0201124.2   [Etc_Config] -> ParkId 추가
//              Cn0201124.3   [Etc_Config] -> AuthToken 추가
namespace NexpaAdapterStandardLib
{
    public class SysConfig : Singleton<SysConfig>
    {
        public readonly string ConfigVersion = "Cn0201124.3";

        private const string config = "config";

        public enum Sections
        {
            [Description("SysConfig")]
            SysConfig,
            [Description("Nexpa_Config")]
            NexpaConfig,
            [Description("HomeNet_Config")]
            HomeNetConfig,
            [Description("Etc_Config")]
            EtcConfig
        }

        #region Config Values

        #region SysConfig

        /// <summary>
        /// 넥스파 Adapter 설정 1=Tcp, 2=Web, 3=Full
        /// </summary>
        public string Sys_NexpaAdapter { get; private set; }
        /// <summary>
        /// 홈넷 Adapter 설정 1=SHT5800, 2=CommaxDaelim(Tcp, Web), 2-1=CommaxDaelim(Tcp), 2-2=CommaxDaelim(Web), 3=Commax 전용, CCM=코콤
        /// </summary>
        public string Sys_HomeNetAdapter { get; private set; }
        /// <summary>
        /// 옵션
        /// </summary>
        public Dictionary<string, string> Sys_Option { get; private set; }
        /// <summary>
        /// Config 버전 일치 여부
        /// </summary>
        public bool ValidateConfig { get; private set; }
        /// <summary>
        /// Program Version
        /// </summary>
        public string Version { get; private set; }

        #endregion

        #region Nexpa_Config

        
        /// <summary>
        /// 넥스파 Tcp 포트
        /// </summary>
        public string Nexpa_TcpPort { get; private set; }
        /// <summary>
        /// 넥스파 Web 포트
        /// </summary>
        public string Nexpa_WebPort { get; private set; }
        /// <summary>
        /// 넥스파 유도 WebSocket IP or URL...
        /// </summary>
        public string Nexpa_UWebIP { get; private set; }
        /// <summary>
        /// 넥스파 유도 WebSocket Port
        /// </summary>
        public string Nexpa_UWebPort { get; private set; }
        /// <summary>
        /// 넥스파 통신 Encoding Type
        /// </summary>
        public Encoding Nexpa_Encoding { get; private set; }
        public string Nexpa_EncodCd { get; private set; }
        public string Target_TcpIP { get; private set; }
        public string Target_TcpPort { get; private set; }

        #endregion

        #region HomeNet_Common

        /// <summary>
        /// HomeNet ID
        /// </summary>
        public string HC_Id { get; private set; }
        /// <summary>
        /// HomeNet Password
        /// </summary>
        public string HC_Pw { get; private set; }

        #endregion

        #region HomeNet_SerialConfig

        /// <summary>
        /// HomeNet Serial BaudRate
        /// </summary>
        public string HS_BaudRate { get; private set; }
        /// <summary>
        /// HomeNet Serial PortName
        /// </summary>
        public string HS_PortName { get; private set; }
        /// <summary>
        /// HomeNet Serial Parity
        /// </summary>
        public string HS_Parity { get; private set; }

        #endregion

        #region HomeNet_WebConfig

        /// <summary>
        /// HomeNet Web Port
        /// </summary>
        public string HW_Port { get; private set; }
        /// <summary>
        /// HomeNet Web Domain
        /// </summary>
        public string HW_Domain { get; private set; }
        
        #endregion

        #region HomeNet_TcpConfig

        /// <summary>
        /// HomeNet Tcp iP
        /// </summary>
        public string HT_IP { get; private set; }
        /// <summary>
        /// HomeNet Tcp Port
        /// </summary>
        public string HT_Port { get; private set; }
        /// <summary>
        /// Homenet Tcp My Port
        /// </summary>
        public string HT_MyPort { get; private set; }

        #endregion

        #region HomeNet_CommonConfig

        public Encoding HomeNet_Encoding { get; private set; }
        public string HomeNet_EncodCd { get; private set; }

        #endregion

        #region Etc

        public string ParkId { get; private set; }
        public string AuthToken { get; private set; }

        #endregion

        #region MSSQL DataBase

        public string DataSource { get; private set; }
        public string InitialCatalog { get; private set; }
        public string UserID { get; private set; }
        public string Password { get; private set; }

        #endregion

        #endregion

        #region Constructor

        public SysConfig()
        {
            Sys_NexpaAdapter = ConfigManager.ReadConfig(config, Sections.SysConfig.GetDescription(), "NexpaAdapter");
            Sys_HomeNetAdapter = ConfigManager.ReadConfig(config, Sections.SysConfig.GetDescription(), "HomeNetAdapter");
            Sys_Option = ReadOption(ConfigManager.ReadConfig(config, Sections.SysConfig.GetDescription(), "Options"));
            ValidateConfig = ConfigVersion.Equals(ConfigManager.ReadConfig(config, Sections.SysConfig.GetDescription(), "Config_Version"));
            Version = ConfigManager.ReadConfig(config, Sections.SysConfig.GetDescription(), "PVersion");

            Nexpa_TcpPort = ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "TcpPort");
            Nexpa_WebPort = ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "WebPort");
            Nexpa_UWebIP = ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "UWebIP");
            Nexpa_UWebPort = ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "UWebPort");
            Nexpa_EncodCd = ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "Encoding");
            Nexpa_Encoding = GetEncoding(Nexpa_EncodCd);
            DataSource = $"{ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "DBIP")},{ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "DBPort")}";
            InitialCatalog = ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "DBName");
            UserID = ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "DBID");
            Password = ConfigManager.ReadConfig(config, Sections.NexpaConfig.GetDescription(), "DBPW");

            HS_BaudRate = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "Serial_BaudRate");
            HS_PortName = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "Serial_PortName");
            HS_Parity = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "Serial_Parity");
            HW_Port = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "Web_Port");
            HW_Domain = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "Web_Domain");
            HT_IP = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "Tcp_IP");
            HT_Port = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "Tcp_Port");
            HT_MyPort = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "My_Port");
            HC_Id = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "HId");
            HC_Pw = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "HPw");
            HomeNet_EncodCd = ConfigManager.ReadConfig(config, Sections.HomeNetConfig.GetDescription(), "Encoding");
            HomeNet_Encoding = GetEncoding(HomeNet_EncodCd);

            ParkId = ConfigManager.ReadConfig(config, Sections.EtcConfig.GetDescription(), "ParkId");
            AuthToken = ConfigManager.ReadConfig(config, Sections.EtcConfig.GetDescription(), "AuthToken");

            
        }

        #endregion

        #region Methods

        public void WriteConfig(Sections section, string key, string value)
        {
            ConfigManager.WriteConfig(config, section.GetDescription(), key, value);
        }

        /// <summary>
        /// 옵션을가져온다.
        /// </summary>
        /// <param name="option">Format = [key]|[value] ^[key]|[value] ^ ...</param>
        private Dictionary<string, string> ReadOption(string pOption)
        {
            try
            {
                Dictionary<string, string> dicOption;
                dicOption = new Dictionary<string, string>();

                string[] optionArr = pOption.Split('^');
                if (optionArr != null && optionArr.Length > 0)
                {
                    foreach (var option in optionArr)
                    {
                        string[] tempOptionArr = option.Split('|');
                        if (tempOptionArr != null && tempOptionArr.Length > 1)
                        {
                            dicOption.Add(tempOptionArr[0], tempOptionArr[1]);
                        }
                    }
                }

                return dicOption;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SysConfig | ReadOption", $"옵션을 가져오는데 실패하였습니다. : {ex.Message}", LogAdpType.Nexpa);
                return null;
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
                case "kr":
                    int iEuckr = 51949;
                    Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                    return Encoding.GetEncoding(iEuckr);
                default:
                    return Encoding.UTF8;
            }
        }

        #endregion

    }
}
