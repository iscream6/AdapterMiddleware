using Newtonsoft.Json.Linq;
using NpmAdapter.Payload;
using NpmCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class MilitaryAdapter : IAdapter
    {
        Dictionary<string, string> dicHeader = new Dictionary<string, string>();

        private bool isRun = false;
        private bool bResponseSuccess = false;
        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private Uri smtUri;

        public event IAdapter.ShowBallonTip ShowTip;

        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning => isRun;
        public string reqPid { get; set; }
        public void Dispose()
        {

        }

        public bool Initialize()
        {
            smtUri = new Uri(SysConfig.Instance.HW_Domain);
            return true;
        }

        public bool StartAdapter()
        {
            isRun = true;
            return isRun;
        }

        public bool StopAdapter()
        {
            isRun = false;
            return isRun;
        }

        private void Test()
        {
            //Test
            JObject responseTest = new JObject();
            responseTest["command"] = "cust_reg";
            JObject responseDataTest = new JObject();
            responseDataTest["park_no"] = "1";
            responseDataTest["reg_no"] = "12345";
            responseDataTest["car_number"] = "11가1111";
            responseDataTest["type"] = "1";
            responseTest["data"] = responseDataTest;
            JObject responseResultTest = new JObject();
            responseResultTest["status"] = "200";
            responseResultTest["message"] = "OK";
            responseTest["result"] = responseResultTest;

            byte[] test = responseTest.ToByteArray(SysConfig.Instance.Nexpa_Encoding);
            this.SendMessage(test, 0, test.Length);
        }

        /// <summary>
        /// Nexpa 관제로부터 수신
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            try
            {
                receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));

                Thread.Sleep(10);
                receiveMessageBuffer.Clear();

                Log.WriteLog(LogType.Info, $"MilitaryAdapter | SendMessage", $"Adapter 수신\n{jobj}", LogAdpType.HomeNet);

                string cmd = jobj["command"].ToString();
                JObject data = jobj["data"] as JObject;
                switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
                {
                    case CmdType.alert_incar:
                    case CmdType.alert_outcar:
                        {
                            ResponsePayload responsePayload = new ResponsePayload();
                            byte[] responseBuffer;
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);

                            //if (payload.data.kind.Equals("n"))
                            //{
                            //    responsePayload.result = ResultType.NonContent; //처리할게 없음.
                            //    responsePayload.command = payload.command;
                            //    responseBuffer = responsePayload.Serialize();
                            //    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, pid);
                            //    break;
                            //}

                            //car_imgae 의 car_number 와 car_number 간 비교를 통해 두 차량번호가 다르다면
                            //car_image 의 car_number 로 car_number 필드를 update 한다.
                            string car_img = payload.data.car_image;
                            var imgIdx = car_img.LastIndexOf('_') + 1;
                            if (imgIdx != 0)
                            {
                                var jpg = car_img.Substring(imgIdx);
                                var jIdx = jpg.LastIndexOf('.');
                                var valid_Car_number = jpg.Substring(0, jIdx);

                                if (payload.data.car_number != valid_Car_number)
                                {
                                    Log.WriteLog(LogType.Info, "MilitaryAdapter | SendMessage", $"== 차량 번호 보정 : {payload.data.car_number} -> {valid_Car_number}", LogAdpType.HomeNet);
                                    payload.data.car_number = valid_Car_number;
                                }
                            }

                            SmmIOPayload ioPayload = new SmmIOPayload(payload.command);
                            ioPayload.Deserialize(data);

                            byte[] requestData = ioPayload.Serialize();
                            string responseData = string.Empty;
                            string responseHeader = string.Empty;

                            Log.WriteLog(LogType.Info, "MilitaryAdapter | SendMessage", $"Web 송신\n{ioPayload.ToJson()}", LogAdpType.HomeNet);

#if (DEBUG)
                        //디버깅 모드에서는 Web Server 통신 없음.
                        responsePayload.result = ResultType.OK;
                        responsePayload.command = payload.command;
                        responseBuffer = responsePayload.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, pid);
#elif (!DEBUG)
                            if (NetworkWebClient.Instance.SendData(smtUri, NetworkWebClient.RequestType.POST, ContentType.Json, requestData, ref responseData, ref responseHeader, header: dicHeader))
                            {
                                try
                                {
                                    if (responseData.StartsWith("ERR"))
                                    {
                                        Log.WriteLog(LogType.Error, $"MilitaryAdapter | SendMessage", $"Web 수신\n{responseData}", LogAdpType.HomeNet);
                                        string[] errMsgs = responseData.Split(',');
                                        //Error 처리...
                                        switch (errMsgs[1])
                                        {
                                            case "400":
                                                responsePayload.result = ResultType.BadRequest;
                                                break;
                                            case "401":
                                                responsePayload.result = ResultType.Unauthorized;
                                                break;
                                            case "404":
                                                responsePayload.result = ResultType.NotFound;
                                                break;
                                            case "405":
                                                responsePayload.result = ResultType.MethodNotAllowed;
                                                break;
                                            default:
                                                responsePayload.result = ResultType.Fail;
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        responsePayload.result = ResultType.OK;
                                    }

                                    var responseJobj = JObject.Parse(responseData);

                                    if (responseJobj != null && Helper.NVL(responseJobj["responseCode"]) == "2000")
                                    {
                                        responsePayload.command = payload.command;
                                        responseBuffer = responsePayload.Serialize();
                                    }
                                    else
                                    {
                                        responsePayload.command = payload.command;
                                        responsePayload.result = ResultType.ExceptionERROR;
                                        responseBuffer = responsePayload.Serialize();
                                    }

                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, pid);
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteLog(LogType.Error, "MilitaryAdapter | SendMessage", $"{ex.StackTrace}", LogAdpType.HomeNet);
                                }
                            }
#endif

                        }
                        break;
                }
            }
            catch (Exception)
            {
                receiveMessageBuffer.Clear();
                throw;
            }
            
        }

        public void TestReceive(byte[] buffer)
        {
            string json = "{\"command\": \"alert_incar\",\"data\": {\"dong\" : \"101\"," +
                            "\"ho\" : \"101\"," +
                            $"\"car_number\" : \"46부5989\"," +
                            "\"date_time\" : \"20210305042525\"," +
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
