using Newtonsoft.Json.Linq;
using NpmCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseSdWhiteListPayload : IPayload
    {
        public List<ResponseSdWhiteListContentPayload> contents { get; set; }

        public ResponseSdWhiteListPayload()
        {
            contents = new List<ResponseSdWhiteListContentPayload>();
        }

        public void Deserialize(JToken json)
        {
            JArray array = json as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    ResponseSdWhiteListContentPayload content = new ResponseSdWhiteListContentPayload();
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

    class ResponseSdWhiteListContentPayload : IPayload
    {
        public string type { get; set; }
        public string regId { get; set; }
        public string carNo { get; set; }
        public string startDateTime { get; set; }
        public string endDateTime { get; set; }
        public string customerName { get; set; }
        public string mobileNo { get; set; }
        public string metaData1 { get; set; }
        public string metaData2 { get; set; }
        public string memo { get; set; }
        public bool repeatable { get; set; }
        public string createdAt { get; set; }
        public string updatedAt { get; set; }

        public void Deserialize(JToken json)
        {
            type = Helper.NVL(json["type"]);
            regId = Helper.NVL(json["regId"]);
            carNo = Helper.NVL(json["carNo"]);
            startDateTime = Helper.NVL(json["startDateTime"]);
            endDateTime = Helper.NVL(json["endDateTime"]);
            customerName = Helper.NVL(json["customerName"]);
            mobileNo = Helper.NVL(json["mobileNo"]);
            metaData1 = Helper.NVL(json["metaData1"]);
            metaData2 = Helper.NVL(json["metaData2"]);
            memo = Helper.NVL(json["memo"]);
            repeatable = bool.Parse(Helper.NVL(json["repeatable"]));
            createdAt = Helper.NVL(json["createdAt"]);
            updatedAt = Helper.NVL(json["updatedAt"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["regId"] = regId;
            json["carNo"] = carNo;
            json["startDateTime"] = startDateTime;
            json["endDateTime"] = endDateTime;
            json["customerName"] = customerName;
            json["mobileNo"] = mobileNo;
            json["metaData1"] = metaData1;
            json["metaData2"] = metaData2;
            json["memo"] = memo;
            json["type"] = type;
            json["createdAt"] = createdAt;
            json["updatedAt"] = updatedAt;
            json["repeatable"] = repeatable.ToString().ToLower();

            return json;
        }
    }
}
