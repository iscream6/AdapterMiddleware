using Newtonsoft.Json.Linq;
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
            car_num = json["car_num"].ToString();
            reg_date = json["reg_date"].ToString();
            alias = json["alias"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
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
