//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    /// <summary>
    /// 코맥스 대림 방문차량 리스트 요청 데이터
    /// </summary>
    class RequestCmdVisitListData : IPayload
    {
        public string dongho { get; set; }
        public string page { get; set; }
        public string count { get; set; }

        public void Deserialize(JObject json)
        {
            dongho = json["dongho"].ToString();
            page = json["page"].ToString();
            count = json["count"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dongho"] = dongho;
            json["page"] = page;
            json["count"] = count;
            return json;
        }
    }
}
