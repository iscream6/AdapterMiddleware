using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System.Dynamic;

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
        /// 입차/출차 일시(yyyyMMddHHmmss)
        /// </summary>
        public string date_time { get; set; }
        /// <summary>
        /// 차량 구분 a : 세대원, v 방문객, n : 일반
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
        /// <summary>
        /// 차량 ID
        /// </summary>
        public string reg_no { get; set; }
        /// <summary>
        /// 방문시작일시 (kind가 v일 경우 외 빈값)
        /// </summary>
        public string visit_in_date_time { get; set; }
        /// <summary>
        /// 방문종료일시 (kind가 v일 경우 외 빈값)
        /// </summary>
        public string visit_out_date_time { get; set; }
        /// <summary>
        /// 소유자명
        /// </summary>
        public string owner_name { get; set; }
        /// <summary>
        /// 그룹명
        /// </summary>
        public string group_name { get; set; }
        /// <summary>
        /// 부서명
        /// </summary>
        public string dept_name { get; set; }


        public void Deserialize(JToken json)
        {
            if (json == null) return;

            dong = json.NPGetValue(NPElements.Dong);
            ho = json.NPGetValue(NPElements.Ho);
            car_number = json.NPGetValue(NPElements.Car_Number);
            date_time = json.NPGetValue(NPElements.Date_Time);
            kind = json.NPGetValue(NPElements.Kind);
            lprID = json.NPGetValue(NPElements.LprID);
            car_image = json.NPGetValue(NPElements.Car_Image);
            reg_no = json.NPGetValue(NPElements.Reg_No);
            visit_in_date_time = json.NPGetValue(NPElements.Visit_In_Date_Time);
            visit_out_date_time = json.NPGetValue(NPElements.Visit_Out_Date_Time);
            owner_name = json.NPGetValue(NPElements.Owner_Name);
            group_name = json.NPGetValue(NPElements.Group_Name);
            dept_name = json.NPGetValue(NPElements.Dept_Name);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            json["car_number"] = car_number;
            json["date_time"] = date_time;
            json["kind"] = kind;
            json["lprid"] = lprID;
            json["car_image"] = car_image;
            json["reg_no"] = reg_no;
            json["visit_in_date_time"] = visit_in_date_time;
            json["visit_out_date_time"] = visit_out_date_time;
            json["owner_name"] = owner_name;
            json["group_name"] = group_name;
            json["dept_name"] = dept_name;
            return json;
        }
    }
}
