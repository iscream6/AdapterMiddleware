using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseCmdRemainPointData : IPayload
    {
        public string point { get; set; }

        public void Deserialize(JToken json)
        {
            point = Helper.NVL(json["point"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["point"] = Helper.NVL(point);
            return json;
        }
    }
}
