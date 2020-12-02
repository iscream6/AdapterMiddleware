using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NpmAdapter.Payload
{
    class HdnAlertInOutCarPayload : IPayload
    {
        /// <summary>
        /// 8byte 데이터 길이
        /// </summary>
        private UInt64 DataLength = 0;
        /// <summary>
        /// 동
        /// </summary>
        public string dong { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string ho { get; set; }
        /// <summary>
        /// 차량번호
        /// </summary>
        public string car_number { get; set; }
        /// <summary>
        /// yyyyMMddHHmmss
        /// </summary>
        public string date_time { get; set; }
        /// <summary>
        /// 차량 구분 a : 세대원, v 방문객
        /// </summary>
        public string kind { get; set; }
        /// <summary>
        /// IN/OUT
        /// </summary>
        public string type { get; set; }

        public void Deserialize(JObject json)
        {
            dong = json["dong"]?.ToString();
            ho = json["ho"]?.ToString();
            car_number = json["car_number"]?.ToString();
            date_time = json["date_time"]?.ToString();
            kind = json["kind"]?.ToString();
        }

        public byte[] Serialize()
        {
            StringBuilder data = new StringBuilder();
            data.Append($"Type=PARKING");
            data.Append("&");
            data.Append($"Dong={dong}");
            data.Append("&");
            data.Append($"Ho={ho}");
            data.Append("&");
            data.Append($"CarNo={car_number}");
            data.Append("&");
            data.Append($"DateTime={date_time.Substring(0, 12)}");
            data.Append("&");
            data.Append($"InOut={(kind == "a"? "" : "VISIT_") + type}");
            byte[] bData = data.ToString().ToByte(SysConfig.Instance.HomeNet_Encoding);
            DataLength = (UInt64)bData.Length;
            byte[] bLength = BitConverter.GetBytes(DataLength);
            
            return bLength.Concat(bData).ToArray();
        }

        public JObject ToJson()
        {
            throw new NotImplementedException();
        }
    }
}
