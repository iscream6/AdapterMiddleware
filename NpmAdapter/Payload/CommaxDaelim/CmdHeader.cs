//using NexpaAdapterStandardLib.IO.Json;
//using NexpaAdapterStandardLib.Network.Payload;
//using NexpaAdapterStandardLib.Payload;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    /// <summary>
    /// 코맥스 대림 헤더
    /// </summary>
    class CmdHeader : IPayload
    {
        public string category { get; set; }

        public CmdPayloadManager.Type type { get; set; }

        public CmdPayloadManager.Command command { get; set; }

        public void Deserialize(JObject json)
        {
            category = json["category"].ToString();
            type = (CmdPayloadManager.Type)Enum.Parse(typeof(CmdPayloadManager.Type), json["type"].ToString()); 
            command = (CmdPayloadManager.Command)Enum.Parse(typeof(CmdPayloadManager.Command), json["command"].ToString());
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["category"] = category;
            json["type"] = type.ToString();
            json["command"] = command.ToString();
            return json;
        }
    }
}
