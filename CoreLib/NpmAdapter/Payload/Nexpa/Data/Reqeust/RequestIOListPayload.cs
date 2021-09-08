using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestIOListPayload : IPayload
    {
        public string park_no { get; set; }

        public string page { get; set; }
        public string count { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string car_number { get; set; }
        public string tel_number { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }

        public void Deserialize(JToken json)
        {
            
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["park_no"] = park_no;
            json["page"] = page;
            json["count"] = count;
            json["start_date"] = start_date;
            json["end_date"] = end_date;
            json["car_number"] = car_number;
            json["tel_number"] = tel_number;
            json["dong"] = dong;
            json["ho"] = ho;

            return json;
        }
    }
}
