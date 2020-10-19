using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestCarInfoPayload : IPayload
    {
        public string dong { get; set; }

        public string ho { get; set; }

        public void Deserialize(JObject json)
        {
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            return json;
        }
    }
}
