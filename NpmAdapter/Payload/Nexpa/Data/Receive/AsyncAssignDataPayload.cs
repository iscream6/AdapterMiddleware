using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class AsyncAssignDataListPayload : IPayload
    {
        public List<AsyncAssignDataPayload> list { get; set; }

        public AsyncAssignDataListPayload()
        {
            list = new List<AsyncAssignDataPayload>();
        }

        public void Deserialize(JToken json)
        {
            JArray array = json["list"] as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    AsyncAssignDataPayload t = new AsyncAssignDataPayload();
                    t.Deserialize(item as JObject);
                    list.Add(t);
                }
            }
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            JArray array = new JArray();
            foreach (var item in list)
            {
                array.Add(item.ToJson());
            }
            json["list"] = array;
            return json;
        }
    }

    class AsyncAssignDataPayload : IPayload
    {
        public string park_no { get; set; }
        /// <summary>
        /// 동
        /// </summary>
        public string dong { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string ho { get; set; }
        /// <summary>
        /// 포인트 Unit
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 가용 포인트
        /// </summary>
        public string enable_point { get; set; }
        /// <summary>
        /// 사용 포인트
        /// </summary>
        public string used_point { get; set; }
        /// <summary>
        /// 포인트 가용 시작일
        /// </summary>
        public string acp_date { get; set; }
        /// <summary>
        /// 포인트 가용 종료일
        /// </summary>
        public string exp_date { get; set; }

        public void Deserialize(JToken json)
        {
            if (json == null) return;

            park_no = json.NPGetValue(NPElements.Park_No);
            dong = json.NPGetValue(NPElements.Dong);
            ho = json.NPGetValue(NPElements.Ho);
            type = json.NPGetValue(NPElements.Type);
            enable_point = json.NPGetValue(NPElements.Enable_Point);
            used_point = json.NPGetValue(NPElements.Used_Point);
            acp_date = json.NPGetValue(NPElements.Acp_Date);
            exp_date = json.NPGetValue(NPElements.Exp_Date);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["park_no"] = park_no;
            json["dong"] = dong;
            json["ho"] = ho;
            json["type"] = type;
            json["enable_point"] = enable_point;
            json["used_point"] = used_point;
            json["acp_date"] = acp_date;
            json["exp_date"] = exp_date;
            return json;
        }
    }
}
