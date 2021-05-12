//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System.ComponentModel;

namespace NpmAdapter.Payload
{
    public enum ResultType
    {
        [Description("None")]
        None,
        /// <summary>
        /// 000200 응답리턴시 통신정상
        /// </summary>
        [Description("OK")]
        OK = 200,
        /// <summary>
        /// 잘못된 인수
        /// </summary>
        [Description("Invalid Arguments")]
        ArgumentExcetionError = 400,
        /// <summary>
        /// 내부 오류
        /// </summary>
        [Description("Inneer Exception Error")]
        ExceptionERROR = 401,
        /// <summary>
        /// Argument 포멧이 잘못됨.
        /// </summary>
        [Description("Argument format is wrong")]
        FailFormatError = 402,
        [Description("Disconnected HomenetServer")]
        HomenetkDisconnected = 403,
        [Description("Failed Send Message to HomenetServer")]
        FailSendMessage = 404,
        [Description("Failed interface")]
        FailInterface = 405,
        /// <summary>
        /// URL 잘못 보냄
        /// </summary>
        [Description("URL is wrong")]
        InvalidURL = 500,
        [Description("현재 주차관제 서버와 연동중이지 않습니다")]
        notinterface_kwanje = 503,
        [Description("현재 주차위치 서버와 연동중이지 않습니다")]
        notinterface_udo = 504,
        [Description("현재 원패스 서버와 연동중이지 않습니다")]
        notinterface_onepass = 505,
        [Description("현재 LPR과 연동중이지 않습니다")]
        notinterface_lpr = 506,
        [Description("Server Exception Error")]
        server_error = 516,
        [Description("인증정보가 유효하지 않습니다.")]
        faild_authorization = -1004
    }

    public class ResultPayload : IPayload
    {
        public string code { get; set; }
        public string message { get; set; }

        public void Deserialize(JObject json)
        {
            code = json["status"].ToString();
            message = json["message"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["status"] = code;
            json["message"] = message;
            return json;
        }

        public static ResultPayload GetStatusPayload(ResultType code)
        {
            ResultPayload payload = new ResultPayload();
            
            payload.code = ((int)code).ToString();
            payload.message = code.GetDescription();

            return payload;
        }
    }
}
