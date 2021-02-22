﻿using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;

namespace NpmAdapter.Payload
{
    class RequestVisitList2Payload : IPayload
    {
        public string car_number { get; set; }
        /// <summary>
        /// 동
        /// </summary>
        public string dong { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string ho { get; set; }

        public void Deserialize(JObject json)
        {
            car_number = json["car_number"].ToString();
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
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
            return json;
        }
    }
}
