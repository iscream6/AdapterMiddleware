using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestBlackListDelPayload : IPayload
    {
        /// <summary>
        /// 차량번호
        /// </summary>
        public string car_number { get; set; }
        /// <summary>
        /// 차량 식별 할 등록 번호
        /// </summary>
        public string reg_no { get; set; }

        public void Deserialize(JObject json)
        {
            car_number = json["car_number"].ToString();
            reg_no = json["reg_no"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["reg_no"] = reg_no;
            return json;
        }
    }
}
