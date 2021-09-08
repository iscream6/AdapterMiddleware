using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestRemainPointPayload : IPayload
    {
        public string point { get; set; }

        public void Deserialize(JToken json)
        {
            point = Helper.NVL(json["point"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["point"] = Helper.NVL(point);
            return json;
        }
    }
}
