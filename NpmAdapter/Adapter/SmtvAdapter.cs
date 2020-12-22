using HttpServer;
using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NLog.Targets;
using NpmAdapter.Payload;
using NpmAdapter.Payload.CommaxDaelim.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Xml;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 스마트빌리지 아답터
    /// </summary>
    class SmtvAdapter : IAdapter
    {
        private const string REQ_POST_STATUS = "/api/v1/events";

        private Uri uri = null;
        private Dictionary<string, string> dicHeader;
        private bool isRun = false;

        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning { get => isRun; }

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "SmtvAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.HomeNet);
                return false;
            }
            dicHeader = new Dictionary<string, string>();
            string domain = SysConfig.Instance.HW_Domain;
            string strAuth = SysConfig.Instance.HC_Id + ":" + SysConfig.Instance.HC_Pw;

            uri = new Uri(domain + REQ_POST_STATUS);
            dicHeader.Add("Authorization", $"Basic {strAuth.Base64Encode()}");
            Log.WriteLog(LogType.Info, "SmtvAdapter | Initialize", $"인증토큰 : Basic {strAuth.Base64Encode()}", LogAdpType.HomeNet);
            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size)
        {
            var jobj = JObject.Parse(buffer.ToString(SysConfig.Instance.HomeNet_Encoding, size));
            JObject data = jobj["data"] as JObject;
            string cmd = jobj["command"].ToString();

            JObject result = new JObject();
            //SmtvLotID
            switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
            {
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                    payload.Deserialize(jobj);

                    result["parkingLotId"] = SysConfig.Instance.ParkId;

                    if((CmdType)Enum.Parse(typeof(CmdType), cmd) == CmdType.alert_incar)
                    {
                        result["type"] = "COME_IN";
                    }
                    else
                    {
                        result["type"] = "GO_OUT";
                    }

                    result["carNo"] = payload.data.car_number;
                    result["eventDt"] = payload.data.date_time.ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");

                    Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage | WebClientResponse", $"전송메시지 {result}", LogAdpType.HomeNet);

                    byte[] requestData = result.ToByteArray(SysConfig.Instance.HomeNet_Encoding);
                    string responseData = string.Empty;
                    if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.PUT, ContentType.Json, requestData, ref responseData, dicHeader))
                    {
                        Log.WriteLog(LogType.Info, "SmtvAdapter | SendMessage | WebClientResponse", $"==응답== {responseData}", LogAdpType.HomeNet);
                        
                        ResponsePayload responsePayload = new ResponsePayload();
                        byte[] responseBuffer;

                        responsePayload.command = payload.command;
                        responsePayload.result = ResultType.OK;
                        responseBuffer = responsePayload.Serialize();

                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                    }

                    break;
                default:
                    return;
            }
        }

        public bool StartAdapter()
        {
            return true;
        }

        public bool StopAdapter()
        {
            return true;
        }

        public void TestReceive(byte[] buffer)
        {

        }
    }
}
