//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NpmAdapter.Payload
{
    public interface IPayload
    {
        void Deserialize(JObject json);

        byte[] Serialize();

        JObject ToJson();
    }
}
