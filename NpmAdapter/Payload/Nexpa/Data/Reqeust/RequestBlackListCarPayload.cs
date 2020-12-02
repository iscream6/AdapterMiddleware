using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;

namespace NpmAdapter.Payload
{
    class RequestBlackListCarPayload : IPayload
    {
        public string car_number { get; set; }

        public void Deserialize(JObject json)
        {
            car_number = json.NPGetValue(NPElements.Car_Number);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            return json;
        }
    }
}
