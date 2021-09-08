using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestSearchIONPayload : IPayload
    {
        public string car_number { get; set; }
        public string start_date_time { get; set; }
        public string end_date_time { get; set; }

        public void Deserialize(JToken json)
        {
            car_number = Helper.NVL(json["car_number"]);
            start_date_time = Helper.NVL(json["start_date_time"]);
            end_date_time = Helper.NVL(json["end_date_time"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["start_date_time"] = start_date_time;
            json["end_date_time"] = end_date_time;
            return json;
        }
    }
}
