using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestEzInOutCarPayload : IPayload
    {
        public enum IOType
        {
            InCar,
            OutCar
        }

        public RequestEzInOutCarPayload(CmdType type)
        {
            if (type == CmdType.alert_incar) dateName = "in_date";
            else dateName = "out_date";
        }

        public string apt_idx { get; set; }
        public string car_number { get; set; }
        public string date { get; set; }

        private IOType type;
        private string dateName = "";

        public void Deserialize(JObject json)
        {
            apt_idx = Helper.NVL(json["apt_id"]);
            car_number = Helper.NVL(json["car_number"]);
            date = Helper.NVL(json[dateName]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["apt_id"] = Helper.NVL(apt_idx);
            json["car_number"] = Helper.NVL(car_number);
            json[dateName] = Helper.NVL(date);
            return json;
        }
    }
}
