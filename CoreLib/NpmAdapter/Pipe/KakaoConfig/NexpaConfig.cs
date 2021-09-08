using System;
using System.Configuration;
using System.IO;

namespace NpmAdapter.kakao
{
    class NexpaConfig
    {
        private const string CONFIG_FILE_NAME = "nexpa.config";
        private static NexpaConfig instance = null;

        /// <summary>
        /// Config Setting 성공 여부
        /// </summary>
        public bool ReadSuccess { get; }
        public int LoopTime { get; }
        public int ParkingDBType { get; }
        public bool ParkingGuidanceUse { get; }
        public int ParkingGuidanceDBType { get; }
        public int ParkingGuidanceLocation { get; }

        private NexpaConfig()
        {
            ReadSuccess = false;

            try
            {
                string configPath = Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().Location, @"Config\\" + CONFIG_FILE_NAME);
                // Urazilation.config 파일에서 providerType과 Connection String을 읽는다...
                ExeConfigurationFileMap configMap = new ExeConfigurationFileMap();
                configMap.ExeConfigFilename = configPath;
                Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);

                // 주차장 설정값 가져오기
                // providerType 설정값 가져오기...
                AppSettingsSection section = (AppSettingsSection)config.GetSection("appSettings");

                LoopTime = int.Parse(section.Settings["loop_time"].Value);
                ParkingDBType = int.Parse(section.Settings["ParkingDBType"].Value);
                ParkingGuidanceUse = bool.Parse(section.Settings["dbParkingGuidanceUse"].Value);
                ParkingGuidanceDBType = int.Parse(section.Settings["dbParkingGuidanceDBType"].Value);
                ParkingGuidanceLocation = int.Parse(section.Settings["parkingGuidanceLocation"].Value);

                ReadSuccess = true;
            }
            catch (Exception)
            {
                ReadSuccess = false;
            }
        }

        public static NexpaConfig GetInstance()
        {
            if (instance == null) instance = new NexpaConfig();
            return instance;
        }
    }
}
