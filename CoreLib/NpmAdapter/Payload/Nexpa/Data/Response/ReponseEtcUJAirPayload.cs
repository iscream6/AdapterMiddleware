using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class ReponseEtcUJAirPayload : IPayload
    {
        public string result { get; set; }
        public string msg { get; set; }
        public string msgCode { get; set; }

        public void Deserialize(JToken json)
        {
            result = Helper.NVL(json["result"]);
            msg = Helper.NVL(json["msg"]);
            msgCode = Helper.NVL(json["msgCode"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["result"] = result;
            json["msg"] = msg;
            json["msgCode"] = msgCode;
            return json;
        }
    }
}
