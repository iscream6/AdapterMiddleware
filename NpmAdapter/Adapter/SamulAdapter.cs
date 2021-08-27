using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 샘물 아답터
    /// </summary>
    class SamulAdapter : IAdapter
    {
        private string hostDomain = "";
        private string authToken = "";
        private static string REQ_POST_STATUS = "/api/lpr";
        private StringBuilder receiveMessageBuffer = new StringBuilder();

        public event IAdapter.ShowBallonTip ShowTip;

        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => true;

        public string reqPid { get; set; }

        public void Dispose()
        {
            //Do nothing...
        }

        public bool Initialize()
        {
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "SamulAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.HomeNet);
                return false;
            }

            hostDomain = SysConfig.Instance.HW_Domain;
            authToken = SysConfig.Instance.AuthToken;

            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(receiveMessageBuffer.ToString());
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"SamulAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
            JObject data = jobj["data"] as JObject;
            string cmd = jobj["command"].ToString();
            switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
            {
                //입/출차 통보
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    {
                        try
                        {
                            byte[] ret;
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);

                            StringBuilder header = new StringBuilder();
                            header.Append("Content-Disposition: form-data; ");
                            header.Append($"authToken=\"{authToken}\"; ");
                            header.Append($"datetime=\"{payload.data.date_time}\"; ");
                            header.Append($"cameraID=\"{payload.data.lprID}\"; ");
                            header.Append($"lpnum=\"{payload.data.car_number}\"");

                            string sTempLogMsg = header.ToString();

                            if(payload.data.car_image != null && payload.data.car_image != string.Empty)
                            {
                                header.Append($"\r\n");
                                header.Append($"Content-Type: image/jpeg\r\n\r\n");

                                byte[] bHeader = SysConfig.Instance.HomeNet_Encoding.GetBytes(header.ToString());
                                using (Stream stream = new MemoryStream())
                                {
                                    stream.Write(bHeader, 0, bHeader.Length);

                                    //==== 여기부터 File Byte 구성 =====
                                    FileInfo fileInfo = new FileInfo(payload.data.car_image);

                                    using(Stream fileStream = fileInfo.OpenRead())
                                    {
                                        fileStream.CopyTo(stream);
                                    }
                                    //stream을 byte로 변환할 버퍼 생성
                                    ret = new byte[stream.Length];
                                    //스트림 seek 이동
                                    stream.Seek(0, SeekOrigin.Begin);
                                    //스트림을 byte로 이동
                                    stream.Read(ret, 0, ret.Length);
                                }
                            }
                            else
                            {
                                ret = SysConfig.Instance.HomeNet_Encoding.GetBytes(header.ToString());
                            }

                            string responseData = string.Empty;

                            Log.WriteLog(LogType.Info, $"SamulAdapter | SendMessage", $"전송 URI : {hostDomain + REQ_POST_STATUS}", LogAdpType.HomeNet);
                            if (NetworkWebClient.Instance.SendDataPost(new Uri($"{hostDomain + REQ_POST_STATUS}"), 
                                ret, ref responseData, ContentType.Multipart_FormData))
                            {
                                var serverResponse = JObject.Parse(responseData);
                                string code = serverResponse.Value<string>("code");
                                string message = serverResponse.Value<string>("message");
                                if (!code.StartsWith("2"))
                                {
                                    //2가 아니라면 에러임. Log를 남긴다.
                                    Log.WriteLog(LogType.Info, $"SamulAdapter | SendMessage", $"Error Result:{code}, Error Message:{message}", LogAdpType.HomeNet);
                                }
                            }
                            else
                            {
                                //WebClient Send Post 실패
                                Log.WriteLog(LogType.Error, $"SamulAdapter | SendMessage", $"WebClient Send Post 실패");
                            }

                            Log.WriteLog(LogType.Info, $"SamulAdapter | SendMessage", $"전송 완료 : {sTempLogMsg}", LogAdpType.HomeNet);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(LogType.Error, $"SamulAdapter | SendMessage", $"{ex.Message}");
                        }
                    }
                    break;
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
            throw new NotImplementedException();
        }
    }
}
