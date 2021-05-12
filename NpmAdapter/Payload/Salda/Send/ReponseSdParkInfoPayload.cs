using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ReponseSdParkInfoPayload : IPayload
    {
        /// <summary>
        /// 주차장이름
        /// </summary>
        public string zoneName { get; set; }
        /// <summary>
        /// 주소
        /// </summary>
        public string zoneAddress { get; set; }
        /// <summary>
        /// 사업자명
        /// </summary>
        public string companyName { get; set; }
        /// <summary>
        /// 사업자번호
        /// </summary>
        public string licenseNo { get; set; }

        public void Deserialize(JObject json)
        {
            zoneName = Helper.NVL(json["zoneName"]);
            zoneAddress = Helper.NVL(json["zoneAddress"]);
            companyName = Helper.NVL(json["companyName"]);
            licenseNo = Helper.NVL(json["licenseNo"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["zoneName"] = zoneName;
            json["zoneAddress"] = zoneAddress;
            json["companyName"] = companyName;
            json["licenseNo"] = licenseNo;
            return json;
        }
    }
}
