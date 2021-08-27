using NexpaAdapterStandardLib;
using Newtonsoft.Json.Linq;
//using NexpaAdapterStandardLib.Payload;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestVisitReg2Payload : IPayload
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
        public string start_date_time { get; set; }
        /// <summary>
        /// 방문일 수(몇일동안 방문할것인지)
        /// </summary>
        public string end_date_time { get; set; }

        public void Deserialize(JToken json)
        {
            if (json == null) return;

            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            car_number = Helper.NVL(json["car_number"]);
            start_date_time = Helper.NVL(json["start_date_time"]);
            end_date_time = Helper.NVL(json["end_date_time"]);
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
            json["start_date_time"] = start_date_time;
            json["end_date_time"] = end_date_time;
            return json;
        }
    }
}
