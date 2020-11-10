using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace NpmAdapter.Adapter
{
    class CmxAdapter : IAdapter
    {
        private bool isRun;
        private string tcpServerIp = "0.0.0.0";
        private string tcpPort = "29712";
        private string myport = "42143";

        private INetwork MyTcpClientNetwork { get; set; }
        private INetwork MyTcpServerNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => isRun;

        public void Dispose()
        {
            //Do nothing...
        }

        public bool Initialize()
        {
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "CmxAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.HomeNet);
                return false;
            }

            tcpServerIp = SysConfig.Instance.HT_IP;
            tcpPort = SysConfig.Instance.HT_Port;
            myport = SysConfig.Instance.HT_MyPort;

            MyTcpClientNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, tcpServerIp, tcpPort);
            MyTcpServerNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpServer, myport);

            return true;
        }

        public bool StartAdapter()
        {
            try
            {
                MyTcpClientNetwork.ReceiveFromPeer += MyTcpClientNetwork_ReceiveFromPeer;
                MyTcpServerNetwork.ReceiveFromPeer += MyTcpServerNetwork_ReceiveFromPeer;
                isRun = MyTcpServerNetwork.Run();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "CmxAdapter | StartAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }
            
            return isRun;
        }

        public bool StopAdapter()
        {
            try
            {
                MyTcpClientNetwork.ReceiveFromPeer -= MyTcpClientNetwork_ReceiveFromPeer;
                MyTcpServerNetwork.ReceiveFromPeer -= MyTcpServerNetwork_ReceiveFromPeer;
                isRun = !MyTcpServerNetwork.Down();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "CmxAdapter | StopAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }
            return isRun;
        }

        private void MyTcpServerNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null)
        {
            throw new NotImplementedException();
        }

        private void MyTcpClientNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null)
        {
            Log.WriteLog(LogType.Info, $"CmxDLAdapter | MyTcpClientNetwork_ReceiveFromPeer", $"받은 메시지 : {buffer}", LogAdpType.HomeNet);
            MyTcpClientNetwork.Down();
        }

        public void SendMessage(byte[] buffer, long offset, long size)
        {
            try
            {
                var jobj = JObject.Parse(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
                JObject data = jobj["data"] as JObject;
                switch (buffer[..(int)size].GetCommand(SysConfig.Instance.Nexpa_Encoding))
                {
                    #region 입출차 통보

                    //입/출차 통보
                    case CmdType.alert_incar:
                    case CmdType.alert_outcar:
                        //TCPNetwork를 연결한다.
                        if (MyTcpClientNetwork.Run())
                        {
                            //Json값을 파싱하자.
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);

                            //동호가 없으면 PASS시킨다.
                            if (payload.data.dong == null || payload.data.ho == null || payload.data.dong == "" || payload.data.ho == "")
                            {
                                ResponseResultPayload resultPayload = new ResponseResultPayload();
                                resultPayload.command = payload.command;
                                resultPayload.Result = ResponseResultPayload.Status.FailFormatError;
                                byte[] result = resultPayload.Serialize();
                                TargetAdapter.SendMessage(result, 0, result.Length);
                                Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"전송메시지 : {resultPayload.ToJson().ToString()}", LogAdpType.Nexpa);
                            }
                            else
                            {
                                string sInOut = "in";
                                if (payload.command == CmdType.alert_incar)
                                {
                                    sInOut = "in";
                                }
                                else if (payload.command == CmdType.alert_outcar)
                                {
                                    sInOut = "out";
                                }

                                if (payload.data.kind != null && payload.data.kind != "")
                                {
                                    if (payload.data.kind.ToUpper().Equals("V")) sInOut = payload.data.kind.ToLower() + sInOut;
                                }

                                XmlDocument doc = new XmlDocument();
                                XmlElement cmx = doc.CreateElement("cmx");
                                XmlElement park = doc.CreateElement("park");

                                XmlElement dong = doc.CreateElement("dong");
                                dong.InnerText = $"{payload.data.dong}";
                                XmlElement ho = doc.CreateElement("ho");
                                ho.InnerText = $"{payload.data.ho}";
                                XmlElement car = doc.CreateElement("car");
                                car.InnerText = $"{payload.data.car_number}";
                                XmlElement inout = doc.CreateElement("inout");
                                inout.InnerText = $"{sInOut}";
                                DateTime dateTime = DateTime.ParseExact(payload.data.date_time, "yyyyMMddHHmmss", null);
                                XmlElement year = doc.CreateElement("year");
                                year.InnerText = $"{dateTime.Year}";
                                XmlElement mon = doc.CreateElement("mon");
                                mon.InnerText = $"{dateTime.Month}";
                                XmlElement day = doc.CreateElement("day");
                                day.InnerText = $"{dateTime.Day}";
                                XmlElement hour = doc.CreateElement("hour");
                                hour.InnerText = $"{dateTime.Hour}";
                                XmlElement min = doc.CreateElement("min");
                                min.InnerText = $"{dateTime.Minute}";
                                XmlElement sec = doc.CreateElement("sec");
                                sec.InnerText = $"{dateTime.Second}";

                                park.AppendChild(dong);
                                park.AppendChild(ho);
                                park.AppendChild(car);
                                park.AppendChild(inout);
                                park.AppendChild(year);
                                park.AppendChild(mon);
                                park.AppendChild(day);
                                park.AppendChild(hour);
                                park.AppendChild(min);
                                park.AppendChild(sec);

                                cmx.AppendChild(park);

                                byte[] dataBytes;
                                using (StringWriter stringWriter = new StringWriter())
                                using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
                                {
                                    cmx.WriteTo(xmlTextWriter);
                                    xmlTextWriter.Flush();
                                    Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"전송메시지 : {stringWriter.GetStringBuilder().ToString()}", LogAdpType.HomeNet);
                                    dataBytes = stringWriter.GetStringBuilder().ToString().ToByte(SysConfig.Instance.HomeNet_Encoding);
                                }

                                //코맥스 TCP Server로 Data를 전송한다.
                                MyTcpClientNetwork.SendToPeer(dataBytes, 0, dataBytes.Length);

                                //넥스파로 잘 받았다고 응답처리하자.
                                ResponseResultPayload resultPayload = new ResponseResultPayload();
                                resultPayload.command = payload.command;
                                resultPayload.Result = ResponseResultPayload.Status.OK;
                                byte[] result = resultPayload.Serialize();
                                TargetAdapter.SendMessage(result, 0, result.Length);
                            }
                        }
                        else
                        {
                            //연결 실패함. 넥스파로 실패 로그처리....
                            Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"연결실패함", LogAdpType.HomeNet);
                        }
                        break;

                    #endregion
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"CmxDLAdapter | SendMessage", $"Nexpa Adpater로부터 온 Message를 처리 중 오류 : {ex.Message}", LogAdpType.HomeNet);
                throw;
            }
        }

        public void TestReceive(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
