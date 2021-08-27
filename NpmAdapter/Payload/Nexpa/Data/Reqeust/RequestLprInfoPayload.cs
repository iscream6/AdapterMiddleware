using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestLprInfoPayload : IPayload
    {
        public string park_no { get; set; }

        public void Deserialize(JToken json)
        {
            park_no = Helper.NVL(json["park_no"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["park_no"] = park_no;
            return json;
        }
    }
}
