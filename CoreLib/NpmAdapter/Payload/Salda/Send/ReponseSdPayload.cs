using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class ReponseSdPayload : IPayload
    {
        public string resultCode { get; set; }
        public string resultMsg { get; set; }
        public IPayload content { get; set; }
        
        public void Initialize()
        {
            resultCode = "";
            resultMsg = "";
            content = null;
        }

        public void Deserialize(JToken json)
        {
            //Do nothing
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["resultCode"] = resultCode;
            json["resultMsg"] = resultMsg;
            if(content != null)
            {
                json["content"] = content.ToJson();
            }
            return json;
        }
    }
}
