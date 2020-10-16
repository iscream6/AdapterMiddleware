//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
//using NexpaAdapterStandardLib.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NpmAdapter.Payload
{
    /// <summary>
    /// 코맥스 대림 입차리스트 요청 데이터
    /// </summary>
    class ResponseCmdListData<T> : IPayload where T : IPayload, new()
    {
        public string page { get; set; }
        public string count { get; set; }
        public string next_page { get; set; }
        public List<T> list { get; set; }

        public void Deserialize(JObject json)
        {
            page = json["page"].ToString();
            count = json["count"].ToString();
            next_page = json["next_page"].ToString();

            JArray array = json["list"] as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    T t = new T();
                    t.Deserialize(item as JObject);
                    list.Add(t);
                }
            }
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["page"] = page;
            json["count"] = count;
            json["next_page"] = next_page;
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

    class ResponseCmdIncarListData : IPayload
    {
        public string car_num { get; set; }
        public string car_type { get; set; }
        public string datetime { get; set; }

        public void Deserialize(JObject json)
        {
            car_num = json["car_num"].ToString();
            car_type = json["car_type"].ToString();
            datetime = json["datetime"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["car_num"] = car_num;
            json["car_type"] = car_type;
            json["datetime"] = datetime;
            return json;
        }
    }

    class ResponseCmdVisitListData : IPayload
    {
        public string reg_num { get; set; }
        public string car_num { get; set; }
        public string reg_date { get; set; }
        public string term { get; set; }
        public string status { get; set; }

        public void Deserialize(JObject json)
        {
            reg_num = json["reg_num"].ToString();
            car_num = json["car_num"].ToString();
            reg_date = json["reg_date"].ToString();
            term = json["term"].ToString();
            status = json["status"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["reg_num"] = reg_num;
            json["car_num"] = car_num;
            json["reg_date"] = reg_date;
            json["term"] = term;
            json["status"] = status;
            return json;
        }
    }
}
