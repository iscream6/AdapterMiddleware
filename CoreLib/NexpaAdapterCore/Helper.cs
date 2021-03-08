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
    public enum EncoderType
    {
        [Description("D")]
        Default,
        [Description("A")]
        ASCII,
        [Description("U")]
        UNICODE,
        [Description("7")]
        UTF7,
        [Description("8")]
        UTF8,
        [Description("32")]
        UTF32,
        [Description("kr")]
        EUC_KR,
        [Description("ksc_5601")]
        KSC_5601
    }

    public enum NexpaAdapterType
    {
        [Description("3")]
        All,
        [Description("1")]
        Tcp_Only,
        [Description("2")]
        Web_Only,
        [Description("4")]
        AutoBooth,
        [Description("TCP")]
        Tcp,
        [Description("WEB")]
        Web,
        [Description("AB")]
        NPAutoBooth
    }

    public enum HomeNetAdapterType
    {
        [Description("0")]
        None,
        [Description("1")]
        SHT5800,
        [Description("2")]
        Commax_All,
        [Description("2-1")]
        Commax_Tcp,
        [Description("2-2")]
        Commax_Web,
        [Description("3")]
        Commax_Only,
        [Description("CCM")]
        Cocom,
        [Description("APS")]
        ApartStory,
        [Description("SMTV")]
        SmartVillage,
        [Description("SML")]
        Samul,
        [Description("KKM")]
        KakaoMovil,
        [Description("EZV")]
        ezVille,
        [Description("USN")]
        Ulsan,
        [Description("UJA")]
        UjungAir,
    }

    public static class StdHelper
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

        public static string GetDescription<T>(this T t) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            DescriptionAttribute[] attributes = (DescriptionAttribute[])t
           .GetType()
           .GetField(t.ToString())
           .GetCustomAttributes(typeof(DescriptionAttribute), false);

            string message = attributes.Length > 0 ? attributes[0].Description : string.Empty;

            return message;
        }

        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }
            
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            return default(T);
        }
    }
}
