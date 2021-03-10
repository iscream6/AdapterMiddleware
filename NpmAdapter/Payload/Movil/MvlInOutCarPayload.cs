using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class MvlInOutCarPayload : IPayload
    {
        public MvlInOutCarPayload(CmdType type)
        {
            if (type == CmdType.alert_incar) dateName = "in_date";
            else dateName = "out_date";
        }

        public int apt_idx { get; set; }
        public string car_number { get; set; }
        public long date { get; set; }

        private string dateName = "";

        public void Deserialize(JObject json)
        {
            int aptid = 0; 
            int.TryParse(Helper.NVL(json["apt_idx"]), out aptid);
            apt_idx = aptid;
            car_number = Helper.NVL(json["car_number"]);
            long iDate = 0;
            long.TryParse(Helper.NVL(json[dateName]), out iDate);
            date = iDate;
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            int aptid = 0;
            int.TryParse(Helper.NVL(apt_idx), out aptid);
            json["apt_idx"] = aptid;
            json["car_number"] = Helper.NVL(car_number);
            long iDate = 0;
            long.TryParse(Helper.NVL(date), out iDate);
            json[dateName] = iDate;
            return json;
        }
    }
}
