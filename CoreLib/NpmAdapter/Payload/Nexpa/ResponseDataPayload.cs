using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class ResponseDataPayload : ResponsePayload
    {
        public IPayload data { get; set; }

        public override void Deserialize(JToken json)
        {
            base.Deserialize(json);
            data = NexpaPayloadManager.MakeResponseDataPayload(command, json["data"] as JObject);
        }

        public override byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public override JToken ToJson()
        {
            JToken json = base.ToJson();
            json["data"] = data?.ToJson();
            return json;
        }
    }
}
