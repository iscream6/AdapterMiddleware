using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;

namespace NpmAdapter.Payload
{
    class AlertInOutCarPayload : IPayload
    {
        /// <summary>
        /// 동
        /// </summary>
        public string dong { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string ho { get; set; }
        /// <summary>
        /// 차량번호
        /// </summary>
        public string car_number { get; set; }
        /// <summary>
        /// yyyyMMddHHmmss
        /// </summary>
        public string date_time { get; set; }
        /// <summary>
        /// 차량 구분 a : 세대원, v 방문객
        /// </summary>
        public string kind { get; set; }
        /// <summary>
        /// LPR 번호
        /// </summary>
        public string lprID { get; set; }
        /// <summary>
        /// 차량 이미지 정보
        /// </summary>
        public string car_image { get; set; }

        public void Deserialize(JObject json)
        {
            if (json == null) return;

            dong = json.NPGetValue(NPElements.Dong);
            ho = json.NPGetValue(NPElements.Ho);
            car_number = json.NPGetValue(NPElements.Car_Number);
            date_time = json.NPGetValue(NPElements.Date_Time);
            kind = json.NPGetValue(NPElements.Kind);
            lprID = json.NPGetValue(NPElements.LprID);
            car_image = json.NPGetValue(NPElements.Car_Image);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            json["car_number"] = car_number;
            json["date_time"] = date_time;
            json["kind"] = kind;
            json["lprid"] = lprID;
            json["car_image"] = car_image;
            return json;
        }
    }
}
