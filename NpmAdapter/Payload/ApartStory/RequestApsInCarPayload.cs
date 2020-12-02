using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestApsInCarPayload : IPayload
    {
        public string apt_id { get; set; }
        public string car_number { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public string phone_number { get; set; }
        public string gate { get; set; }
        public string visit_in_date { get; set; }
        public string visit_out_date { get; set; }
        public string visit_type { get; set; }
        public string parking_in_datetime { get; set; }

        public void Deserialize(JObject json)
        {
            apt_id = Helper.NVL(json["apt_id"]);
            car_number = Helper.NVL(json["car_number"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            phone_number = Helper.NVL(json["phone_number"]);
            gate = Helper.NVL(json["gate"]);
            visit_in_date = Helper.NVL(json["visit_in_date"]);
            visit_out_date = Helper.NVL(json["visit_out_date"]);
            visit_type = Helper.NVL(json["visit_type"]);
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
            json["dong"] = dong;
            json["ho"] = ho;
            json["phone_number"] = phone_number;
            json["gate"] = gate;
            json["visit_in_date"] = visit_in_date;
            json["visit_out_date"] = visit_out_date;
            json["visit_type"] = visit_type;
            json["parking_in_datetime"] = parking_in_datetime;
            return json;
        }
    }
}
