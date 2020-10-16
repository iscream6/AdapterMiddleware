//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestPayload<T> : IPayload where T : IPayload, new()
    {
        public CmdType command { get; set; }
        public T data { get; set; }

        public void Deserialize(JObject json)
        {
            command = (CmdType)Enum.Parse(typeof(CmdType), json["command"].ToString());
            var dataJson = json["data"];
            data = new T();
            data.Deserialize(dataJson as JObject);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["command"] = command.ToString();
            json["data"] = data.ToJson();
            return json;
        }
    }
}
