using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;

namespace NpmAdapter.Payload
{
    class RequestVisitSingleIOPayload : IPayload
    {
        /// <summary>
        /// 동
        /// </summary>
        public string dong { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string ho { get; set; }
        public string car_number { get; set; }

        public void Deserialize(JToken json)
        {
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
            car_number = Helper.NVL(json["car_number"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            json["car_number"] = car_number;
            return json;
        }
    }
}
