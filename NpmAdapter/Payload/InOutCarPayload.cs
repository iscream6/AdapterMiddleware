//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;

namespace NpmAdapter.Payload
{
    public class InOutCarPayload : IPayload
    {
        public string dong { get; set; }
        public string ho { get; set; }
        public string car_number { get; set; }
        public string date_time { get; set; }

        public void Deserialize(JObject json)
        {
            dong = json["dong"].ToString();
            ho = json["ho"].ToString();
            car_number = json["car_number"].ToString();
            date_time = json["date_time"].ToString();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray();
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["dong"] = dong;
            json["ho"] = ho;
            json["car_number"] = car_number;
            json["date_time"] = date_time;
            return json;
        }
    }
}
