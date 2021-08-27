using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class HipassRequestPayload : IPayload
    {
        public string unit_no { get; set; }
        public string tk_no { get; set; }
        public string car_number { get; set; }
        public string fee { get; set; }

        public void Deserialize(JToken json)
        {
            unit_no = Helper.NVL(json["unit_no"]);
            tk_no = Helper.NVL(json["tk_no"]);
            car_number = Helper.NVL(json["car_number"]);
            fee = Helper.NVL(json["fee"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JObject json = new JObject();
            json["unit_no"] = unit_no;
            json["tk_no"] = tk_no;
            json["car_number"] = car_number;
            json["fee"] = fee;
            return json;
        }

        public byte[] ResponseSerialize(JToken data, bool isSuccess)
        {
            var json = ToJson();
            if(data != null)
            {
                json["data"] = data;
            }
            json["result_code"] = isSuccess ? "OK" : "Fail";
            json["result_message"] = isSuccess ? "" : "하이패스 장비 연동에 실패하였습니다.";

            Log.WriteLog(LogType.Info, "HipassRequestPayload | ResponseSerialize", $"{json}", LogAdpType.Nexpa);

            return json.ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }
    }
}
