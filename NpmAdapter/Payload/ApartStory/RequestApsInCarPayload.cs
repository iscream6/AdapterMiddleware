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
        public string partner_visit_id { get; set; }


        public void Deserialize(JToken json)
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
            partner_visit_id = Helper.NVL(json["partner_visit_id"]);
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
            json["dong"] = Helper.NVL(dong);
            json["ho"] = Helper.NVL(ho);
            json["phone_number"] = Helper.NVL(phone_number);
            //아파트 스토리측에서 주석처리요청
            //json["gate"] = Helper.NVL(gate); 
            json["visit_in_date"] = Helper.NVL(visit_in_date);
            json["visit_out_date"] = Helper.NVL(visit_out_date);
            //아파트 스토리측에서 주석처리요청
            //json["visit_type"] = Helper.NVL(visit_type);
            json["parking_in_datetime"] = Helper.NVL(parking_in_datetime);
            json["partner_visit_id"] = Helper.NVL(partner_visit_id);
            return json;
        }
    }
}
