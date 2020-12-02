using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseCmdFindCarListData : IPayload
    {
        public string car_num { get; set; }
        public string reg_date { get; set; }
        public string alias { get; set; }

        public void Deserialize(JObject json)
        {
            car_num = Helper.NVL(json["car_num"]);
            reg_date = Helper.NVL(json["reg_date"]);
            alias = Helper.NVL(json["alias"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["car_num"] = car_num;
            json["reg_date"] = reg_date;
            json["alias"] = alias;
            return json;
        }
    }
}
