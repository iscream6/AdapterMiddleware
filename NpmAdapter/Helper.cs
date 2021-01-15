using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace NpmAdapter
{
    public enum NPElements
    {
        Code,
        Dong,
        Ho,
        Car_Number,
        Date,
        Date_Time,
        Kind,
        Type,
        Page,
        Count,
        Term,
        Reg_No,
        Visit_Flag,
        Register,
        Alias,
        Location_Text,
        Pixel_X,
        Pixel_Y,
        In_DateTime,
        Car_Image,
        Remain_Page,
        Reason,
        LprID,
        Visit_In_Date_Time,
        Visit_Out_Date_Time
    }

    public static class Helper
    {
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string ToString(this byte[] bytes, Encoding encodeing, long size = 0)
        {
            string str = "";
            if (size == 0)
            {
                str = encodeing.GetString(bytes);
            }
            else
            {
                str = encodeing.GetString(bytes[..(int)size]);
            }
            
            return str;
        }

        public static byte[] ToByte(this string str, Encoding encoding)
        {
            byte[] bytes = encoding.GetBytes(str);
            return bytes;
        }

        public static byte CalCheckSum(this byte[] _PacketData, int offset, int size)
        {
            try
            {
                Byte _CheckSumByte = 0x00;
                int latestIdx = offset + size - 1;

                byte[] b = _PacketData[(_PacketData.Length - 1).._PacketData.Length]; 

                for (int i = 0; i < b.Length; i++)
                    _CheckSumByte ^= b[i];
                return _CheckSumByte;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static CmdType GetCommand(this byte[] buffer, Encoding encoding = null)
        {
            try
            {
                JObject jobj;
                if (encoding == null)
                {
                    jobj = JObject.Parse(Encoding.UTF8.GetString(buffer));
                }
                else
                {
                    jobj = JObject.Parse(encoding.GetString(buffer));
                }
                jobj = JObject.Parse(Encoding.UTF8.GetString(buffer));
                string cmd = jobj["command"].ToString();
                return (CmdType)Enum.Parse(typeof(CmdType), cmd);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "Helper | GetCommand", ex.Message);
                return CmdType.none;
            }
        }

        public static byte CalCheckSum(this List<byte> _PacketData, int offset = 0)
        {
            Byte _CheckSumByte = 0x00;
            byte[] b = _PacketData.ToArray();

            for (int i = offset; i < b.Length; i++)
                _CheckSumByte ^= b[i];
            return _CheckSumByte;
        }

        /// <summary>
        /// 4자리수 기준 String 값을 2Byte로 변환한다.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] FourStringTo2Byte(this string str)
        {
            if (str.Length > 4) return null;

            if (str.Length % 2 != 0) str = "0" + str;
            if (str.Length < 4) str = "00" + str;

            byte[] convert = new byte[str.Length / 2];

            int length = convert.Length;
            for (int i = 0; i < length; i++)
            {
                convert[i] = Convert.ToByte(str.Substring(i * 2, 2));
            }

            return convert;
        }

        public static byte[] HexToBytes(this string value)
        {
            if (value == null || value.Length == 0)
                return Array.Empty<byte>();
            if (value.Length % 2 == 1)
                throw new FormatException();
            byte[] result = new byte[value.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.AllowHexSpecifier);
            return result;
        }

        public static string ToHexString(this byte[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in value)
                sb.AppendFormat("{0,3:x2}", b);
            return sb.ToString().ToUpper();
        }

        public static string ToHexString(this byte[] value, bool reverse = false)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
                sb.AppendFormat("{0,3:x2}", value[reverse ? value.Length - i - 1 : i]);
            return sb.ToString().ToUpper();
        }

        public static string ToHexString(this ReadOnlySpan<byte> value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in value)
                sb.AppendFormat("{0,3:x2}", b);
            return sb.ToString().ToUpper();
        }

        public static uint ToTimestamp(this DateTime time)
        {
            return (uint)(time.ToUniversalTime() - unixEpoch).TotalSeconds;
        }

        public static ulong ToTimestampMS(this DateTime time)
        {
            return (ulong)(time.ToUniversalTime() - unixEpoch).TotalMilliseconds;
        }

        public static string uni_to_kr(this string uni)
        {
            StringBuilder writeFileContent = new StringBuilder();
            char[] uniarray = uni.ToCharArray();
            for(int loop=0; loop < uniarray.Length;)
            {
                char c = uniarray[loop];
                char u = '\0';
                if (loop + 1 < uniarray.Length) u = uniarray[loop + 1];
                if(c=='\\' || u == 'u')
                {
                    try
                    {
                        byte unibyte00 = Convert.ToByte("" + uniarray[loop + 4] + uniarray[loop + 5], 16);
                        byte unibyte01 = Convert.ToByte("" + uniarray[loop + 2] + uniarray[loop + 3], 16);
                        writeFileContent.Append(UnicodeEncoding.Unicode.GetString(new byte[] { unibyte00, unibyte01 }));
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                    finally
                    {
                        loop += "\\uaaaa".Length;
                    }
                }
                else
                {
                    writeFileContent.Append(c);
                    loop++;
                }
            }

            return writeFileContent.ToString();
        }

        public static string AsString(this XmlDocument xmlDoc)
        {
            using (StringWriter sw = new StringWriter())
            {
                using (XmlTextWriter tx = new XmlTextWriter(sw))
                {
                    xmlDoc.WriteTo(tx);
                    string strXmlText = sw.ToString();
                    return strXmlText;
                }
            }
        }

        public static string ConvertDateTimeFormat(this string dateTime, string from, string to)
        {
            if(dateTime == null || dateTime == "")
            {
                return "";
            }
            else
            {
                return DateTime.ParseExact(dateTime, from, null).ToString(to);
            }
        }

        public static byte[] ToByteArray(this JObject json, Encoding encoding = null)
        {
            if(encoding == null)
            {
                return Encoding.UTF8.GetBytes(json.ToString());
            }
            else
            {
                return encoding.GetBytes(json.ToString());
            }
        }

        public static string GetValueToUpper(this Dictionary<string, string> dic, string key)
        {
            if (dic.ContainsKey(key)) return dic[key].ToUpper();
            else return "";
        }

        public static string GetValue(this Dictionary<string, string> dic, string key)
        {
            if (dic.ContainsKey(key)) return dic[key];
            else return "";
        }

        public static Dictionary<string, string> DoubleSplit(this string str, char BigSep, char SmallSep)
        {
            Dictionary<string, string> dicResult = new Dictionary<string, string>();

            try
            {
                string[] bigSplit = str.Split(BigSep);
                foreach (var item in bigSplit)
                {
                    string[] smallSplit = item.Split(SmallSep);
                    dicResult.Add(smallSplit[0].ToUpper(), smallSplit[1]);
                }
                return dicResult;
            }
            catch (Exception)
            {
                return dicResult;
            }
        }

        public static string NVL(JObject jObject, string defaultValue = "")
        {
            if (jObject == null) return defaultValue;
            else
            {
                return jObject.ToString();
            }
        }

        public static string NVL(JToken jToken, string defaultValue = "")
        {
            if (jToken == null) return defaultValue;
            else
            {
                return jToken.ToString();
            }
        }

        public static string NVL(string str, string defaultValue = "")
        {
            if (str == null) return defaultValue;
            else return str;
        }

        public static string NPGetValue(this JObject jObject, NPElements elements, string defaultValue = "")
        {
            var key = elements.ToString().ToLower();
            return NVL(jObject[key], defaultValue);
        }

        public static string NPGetValue(this JToken jToken, NPElements elements, string defaultValue = "")
        {
            var key = elements.ToString().ToLower();
            return NVL(jToken[key], defaultValue);
        }

        public static string Base64Encode(this string data)
        {
            try
            {
                byte[] encData_byte = new byte[data.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
                string encodedData = Convert.ToBase64String(encData_byte);
                return encodedData;
            }
            catch (Exception e)
            {
                throw new Exception("Error in Base64Encode: " + e.Message);
            }

        }
    }
}
