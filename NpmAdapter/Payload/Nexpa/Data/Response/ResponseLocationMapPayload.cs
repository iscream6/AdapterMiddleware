using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System.Collections.Generic;

namespace NpmAdapter.Payload
{
    class ResponseLocationMapPayload : IPayload
    {
        /// <summary>
        /// 차량번호
        /// </summary>
        public string car_number { get; set; }
        /// <summary>
        /// 별칭
        /// </summary>
        public string alias { get; set; }
        /// <summary>
        /// 위치정보 텍스트
        /// </summary>
        public string location_text { get; set; }
        /// <summary>
        /// 이미지 내 X좌표
        /// </summary>
        public string pixel_x { get; set; }
        /// <summary>
        /// 이미지 내 Y좌표
        /// </summary>
        public string pixel_y { get; set; }
        /// <summary>
        /// yyyy-MM-dd hh:mm:ss
        /// </summary>
        public string in_datetime { get; set; }
        /// <summary>
        /// 차량 이미지
        /// </summary>
        public string car_image { get; set; }

        public void Deserialize(JToken json)
        {
            car_number = json["car_number"].ToString();
            alias = json["alias"].ToString();
            location_text = json["location_text"].ToString();
            pixel_x = json["pixel_x"].ToString();
            pixel_y = json["pixel_y"].ToString();
            in_datetime = json["in_datetime"].ToString();
            car_image = json["car_image"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["alias"] = alias;
            json["location_text"] = location_text;
            json["pixel_x"] = pixel_x;
            json["pixel_y"] = pixel_y;
            json["in_datetime"] = in_datetime;
            json["car_image"] = car_image;
            return json;
        }
    }

    class ResponseLocationListPayload : IPayload
    {
        public string location_type { get; set; }
        public List<ResponseLocationMapPayload> list { get; set; }

        public ResponseLocationListPayload()
        {
            list = new List<ResponseLocationMapPayload>();
        }

        public void Deserialize(JToken json)
        {
            location_type = json.Value<string>("location_type");
            JArray array = json.Value<JArray>("list");
            if (array != null)
            {
                foreach (var item in array)
                {
                    ResponseLocationMapPayload t = new ResponseLocationMapPayload();
                    t.Deserialize(item as JObject);
                    if (list == null) list = new List<ResponseLocationMapPayload>();
                    list.Add(t);
                }
            }
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
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
}
