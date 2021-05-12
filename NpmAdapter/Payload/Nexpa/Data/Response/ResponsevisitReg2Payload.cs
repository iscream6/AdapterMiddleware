using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponsevisitReg2Payload : IPayload
    {
        public string reg_no { get; set; }
        public void Deserialize(JObject json)
        {
            reg_no = Helper.NVL(json["reg_no"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["reg_no"] = reg_no;
            return json;
        }
    }
}
