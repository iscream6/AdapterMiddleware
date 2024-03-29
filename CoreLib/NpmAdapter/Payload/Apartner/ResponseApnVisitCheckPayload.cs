﻿using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class ResponseApnVisitCheckPayload : IPayload
    {
        public string check_yon { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public string remark { get; set; }

        public void Deserialize(JToken json)
        {
            check_yon = Helper.NVL(json["check_yon"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            remark = Helper.NVL(json["remark"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["check_yon"] = check_yon;
            json["dong"] = dong;
            json["ho"] = ho;
            json["remark"] = remark;
            return json;
        }
    }
}
