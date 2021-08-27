using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload.Nexpa.Data.Response
{
    class ResponseAutoBoothPayload : IPayload
    {
        public enum divType
        {
            Booth,
            Lpr,
            Discount
        }

        public List<AutoBoothPayload> boothList { get; set; }
        public List<AutoBoothPayload> lprList { get; set; }
        public List<AutoBoothPayload> dcInfoList { get; set; }

        public ResponseAutoBoothPayload()
        {
            boothList = new List<AutoBoothPayload>();
            lprList = new List<AutoBoothPayload>();
            dcInfoList = new List<AutoBoothPayload>();
        }

        public void Deserialize(JToken json)
        {
            {
                JArray array = json["booth_list"] as JArray;
                if (array != null)
                {
                    foreach (var item in array)
                    {
                        AutoBoothPayload t = new AutoBoothPayload(divType.Booth);
                        t.Deserialize(item as JObject);
                        boothList.Add(t);
                    }
                }
            }

            {
                JArray array = json["lpr_list"] as JArray;
                if (array != null)
                {
                    foreach (var item in array)
                    {
                        AutoBoothPayload t = new AutoBoothPayload(divType.Lpr);
                        t.Deserialize(item as JObject);
                        lprList.Add(t);
                    }
                }
            }

            {
                JArray array = json["dcinfo_list"] as JArray;
                if (array != null)
                {
                    foreach (var item in array)
                    {
                        AutoBoothPayload t = new AutoBoothPayload(divType.Discount);
                        t.Deserialize(item as JObject);
                        dcInfoList.Add(t);
                    }
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

            {
                JArray array = new JArray();
                foreach (var item in boothList)
                {
                    array.Add(item.ToJson());
                }
                json["booth_list"] = array;
            }

            {
                JArray array = new JArray();
                foreach (var item in lprList)
                {
                    array.Add(item.ToJson());
                }
                json["lpr_list"] = array;
            }

            {
                JArray array = new JArray();
                foreach (var item in dcInfoList)
                {
                    array.Add(item.ToJson());
                }
                json["dcinfo_list"] = array;
            }

            return json;
        }

        public static AutoBoothPayload GetSubPayload(divType type)
        {
            return new AutoBoothPayload(type);
        }

        public class AutoBoothPayload : IPayload
        {
            private string divNoDesc = string.Empty;
            private string divNameDesc = string.Empty;

            public AutoBoothPayload(divType type)
            {
                switch (type)
                {
                    case divType.Booth:
                    case divType.Lpr:
                        divNoDesc = "unit_no";
                        divNameDesc = "unit_name";
                        break;
                    case divType.Discount:
                        divNoDesc = "dc_no";
                        divNameDesc = "dc_name";
                        break;
                }
            }

            public string park_no { get; set; }
            public string div_no { get; set; }
            public string div_name { get; set; }

            public void Deserialize(JToken json)
            {
                park_no = Helper.NVL(json["park_no"]);
                div_no = Helper.NVL(json[divNoDesc]);
                div_name = Helper.NVL(json[divNameDesc]);
            }

            public byte[] Serialize()
            {
                return ToJson().ToByteArray();
            }

            public JToken ToJson()
            {
                JObject json = new JObject();
                json["park_no"] = park_no;
                json[divNoDesc] = div_no;
                json[divNameDesc] = div_name;
                return json;
            }
        }
    }

    
}
