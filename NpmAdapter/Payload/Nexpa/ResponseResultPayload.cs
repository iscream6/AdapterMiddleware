//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.ComponentModel;

namespace NpmAdapter.Payload
{
    class ResponseResultPayload : IPayload
    {
        public enum Status
        {
            [Description("I don't know what's wrong.")]
            None,
            /// <summary>
            /// 000200 응답리턴시 통신정상
            /// </summary>
            [Description("OK")]
            OK,
            /// <summary>
            /// 잘못된 인수
            /// </summary>
            [Description("Invalid Arguments")]
            ArgumentExcetionError,
            /// <summary>
            /// 내부 오류
            /// </summary>
            [Description("Inneer Exception Error")]
            ExceptionERROR,
            /// <summary>
            /// Argument 포멧이 잘못됨.
            /// </summary>
            [Description("Argument format is wrong")]
            FailFormatError,
            /// <summary>
            /// URL 잘못 보냄
            /// </summary>
            [Description("URL is wrong")]
            InvalidURL
        }

        public CmdType command { get; set; }

        public Status Result
        {
            get
            {
                return (Status)Enum.Parse(typeof(Status), resultPayload.code);
            }
            set
            {
                //Result가 들어오면 StatusPayload를 셋팅한다.
                resultPayload = SetStatusCode(value);
            }
        }

        private StatusPayload resultPayload { get; set; }

        public void Deserialize(JObject json)
        {
            command = (CmdType)Enum.Parse(typeof(CmdType), json["command"].ToString());
            resultPayload.Deserialize(json["result"] as JObject);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["command"] = command.ToString();
            json["result"] = resultPayload.ToJson();
            return json;
        }

        public StatusPayload SetStatusCode(Status code)
        {
            StatusPayload payload = new StatusPayload();
            switch (code)
            {
                case Status.OK:
                    payload.code = "200";
                    payload.message = code.ToString();
                    break;
                case Status.ArgumentExcetionError:
                case Status.ExceptionERROR:
                case Status.FailFormatError:
                    payload.code = "400";
                    payload.message = code.GetDescription();
                    break;
                case Status.InvalidURL:
                    payload.code = "404";
                    payload.message = code.GetDescription();
                    break;
                default:
                    payload.code = "400";
                    payload.message = code.GetDescription();
                    break;
                
            }

            return payload;
        }
    }
}
