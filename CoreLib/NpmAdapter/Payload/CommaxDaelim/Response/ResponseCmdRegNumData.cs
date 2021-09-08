using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class ResponseCmdRegNumData : IPayload
    {
        public string reg_num { get; set; }

        public void Deserialize(JToken json)
        {
            reg_num = Helper.NVL(json["reg_num"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["reg_num"] = Helper.NVL(reg_num);
            return json;
        }
    }
}
