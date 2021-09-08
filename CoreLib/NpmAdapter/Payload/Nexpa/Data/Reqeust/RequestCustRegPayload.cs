using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestCustRegPayload : IPayload
    {
        public string dong { get; set; }
        public string ho  { get; set; }
        public string car_number { get; set; }
        public string name { get; set; }
        public string tel_number { get; set; }
        public string remark { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }

        public void Deserialize(JToken json)
        {
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            car_number = Helper.NVL(json["car_number"]);
            name = Helper.NVL(json["name"]);
            tel_number = Helper.NVL(json["tel_number"]);
            remark = Helper.NVL(json["remark"]);
            start_date = Helper.NVL(json["start_date"]);
            end_date = Helper.NVL(json["end_date"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            json["car_number"] = car_number;
            json["name"] = name;
            json["tel_number"] = tel_number;
            json["remark"] = remark;
            json["start_date"] = start_date;
            json["end_date"] = end_date;
            return json;
        }
    }
}
