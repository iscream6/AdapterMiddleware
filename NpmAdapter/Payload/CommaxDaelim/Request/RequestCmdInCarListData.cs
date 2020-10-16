//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
//using NexpaAdapterStandardLib.Network.Payload;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    /// <summary>
    /// 코맥스 대림 입차리스트 요청 데이터
    /// </summary>
    class RequestCmdInCarListData : IPayload
    {
        public string dongho { get; set; }
        public CmdPayloadManager.CarType car_type { get; set; }
        public string page { get; set; }
        public string count { get; set; }

        public void Deserialize(JObject json)
        {
            dongho = json["dongho"].ToString();
            car_type = (CmdPayloadManager.CarType)Enum.Parse(typeof(CmdPayloadManager.CarType), json["car_type"].ToString());
            page = json["page"].ToString();
            count = json["count"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dongho"] = dongho;
            json["car_type"] = car_type.ToString();
            json["page"] = page;
            json["count"] = count;
            return json;
        }
    }
}
