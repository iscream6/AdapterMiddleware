using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestCustListPayload : IPayload
    {
        public string car_number { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }

        public void Deserialize(JToken json)
        {
            car_number = Helper.NVL(json["car_number"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["dong"] = dong;
            json["ho"] = ho;
            return json;
        }
    }
}
