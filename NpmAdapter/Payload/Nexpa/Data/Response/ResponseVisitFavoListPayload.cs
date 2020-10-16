using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System.Collections.Generic;

namespace NpmAdapter.Payload
{
    class ResponseVisitFavoListPayload : IPayload
    {
        public List<VisitFavoInfoPayload> list { get; set; }

        public void Deserialize(JObject json)
        {
            JArray array = json["list"] as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    VisitFavoInfoPayload t = new VisitFavoInfoPayload();
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

    class VisitFavoInfoPayload : IPayload
    {
        public string reg_no { get; set; }
        public string car_number { get; set; }
        public string date_time { get; set; }
        public string register { get; set; }

        public void Deserialize(JObject json)
        {
            reg_no = json["reg_no"].ToString();
            car_number = json["car_number"].ToString();
            date_time = json["date_time"].ToString();
            register = json["register"].ToString();
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
            json["date_time"] = date_time;
            json["register"] = register;
            return json;
        }
    }
}
