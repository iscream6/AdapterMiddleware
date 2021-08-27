using NexpaAdapterStandardLib;
using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    class RequestVisitDel2Payload : IPayload
    {
        public string dong { get; set; }
        public string ho { get; set; }
        /// <summary>
        /// 차량번호
        /// </summary>
        public string car_number { get; set; }

        public void Deserialize(JToken json)
        {
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
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
