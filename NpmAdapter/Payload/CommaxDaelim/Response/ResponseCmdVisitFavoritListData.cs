﻿//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
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
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            //json["list"] = list.Select(p => p.ToJson()).ToArray();
            if (list != null && list.Count > 0)
            {
                JArray array = new JArray();
                foreach (var item in list)
                {
                    array.Add(item.ToJson());
                }
                json["list"] = array;
            }
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
            reg_num = Helper.NVL(json["reg_num"]);
            car_num = Helper.NVL(json["car_num"]);
            reg_date = Helper.NVL(json["reg_date"]);
            register = Helper.NVL(json["register"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
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
