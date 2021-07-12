//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.ComponentModel;

namespace NpmAdapter.Payload
{
    class ResponsePayload : IPayload
    {
        
        public CmdType command { get; set; }
        protected ResultPayload resultPayload { get; set; }

        public ResponsePayload()
        {
            resultPayload = new ResultPayload();
        }

        public virtual ResultType result
        {
            get
            {
                return (ResultType)Enum.Parse(typeof(ResultType), resultPayload.code);
            }
            set
            {
                //Result가 들어오면 StatusPayload를 셋팅한다.
                resultPayload = ResultPayload.GetStatusPayload(value);
            }
        }

        public void SetCustomResult(string code = null, string message = null)
        {
            if(resultPayload == null) resultPayload = new ResultPayload();

            if(code != null)
            {
                resultPayload.code = code;
            }

            if(message != null)
            {
                resultPayload.message = message;
            }
            
        }

        public virtual void Deserialize(JObject json)
        {
            command = (CmdType)Enum.Parse(typeof(CmdType), json["command"].ToString());
            resultPayload.Deserialize(json["result"] as JObject);
        }

        public virtual byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public virtual JObject ToJson()
        {
            JObject json = new JObject();
            json["command"] = command.ToString();
            json["result"] = resultPayload.ToJson();
            return json;
        }
    }
}
