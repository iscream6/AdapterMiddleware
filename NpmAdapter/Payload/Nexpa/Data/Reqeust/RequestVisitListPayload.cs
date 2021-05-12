using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;

namespace NpmAdapter.Payload
{
    class RequestVisitList2Payload : IPayload
    {
        public enum EventType
        {
            /// <summary>
            /// All(전체)
            /// </summary>
            A,
            /// <summary>
            /// Future(미래)
            /// </summary>
            F,
            /// <summary>
            /// History(과거이력)
            /// </summary>
            H
        }

        public string car_number { get; set; }
        /// <summary>
        /// 동
        /// </summary>
        public string dong { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string ho { get; set; }

        public string event_date_time { get; set; }
        public EventType eventType { get; set; }

        public void Deserialize(JObject json)
        {
            car_number = Helper.NVL(json["car_number"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            event_date_time = Helper.NVL(json["event_date_time"]);
            eventType = (EventType)Enum.Parse(typeof(EventType), Helper.NVL(json["eventType"]));
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.Nexpa_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["car_number"] = car_number;
            json["dong"] = dong;
            json["ho"] = ho;
            json["event_date_time"] = event_date_time;
            json["eventType"] = eventType.ToString();
            return json;
        }
    }
}
