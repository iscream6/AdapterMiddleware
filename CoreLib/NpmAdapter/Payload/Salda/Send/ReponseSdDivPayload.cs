using Newtonsoft.Json.Linq;
using NpmCommon;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class ResponseSdDivPayload : IPayload
    {
        public List<ResponseSdDivContentPayload> contents { get; set; }

        public ResponseSdDivPayload()
        {
            contents = new List<ResponseSdDivContentPayload>();
        }

        public void Deserialize(JToken json)
        {
            JArray array = json as JArray;
            if(array != null)
            {
                foreach (var item in array)
                {
                    ResponseSdDivContentPayload content = new ResponseSdDivContentPayload();
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

    class ResponseSdDivContentPayload : IPayload
    {
        /// <summary>
        /// 장치ID
        /// </summary>
        public string deviceId { get; set; }
        /// <summary>
        /// 장치이름
        /// </summary>
        public string deviceName { get; set; }
        /// <summary>
        /// 장치 구분 In, Out, All
        /// </summary>
        public string deviceType { get; set; }

        public void Deserialize(JToken json)
        {
            deviceId = Helper.NVL(json["unit_no"]);
            deviceName = Helper.NVL(json["unit_name"]);
            deviceType = Helper.NVL(json["unit_kind"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["deviceId"] = deviceId;
            json["deviceName"] = deviceName;
            json["deviceType"] = deviceType;
            return json;
        }
    }
}
