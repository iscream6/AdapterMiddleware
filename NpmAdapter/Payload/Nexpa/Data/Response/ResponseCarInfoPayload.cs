using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseCarInfoPayload : IPayload
    {
        public string car_number { get; set; }

        public string date { get; set; }
        public string alias { get; set; }

        public void Deserialize(JToken json)
        {
            car_number = json["car_number"].ToString();
            date = json["date"].ToString();
            alias = json["alias"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["date"] = date;
            json["alias"] = alias;
            return json;
        }
    }
}
