using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;

namespace NpmAdapter.Payload
{
    class RequestVisitListPayload : IPayload
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
        /// 쿼리 할 자료 총 합계를 count로 나눈 총 page 수 값 중 몇번째 page를 가져올지의 값, 0일경우 전체가져오기
        /// </summary>
        public string page { get; set; }
        /// <summary>
        /// 쿼리할 자료 수
        /// </summary>
        public string count { get; set; }

        public void Deserialize(JToken json)
        {
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
            page = json["page"].ToString();
            count = json["count"].ToString();
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
            json["page"] = page;
            json["count"] = count;
            return json;
        }
    }
}
