using NexpaAdapterStandardLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseVisitListPayload : IPayload
    {
        public string page { get; set; }
        public string list_count { get; set; }
        public string remain_page { get; set; }

        public List<VisitCarInfoPayload> list { get; set; }

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
                    VisitCarInfoPayload t = new VisitCarInfoPayload();
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

    class VisitCarInfoPayload : IPayload
    {
        public string reg_no { get; set; }
        public string car_number { get; set; }
        public string date { get; set; }
        public string term { get; set; }
        public string visit_flag { get; set; }

        public void Deserialize(JObject json)
        {
            reg_no = json["reg_no"].ToString();
            car_number = json["car_number"].ToString();
            date = json["date"].ToString();
            term = json["term"].ToString();
            visit_flag = json["visit_flag"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["reg_no"] = reg_no;
            json["car_number"] = car_number;
            json["date"] = date;
            json["term"] = term;
            json["visit_flag"] = visit_flag;
            return json;
        }
    }
}
