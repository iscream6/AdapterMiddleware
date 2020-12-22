using System;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace Common
{
    public class XMLConfig
    {
        /// <summary>
        /// 환경설정파일(.config)에서 값을 읽어옴
        /// 
        /// ExeConfigurationFileMap 을 사용하기 위해서는 참조에 System.Configuration 을 추가해야 한다.
        /// </summary>
        /// <param name="sKey"></param>
        /// <returns></returns>
        public String GetAppValue(String sKey)
        {
            #region
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();

            String strFileName = Path.ChangeExtension(Application.ExecutablePath, ".config").ToUpper();

            configFileMap.ExeConfigFilename = strFileName;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            try
            {
                return config.AppSettings.Settings[sKey].Value;
            }
            catch
            {
                return String.Empty;
            }
            #endregion
        }

        /// <summary>
        /// 환경설정파일(.config)에 값을 저장
        /// </summary>
        /// <param name="sKey"></param>
        /// <param name="sValue"></param>
        public void SetAppValue(String sKey, String sValue)
        {
            #region
            ExeConfigurationFileMap configFileMap = new ExeConfigurationFileMap();

            String strFileName = Path.ChangeExtension(Application.ExecutablePath, ".config").ToUpper();

            configFileMap.ExeConfigFilename = strFileName;
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFileMap, ConfigurationUserLevel.None);

            try
            {
                AppSettingsSection cfSection = config.AppSettings;
                cfSection.Settings.Remove(sKey);
                cfSection.Settings.Add(sKey, sValue);
                config.Save(ConfigurationSaveMode.Full);
            }
            catch
            {
            }
            #endregion
        }
    }
}
