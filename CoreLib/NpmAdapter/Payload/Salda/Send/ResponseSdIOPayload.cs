using Newtonsoft.Json.Linq;
using NpmCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseSdIOPayload : IPayload
    {
        public List<ResponseSdIOContentPayload> contents { get; set; }

        public ResponseSdIOPayload()
        {
            contents = new List<ResponseSdIOContentPayload>();
        }

        public void Deserialize(JToken json)
        {
            JArray array = json as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    ResponseSdIOContentPayload content = new ResponseSdIOContentPayload();
                    content.Deserialize(item);
                    contents.Add(content);
                }
            }
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JArray jarr = new JArray();
            foreach (var item in contents)
            {
                jarr.Add(item.ToJson());
            }
            return jarr;
        }
    }

    class ResponseSdIOContentPayload : IPayload
    {
        public string eventId { get; set; }
        public string carNo { get; set; }
        public string deviceId { get; set; }
        public string whitelistType { get; set; }
        public string eventType { get; set; }
        public string eventDateTime { get; set; }
        public string customerName { get; set; }
        public string mobileNo { get; set; }
        public string metaData1 { get; set; }
        public string metaData2 { get; set; }
        public string link { get; set; }

        public void Deserialize(JToken json)
        {
            eventId = Helper.NVL(json["eventId"]);
            carNo = Helper.NVL(json["carNo"]);
            deviceId = Helper.NVL(json["deviceId"]);
            whitelistType = Helper.NVL(json["whitelistType"]);
            eventType = Helper.NVL(json["eventType"]);
            eventDateTime = Helper.NVL(json["eventDateTime"]);
            customerName = Helper.NVL(json["customerName"]);
            mobileNo = Helper.NVL(json["mobileNo"]);
            metaData1 = Helper.NVL(json["metaData1"]);
            metaData2 = Helper.NVL(json["metaData2"]);
            link = Helper.NVL(json["link"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["eventId"] = eventId;
            json["carNo"] = carNo;
            json["deviceId"] = deviceId;
            json["whitelistType"] = whitelistType;
            json["eventType"] = eventType;
            json["eventDateTime"] = eventDateTime;
            json["customerName"] = customerName;
            json["mobileNo"] = mobileNo;
            json["metaData1"] = metaData1;
            json["metaData2"] = metaData2;
            json["link"] = link;
            return json;
        }
    }
}
