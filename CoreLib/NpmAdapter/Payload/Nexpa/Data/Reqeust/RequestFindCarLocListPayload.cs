using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    class RequestFindCarLocListPayload : IPayload
    {
        public string location_type { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public void Deserialize(JToken json)
        {
            location_type = json["location_type"].ToString();
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
        }

        public byte[] Serialize()
        {
            //ToByteArray할때 Nexpa Encoding을 하면 유도 Web Server에 요청을 못한다. 
            //Default UTF-8 Encoding을 하자.
            return ToJson().ToByteArray();
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["location_type"] = location_type;
            json["dong"] = dong;
            json["ho"] = ho;
            return json;
        }
    }
}
