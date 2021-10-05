using HttpServer;
using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NpmAdapter.Payload;
using NpmCommon;
using NpmNetwork;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class NexpaAdapter : IAdapter
    {
        public enum Status
        {
            Full,
            TcpOnly,
            WebOnly
        }
        
        private object lockObj = new object();
        private Uri uri = null;
        private Dictionary<string, string> _OptionDetail;
        private Status runStatus = Status.Full;
        private bool isRun = false;
        private byte[] arrSelfResponseData = null;
        private bool bResponseSuccess = true;
        private StringBuilder receiveMessageBuffer;
        private NpmThread ResponseThread;

        public const string REQ_POST_STATUS = "/nexpa/mdl/";
        public const string RSP_POST_STATUS = "/nxpms/v2.0/mdl";
        public event IAdapter.ShowBallonTip ShowTip;

        private INetwork MyTcpNetwork { get; set; }
        private INetwork MyHttpNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning { get => isRun; }

        public string reqPid { get; set; }

        public NexpaAdapter(Status status)
        {
            runStatus = status;
        }
        
        public bool Initialize()
        {
            reqPid = "";
            int port = 30542;
            receiveMessageBuffer = new StringBuilder();

            try
            {
                //Config Version Check~!
                if (!SysConfig.Instance.ValidateConfig)
                {
                    Log.WriteLog(LogType.Error, "NexpaTcpAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.Nexpa);
                    return false;
                }

                //옵션을가져온다. 
                _OptionDetail = new Dictionary<string, string>();

                if (runStatus == Status.TcpOnly || runStatus == Status.Full)
                {
                    int.TryParse(SysConfig.Instance.Nexpa_TcpPort, out port);
                    MyTcpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpServer, port.ToString());
                    if (MyTcpNetwork != null) Log.WriteLog(LogType.Info, $"NexpaTcpAdapter | 생성자", $"TcpServer 생성 : Port={port}", LogAdpType.Nexpa);
                    else Log.WriteLog(LogType.Info, $"NexpaAdapter | Initialize", $"TcpServer 생성실패", LogAdpType.Nexpa);

                    //TCP 통신에서만 사용한다.
                    ResponseThread = new NpmThread("", TimeSpan.FromSeconds(15));
                    ResponseThread.ThreadAction = ResponseAction;
                }

                if (runStatus == Status.WebOnly || runStatus == Status.Full)
                {
                    //http://localhost:35042/nxpms/v2.0/mdl
                    string domain = SysConfig.Instance.Nexpa_WebDomain;
                    uri = new Uri(domain + RSP_POST_STATUS);
                    Log.WriteLog(LogType.Info, $"NexpaAdapter | 생성자", $"Web Client Domain : {uri.OriginalString}", LogAdpType.Nexpa);

                    int.TryParse(SysConfig.Instance.Nexpa_WebPort, out port);
                    MyHttpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, port.ToString());
                    if (MyHttpNetwork != null) Log.WriteLog(LogType.Info, $"NexpaTcpAdapter | 생성자", $"WebServer 생성", LogAdpType.Nexpa);
                    else Log.WriteLog(LogType.Info, $"NexpaAdapter | Initialize", $"WebServer 생성실패", LogAdpType.Nexpa);
                }

            }
            catch (Exception)
            {
                //SystemStatus.GetInstnace().IsNotNull_NexpaNetwork = false;
            }

            return true;
        }

        public bool StartAdapter()
        {
            switch (runStatus)
            {
                case Status.TcpOnly:
                    Log.WriteLog(LogType.Info, "NexpaTcpAdapter | StartAdapter", "넥스파 TCP 서버 시작", LogAdpType.Nexpa);
                    ResponseThread.Start();
                    MyTcpNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
                    isRun = MyTcpNetwork.Run();
                    break;
                case Status.WebOnly:
                    Log.WriteLog(LogType.Info, "NexpaTcpAdapter | StartAdapter", "넥스파 Web 서버 시작", LogAdpType.Nexpa);
                    MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
                    isRun = MyHttpNetwork.Run();
                    break;
                case Status.Full:
                    Log.WriteLog(LogType.Info, "NexpaTcpAdapter | StartAdapter", "넥스파 Full 서버 시작", LogAdpType.Nexpa);
                    ResponseThread.Start();
                    MyTcpNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
                    MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
                    isRun = MyTcpNetwork.Run();
                    isRun &= MyHttpNetwork.Run();
                    break;
            }

            return isRun;
        }

        public bool StopAdapter()
        {
            bool bResult = false;

            switch (runStatus)
            {
                case Status.TcpOnly:
                    ResponseThread.Stop();
                    MyTcpNetwork.ReceiveFromPeer -= MyTcpNetwork_ReceiveFromPeer;
                    bResult = MyTcpNetwork.Down();
                    break;
                case Status.WebOnly:
                    MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
                    bResult = MyHttpNetwork.Down();
                    break;
                case Status.Full:
                    ResponseThread.Stop();
                    MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
                    MyTcpNetwork.ReceiveFromPeer -= MyTcpNetwork_ReceiveFromPeer;
                    bResult = MyTcpNetwork.Down();
                    bResult &= MyHttpNetwork.Down();
                    break;
            }
            isRun = !bResult;
            return bResult;
        }

        /// <summary>
        /// 출입통제에서 받은 Message를 처리한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        private void MyTcpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, RequestEventArgs pEvent = null, string id = null, System.Net.EndPoint ep = null)
        {
            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", $"[수신] {(id == null ? "" : id)} ==========", LogAdpType.Nexpa);
            if (!bResponseSuccess)
            {
                arrSelfResponseData = buffer[..(int)size];
                bResponseSuccess = true;
                Log.WriteLog(LogType.Error, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", $"응답대기", LogAdpType.Nexpa);
            }
            else
            {
                try
                {
                    JObject jobj;
                    var strReceiveData = buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size);

                    //Json 전문이 너무 긴 경우 Buffer에서 순차적으로 Data가 들어오므로 아래와같이 연결작업을 실행함.
                    if (strReceiveData.Contains("command")) //전문에 Command 는 반드시 들어가야 함..
                    {
                        receiveMessageBuffer.Clear();
                        receiveMessageBuffer.Append(strReceiveData);
                        try
                        {
                            jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
                        }
                        catch (Exception)
                        {
                            Log.WriteLog(LogType.Error, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", "JSON 변환 중 : " + strReceiveData, LogAdpType.Nexpa);
                            return;
                        }
                    }
                    else
                    {
                        receiveMessageBuffer.Append(strReceiveData);
                        try
                        {
                            jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
                        }
                        catch (Exception)
                        {
                            Log.WriteLog(LogType.Error, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", "JSON 변환 중 : " + strReceiveData, LogAdpType.Nexpa);
                            return;
                        }
                    }

                    Thread.Sleep(10);
                    receiveMessageBuffer.Clear();

                    string cmd = jobj["command"].ToString();
                    CmdType command = (CmdType)Enum.Parse(typeof(CmdType), cmd);

                    if(command == CmdType.hello)
                    {
                        Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", $"JSON 수신 : {jobj.ToString()}", LogAdpType.Nexpa);
                        //바로 응답~! 
                        ResponsePayload responsePayload = new ResponsePayload();
                        byte[] responseBuffer;
                        responsePayload.command = command;
                        responsePayload.result = ResultType.OK;
                        responseBuffer = responsePayload.Serialize();
                        MyTcpNetwork.SendToPeer(responseBuffer, 0, responseBuffer.Length, id);

                        if (Helper.NVL(jobj["data"]) == "alert")
                        {
                            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", "수신 완료(Alert) =====", LogAdpType.Nexpa);
                            return;
                        }
                        else
                        {
                            //Biz
                            TargetAdapter.reqPid = id;
                            this.reqPid = id;
                        }
                    }
                    else
                    {
                        if(command != CmdType.alert_incar && command != CmdType.alert_outcar)
                        {
                            bCompleteNexpaResponse = true;
                        }

                        byte[] sendBuffer = jobj.ToByteArray(SysConfig.Instance.Nexpa_Encoding);
                        TargetAdapter.SendMessage(sendBuffer, 0, sendBuffer.Length, id);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", $"JSON 처리 오류 : {ex.Message}", LogAdpType.Nexpa);
                    receiveMessageBuffer.Clear();
                }
            }
            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", "수신 완료 =====", LogAdpType.Nexpa);
        }

        /// <summary>
        /// 웹통합에서 받은 Message를 처리한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="pEvent"></param>
        private void MyHttpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, RequestEventArgs e = null, string id = null, System.Net.EndPoint ep = null)
        {
            string sJson = Encoding.UTF8.GetString(buffer[..(int)size]);
            JObject json = JObject.Parse(sJson);
            //string cmd = json["command"].AsString();
            string cmd = json["command"].ToString();
            CmdType cmdType = CmdType.none;
            cmdType = (CmdType)Enum.Parse(typeof(CmdType), cmd);


            string urlData = e.Request.Uri.PathAndQuery;

            if (urlData != REQ_POST_STATUS)
            {
                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyHttpNetwork_ReceiveFromPeer", $"잘못된 URL : {urlData}, 전송한 Command : {cmdType}", LogAdpType.Nexpa);
                e.Response.Connection.Type = ConnectionType.Close;
                e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                e.Response.Status = System.Net.HttpStatusCode.BadRequest;
                e.Response.Reason = "Bad Request";
                //에러를 보내자...
                ResponsePayload resultPayload = new ResponsePayload();
                resultPayload.command = cmdType;
                resultPayload.result = ResultType.InvalidURL;
                byte[] result = resultPayload.Serialize();

                e.Response.Body.Write(result, 0, result.Length);
            }
            else
            {
                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyHttpNetwork_ReceiveFromPeer", $"PMS로부터 데이터 수신(전송받은 Command : {cmdType})==========", LogAdpType.Nexpa);
                e.Response.Connection.Type = ConnectionType.Close;
                e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                e.Response.Status = System.Net.HttpStatusCode.OK;
                e.Response.Reason = "OK";

                using (BackgroundWorker worker = new BackgroundWorker())
                {
                    worker.WorkerReportsProgress = false;
                    worker.WorkerSupportsCancellation = true;
                    worker.DoWork += ((object sender, DoWorkEventArgs e) =>
                    {
                        TargetAdapter.SendMessage(buffer, offset, size);
                    });

                    worker.RunWorkerAsync();
                    worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                }

                ResponsePayload resultPayload = new ResponsePayload();
                resultPayload.command = cmdType;
                resultPayload.result = ResultType.OK;
                byte[] result = resultPayload.Serialize();

                //정상처리 응답을 보낸다.
                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyHttpNetwork_ReceiveFromPeer", $"PMS로 응답 송신(전송한 Command : {cmdType})==========\r\n{resultPayload.ToJson()}", LogAdpType.Nexpa);
                e.Response.Body.Write(result, 0, result.Length);
            }
        }

        /// <summary>
        /// 홈넷 요청에 대한 넥스파 응답 여부
        /// </summary>
        private bool bCompleteNexpaResponse = true;
        private int bCompleteCount = 0;

        private void ResponseAction()
        {
            //1초마다 시행함.
            if (bCompleteNexpaResponse) return;
            else
            {
                bCompleteCount -= 1;

                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | ResponseAction", $"Wait Count-Down{bCompleteCount} ", LogAdpType.Nexpa);

                if (bCompleteCount <= 0)
                {
                    if(reqPid != "")
                    {
                        Log.WriteLog(LogType.Error, "NexpaTcpAdapter | ResponseAction", $"{reqPid} Disconnect", LogAdpType.Nexpa);
                        //응답을 기다릴 만큼 기다렸다.
                        bCompleteCount = 0;
                        bCompleteNexpaResponse = true;
                        //연결된 Session을 끊는다.
                        MyTcpNetwork.DisconnectSession(reqPid);
                        reqPid = "";
                    }
                    else
                    {
                        //PASSING--
                        bCompleteCount = 0;
                        bCompleteNexpaResponse = true;
                    }
                }
            }
        }

        /// <summary>
        /// Target Adapter에게 받은 Message 를 Peer에 전달한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            lock (lockObj)
            {
                
                CmdType cmd = buffer[..(int)size].GetCommand(SysConfig.Instance.Nexpa_Encoding);
                //Log.WriteLog(LogType.Info, "NexpaTcpAdapter | SendMessage", $"Target Adapter에게 받은 Command : {cmd}", LogAdpType.Nexpa);
                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | SendMessage", $"{buffer.ToString(SysConfig.Instance.Nexpa_Encoding)}", LogAdpType.Nexpa);
                
                if ((cmd == CmdType.location_map || cmd == CmdType.location_list) && SysConfig.Instance.Sys_Option.GetValueToUpper("UseLocationSearch") == "Y")
                {
                    //TODO : location map은 alias를 관제에서 관리한다..ㅠㅠ 관제를 통해 alias를 가져오는 로직이 추가되어야 한다.
                    RequestUDO_LocationMap(cmd, buffer[..(int)size]);
                }
                else
                {
                    switch (runStatus)
                    {
                        case Status.TcpOnly:
                            {
                                if(cmd != CmdType.alert_incar && cmd != CmdType.alert_outcar)
                                {
                                    //응답 Command가 아닐경우 응답이 안되었음.
                                    if (bCompleteCount <= 0) bCompleteCount = 10; //10초만 기다려주겠음.
                                    bCompleteNexpaResponse = false;
                                }

                                MyTcpNetwork.SendToPeer(buffer, offset, size, pid);
                            }

                            break;
                        case Status.WebOnly:

                            {
                                byte[] requestData = buffer[..(int)size];
                                string responseData = string.Empty;
                                string responseHeader = string.Empty;

                                if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Json, requestData, ref responseData, ref responseHeader))
                                {
                                    try
                                    {
                                        Log.WriteLog(LogType.Info, "NexpaWebAdapter | SendMessage | WebClientResponse", $"==응답== {responseData}", LogAdpType.Nexpa);
                                        ResponsePayload responsePayload = new ResponsePayload();
                                        byte[] responseBuffer = SysConfig.Instance.Nexpa_Encoding.GetBytes(responseData);
                                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, pid);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.WriteLog(LogType.Error, "NexpaWebAdapter | SendMessage | WebClientResponse", $"{responseData}\r\n{ex.Message}", LogAdpType.Nexpa);
                                    }
                                }
                            }
                            break;
                        case Status.Full:

                            {
                                MyTcpNetwork.SendToPeer(buffer, offset, size);
                                byte[] requestData = buffer[..(int)size];
                                string responseData = string.Empty;
                                string responseHeader = string.Empty;

                                if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Json, requestData, ref responseData, ref responseHeader))
                                {
                                    try
                                    {
                                        Log.WriteLog(LogType.Info, "NexpaWebAdapter | SendMessage | WebClientResponse", $"==응답== {responseData}", LogAdpType.Nexpa);
                                        ResponsePayload responsePayload = new ResponsePayload();
                                        byte[] responseBuffer = SysConfig.Instance.Nexpa_Encoding.GetBytes(responseData);
                                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.WriteLog(LogType.Error, "NexpaWebAdapter | SendMessage | WebClientResponse", $"{responseData}\r\n{ex.Message}", LogAdpType.Nexpa);
                                    }
                                }
                            }
                            break;
                    }
                }
            }
        }

        private void RequestUDO_LocationMap(CmdType cmd, byte[] buffer)
        {
            //코맥스에서 요구하는 차량 위치찾기 기능을 구현해야 함.
            //1. 동호만 보내는경우, 차량번호를 포함해 보내는경우가 있음...
            //2-1. 동호만 보내는 경우엔 관제를 통해 차량번호 리스트를 받아야함.
            //2-2. 동호/차량번호를 보내는경우엔 바로 유도로 위치를 요청할 수 있음.
            //3. 2.의 두가지 Case는 command로 구분가능함.

            //URL을 만들자....
            if (!_OptionDetail.ContainsKey("UdoIP"))
            {
                _OptionDetail.Add("UdoIP", SysConfig.Instance.Nexpa_UWebIP);
            }
            if (!_OptionDetail.ContainsKey("UdoPort"))
            {
                _OptionDetail.Add("UdoPort", SysConfig.Instance.Nexpa_UWebPort);
            }

            string sJson = buffer.ToString(SysConfig.Instance.Nexpa_Encoding);
            JObject json = JObject.Parse(sJson);
            if (json["command"].ToString() == "location_map") //차량한대만 찾는경우
            {
                ResponseDataPayload response = new ResponseDataPayload();
                response.command = CmdType.location_map;

                JObject data = json["data"] as JObject;
                RequestFindCarLocPayload payload = new RequestFindCarLocPayload();
                payload.Deserialize(data);
                string responseData = string.Empty;

                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"http://{_OptionDetail["UdoIP"]}:{_OptionDetail["UdoPort"]}/carfind", LogAdpType.Nexpa);

                if (NetworkWebClient.Instance.SendDataPost(new Uri($"http://{_OptionDetail["UdoIP"]}:{_OptionDetail["UdoPort"]}/carfind"), payload.Serialize(), ref responseData, ContentType.Json))
                {
                    try
                    {
                        Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"==응답== {responseData}", LogAdpType.Nexpa);

                        //responseData를 가공하자.....
                        var jobj = JObject.Parse(responseData);
                        ResponseLocationMapPayload dataPayload = null;
                        JArray carFindSub = jobj["carFindSub"] as JArray;
                        if (carFindSub != null)
                        {
                            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"==carFinSub is not null==", LogAdpType.Nexpa);
                            foreach (var item in carFindSub)
                            {
                                dataPayload = new ResponseLocationMapPayload();
                                dataPayload.car_number = item.Value<string>("car_no");
                                dataPayload.alias = item.Value<string>("alias");
                                dataPayload.location_text = item.Value<string>("location");
                                dataPayload.pixel_x = item.Value<string>("pointX");
                                dataPayload.pixel_y = item.Value<string>("pointY");
                                dataPayload.in_datetime = item.Value<string>("car_indate");
                                dataPayload.car_image = item.Value<string>("car_image");

                                break; //1개만 가져옴...
                            }
                        }

                        //홈넷 어댑터가 코맥스 대림일 경우.....
                        if (SysConfig.Instance.Sys_HomeNetAdapter.StartsWith("2"))
                        {
                            if (jobj["totalCount"] == null)
                            {
                                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"==totalCount is null==", LogAdpType.Nexpa);
                                response.result = ResultType.notinterface_udo;
                                    
                            }
                            else
                            {
                                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"==totalCount is not null==", LogAdpType.Nexpa);
                                response.data = dataPayload;
                                response.result = ResultType.OK;
                            }

                            byte[] responseBuffer = response.Serialize();
                            TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogType.Error, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{ex.Message}", LogAdpType.Nexpa);
                    }

                    
                    bResponseSuccess = true;
                    arrSelfResponseData = null;
                }
                else
                {
                    if (SysConfig.Instance.Sys_HomeNetAdapter.StartsWith("2"))
                    {
                        response.result = ResultType.OK;

                        byte[] responseBuffer = response.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                    }
                }
            }
            else if (json["command"].ToString() == "location_list") //차량리스트를 찾는경우
            {
                ResponseDataPayload response = new ResponseDataPayload();
                response.command = CmdType.location_list;

                //1. 관제에 차량 번호를 조회...find_car
                JObject data = json["data"] as JObject;
                string dong = data["dong"]?.ToString();
                string ho = data["ho"]?.ToString();
                string type = data["location_type"]?.ToString();

                ResponseLocationListPayload dataPayload = new ResponseLocationListPayload();
                dataPayload.location_type = type;

                JObject findCarObj = new JObject();
                findCarObj["command"] = "find_car";
                JObject findCarData = new JObject();
                findCarData["dong"] = dong;
                findCarData["ho"] = ho;
                findCarObj["data"] = findCarData;

                bResponseSuccess = false;
                byte[] bytes = SysConfig.Instance.Nexpa_Encoding.GetBytes(findCarObj.ToString());
                MyTcpNetwork.SendToPeer(bytes, 0, bytes.Length);

                //===================== Nexpa 응답 대기 =====================

                int iSec = 100; //1초
                while (iSec > 0 && !bResponseSuccess)
                {
                    Thread.Sleep(10); //0.01초씩..쉰다...
                    iSec -= 1;
                }
                //===================== Nexpa 응답 대기 =====================

                //2. 읽어온 차량번호로 일련의 Data를 만들어보자..
                if (bResponseSuccess && arrSelfResponseData != null)
                {
                    string strFindCarJson = arrSelfResponseData.ToString(SysConfig.Instance.Nexpa_Encoding);
                    Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{strFindCarJson}", LogAdpType.Nexpa);
                    try
                    {
                        JObject findObj = JObject.Parse(strFindCarJson);
                        JObject lstFind = findObj["data"] as JObject;
                        JArray arrFind = lstFind.Value<JArray>("list");

                        if (arrFind != null)
                        {
                            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{1.1}", LogAdpType.Nexpa);
                            StringBuilder tempCarnos = new StringBuilder();
                            //car_number
                            int idx = 0;
                            foreach (var item in arrFind)
                            {
                                idx += 1;
                                tempCarnos.Append(item["car_number"].ToString());
                                if (idx < arrFind.Count)
                                {
                                    tempCarnos.Append(",");
                                }
                            }
                            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{2.2}", LogAdpType.Nexpa);
                            RequestFindCarLocPayload payload = new RequestFindCarLocPayload();
                            payload.dong = dong;
                            payload.ho = ho;
                            payload.car_number = tempCarnos?.ToString();
                            string responseData = string.Empty;

                            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{tempCarnos}", LogAdpType.Nexpa);

                            if (tempCarnos != null && tempCarnos.ToString().Trim() != "")
                            {
                                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"http://{_OptionDetail["UdoIP"]}:{_OptionDetail["UdoPort"]}/carfind", LogAdpType.Nexpa);

                                if (NetworkWebClient.Instance.SendDataPost(new Uri($"http://{_OptionDetail["UdoIP"]}:{_OptionDetail["UdoPort"]}/carfind"), payload.Serialize(), ref responseData, ContentType.Json))
                                {
                                    //responseData를 가공하자.....
                                    var jobj = JObject.Parse(responseData);
                                    JArray carFindSub = jobj["carFindSub"] as JArray;

                                    if (carFindSub != null && carFindSub.Count != 0)
                                    {
                                        foreach (var item in carFindSub)
                                        {
                                            ResponseLocationMapPayload subDataPayload = new ResponseLocationMapPayload();
                                            subDataPayload.car_number = item.Value<string>("car_no");
                                            
                                            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{3}", LogAdpType.Nexpa);
                                            subDataPayload.alias = item.Value<string>("alias");
                                            subDataPayload.location_text = item.Value<string>("location");
                                            subDataPayload.pixel_x = item.Value<string>("pointX");
                                            subDataPayload.pixel_y = item.Value<string>("pointY");
                                            subDataPayload.in_datetime = item.Value<string>("car_indate");
                                            subDataPayload.car_image = item.Value<string>("car_image");
                                            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{4}", LogAdpType.Nexpa);
                                            dataPayload.list.Add(subDataPayload);
                                            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{5}", LogAdpType.Nexpa);
                                        }
                                    }

                                    if (jobj["totalCount"] == null)
                                    {
                                        Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{6}", LogAdpType.Nexpa);
                                        response.result = ResultType.OK;
                                    }
                                    else
                                    {
                                        Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{7}", LogAdpType.Nexpa);
                                        response.data = dataPayload;
                                        response.result = ResultType.OK;
                                    }

                                    byte[] responseBuffer = response.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                }
                                else
                                {
                                    Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{8}", LogAdpType.Nexpa);
                                    response.data = dataPayload;
                                    response.result = ResultType.OK;
                                    byte[] responseBuffer = response.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                }
                            }
                            else
                            {
                                response.data = dataPayload;
                                response.result = ResultType.OK;
                                byte[] responseBuffer = response.Serialize();
                                TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{99}", LogAdpType.Nexpa);
                        //Null이 떨어졌다? 그건바로.... 검색이 안되서 그런것...
                        response.result = ResultType.OK;
                        response.data = dataPayload;
                        byte[] responseBuffer = response.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                    }

                }
                else
                {
                    Log.WriteLog(LogType.Info, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{2.3}", LogAdpType.Nexpa);
                    response.result = ResultType.OK;
                    response.data = dataPayload;
                    byte[] responseBuffer = response.Serialize();
                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                }

                bResponseSuccess = true;
                arrSelfResponseData = null;
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Do Nothing...
        }

        public void TestReceive(byte[] buffer)
        {
            switch (runStatus)
            {
                case Status.TcpOnly:
                    MyTcpNetwork_ReceiveFromPeer(buffer, 0, buffer.Length);
                    break;
                case Status.WebOnly:
                    MyHttpNetwork_ReceiveFromPeer(buffer, 0, buffer.Length);
                    break;
                case Status.Full:
                    MyTcpNetwork_ReceiveFromPeer(buffer, 0, buffer.Length);
                    MyHttpNetwork_ReceiveFromPeer(buffer, 0, buffer.Length);
                    break;
            }
        }

        public void Dispose()
        {
            ResponseThread.Dispose();
        }
    }
}
