//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseCmdRegNumData : IPayload
    {
        public string reg_num { get; set; }

        public void Deserialize(JObject json)
        {
            reg_num = json["reg_num"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["reg_num"] = reg_num;
            return json;
        }
    }
}
