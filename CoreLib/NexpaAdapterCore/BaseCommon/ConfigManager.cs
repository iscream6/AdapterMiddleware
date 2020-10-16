using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace NexpaAdapterStandardLib
{
    class ConfigManager
    {
        /// <summary>
        /// C의 dll함수 마샬링
        /// </summary>
        /// <param name="section">sectin명</param>
        /// <param name="key">키명</param>
        /// <param name="val">값</param>
        /// <param name="filePath">파일 위치</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// C의 dll함수 마샬링
        /// </summary>
        /// <param name="section">section명</param>
        /// <param name="key">키명</param>
        /// <param name="def">디폴트 값(값이 없을 때 나오는 값)</param>
        /// <param name="retVal">string pointer</param>
        /// <param name="size">크기</param>
        /// <param name="filePath">파일 위치</param>
        /// <returns></returns>
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// ini 파일에 값을 작성하는 함수
        /// </summary>
        /// <param name="file"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public static void WriteConfig(string file, string section, string key, string val)
        {
            WritePrivateProfileString(section, key, val, GetFile(file));
        }

        /// <summary>
        /// ini 파일에 값을 가져오는 함수
        /// </summary>
        /// <param name="file"></param>
        /// <param name="section"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string ReadConfig(string file, string section, string key)
        {
            //C#에서는 포인터를 명시적으로 표현할 수 없기 때문 StringBuilder로 가져옵니다
            StringBuilder temp = new StringBuilder(255);
            int ret = GetPrivateProfileString(section, key, null, temp, 255, GetFile(file));
            //주석처리 된 부분은 빼고 가져오자.
            string[] resultValues = temp.ToString().Split(';');
            if(resultValues.Length == 0)
            {
                return string.Empty;
            }
            else
            {
                //주석은 뒤에 달려 있는것이 원칙...
                string result = resultValues[0].Trim();
                return result;
            }
        }

        private static string GetFile(string file)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file + ".ini");
        }
    }
}
