using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestVisitDelPayload : IPayload
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
        /// 차량 식별 할 등록 번호
        /// </summary>
        public string reg_no { get; set; }

        public void Deserialize(JToken json)
        {
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
            car_number = json["car_number"].ToString();
            reg_no = json["reg_no"].ToString();
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
            json["reg_no"] = reg_no;
            return json;
        }
    }
}
