using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class SmmIOPayload : IPayload
    {
        private string command { get; set; }
        public string car_number { get; set; }
        public string date_time { get; set; }
        public string lprid { get; set; }
        public string kind { get; set; }
        public string owner_name { get; set; }
        public string group_name { get; set; }
        public string dept_name { get; set; }

        public SmmIOPayload(CmdType cmd)
        {
            if (cmd == CmdType.alert_incar) command = "smt_alert_incar";
            else if (cmd == CmdType.alert_outcar) command = "smt_alert_outcar";
        }

        public void Deserialize(JToken json)
        {
            car_number = Helper.NVL(json["car_number"]);
            date_time = Helper.NVL(json["date_time"]);
            lprid = Helper.NVL(json["lprid"]);
            switch (Helper.NVL(json["kind"]))
            {
                case "a":
                    kind = "0";
                    break;
                case "n":
                    kind = "1";
                    break;
                case "v":
                    kind = "2";
                    break;
            }
            
            owner_name = Helper.NVL(json["owner_name"]);
            group_name = Helper.NVL(json["group_name"]);
            dept_name = Helper.NVL(json["dept_name"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["command"] = command;

            JObject dataJson = new JObject();
            dataJson["car_number"] = car_number;
            dataJson["date_time"] = date_time;
            dataJson["lprid"] = lprid;
            dataJson["kind"] = kind;
            dataJson["owner_name"] = owner_name;
            dataJson["group_name"] = group_name;
            dataJson["dept_name"] = dept_name;

            json["data"] = dataJson;

            return json;
        }
    }
}
