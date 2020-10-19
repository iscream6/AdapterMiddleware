using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;

namespace NpmAdapter.Payload
{
    class AlertInOutCarPayload : IPayload
    {
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
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            json["car_number"] = car_number;
            json["date_time"] = date_time;
            json["kind"] = kind;
            return json;
        }
    }
}
