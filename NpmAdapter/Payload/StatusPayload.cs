//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    public class StatusPayload : IPayload
    {
        public string code { get; set; }
        public string message { get; set; }

        public void Deserialize(JObject json)
        {
            code = json["status"].ToString();
            message = json["message"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["status"] = code;
            json["message"] = message;
            return json;
        }
    }
}
