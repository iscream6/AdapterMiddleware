//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NpmAdapter.Payload
{
    class ResponseCmdVisitFavoritListData : IPayload
    {
        public List<CommaxDaelimVisitFavoritResponseData> list { get; set; }

        public void Deserialize(JObject json)
        {
            JArray array = json["list"] as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    CommaxDaelimVisitFavoritResponseData t = new CommaxDaelimVisitFavoritResponseData();
                    t.Deserialize(item as JObject);
                    if (list == null) list = new List<CommaxDaelimVisitFavoritResponseData>();
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

    class CommaxDaelimVisitFavoritResponseData : IPayload
    {
        public string reg_num { get; set; }
        public string car_num { get; set; }
        public string reg_date { get; set; }
        public string register { get; set; }

        public void Deserialize(JObject json)
        {
            reg_num = json["reg_num"].ToString();
            car_num = json["car_num"].ToString();
            reg_date = json["reg_date"].ToString();
            register = json["register"].ToString();
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
            json["register"] = register;
            return json;
        }
    }
}
