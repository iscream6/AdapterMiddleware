using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Common
{
    public class NPSYS
    {
        private static string mCurrentSelectBooth = string.Empty;
        private static NPINCarConfig m_npincarConfig = null;

        public static NPINCarConfig Config
        {
            get
            {
                if (m_npincarConfig == null) m_npincarConfig = new ConfigLoad().GetConfigLoad(ref m_npincarConfig);
                return m_npincarConfig;
            }
        }

        public static string gCurrentSelectBooth
        {
            get { return mCurrentSelectBooth; }
            set { mCurrentSelectBooth = value; }
        }

        public static string ConvetYears_Dash(string p_Years)
        {
            string _n_years = p_Years.Replace(" ", "").Trim();
            if (_n_years.Length == 8)
            {
                return _n_years.Substring(0, 4) + "-" + _n_years.Substring(4, 2) + "-" + _n_years.Substring(6, 2);
            }
            return _n_years;

        }

        public static string ConvetDay_Dash(string p_days)
        {
            string _p_day = p_days.Replace(" ", "").Trim();
            if (_p_day.Length == 6)
            {
                return _p_day.Substring(0, 2) + ":" + _p_day.Substring(2, 2) + ":" + _p_day.Substring(4, 2);
            }
            return _p_day;
        }

        /// <summary>
        /// yyyymmddhhmmss 를 변환
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string datestringParser(string date)
        {
            return date.Substring(0, 4) + "-" + date.Substring(4, 2) + "-" + date.Substring(6, 2) + " " + date.Substring(8, 2) + ":" + date.Substring(10, 2);
        }
        /// <summary>
        /// 하나라도 틀리면 폴스
        /// </summary>
        /// <param name="type"></param>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static bool IsRegexMatch(int type, string plainText)
        {
            Regex rx;

            switch (type)
            {
                case 1:
                    rx = new Regex(@"^[0-9]*$", RegexOptions.None);
                    break;
                case 2:
                    rx = new Regex(@"^[a-zA-Z]*$", RegexOptions.None);
                    break;
                case 3:
                    rx = new Regex(@"^[가-힣]*$", RegexOptions.None);
                    break;
                case 4:
                    rx = new Regex(@"^[a-zA-Z0-9]*$", RegexOptions.None);
                    break;
                case 5:
                    rx = new Regex(@"^[a-zA-Z0-9一-龥]*$", RegexOptions.None);
                    break;
                case 6:
                    rx = new Regex(@"^[a-zA-Z0-9가-힣一-龥]*$", RegexOptions.None);
                    break;
                default:
                    return false;
            }

            return (string.IsNullOrEmpty(plainText)) ? false : rx.IsMatch(plainText);
        }

    }
}
