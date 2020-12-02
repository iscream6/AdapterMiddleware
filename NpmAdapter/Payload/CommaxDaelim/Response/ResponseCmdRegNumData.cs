//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
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
            reg_num = Helper.NVL(json["reg_num"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["reg_num"] = reg_num;
            return json;
        }
    }
}
