using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestBlackListRegPayload : IPayload
    {
        /// <summary>
        /// 차량번호
        /// </summary>
        public string car_number { get; set; }
        /// <summary>
        /// 사유
        /// </summary>
        public string reason { get; set; }

        public void Deserialize(JToken json)
        {
            car_number = json["car_number"].ToString();
            reason = json["reason"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["reason"] = reason;
            return json;
        }
    }
}
