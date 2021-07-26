using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class AptNrAdapter : IAdapter
    {
        private const string VISIT_CHECK = "/pc/visit/check";
        private const string ALERT_INCAR = "/pc/access/in";
        private const string ALERT_OUTCAR = "/pc/access/out";
        private const string VISIT_LIST = "/pc/visit/all";

        private bool isRun = false;
        private string _Domain = string.Empty;
        private Dictionary<string, string> dicHeader = new Dictionary<string, string>();
        private StringBuilder receiveMessageBuffer = new StringBuilder();

        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning => isRun;

        public event IAdapter.ShowBallonTip ShowTip;

        public void Dispose()
        {
        }

        public bool Initialize()
        {
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "AptNrAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.Nexpa);
                return false;
            }

            _Domain = SysConfig.Instance.HW_Domain;
            Log.WriteLog(LogType.Info, $"AptNrAdapter | Initialize", $"==초기화==", LogAdpType.HomeNet);

            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            try
            {
                receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                var jobj = JObject.Parse(receiveMessageBuffer.ToString());
                Thread.Sleep(10);
                receiveMessageBuffer.Clear();

                Log.WriteLog(LogType.Info, $"AptNrAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
                JObject data = jobj["data"] as JObject; //응답 데이터

                string cmd = jobj["command"].ToString();
                CmdType command = (CmdType)Enum.Parse(typeof(CmdType), cmd);
                switch (command)
                {
                    case CmdType.visit_check:
                        {
                            string responseHeader = string.Empty;
                            string responseData = string.Empty;
                            string sendMessage = $"kaptCode={SysConfig.Instance.ParkId}&carNo={Helper.NVL(data["car_number"])}";
                            Uri uri = new Uri(string.Concat(_Domain, VISIT_CHECK));
                            if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.GET, ContentType.FormData, SysConfig.Instance.HomeNet_Encoding.GetBytes(sendMessage), ref responseData, ref responseHeader, header: dicHeader))
                            {
                                ResponseDataPayload responsePayload = new ResponseDataPayload();
                                byte[] responseBuffer = null;

                                responsePayload.command = command;
                                responsePayload.result = ResultType.OK;
                                try
                                {
                                    ResponseApnVisitCheckPayload dataPayload = new ResponseApnVisitCheckPayload();

                                    JObject dataJson = new JObject();
                                    var responseJobj = JObject.Parse(responseData);
                                    if (Helper.NVL(responseJobj["isVisitor"]) == "Y")
                                    {
                                        dataJson["check_yon"] = "Y";
                                        dataJson["dong"] = Helper.NVL(responseJobj["dong"]);
                                        dataJson["ho"] = Helper.NVL(responseJobj["ho"]);
                                        dataJson["remark"] = Helper.NVL(responseJobj["purpose"]);
                                    }
                                    else
                                    {
                                        dataJson["check_yon"] = "N";
                                    }
                                    dataPayload.Deserialize(dataJson);
                                    responsePayload.data = dataPayload;
                                    responseBuffer = responsePayload.Serialize();
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteLog(LogType.Error, $"AptNrAdapter | SendMessage", $"WebClientSendDataException : {ex.Message}", LogAdpType.HomeNet);
                                    responsePayload.result = ResultType.ExceptionERROR;
                                }
                                finally
                                {
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                }
                            }
                        }
                        break;
                    case CmdType.alert_incar: //TODO : 웹통합 - 방문증 출력 시 동호수 입력하는데 그때 같이 보내야 함.... 한글일땐 상관없음. 숫자만..
                    case CmdType.alert_outcar:
                        {
                            string responseHeader = string.Empty;
                            string responseData = string.Empty;
                            StringBuilder sendMessage = new StringBuilder($"kaptCode={SysConfig.Instance.ParkId}&carNo={Helper.NVL(data["car_number"])}");
                            Uri uri;
                            if (command == CmdType.alert_incar)
                            {
                                uri = new Uri(string.Concat(_Domain, ALERT_INCAR));
                                sendMessage.Append($"&dong={Helper.NVL(data["dong"])}");
                                sendMessage.Append($"&ho={Helper.NVL(data["ho"])}");
                                string kind = Helper.NVL(data["kind"]) == "a" ? "Y" : "N";
                                sendMessage.Append($"&isResident={kind}"); 
                            }
                            else
                            {
                                uri = new Uri(string.Concat(_Domain, ALERT_OUTCAR));
                            }

                            sendMessage.Append($"&visitDate={Helper.NVL(data["date_time"]).ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss")}");

                            if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.FormData, SysConfig.Instance.HomeNet_Encoding.GetBytes(sendMessage.ToString()), ref responseData, ref responseHeader, header: dicHeader))
                            {
                                ResponsePayload responsePayload = new ResponsePayload();
                                responsePayload.command = command;
                                //$"ERR,{(int)response.StatusCode},{response.StatusCode.ToString()}";
                                if (responseData.StartsWith("ERR"))
                                {
                                    //Error 처리...
                                    responsePayload.result = ResultType.ExceptionERROR;
                                }
                                else
                                {
                                    responsePayload.result = ResultType.OK;
                                }

                                byte[] responseBuffer = responsePayload.Serialize();
                                TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                            }
                        }
                        break;
                    case CmdType.visit_list:
                        {
                            //TODO : 안쓰인다고 함...;;
                        }
                        break;
                }

            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "AptNrAdapter | SendMessage", $"Exception Message : {ex.Message}", LogAdpType.HomeNet);
            }
        }

        public bool StartAdapter()
        {
            dicHeader.Add("Authorization", SysConfig.Instance.AuthToken);
            return true;
        }

        public bool StopAdapter()
        {
            return true;
        }

        public void TestReceive(byte[] buffer)
        {
            string json = "{\"command\": \"alert_incar\",\"data\": {\"dong\" : \"101\"," +
                            "\"ho\" : \"101\"," +
                            $"\"car_number\" : \"11가1111\"," +
                            "\"date_time\" : \"20210504091225\"," +
                            "\"kind\" : \"v\"," +
                            "\"lprid\" : \"Lpr 식별 번호\"," +
                            "\"car_image\" : \"차량 이미지 경로\"," +
                            $"\"reg_no\" : \"111111\"," +
                            "\"visit_in_date_time\" : \"yyyyMMddHHmmss\"," + //방문시작일시, kind가 v 일 경우 외 빈값
                            "\"visit_out_date_time\" : \"yyyyMMddHHmmss\"" + //방문종료일시, kind가 v 일 경우 외 빈값
                            "}" +
                            "}";
            byte[] test = SysConfig.Instance.Nexpa_Encoding.GetBytes(json);
            SendMessage(test, 0, test.Length);
        }
    }
}
