using Newtonsoft.Json.Linq;
using NpmCommon;

namespace NpmAdapter.Payload
{
    class BeyondPayload : IPayload
    {

        public virtual void Deserialize(JToken json)
        {

        }

        public virtual byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public virtual JToken ToJson()
        {
            JObject json = new JObject();
            json["shadow"] = GetShadowBlock();
            json["siteId"] = SysConfig.Instance.HC_Id;

            return json;
        }

        private JObject GetShadowBlock()
        {
            JObject shadowJson = new JObject();
            shadowJson["zoneName"] = SysConfig.Instance.ParkName;
            shadowJson["parentGroup"] = null;
            shadowJson["group"] = "LPR";
            shadowJson["name"] = Helper.GetLocalIP();

            return shadowJson;
        }
    }

    class RequestByAliveCheckPayload : BeyondPayload
    {
        public string requestType { get; set; }

        public override void Deserialize(JToken json)
        {
            base.Deserialize(json);
            requestType = Helper.NVL(json["requestType"]);
        }

        public override byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public override JToken ToJson()
        {
            JToken json = base.ToJson();
            JObject instantJson = new JObject();
            instantJson["requestType"] = requestType;
            json["instantMessage"] = instantJson;
            return json;
        }
    }

    class RequestByInOutCarPayload : BeyondPayload
    {
        public enum IOType
        {
            IN,
            OUT
        }

        public string carNo { get; set; }
        public IOType ioType { get; set; }
        public string dateTime { get; set; }

        public override void Deserialize(JToken json)
        {
            base.Deserialize(json);
            dateTime = Helper.NVL(json["enterOffsetDateTime"]);
            if(dateTime == null && dateTime == "")
            {
                dateTime = Helper.NVL(json["exitOffsetDateTime"]);
                ioType = IOType.OUT;
            }
            else
            {
                ioType = IOType.IN;
            }
        }

        public override byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public override JToken ToJson()
        {
            JToken json = base.ToJson();
            json["carNo"] = carNo;
            if(ioType == IOType.IN)
            {
                json["enterOffsetDateTime"] = dateTime;
            }
            else
            {
                json["exitOffsetDateTime"] = dateTime;
            }

            return json;
        }
    }
}
