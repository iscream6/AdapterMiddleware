using Newtonsoft.Json.Linq;
using NpmCommon;
using System.ComponentModel;

namespace NpmAdapter.Payload
{
    class MvlResponsePayload : IPayload
    {
        public enum SttCode
        {
            [Description("SUCCESS")]
            OK = 1,
            [Description("Unauthorized, invalid token")]
            InvalidToken = 2,
            [Description("Unauthorized, invalid token")]
            InvalidSecret = 3,
            [Description("Invalid parameters")]
            InvalidParameter = 4,
            [Description("Invalid parameters")]
            InvalidCarNumNDateFormat = 5,
            [Description("Invalid parameters")]
            VisitTimeOver = 6,
            [Description("Not Found")]
            NotFoundPage = 7,
            [Description("Requested method is not supported")]
            NotSupportedMethod = 8,
            [Description("Internal Server Error")]
            InternalServerError = 99,
        }

        public SttCode resultCode;
        public string resultMessage { get; set; }
        public string responseTime { get; set; }
        public virtual void Deserialize(JToken json)
        {
            int iResultCode;
            if (int.TryParse(Helper.NVL(json["resultCode"]), out iResultCode))
            {
                resultCode = (SttCode)iResultCode;
            }
            else
            {
                resultCode = SttCode.InvalidParameter;
            }

            resultMessage = Helper.NVL(json["resultMessage"]);
            responseTime = Helper.NVL(json["responseTime"]);
        }

        public virtual byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public virtual JToken ToJson()
        {
            JObject json = new JObject();
            json["resultCode"] = (int)resultCode;
            json["resultMessage"] = resultMessage;
            json["responseTime"] = responseTime;
            return json;
        }
    }
}
