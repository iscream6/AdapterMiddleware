using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class MvlValuePayload<T> : MvlResponsePayload where T : IPayload, new()
    {
        public List<T> list { get; set; }

        public MvlValuePayload() : base()
        {
            list = new List<T>();
        }

        public override void Deserialize(JObject json)
        {
            base.Deserialize(json);
            JArray array = json["values"] as JArray;
            if (array != null)
            {
                foreach (var item in array)
                {
                    if (item.HasValues)
                    {
                        T t = new T();
                        t.Deserialize(item as JObject);
                        if (list == null) list = new List<T>();
                        list.Add(t);
                    }
                }
            }
        }

        public override byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public override JObject ToJson()
        {
            JObject json = base.ToJson();
            if (list != null && list.Count > 0)
            {
                JArray array = new JArray();
                foreach (var item in list)
                {
                    array.Add(item.ToJson());
                }
                json["values"] = array;
            }
            return json;
        }

        public T GetSubPayload()
        {
            return new T();
        }
    }

    /// <summary>
    /// 일반차량 출입조회
    /// </summary>
    class MvlIONDataPayload : IPayload
    {
        public string tkNo { get; set; }
        public string parkNo { get; set; }
        public string carNo { get; set; }
        public string indatetime { get; set; }
        public string outdatetime { get; set; }

        public void Deserialize(JObject json)
        {
            tkNo = Helper.NVL(json["tkNo"]);
            parkNo = Helper.NVL(json["parkNo"]);
            carNo = Helper.NVL(json["carNo"]);
            indatetime = Helper.NVL(json["indatetime"]);
            outdatetime = Helper.NVL(json["outdatetime"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["tkNo"] = tkNo;
            json["parkNo"] = parkNo;
            json["carNo"] = carNo;
            json["indatetime"] = indatetime;
            json["outdatetime"] = outdatetime;
            return json;
        }
    }

    class MvlCustInfoPayload : IPayload
    {
        public string tkNo { get; set; }
        public string groupNo { get; set; }
        public string carNo { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public string name { get; set; }
        public string contact { get; set; }
        public string remark { get; set; }
        public string effStart { get; set; }
        public string effEnd { get; set; }
        public int chkUse;

        public void Deserialize(JObject json)
        {
            tkNo = Helper.NVL(json["tkNo"]);
            groupNo = Helper.NVL(json["groupNo"]);
            carNo = Helper.NVL(json["carNo"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            name = Helper.NVL(json["name"]);
            contact = Helper.NVL(json["contact"]);
            remark = Helper.NVL(json["remark"]);
            effStart = Helper.NVL(json["effStart"]);
            effEnd = Helper.NVL(json["effEnd"]);
            int.TryParse(Helper.NVL(json["chkUse"]), out chkUse);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["tkNo"] = tkNo;
            json["groupNo"] = groupNo;
            json["carNo"] = carNo;
            json["dong"] = dong;
            json["ho"] = ho;
            json["name"] = name;
            json["contact"] = contact;
            json["remark"] = remark;
            json["effStart"] = effStart;
            json["effEnd"] = effEnd;
            json["chkUse"] = chkUse;
            return json;
        }
    }

    class MvlIOSDataPayload : IPayload
    {
        public string tkNo { get; set; }
        public string parkNo { get; set; }
        public string carNo { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public string indatetime { get; set; }
        public string outdatetime { get; set; }
        public int instatusno { get => 0; } //default value return only
        public int outstatusno { get => 0; } //default value return only

        public void Deserialize(JObject json)
        {
            tkNo = Helper.NVL(json["tkNo"]);
            parkNo = Helper.NVL(json["parkNo"]);
            carNo = Helper.NVL(json["carNo"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            indatetime = Helper.NVL(json["indatetime"]);
            outdatetime = Helper.NVL(json["outdatetime"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["tkNo"] = tkNo;
            json["parkNo"] = parkNo;
            json["carNo"] = carNo;
            json["dong"] = dong;
            json["ho"] = ho;
            json["indatetime"] = indatetime;
            json["outdatetime"] = outdatetime;
            json["instatusno"] = instatusno;
            json["outstatusno"] = outstatusno;
            return json;
        }
    }

    class MvlReserveCarPayload : IPayload
    {
        public string Belong { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public string carNo { get; set; }
        public string reserveStart { get; set; }
        public string reserveEnd { get; set; }
        public string remark { get; set; }

        public void Deserialize(JObject json)
        {
            Belong = Helper.NVL(json["Belong"]);
            carNo = Helper.NVL(json["carNo"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            reserveStart = Helper.NVL(json["reserveStart"]);
            reserveEnd = Helper.NVL(json["reserveEnd"]);
            remark = Helper.NVL(json["remark"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["Belong"] = Belong;
            json["carNo"] = carNo;
            json["dong"] = dong;
            json["ho"] = ho;
            json["reserveStart"] = reserveStart;
            json["reserveEnd"] = reserveEnd;
            json["remark"] = remark;
            return json;
        }
    }

    class MvlIOReservePayload : IPayload
    {
        public string parkNo { get; set; }
        public string carNo { get; set; }
        public string dong { get; set; }
        public string ho { get; set; }
        public string indatetime { get; set; }
        public string outdatetime { get; set; }

        public void Deserialize(JObject json)
        {
            parkNo = Helper.NVL(json["parkNo"]);
            carNo = Helper.NVL(json["carNo"]);
            dong = Helper.NVL(json["dong"]);
            ho = Helper.NVL(json["ho"]);
            indatetime = Helper.NVL(json["indatetime"]);
            outdatetime = Helper.NVL(json["outdatetime"]);
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JObject ToJson()
        {
            JObject json = new JObject();
            json["parkNo"] = parkNo;
            json["carNo"] = carNo;
            json["dong"] = dong;
            json["ho"] = ho;
            json["indatetime"] = indatetime;
            json["outdatetime"] = outdatetime;
            return json;
        }
    }
}
