﻿using Newtonsoft.Json.Linq;
using NpmCommon;
using System.Collections.Generic;

namespace NpmAdapter.Payload.CommaxDaelim.Response
{
    class ResponseCmdLocationListData : IPayload
    {
        public string location_type { get; set; }
        public List<SubLocationListData> list { get; set; }

        public ResponseCmdLocationListData()
        {
            list = new List<SubLocationListData>();
        }

        public void Deserialize(JToken json)
        {
            location_type = json.Value<string>("location_type");
            JArray array = json.Value<JArray>("list");
            if (array != null)
            {
                foreach (var item in array)
                {
                    SubLocationListData t = new SubLocationListData();
                    t.Deserialize(item as JObject);
                    if (list == null) list = new List<SubLocationListData>();
                    list.Add(t);
                }
            }
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["location_type"] = location_type;
            JArray array = new JArray();
            foreach (var item in list)
            {
                array.Add(item.ToJson());
            }
            json["list"] = array;
            return json;
        }
    }

    class SubLocationListData : IPayload
    {
        public string car_number { get; set; }
        public string alias { get; set; }
        public string battery { get; set; }
        public string in_complex { get; set;}
        public string location_text { get; set; }
        public string datetime { get; set; }
        public void Deserialize(JToken json)
        {
            car_number = json.Value<string>("tag_num");
            alias = json.Value<string>("alias");
            battery = json.Value<string>("battery");
            in_complex = json.Value<string>("in_complex");
            location_text = json.Value<string>("location_text");
            datetime = json.Value<string>("datetime");
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["tag_num"] = Helper.NVL(car_number);
            json["alias"] = Helper.NVL(alias);
            json["bettery"] = "00";
            json["im_complex"] = "y";
            json["location_text"] = Helper.NVL(location_text);
            json["datetime"] = Helper.NVL(datetime);
            return json;
        }
    }
}
