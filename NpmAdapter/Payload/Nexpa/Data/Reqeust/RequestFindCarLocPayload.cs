using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;

namespace NpmAdapter.Payload
{
    class RequestFindCarLocPayload : IPayload
    {
        public string car_number { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public void Deserialize(JObject json)
        {
            car_number = Helper.NVL(json["car_no"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
        }

        public byte[] Serialize()
        {
            //ToByteArray할때 Nexpa Encoding을 하면 유도 Web Server에 요청을 못한다. 
            //Default UTF-8 Encoding을 하자.
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["car_no"] = car_number;
            json["dong"] = dong;
            json["ho"] = ho;
            return json;
        }
    }
}
