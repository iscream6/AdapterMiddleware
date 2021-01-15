using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class MvlSingleCustInfoPayload : MvlResponsePayload
    {
        public enum EnrollType
        {
            New = 1,
            Modify = 2
        }
        public string carNo { get; set; }
        public string tkNo { get; set; }
        public EnrollType enrollType; 

        public override void Deserialize(JObject json)
        {
            base.Deserialize(json);
            carNo = Helper.NVL(json["carNo"]);
            tkNo = Helper.NVL(json["tkNo"]);

            int iEnrollType;
            if (int.TryParse(Helper.NVL(json["enrollType"]), out iEnrollType))
            {
                enrollType = (EnrollType)iEnrollType;
            }
            else
            {
                enrollType = EnrollType.New;
            }
            
        }

        public override byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }           

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["carNo"] = carNo;
            json["tkNo"] = tkNo; 
            json["enrollType"] = (int)enrollType;
            return json;
        }
    }

    class MvlSingleReserveCarPayload : MvlResponsePayload
    {
        public int belong;

        public override void Deserialize(JObject json)
        {
            base.Deserialize(json);
            int.TryParse(Helper.NVL(json["enrollType"]), out belong);
        }

        public override byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            json["enrollType"] = belong;
            return json;
        }
    }
}
