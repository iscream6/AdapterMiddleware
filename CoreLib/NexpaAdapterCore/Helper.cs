using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace NexpaAdapterStandardLib
{
    public static class Helper
    {
        private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static string ToString(this byte[] bytes, Encoding encodeing)
        {
            string str = encodeing.GetString(bytes); 
            return str;
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
    }
}
