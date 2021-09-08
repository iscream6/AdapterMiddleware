using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestBlackListPayload : IPayload
    {
        public string page { get; set; }
        /// <summary>
        /// 쿼리할 자료 수
        /// </summary>
        public string count { get; set; }

        public void Deserialize(JToken json)
        {
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
            json["page"] = page;
            json["count"] = count;
            return json;
        }
    }
}
