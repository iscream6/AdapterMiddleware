using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload.CommaxDaelim.Request
{
    class RequestCmdModifyAliasData : IPayload
    {
        public string dongho { get; set; }
        public string car_num { get; set; }
        public string alias { get; set; }

        public void Deserialize(JObject json)
        {
            dongho = json["dongho"].ToString();
            car_num = json["car_num"].ToString();
            alias = json["alias"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dongho"] = dongho;
            json["car_num"] = car_num;
            json["alias"] = alias;
            return json;
        }
    }
}
