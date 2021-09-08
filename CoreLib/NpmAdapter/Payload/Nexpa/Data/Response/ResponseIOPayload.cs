using Newtonsoft.Json.Linq;
using NpmCommon;
using System.Collections.Generic;

namespace NpmAdapter.Payload
{
    class ResponseIOListPayload : IPayload
    {
        public List<ResponseIOPayload> list { get; set; }

        public ResponseIOListPayload()
        {
            list = new List<ResponseIOPayload>();
        }

        public void Deserialize(JToken json)
        {
            JArray array = json["list"] as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    ResponseIOPayload t = new ResponseIOPayload();
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

    class ResponseIOPayload : IPayload
    {
        public string park_no { get; set; }
        public string reg_no { get; set; }
        public string car_nubmer { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public string in_date_time { get; set; }
        public string out_date_time { get; set; }

        public void Deserialize(JToken json)
        {
            park_no = Helper.NVL(json["park_no"]);
            reg_no = Helper.NVL(json["reg_no"]);
            car_nubmer = Helper.NVL(json["car_nubmer"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            in_date_time = Helper.NVL(json["in_date_time"]);
            out_date_time = Helper.NVL(json["out_date_time"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["park_no"] = park_no;
            json["reg_no"] = reg_no;
            json["car_number"] = car_nubmer;
            json["dong"] = dong;
            json["ho"] = ho;
            json["in_date_time"] = in_date_time;
            json["out_date_time"] = out_date_time;
            return json;
        }
    }
}
