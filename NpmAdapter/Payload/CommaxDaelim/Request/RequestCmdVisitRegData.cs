//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    /// <summary>
    /// 코맥스 대림 방문차량 등록 요청 데이터
    /// </summary>
    class RequestCmdVisitRegData : IPayload
    {
        public string dongho { get; set; }
        public string car_num { get; set; }
        public string reg_date { get; set; }
        public string term { get; set; }

        public void Deserialize(JObject json)
        {
            dongho = json["dongho"].ToString();
            car_num = json["car_num"].ToString();
            reg_date = json["reg_date"].ToString();
            term = json["term"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dongho"] = dongho;
            json["car_num"] = car_num;
            json["reg_date"] = reg_date;
            json["term"] = term;
            return json;
        }
    }
}
