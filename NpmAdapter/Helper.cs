using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
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
        Visit_Out_Date_Time,
        Point,
        Park_No,
        Enable_Point,
        Used_Point,
        Acp_Date,
        Exp_Date
    }

    public enum TimeType
    {
        Hour,
        Minute,
        Second,
        Millisecond,
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

        public static int GetByteLength(this string STR)
        {
            char[] charobj = STR.ToCharArray();
            int maxLEN = 0;

            for (int i = 0; i < charobj.Length; i++)
            {
                byte oF = (byte)((charobj[i] & 0xff00) >> 7);
                byte oB = (byte)(charobj[i] & 0x00ff);

                if (oF == 0)
                    maxLEN++;
                else
                    maxLEN += 2;
            }

            return maxLEN;
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

        public static byte[] StringToAsciiByte(this string str)
        {
            char[] charArr = str.ToCharArray();
            return ASCIIEncoding.Default.GetBytes(charArr);
        }

        public static byte[] FourStringTo4ByteAscii(this string str)
        {
            if (str.Length > 4) return null;
            else if (str.Length == 1) str = "000" + str;
            else if (str.Length == 2) str = "00" + str;
            else if (str.Length == 3) str = "0" + str;

            char[] charArr = str.ToCharArray();
            return ASCIIEncoding.Default.GetBytes(charArr);
        }

        /// <summary>
        /// 4자리수 기준 String 값을 4Byte로 변환한다.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static byte[] FourStringTo4Byte(this string str)
        {
            if (str.Length > 4) return null;
            else if (str.Length == 1) str = "000" + str;
            else if (str.Length == 2) str = "00" + str;
            else if (str.Length == 3) str = "0" + str;

            byte[] convert = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                convert[i] = Convert.ToByte(str.Substring(i, 1));
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
                try
                {
                    if(from == "ISO8601")
                    {
                        DateTime dt;
                        if(DateTime.TryParse(dateTime, out dt))
                        {
                            return dt.ToString(to);
                        }
                        else
                        {
                            return "";
                        }
                    }
                    else if(to == "ISO8601")
                    {
                        return DateTime.ParseExact(dateTime, from, null).ToString("yyyy-MM-ddTHH:mm:ss");
                    }
                    else
                    {
                        return DateTime.ParseExact(dateTime, from, null).ToString(to);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "Helper | ConvertDateTimeFormat", $"{dateTime} : {ex.Message}");
                    return "";
                }
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

        public static byte[] ConvertHexStringToByte(this string convertString)
        {
            try
            {
                byte[] convertArr = new byte[convertString.Length / 2];

                for (int i = 0; i < convertArr.Length; i++)
                {
                    convertArr[i] = Convert.ToByte(convertString.Substring(i * 2, 2), 16);
                }
                return convertArr;
            }
            catch (Exception e)
            {
                throw new Exception("Error in Base64Encode: " + e.Message);
            }
        }

        public static ResultPayload GetResultPayload(this JObject obj)
        {
            //결과 Payload 생성 =======
            ResultPayload resultPayload = null;

            if (obj != null && Helper.NVL(obj["status"]) != "200")
            {
                resultPayload = new ResultPayload();
                string sCode = "";

                if (Helper.NVL(obj["status"]) == "204") sCode = "404";
                else sCode = Helper.NVL(obj["status"]);

                resultPayload.code = sCode;
                resultPayload.message = Helper.NVL(obj["message"]);
            }
            else
            {
                resultPayload = new ResultPayload();
                resultPayload.code = "200";
                resultPayload.message = "oK";
            }
            //결과 Payload 생성완료 =======

            return resultPayload;
        }

        public static string GetLocalIP()
        {
            string localIP = "Not available, please check your network settings!";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if(ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    Log.WriteLog(LogType.Error, "Helper | GetLocalIP", localIP);
                    break;
                }
            }

            return localIP;
        }

        public static List<string> GetLocalIPs()
        {
            List<string> localIPs = new List<string>();
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    localIPs.Add(ip.ToString());
                    Log.WriteLog(LogType.Error, "Helper | GetLocalIP", ip.ToString());
                }
            }

            return localIPs;
        }

        public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.ContainsKey(key) ? dictionary[key] : default(TValue);
        }

        public static IEnumerable<TValue> TryGetValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys)
        {
            TValue value;
            foreach (TKey key in keys)
                if (dictionary.TryGetValue(key, out value))
                    yield return value;
        }

        public static IEnumerable<TValue> TryGetValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys, Action<TKey> notFoundHandler)
        {
            TValue value;
            foreach (TKey key in keys)
                if (dictionary.TryGetValue(key, out value))
                    yield return value;
                else
                    notFoundHandler(key);
        }

        public static long GetUTCMillisecond(string strTime)
        {
            try
            {
                DateTime date = DateTime.ParseExact(strTime, "yyyyMMddHHmmss", null);
                var utc = (long)date.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMinutes;
                return utc;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "Helper | GetUTCMillisecond", ex.Message);
                return 0;
            }
        }

        public static XmlDocument MakeXmlDeclareDocument(string ver, string encoding)
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDecl;
            xmlDecl = doc.CreateXmlDeclaration(ver, null, null);
            xmlDecl.Encoding = encoding;

            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDecl, root);

            return doc;
        }
        public static byte[] ConvertToLittleEndian(int value)
        {
            return new byte[]{
                (byte) value,
                (byte) (value >> 8),
                (byte) (value >> 16),
                (byte) (value >> 24)
        };
        }

        public static byte[] ConvertToBigEndian(int value)
        {
            return new byte[]{
                (byte) (value >> 24),
                (byte) (value >> 16),
                (byte) (value >> 8),
                (byte) value
        };
        }

        public static string ByteToStr(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(string.Format("%02x ", b));
            }

            return builder.ToString();
        }

        public static int BytesToInt(byte[] bytes)
        {
            int result = (int)bytes[3] & 0xFF;
            result |= (int)bytes[2] << 8 & 0xFF00;
            result |= (int)bytes[1] << 16 & 0xFF0000;
            result |= (int)bytes[0] << 24;

            return result;
        }

        public static string ValidateJsonParseingData(string strJson)
        {
            char[] cArr = strJson.ToCharArray();
            if (cArr != null && cArr.Length > 0)
            {
                int iBracket = 0;
                int iCharCnt = 0;
                foreach (var c in cArr)
                {
                    if (c.Equals('{')) iBracket++;
                    else if (c.Equals('}')) iBracket--;
                    iCharCnt += 1;

                    if (iBracket == 0 && iCharCnt > 1)
                    {
                        break;
                    }
                }

                return strJson.Substring(0, iCharCnt);
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// 문자열 안에서 몇번째 Char가 발견되는 Index를 반환한다.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="value">찾을 Char 값</param>
        /// <param name="count">찾은 Char의 갯수</param>
        /// <returns></returns>
        public static int IndexOfChar(this string str, char value, int count)
        {
            int startIdx = 0;
            int charCnt = 0;
            foreach (var c in str.ToCharArray())
            {
                startIdx += 1;
                if (c == value)
                {
                    charCnt += 1;
                }
                if (charCnt == count) break;
            }

            return startIdx;
        }
    }
}
