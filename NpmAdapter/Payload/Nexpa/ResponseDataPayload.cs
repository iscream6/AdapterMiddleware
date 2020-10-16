//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;

namespace NpmAdapter.Payload
{
    class ResponseDataPayload : IPayload
    {
        public CmdType command { get; set; }

        public IPayload data { get; set; }

        public StatusPayload result { get; set; }

        public void Deserialize(JObject json)
        {
            command = (CmdType)Enum.Parse(typeof(CmdType), json["command"].ToString());
            data = NexpaPayloadManager.MakeResponseDataPayload(command, json["data"] as JObject);
            result.Deserialize(json["result"] as JObject);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["command"] = command.ToString();
            json["data"] = data?.ToJson();
            json["result"] = result.ToJson();
            return json;
        }
    }
}
