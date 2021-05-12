using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestRemainPointPayload : IPayload
    {
        public string point { get; set; }

        public void Deserialize(JObject json)
        {
            point = Helper.NVL(json["point"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["point"] = Helper.NVL(point);
            return json;
        }
    }
}
