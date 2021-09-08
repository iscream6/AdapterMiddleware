using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class RequestSdReg : IPayload
    {
        public string zoneId { get; set; }
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
        public string deletedAt { get; set; }

        public void Deserialize(JToken json)
        {
            zoneId = Helper.NVL(json["zoneId"]);
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
            json["updatedAt"] = "";
            json["deletedAt"] = "";
            return json;

        }
    }
}
