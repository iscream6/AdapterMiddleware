using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestListPayload : IPayload
    {
        public enum SearchType
        {
            /// <summary>
            /// All(전체)
            /// </summary>
            all,
            /// <summary>
            /// 입차 가능 시작일시
            /// </summary>
            cust,
            /// <summary>
            /// 입차 가능 종료일시
            /// </summary>
            visit
        }

        public enum FilterType
        {
            /// <summary>
            /// All(전체)
            /// </summary>
            all,
            /// <summary>
            /// 입차 가능 시작일시
            /// </summary>
            start,
            /// <summary>
            /// 입차 가능 종료일시
            /// </summary>
            end
        }
        /// <summary>
        /// 조회 타입
        /// </summary>
        public SearchType search_type { get; set; }
        /// <summary>
        /// 주차장번호
        /// </summary>
        public string park_no { get; set; }
        /// <summary>
        /// 입차가능 Start/End 구분
        /// </summary>
        public FilterType filter_type { get; set; }
        /// <summary>
        /// 조회 시작일
        /// </summary>
        public string start_date { get; set; }
        /// <summary>
        /// 조회 종료일
        /// </summary>
        public string end_date { get; set; }
        /// <summary>
        /// LPR 번호
        /// </summary>
        public string lprID { get; set; }
        /// <summary>
        /// 차량번호
        /// </summary>
        public string car_number { get; set; }
        /// <summary>
        /// 전화번호
        /// </summary>
        public string tel_number { get; set; }
        /// <summary>
        /// 동
        /// </summary>
        public string dong { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string ho { get; set; }
        /// <summary>
        /// 쿼리 할 자료 총 합계를 count로 나눈 총 page 수 값 중 몇번째 page를 가져올지의 값, 0일경우 전체가져오기
        /// </summary>
        public string page { get; set; }
        /// <summary>
        /// 쿼리할 자료 수
        /// </summary>
        public string count { get; set; }
        public string reg_no { get; set; }

        public void Deserialize(JToken json)
        {
            
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["search_type"] = search_type.ToString();
            json["park_no"] = park_no;
            json["filter_type"] = filter_type.ToString();
            json["start_date"] = start_date;
            json["end_date"] = end_date;
            json["lprID"] = lprID;
            json["car_number"] = car_number;
            json["tel_number"] = tel_number;
            json["dong"] = dong;
            json["ho"] = ho;
            json["page"] = page;
            json["count"] = count;
            json["reg_no"] = reg_no;

            return json;
        }
    }
}
