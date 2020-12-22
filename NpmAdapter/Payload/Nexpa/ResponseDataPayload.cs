//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;

namespace NpmAdapter.Payload
{
    class ResponseDataPayload : ResponsePayload
    {
        public IPayload data { get; set; }

        public override void Deserialize(JObject json)
        {
            base.Deserialize(json);
            data = NexpaPayloadManager.MakeResponseDataPayload(command, json["data"] as JObject);
        }

        public override byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["data"] = data?.ToJson();
            return json;
        }
    }
}
