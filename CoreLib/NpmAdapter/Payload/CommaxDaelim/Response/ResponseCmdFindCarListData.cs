﻿using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class ResponseCmdFindCarListData : IPayload
    {
        public string car_num { get; set; }
        public string reg_date { get; set; }
        public string alias { get; set; }

        public void Deserialize(JToken json)
        {
            car_num = Helper.NVL(json["car_num"]);
            reg_date = Helper.NVL(json["reg_date"]);
            alias = Helper.NVL(json["alias"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["car_num"] = Helper.NVL(car_num);
            json["reg_date"] = Helper.NVL(reg_date);
            json["alias"] = Helper.NVL(alias);
            return json;
        }
    }
}
