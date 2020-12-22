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
using System.Threading.Tasks;
using System.Xml;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 2020-09-07 코맥스 대림 연동 (웹 서버, Tcp Client)
    /// </summary>
    class CmxDLAdapter : IAdapter
    {
        public enum Status
        {
            Full,
            TcpOnly,
            WebOnly
        }
        
        public static string REQ_POST_STATUS = "/nxmdl/cmx";

        private object lockObj = new object();
        private string webport = "42141";
        private string tcpServerIp = "0.0.0.0";
        private string tcpport = "29712";
        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private ResponseCmdPayload responsePayload = null;
        private SyncResonseWait resonseWait;
        /// <summary>
        /// Adapter 실행모드 : TCP/WEB 
        /// </summary>
        private Status runStatus = Status.Full;
        //==== Alive Check ====
        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private delegate void SafeCallDelegate();
        private bool isRun;
        //==== Alive Check ====

        private INetwork MyTcpNetwork { get; set; }
        private INetwork MyHttpNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning { get => isRun; }

        public CmxDLAdapter(Status status)
        {
            runStatus = status;
        }

        public bool Initialize()
        {
            //Config Version Check~!
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "CmxDLAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.HomeNet);
                return false;
            }

            resonseWait = new SyncResonseWait();

            webport = SysConfig.Instance.HW_Port;
            tcpServerIp = SysConfig.Instance.HT_IP;
            tcpport = SysConfig.Instance.HT_Port;
            
            MyHttpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, webport);
            MyTcpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, tcpServerIp, tcpport);

            Log.WriteLog(LogType.Info, $"CmxDLAdapter | Initialize", $"TpcNetwork iP :{tcpServerIp}, Port :{tcpport}", LogAdpType.HomeNet);

            responsePayload = new ResponseCmdPayload();
            //responseResult = new StatusPayload();

            //Alive Check
            if (SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckUse").Equals("Y"))
            {
                aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
                aliveCheckThread.Name = "Cmx thread for alive check";
                if (!TimeSpan.TryParse(SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckTime"), out waitForWork))
                {
                    //Default 30분
                    waitForWork = TimeSpan.FromMinutes(30);
                }
            }
            //Alive Check

            return true;
        }

        public bool StartAdapter()
        {
            try
            {
                switch (runStatus)
                {
                    case Status.Full:
                        MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
                        MyTcpNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
                        isRun = MyHttpNetwork.Run();
                        break;
                    case Status.TcpOnly:
                        MyTcpNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
                        isRun = true;
                        break;
                    case Status.WebOnly:
                        MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
                        isRun = MyHttpNetwork.Run();
                        break;
                }

                if (SysConfig.Instance.Sys_Option.GetValueToUpper("CmxAliveCheckUse").Equals("Y"))
                {
                    //Alive Check Thread 시작
                    if (aliveCheckThread.IsAlive)
                    {
                        _pauseEvent.Set();
                    }
                    else
                    {
                        aliveCheckThread.Start();
                        _pauseEvent.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "CmxDLAdapter | StartAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }

            return isRun;
        }

        public bool StopAdapter()
        {
            bool bResult = false;
            try
            {
                switch (runStatus)
                {
                    case Status.Full:
                        MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
                        MyTcpNetwork.ReceiveFromPeer -= MyTcpNetwork_ReceiveFromPeer;
                        bResult = MyHttpNetwork.Down();
                        break;
                    case Status.TcpOnly:
                        MyTcpNetwork.ReceiveFromPeer -= MyTcpNetwork_ReceiveFromPeer;
                        bResult = true;
                        break;
                    case Status.WebOnly:
                        MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
                        bResult = MyHttpNetwork.Down();
                        break;
                }

                //Alive Check Thread pause
                _pauseEvent.Reset();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "CmxDLAdapter | StopAdapter", $"{ex.Message}", LogAdpType.HomeNet);
                return false;
            }
            isRun = !bResult;
            return bResult;
        }

        /// <summary>
        /// 30분 단위로 AliveCheck 를 날린다.
        /// </summary>
        private void AliveCheck()
        {
            do
            {
                if (shutdownEvent.IsSet) return;

                {
                    //Alive Check 서버로 전달....
                    Log.WriteLog(LogType.Info, $"CmxDLAdapter | AliveCheck", $"Alive Check~!");
                    
                    try
                    {
                        //s_prod_company : 업체명
                        //s_prod_type : 2002 주차관제
                        //s_prod_version : product version
                        //s_prod_mac : mac address
                        //s_prod_status : 서버상태 (1:OK, 2:NG)
                        string postMessage =
                            $"s_prod_company=Nexpa" +
                            $"&s_prod_type=2002" +
                            $"&s_prod_version={SysConfig.Instance.Version}" +
                            $"&s_prod_mac={NetworkInterface.GetAllNetworkInterfaces()[0].GetPhysicalAddress().ToString()}" +
                            $"&s_prod_status={(TargetAdapter.IsRuning ? "1" : "2")}";
                        string responseData = string.Empty;

                        if (NetworkWebClient.Instance.SendDataPost(new Uri($"{SysConfig.Instance.Sys_Option.GetValue("CmxAliveCheckURL")}"), SysConfig.Instance.HomeNet_Encoding.GetBytes(postMessage), ref responseData, ContentType.FormData ))
                        {
                            var serverResponse = JObject.Parse(responseData);
                            string result = serverResponse.Value<string>("result");
                            string message = serverResponse.Value<string>("message");
                            if (!result.StartsWith("2"))
                            {
                                //2가 아니라면 에러임. Log를 남긴다.
                                Log.WriteLog(LogType.Info, $"CmxDLAdapter | AliveCheck", $"Error Result:{result}, Error Message:{message}", LogAdpType.HomeNet);
                            }
                        }
                        else
                        {
                            //WebClient Send Post 실패
                            Log.WriteLog(LogType.Error, $"CmxDLAdapter | AliveCheck", $"WebClient Send Post 실패");
                        }
                    }
                    catch (Exception)
                    {

                    }
                }

                shutdownEvent.Wait(waitForWork);
            }
            while (_pauseEvent.WaitOne());
        }

        public void TestReceive(byte[] buffer)
        {
            MyTcpNetwork_ReceiveFromPeer(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 대림코맥스 TCP로부터 요청 메시지 처리
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="pEvent"></param>
        private void MyTcpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null)
        {
            //Client 연결을 끊어줘야 하나?? 응답 메시지가 뭐가 날라올지 모르겠다....
            //일단 연결을 끊는다.
            Log.WriteLog(LogType.Info, $"CmxDLAdapter | MyTcpNetwork_ReceiveFromPeer", $"대림받은 메시지 : {buffer}", LogAdpType.HomeNet);
            MyTcpNetwork.Down();
        }

        /// <summary>
        /// 대림코맥스 Http로부터 요청 메시지 처리
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="e"></param>
        private void MyHttpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs e = null)
        {
            lock (lockObj)
            {
                responsePayload.Initialize();
                JObject json = JObject.Parse(SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]));

                //DefaultURL을 만들어야 함.....
                //http://localhost:42142/nxmdl/cmx/... 까지..
                string urlData = e.Request.Uri.PathAndQuery;
                Log.WriteLog(LogType.Info, $"CmxDLAdapter | MyHttpNetwork_ReceiveFromPeer", $"URL : {urlData}", LogAdpType.HomeNet);
                if (urlData != REQ_POST_STATUS)
                {
                    e.Response.Connection.Type = ConnectionType.Close;
                    e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                    e.Response.Status = System.Net.HttpStatusCode.BadRequest;
                    e.Response.Reason = "Bad Request";

                    //에러 메시지를 만들어 대림으로 보낸다.
                    CmdHeader responseHeader = new CmdHeader();
                    responseHeader.Deserialize(json["header"] as JObject);
                    responsePayload.header = responseHeader;
                    responsePayload.data = null;
                    responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.bad_request);
                    byte[] result = responsePayload.Serialize();

                    e.Response.Body.Write(result, 0, result.Length);
                }
                else
                {
                    Log.WriteLog(LogType.Info, $"CmxDLAdapter | MyHttpNetwork_ReceiveFromPeer", $"{json}", LogAdpType.HomeNet);
                    RequestToNexpa(json);
                    byte[] result = responsePayload.Serialize();

                    e.Response.Connection.Type = ConnectionType.Close;
                    e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                    e.Response.Reason = "OK";
                    e.Response.Body.Write(result, 0, result.Length);
                }
            }
        }

        /// <summary>
        /// Nexpa로 데이터를 전달하여 응답값을 받아 리턴한다.
        /// </summary>
        private void RequestToNexpa(JObject json)
        {
            // ResponsePayload의 Header를 만든다.
            CmdHeader responseHeader = new CmdHeader();
            responseHeader.Deserialize(json["header"] as JObject);
            responsePayload.header = responseHeader;

            resonseWait.bResponseSuccess = false;
            //넥스파 Payload를 가져온다.
            CmdHelper.ResultReqInfo result = CmdHelper.ConvertNexpaRequestPayload(json);

            if (result.type != CmdType.none)
            {
                //넥스파로 payload를 보낸다.
                byte[] sendMsg = result.payload.Serialize();

                TargetAdapter.SendMessage(sendMsg, 0, sendMsg.Length);

                int iSec = 3 * 100; //3초
                while (iSec > 0 && !resonseWait.bResponseSuccess)
                {
                    Thread.Sleep(10); //0.01초씩..쉰다...
                    iSec -= 1;
                }

                receiveMessageBuffer.Clear();

                if (resonseWait.bResponseSuccess) //응답성공
                {
                    //응답이 왔으므로.... Data는 채워져 있을거임...
                    if(responsePayload.result == null)
                    {
                        responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.ok);
                    }
                }
                else //응답 실패
                {
                    //에러 Payload를 만들어 보낸다.
                    switch (responsePayload.header.type)
                    {
                        case CmdHelper.Type.visitor_car_book_add:
                        case CmdHelper.Type.visitor_car_book_delete:
                        case CmdHelper.Type.location_nick:
                            responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notinterface_kwanje);
                            break;
                        case CmdHelper.Type.location_list:
                        case CmdHelper.Type.location_map:
                            responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notinterface_udo);
                            break;
                        default:
                            responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notresponse);
                            break;
                    }
                }
            }
            else //요청 페이로드를 못만들었다.
            {
                //에러 Payload를 만들어 보낸다.
                responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notresponse);
            }
        }

        

        /// <summary>
        /// Nexpa Adpater로부터 온 Message를 처리한다. JSon임...
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendMessage(byte[] buffer, long offset, long size)
        {
            try
            {
                receiveMessageBuffer.Append(buffer[..(int)size].ToString(SysConfig.Instance.Nexpa_Encoding, size));
                var jobj = JObject.Parse(receiveMessageBuffer.ToString());
                Thread.Sleep(10);
                receiveMessageBuffer.Clear();

                Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
                JObject data = jobj["data"] as JObject;

                if (data == null || data.Count == 0) return;

                string cmd = jobj["command"].ToString();
                switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
                {
                    #region 입출차 통보

                    //입/출차 통보
                    case CmdType.alert_incar:
                    case CmdType.alert_outcar:
                        if (runStatus == Status.WebOnly) return;
                        //TCPNetwork를 연결한다.
                        if (MyTcpNetwork.Run())
                        {
                            //Json값을 파싱하자.
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);

                            //동호가 없으면 PASS시킨다.
                            if(payload.data == null || payload.data.dong == null || payload.data.ho == null || payload.data.dong == "" || payload.data.ho == "")
                            {
                                ResponsePayload resultPayload = new ResponsePayload();
                                resultPayload.command = payload.command;
                                resultPayload.result = ResultType.FailFormatError;
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

                                //코맥스 대림 TCP로 Data를 전송한다.
                                MyTcpNetwork.SendToPeer(dataBytes, 0, dataBytes.Length);
                                Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"전송완료", LogAdpType.HomeNet);
                                //넥스파로 잘 받았다고 응답처리하자.
                                ResponsePayload resultPayload = new ResponsePayload();
                                resultPayload.command = payload.command;
                                resultPayload.result = ResultType.OK;
                                byte[] result = resultPayload.Serialize();
                                
                                TargetAdapter.SendMessage(result, 0, result.Length);
                                MyTcpNetwork.Down();
                            }
                        }
                        else
                        {
                            //연결 실패함. 넥스파로 실패 로그처리....
                            Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"연결실패함", LogAdpType.HomeNet);
                        }
                        break;

                    #endregion

                    #region 입차리스트 응답 처리

                    case CmdType.incar_list: //입차리스트 응답
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;

                        {
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdListData<ResponseCmdIncarListData> dataPayload = new ResponseCmdListData<ResponseCmdIncarListData>();
                                dataPayload.page = data["page"].ToString();
                                dataPayload.count = data["list_count"].ToString();
                                int nPage = 0;
                                if (int.TryParse(data["remain_page"].ToString(), out nPage))
                                {
                                    if (nPage > 0) dataPayload.next_page = "y";
                                    else dataPayload.next_page = "n";
                                }
                                else
                                {
                                    dataPayload.next_page = "n";
                                }

                                JArray list = data["list"] as JArray;
                                if (list != null)
                                {
                                    foreach (JObject item in list)
                                    {
                                        ResponseCmdIncarListData incarlst = new ResponseCmdIncarListData();
                                        incarlst.car_num = item["car_number"].ToString();
                                        incarlst.car_type = item["type"].ToString();
                                        incarlst.datetime = item["date_time"].ToString().ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                        dataPayload.list.Add(incarlst);
                                    }
                                }
                                responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());
                            }
                            else
                            {
                                
                            }

                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }

                        break;

                    #endregion

                    #region 방문자 응답 처리

                    case CmdType.visit_list: //방문자 리스트 응답
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;
                        {
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdListData<ResponseCmdVisitListData> dataPayload = new ResponseCmdListData<ResponseCmdVisitListData>();
                                dataPayload.page = data["page"].ToString();
                                dataPayload.count = data["list_count"].ToString();
                                int nPage = 0;
                                if (int.TryParse(data["remain_page"].ToString(), out nPage))
                                {
                                    if (nPage > 0) dataPayload.next_page = "y";
                                    else dataPayload.next_page = "n";
                                }
                                else
                                {
                                    dataPayload.next_page = "n";
                                }

                                JArray list = data["list"] as JArray;
                                if (list != null)
                                {
                                    foreach (JObject item in list)
                                    {
                                        ResponseCmdVisitListData visitlst = new ResponseCmdVisitListData();
                                        visitlst.dong = item["dong"]?.ToString();
                                        visitlst.ho = item["ho"]?.ToString();
                                        visitlst.reg_num = item["reg_no"].ToString();
                                        visitlst.car_num = item["car_number"].ToString();
                                        visitlst.reg_date = item["date"].ToString().ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                        visitlst.term = item["term"].ToString();
                                        visitlst.status = item["visit_flag"].ToString().ToLower() == "y" ? "방문완료" : "미방문";
                                        dataPayload.list.Add(visitlst);
                                    }
                                }
                                responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());
                            }

                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }

                        break;
                    case CmdType.visit_reg:
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;

                        {
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdRegNumData dataPayload = new ResponseCmdRegNumData();
                                dataPayload.reg_num = data["reg_no"].ToString();
                                responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());
                            }
                            else
                            {
                                responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notinterface_kwanje);
                            }

                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.visit_del:
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;

                        {
                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }
                        break;

                    #endregion

                    #region 방문자 즐겨찾기 처리

                    case CmdType.visit_favo_list:
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;

                        {
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdVisitFavoritListData dataPayload = new ResponseCmdVisitFavoritListData();
                                dataPayload.list = new List<CommaxDaelimVisitFavoritResponseData>();

                                JArray list = data["list"] as JArray;
                                if (list != null)
                                {
                                    foreach (JObject item in list)
                                    {
                                        CommaxDaelimVisitFavoritResponseData favoritData = new CommaxDaelimVisitFavoritResponseData();
                                        favoritData.car_num = item["car_number"].ToString();
                                        favoritData.register = item["register"].ToString();
                                        favoritData.reg_date = item["date_time"].ToString().ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                        favoritData.reg_num = item["reg_no"].ToString();
                                        dataPayload.list.Add(favoritData);
                                    }
                                }

                                responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());
                            }

                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.visit_favo_reg:
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;

                        {
                            var result = jobj["result"];
                            if (result != null && result.HasValues && result.Value<string>("status") == "319")
                            {
                                //방문차량 번호 중복 등록 시도 시...
                                responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.already_reg_favorit_carnumber);
                            }
                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.visit_favo_del:
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;

                        {
                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                            //503 현재 주차관제 서버와 연동중이지 않습니다
                        }
                        break;

                    #endregion

                    #region 차량위치찾기
                    case CmdType.location_map:
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;
                        Log.WriteLog(LogType.Info, "CmxDLAdapter | SendMessage", $"==location_map==", LogAdpType.HomeNet);
                        {
                            //responsePayload 의 Data를 채워넣는다......
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdLocationMapData dataPayload = new ResponseCmdLocationMapData();
                                dataPayload.car_number = data["car_number"].ToString();
                                dataPayload.alias = data["alias"].ToString();
                                dataPayload.location_text = data["location_text"].ToString();
                                dataPayload.image = data["car_image"].ToString();
                                dataPayload.pixel_x = data["pixel_x"].ToString();
                                dataPayload.pixel_y = data["pixel_y"].ToString();
                                dataPayload.datetime = data["in_datetime"].ToString();
                                responsePayload.data = dataPayload;
                            }

                            if (Helper.NVL(jobj["result"]["status"]) == "200")
                            {
                                jobj["result"]["status"] = "000";
                            }

                            ResultPayload status = new ResultPayload();
                            status.Deserialize(jobj["result"] as JObject);
                            responsePayload.result = status;

                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.location_list:
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;

                        {
                            ResponseCmdLocationListData dataPayload = new ResponseCmdLocationListData();
                            if(data != null)
                            {
                                JArray list = data["list"] as JArray;
                                if (list != null && list.Count > 0)
                                {
                                    foreach (JObject item in list)
                                    {
                                        SubLocationListData subDataPayload = new SubLocationListData();
                                        subDataPayload.car_number = item["car_number"].ToString();
                                        subDataPayload.alias = item["alias"].ToString();
                                        subDataPayload.location_text = item["location_text"].ToString();
                                        subDataPayload.datetime = item["in_datetime"].ToString();
                                        dataPayload.list.Add(subDataPayload);
                                    }
                                }
                            }

                            dataPayload.location_type = data.Value<string>("location_type");
                            responsePayload.data = dataPayload;

                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }
                        break;
                    #endregion

                    case CmdType.find_car:
                        if (runStatus == Status.TcpOnly || resonseWait.bResponseSuccess) return;

                        {
                            ResponseCmdFindCarListData dataPayload = new ResponseCmdFindCarListData();

                            JArray list = data["list"] as JArray;
                            if (list != null)
                            {
                                foreach (JObject item in list)
                                {
                                    dataPayload.car_num = item["car_number"].ToString();
                                    dataPayload.reg_date = item["date"].ToString().ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                    dataPayload.alias = item["alias"].ToString();
                                    responsePayload.data = dataPayload;
                                    break;
                                }
                            }

                            //Response 완료~!
                            resonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.modify_alias:
                        resonseWait.bResponseSuccess = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"CmxDLAdapter | SendMessage", $"Nexpa Adpater로부터 온 Message를 처리 중 오류 : {ex.Message}", LogAdpType.HomeNet);
                throw;
            }
        }

        public void Dispose()
        {
            _pauseEvent.Set();
            shutdownEvent.Set();
        }
    }
}
