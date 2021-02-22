using NexpaAdapterStandardLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseVisitList2Payload : IPayload
    {
        public List<VisitCarInfo2Payload> list { get; set; }

        public void Deserialize(JObject json)
        {
            JArray array = json["list"] as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    VisitCarInfo2Payload t = new VisitCarInfo2Payload();
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
            JArray array = new JArray();
            foreach (var item in list)
            {
                array.Add(item.ToJson());
            }
            json["list"] = array;
            return json;
        }
    }

    class VisitCarInfo2Payload : IPayload
    {
        public string reg_no { get; set; }
        public string car_number { get; set; }
        public string start_date_time { get; set; }
        public string end_date_time { get; set; }
        public string visit_flag { get; set; }

        public void Deserialize(JObject json)
        {
            reg_no = json["reg_no"].ToString();
            car_number = json["car_number"].ToString();
            start_date_time = json["start_date_time"].ToString();
            end_date_time = json["end_date_time"].ToString();
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
            json["start_date_time"] = start_date_time;
            json["end_date_time"] = end_date_time;
            json["visit_flag"] = visit_flag;
            return json;
        }
    }
}
