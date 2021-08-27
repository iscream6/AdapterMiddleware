using NexpaAdapterStandardLib;
using Newtonsoft.Json.Linq;
//using NexpaAdapterStandardLib.Payload;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestVisitRegPayload : IPayload
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
        public string date { get; set; }
        /// <summary>
        /// 방문일 수(몇일동안 방문할것인지)
        /// </summary>
        public string term { get; set; }
        public string remark { get; set; }

        public void Deserialize(JToken json)
        {
            if (json == null) return;

            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            car_number = Helper.NVL(json["car_number"]);
            date = Helper.NVL(json["date"]);
            term = Helper.NVL(json["term"]);
            remark = Helper.NVL(json["remark"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            json["car_number"] = car_number;
            json["date"] = date;
            json["term"] = term;
            json["remark"] = remark;
            return json;
        }
    }
}
