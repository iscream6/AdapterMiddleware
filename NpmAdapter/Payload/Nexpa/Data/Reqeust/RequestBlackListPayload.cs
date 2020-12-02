using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestBlackListPayload : IPayload
    {
        public string page { get; set; }
        /// <summary>
        /// 쿼리할 자료 수
        /// </summary>
        public string count { get; set; }

        public void Deserialize(JObject json)
        {
            page = json["page"].ToString();
            count = json["count"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["page"] = page;
            json["count"] = count;
            return json;
        }
    }
}
