﻿//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    /// <summary>
    /// 코맥스 대림 요청
    /// </summary>
    class RequestCmdPayload<T> : IPayload where T : IPayload, new()
    {
        CmdHeader header { get; set; }
        T data { get; set; }

        public void Deserialize(JObject json)
        {
            var headerJson = json["header"];
            var dataJson = json["data"];
            header.Deserialize(headerJson as JObject);
            data = new T();
            data.Deserialize(dataJson as JObject);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["header"] = header.ToJson();
            json["data"] = data.ToJson();
            return json;
        }
    }
}
