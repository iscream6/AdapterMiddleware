using NexpaAdapterStandardLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestVisitFavoRegPayload : IPayload
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
        /// 등록자
        /// </summary>
        public string register { get; set; }

        public void Deserialize(JObject json)
        {
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
            car_number = json["car_number"].ToString();
            register = json["register"].ToString();
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
            json["register"] = register;
            return json;
        }
    }
}
