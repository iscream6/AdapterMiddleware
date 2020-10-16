//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    /// <summary>
    /// 코맥스 대림 방문차량 삭제 요청 데이터
    /// </summary>
    class RequestCmdVisitDelData : IPayload
    {
        public string dongho { get; set; }
        public string reg_num { get; set; }
        public string car_num { get; set; }

        public void Deserialize(JObject json)
        {
            dongho = json["dongho"].ToString();
            reg_num = json["reg_num"].ToString();
            car_num = json["car_num"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dongho"] = dongho;
            json["reg_num"] = reg_num;
            json["car_num"] = car_num;
            return json;
        }
    }
}
