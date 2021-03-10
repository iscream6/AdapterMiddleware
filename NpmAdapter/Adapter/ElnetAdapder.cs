using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Xml;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 미추홀 구청 행자부연동
    /// </summary>
    class ElnetAdapder : IAdapter
    {
        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private Uri uri = null;
        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => throw new NotImplementedException();

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            try
            {
                uri = new Uri(SysConfig.Instance.HW_Domain);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "ElnetAdapder | Initialize", $"{ex.Message}");
                return false;
            }
            
            return true;   
        }

        public bool StartAdapter()
        {
            return true;
        }

        public bool StopAdapter()
        {
            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            //Nexpa adapter로부터 수신...
            receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(receiveMessageBuffer.ToString());
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
            JObject data = jobj["data"] as JObject; //응답 데이터

            JObject result = jobj["result"] as JObject; //응답 결과
            ResultPayload resultPayload = result.GetResultPayload();

            if (resultPayload.code == "200")
            {
                string cmd = jobj["command"].ToString();
                switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
                {
                    case CmdType.alert_incar:
                    case CmdType.alert_outcar:
                        {
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);

                            XmlDocument doc = new XmlDocument();
                            // Create an XML declaration.
                            XmlDeclaration xmldecl;
                            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
                            xmldecl.Encoding = "utf-8";

                            // Add the new node to the document.
                            XmlElement root = doc.DocumentElement;
                            doc.InsertBefore(xmldecl, root);

                            XmlElement xEnvelope = doc.CreateElement("Envelope");
                            xEnvelope.SetAttribute("xmlns", "http://schemas.xmlsoap.org/soap/envelop");

                            //==== Header ====
                            XmlElement xHeader = doc.CreateElement("Header");
                            XmlElement xBasicData = doc.CreateElement("BasicData");

                            XmlElement xServiceName = doc.CreateElement("ServiceName");
                            XmlElement xParkNo = doc.CreateElement("ParkNo");

                            xServiceName.InnerText = "주차감면서비스";
                            xParkNo.InnerText = SysConfig.Instance.ParkId; //NXP01:용현5동 제 2노외, NXP02:주안2동 제 8노외

                            xBasicData.AppendChild(xServiceName);
                            xBasicData.AppendChild(xParkNo);
                            xHeader.AppendChild(xBasicData);
                            xEnvelope.AppendChild(xHeader);
                            //==== Header ====
                            //==== Body ====
                            XmlElement xBody = doc.CreateElement("Body");
                            XmlElement xRequest = doc.CreateElement("Request");
                            XmlElement xCarNo = doc.CreateElement("CarNo");
                            XmlElement xSendDateTime = doc.CreateElement("SendDateTime");

                            xCarNo.InnerText = payload.data.car_number;
                            xSendDateTime.InnerText = payload.data.date_time;

                            xRequest.AppendChild(xCarNo);
                            xRequest.AppendChild(xSendDateTime);
                            xBody.AppendChild(xRequest);
                            xEnvelope.AppendChild(xBody);
                            //==== Body ====
                            doc.AppendChild(xEnvelope);

                            // Display the modified XML document
                            Console.WriteLine(doc.OuterXml);
                            byte[] sendData = SysConfig.Instance.HomeNet_Encoding.GetBytes(doc.OuterXml);
                            string responseData = string.Empty;
                            string responseHeader = string.Empty;
                            if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Xml, sendData, ref responseData, ref responseHeader))
                            {
                                try
                                {
                                    //TODO : 응답 response xml을 분석하여 GovInterface Table에 Insert 하도록 한다.... ㅅㅂ...
                                    ResponsePayload responsePayload = new ResponsePayload();
                                    byte[] responseBuffer;

                                    var responseJobj = JObject.Parse(responseData);
                                    if (responseJobj != null && Helper.NVL(responseJobj["code"]) == "0000")
                                    {
                                        responsePayload.command = payload.command;
                                        responsePayload.result = ResultType.OK;
                                        responseBuffer = responsePayload.Serialize();
                                    }
                                    else
                                    {
                                        responsePayload.command = payload.command;
                                        responsePayload.result = ResultType.ExceptionERROR;
                                        responseBuffer = responsePayload.Serialize();
                                    }

                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteLog(LogType.Error, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{ex.Message}", LogAdpType.Nexpa);
                                }
                            }
                        }
                        break;
                }
            }
        }

        public void TestReceive(byte[] buffer)
        {
            
        }
    }
}
