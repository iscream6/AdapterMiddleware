using NexpaAdapterStandardLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseCarListPayload : IPayload
    {
        public string page { get; set; }
        public string list_count { get; set; }
        public string remain_page { get; set; }

        public List<IncarInfoPayload> list { get; set; }

        public void Deserialize(JObject json)
        {
            page = json["page"].ToString();
            list_count = json["list_count"].ToString();
            remain_page = json["remain_page"].ToString();

            JArray array = json["list"] as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    IncarInfoPayload t = new IncarInfoPayload();
                    t.Deserialize(item as JObject);
                    list.Add(t);
                }
            }
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["page"] = page;
            json["count"] = list_count;
            json["next_page"] = remain_page;
            //json["list"] = list.Select(p => p.ToJson()).ToArray();
            JArray array = new JArray();
            foreach (var item in list)
            {
                array.Add(item.ToJson());
            }
            json["list"] = array;
            return json;
        }

    }

    class IncarInfoPayload : IPayload
    {
        public string car_number { get; set; }
        public NexpaPayloadManager.Type type { get; set; }
        public string date_time { get; set; }

        public void Deserialize(JObject json)
        {
            car_number = json["car_number"].ToString();
            type = (NexpaPayloadManager.Type)Enum.Parse(typeof(NexpaPayloadManager.Type), json["type"].ToString());
            date_time = json["date_time"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["type"] = type.ToString();
            json["date_time"] = date_time;
            return json;
        }
    }
}
