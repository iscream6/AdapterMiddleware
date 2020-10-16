//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace NpmAdapter.Payload
{
    public class Payload<T> : IPayload where T : IPayload, new()
    {
        public string command { get; set; }
        public List<T> data { get; set; }

        public void Deserialize(JObject json)
        {
            command = json["command"].ToString();
            JArray array = json["data"] as JArray;
            if(array != null)
            {
                foreach (var item in array)
                {
                    T t = new T();
                    t.Deserialize(item as JObject);
                    data.Add(t);
                }
            }
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["command"] = command;
            //json["data"] = data.Select(p => p.ToJson()).ToArray();
            JArray array = new JArray();
            foreach (var item in data)
            {
                array.Add(item.ToJson());
            }
            json["data"] = array;
            return json;
        }
    }
}
