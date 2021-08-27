using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class RequestSdAlertCarPayload : IPayload
    {
        /// <summary>
        /// 주차장번호
        /// </summary>
        public string zoneId { get; set; }
        /// <summary>
        /// 이벤트ID
        /// </summary>
        public string eventId { get; set; }
        /// <summary>
        /// 차량번호
        /// </summary>
        public string carNo { get; set; }
        /// <summary>
        /// 장비ID
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 입차시 In, 출차시 Out
        /// </summary>
        public string eventType { get; set; }
        /// <summary>
        /// yyyy-MM-ddTHH:mm:ss(ISO8601형식)
        /// </summary>
        public string eventDateTime { get; set; }
        /// <summary>
        /// 입출차 시 촬영된 차량사진 링크
        /// </summary>
        public string link { get; set; }
        /// <summary>
        /// 예약방문차량 Visitor, 정기차량 Regular
        /// </summary>
        public string whitelistType { get; set; }
        /// <summary>
        /// /동
        /// </summary>
        public string metaData1 { get; set; }
        /// <summary>
        /// 호
        /// </summary>
        public string metaData2 { get; set; }

        public void Deserialize(JToken json)
        {
            zoneId = Helper.NVL(json["zoneId"]);
            eventId = Helper.NVL(json["eventId"]);
            carNo = Helper.NVL(json["carNo"]);
            deviceId = Helper.NVL(json["deviceId"]);
            eventType = Helper.NVL(json["eventType"]);
            eventDateTime = Helper.NVL(json["eventDateTime"]);
            link = Helper.NVL(json["link"]);
            whitelistType = Helper.NVL(json["whitelistType"]);
            metaData1 = Helper.NVL(json["metaData1"]);
            metaData2 = Helper.NVL(json["metaData2"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["zoneId"] = zoneId;
            json["eventId"] = eventId;
            json["carNo"] = carNo;
            json["deviceId"] = deviceId;
            json["eventType"] = eventType;
            json["eventDateTime"] = eventDateTime;
            json["link"] = link;
            json["whitelistType"] = whitelistType;
            json["metaData1"] = metaData1;
            json["metaData2"] = metaData2;
            return json;
        }
    }
}
