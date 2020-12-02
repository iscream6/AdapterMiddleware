using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestApsOutCarPayload : IPayload
    {
        public string apt_id { get; set; }
        public string car_number { get; set; }
        public string gate { get; set; }
        public string parking_in_datetime { get; set; }

        public void Deserialize(JObject json)
        {
            apt_id = Helper.NVL(json["apt_id"]);
            car_number = Helper.NVL(json["car_number"]);
            gate = Helper.NVL(json["gate"]);
            parking_in_datetime = Helper.NVL(json["parking_in_datetime"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["apt_id"] = apt_id;
            json["car_number"] = car_number;
            json["gate"] = gate;
            json["parking_in_datetime"] = parking_in_datetime;
            return json;
        }
    }
}
