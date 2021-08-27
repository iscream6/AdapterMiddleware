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
        public string parking_out_datetime { get; set; }

        public void Deserialize(JToken json)
        {
            apt_id = Helper.NVL(json["apt_id"]);
            car_number = Helper.NVL(json["car_number"]);
            gate = Helper.NVL(json["gate"]);
            parking_out_datetime = Helper.NVL(json["parking_out_datetime"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["apt_id"] = Helper.NVL(apt_id);
            json["car_number"] = Helper.NVL(car_number);
            //json["gate"] = Helper.NVL(gate);
            json["parking_out_datetime"] = Helper.NVL(parking_out_datetime);
            return json;
        }
    }
}
