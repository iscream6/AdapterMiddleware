using Newtonsoft.Json.Linq;
using NpmCommon;
using System;

namespace NpmAdapter.Payload
{
    class ResponseSdParkingZonePayload : IPayload
    {
        //Array
        public JArray arrZone { get; set; }

        class Zone
        {
            public string partName { get; set; }
            public int maxCount { get; set; }
            public int currentCount { get; set; }
            public int avaliableCount { get; set; }
            public int unableCount { get; set; }
        }

        public void Deserialize(JToken json)
        {
            arrZone = json["content"] as JArray;
        }

        public JArray Parse()
        {
            throw new NotImplementedException();
        }

        public byte[] Serialize()
        {
            return ToJson().ToByteArray(SysConfig.Instance.HomeNet_Encoding);
        }

        public JToken ToJson()
        {
            JArray jArray = new JArray();

            return jArray;
        }

        /// <summary>
        /// JArray에 Object를 추가한다.
        /// </summary>
        /// <param name="partName">층 또는 구역이름</param>
        /// <param name="maxCount">총 주차면 수</param>
        /// <param name="currentCount">현재 주차된 면수</param>
        /// <param name="avaliableCount">주차 가능한 남은 면수</param>
        /// <param name="unableCount">주차 불가능한 주차 면수</param>
        public void Add(string partName, int maxCount, int currentCount, int avaliableCount, int unableCount)
        {
            if (arrZone == null) arrZone = new JArray();

            Zone zone = new Zone()
            {
                partName = partName,
                maxCount = maxCount,
                currentCount = currentCount,
                avaliableCount = avaliableCount,
                unableCount = unableCount
            };
            JObject obj = JObject.FromObject(zone);
            arrZone.Add(obj);
        }

        public void Remove(int idx)
        {
            arrZone.RemoveAt(idx);
        }

        public void Remove(string partName)
        {
            for (int i = arrZone.Count -1; i >= 0; i--)
            {
                JObject obj = arrZone[i] as JObject;
                if(obj != null && Helper.NVL(obj["partName"]) == partName)
                {
                    arrZone.RemoveAt(i);
                }
            }
        }
    }
}
