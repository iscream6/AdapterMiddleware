﻿using Newtonsoft.Json.Linq;
using System;

namespace NpmAdapter.Payload
{
    /// <summary>
    /// 코맥스 대림 헤더
    /// </summary>
    class CmdHeader : IPayload
    {
        public string category { get; set; }

        public CmdHelper.Type type { get; set; }

        public CmdHelper.Command command { get; set; }

        public void Deserialize(JToken json)
        {
            category = Helper.NVL(json["category"]);
            type = (CmdHelper.Type)Enum.Parse(typeof(CmdHelper.Type), json["type"].ToString()); 
            //command = (CmdHelper.Command)Enum.Parse(typeof(CmdHelper.Command), json["command"].ToString());
            if((CmdHelper.Command)Enum.Parse(typeof(CmdHelper.Command), json["command"].ToString()) == CmdHelper.Command.query_request)
            {
                command = CmdHelper.Command.query_response;
            }
            else if((CmdHelper.Command)Enum.Parse(typeof(CmdHelper.Command), json["command"].ToString()) == CmdHelper.Command.control_request)
            {
                command = CmdHelper.Command.control_response;
            }
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["category"] = category;
            json["type"] = type.ToString();
            json["command"] = command.ToString();
            return json;
        }
    }
}
