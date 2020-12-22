//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NpmAdapter.Payload.Nexpa.Data.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    static class NexpaPayloadManager
    {
        public enum Type
        {
            none,
            all,
            family,
            visitor
        }

        public static IPayload MakeResponseDataPayload(CmdType cmd, JObject json)
        {
            IPayload payload = null;
            switch (cmd)
            {
                case CmdType.incar_list:
                    payload = new ResponseCarListPayload();
                    payload.Deserialize(json);
                    break;
                case CmdType.visit_list:
                    payload = new ResponseVisitListPayload();
                    payload.Deserialize(json);
                    break;
                case CmdType.visit_favo_list:
                    payload = new ResponseVisitFavoListPayload();
                    payload.Deserialize(json);
                    break;
                case CmdType.alive_check:
                    payload = new ResponseAutoBoothPayload();
                    payload.Deserialize(json);
                    break;
            }

            return payload;
        }
    }
}
