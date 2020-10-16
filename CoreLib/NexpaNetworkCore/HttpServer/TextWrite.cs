
using System;
using System.Collections.Generic;
using System.IO;

using System.Text;

namespace HttpServer
{
    public class TextWrite
    {
        private static string logPath = @"c:\" + DateTime.Now.ToString("yyyyMMdd").Substring(0, 4) + "\\" + DateTime.Now.ToString("yyyyMMdd").Substring(4, 2) + "\\";
        private static string MakeLogDocument(string p_Log_Data)
        {
            string nowdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            string returnmessage = "";
            returnmessage = "[" + nowdate + "]" + " " + p_Log_Data;
            return returnmessage;
        }
        public static Boolean getWriteLogData(string p_log_data)
        {


            try
            {

                StreamWriter sw = new StreamWriter(@"c:\temp\" + "Http.txt", true);
                sw.WriteLine(MakeLogDocument(p_log_data));
                sw.Close();
                sw = null;
                return true;
            }
            catch (Exception ex)
            {


                return false;
            }
            finally
            {

            }
        }
    }
}
