﻿using NexpaAdapterStandardLib;
using Newtonsoft.Json.Linq;
//using NexpaAdapterStandardLib.Payload;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestModifyAliasPayload : IPayload
    {
        /// <summary>
        /// 동
        /// </summary>
        public string dong { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string ho { get; set; }
        /// <summary>
        /// 차량번호
        /// </summary>
        public string car_number { get; set; }
        /// <summary>
        /// 등록(방문)일 (yyyyMNdd)
        /// </summary>
        public string alias { get; set; }

        public void Deserialize(JObject json)
        {
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
            car_number = json["car_number"].ToString();
            alias = json["alias"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            json["car_number"] = car_number;
            json["alias"] = alias;
            return json;
        }
    }
}
