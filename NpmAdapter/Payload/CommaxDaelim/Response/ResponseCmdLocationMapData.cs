﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseCmdLocationMapData : IPayload
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
        /// 차량 이미지
        /// </summary>
        public string image { get; set; }
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
        public string datetime { get; set; }
        /// <summary>
        /// 베터리 잔량(미지원시 00)
        /// </summary>
        public string bettery { get; set; }
        /// <summary>
        /// 단지 내 있음(y)/없음(n)
        /// </summary>
        public string in_complex { get; set; }

        public void Deserialize(JObject json)
        {
            car_number = Helper.NVL(json["tag_num"]);
            alias = Helper.NVL(json["alias"]);
            location_text = Helper.NVL(json["location_text"]);
            pixel_x = Helper.NVL(json["pixel_x"]);
            pixel_y = Helper.NVL(json["pixel_y"]);
            datetime = Helper.NVL(json["datetime"]);
            image = Helper.NVL(json["image"]);
            bettery = Helper.NVL(json["bettery"]);
            in_complex = Helper.NVL(json["in_complex"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["tag_num"] = car_number;
            json["alias"] = alias;
            json["location_text"] = location_text;
            json["pixel_x"] = pixel_x;
            json["pixel_y"] = pixel_y;
            json["datetime"] = datetime;
            json["image"] = image;
            json["bettery"] = "00";
            json["in_complex"] = "y";
            return json;
        }
    }
}
