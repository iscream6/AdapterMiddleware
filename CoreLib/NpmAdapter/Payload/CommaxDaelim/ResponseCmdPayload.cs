using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    class ResponseCmdPayload : IPayload
    {
        public enum Status
        {
            None,
            /// <summary>
            /// 500으로 응답왔는대 데이터 없는경우
            /// </summary>
            ERROR,
            /// <summary>
            /// 000200 응답리턴시 통신정상
            /// </summary>
            OK,
            /// <summary>
            /// 000500 응답리턴시 통신포맷오류
            /// </summary>
            FailFormat,
            /// <summary>
            /// 050999 Server통신오류(자체제작)
            /// </summary>
            Server_FailConnect
        }

        public CmdHeader header { get; set; }

        public IPayload data { get; set; }

        public ResultPayload result { get; set; }

        public void Initialize()
        {
            header = null;
            data = null;
            result = null;
        }

        public void Deserialize(JToken json)
        {
            header.Deserialize(json["header"] as JObject);
            result.Deserialize(json["result"] as JObject);

            if (json["data"] != null)
            {
                data = CmdHelper.MakeResponseDataPayload(header.type, json["data"] as JObject);
            }
        }

        public void DeserializeData(CmdHelper.Type type, JToken json)
        {
            data = CmdHelper.MakeResponseDataPayload(type, json);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["header"] = header?.ToJson();
            if (data != null)
            {
                json["data"] = data?.ToJson();
            }
            json["result"] = result?.ToJson();
            return json;
        }
    }
}
