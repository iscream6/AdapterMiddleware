﻿using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestCustDelPayload : IPayload
    {
        public string car_number { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public string reg_no { get; set; }

        public void Deserialize(JObject json)
        {
            car_number = Helper.NVL(json["car_number"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            reg_no = Helper.NVL(json["reg_no"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["dong"] = dong;
            json["ho"] = ho;
            json["reg_no"] = reg_no;
            return json;
        }
    }
}
