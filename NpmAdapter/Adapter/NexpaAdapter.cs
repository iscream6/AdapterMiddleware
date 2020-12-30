using HttpServer;
//using NexpaAdapterStandardLib.IO.Json;
using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        private Dictionary<string, string> _OptionDetail;
        private Status runStatus = Status.Full;
        private bool isRun = false;
        private byte[] arrSelfResponseData = null;
        private bool bResponseSuccess = true;

        public static string REQ_POST_STATUS = "/nexpa/mdl/";

        private INetwork MyTcpNetwork { get; set; }
        private INetwork MyHttpNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning { get => isRun; }
        public NexpaAdapter(Status status)
        {
            runStatus = status;
        }

        public bool Initialize()
        {
            int port = 42131;

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
                    if (MyTcpNetwork != null) Log.WriteLog(LogType.Info, $"NexpaTcpAdapter | 생성자", $"TcpServer 생성", LogAdpType.Nexpa);
                    else Log.WriteLog(LogType.Info, $"NexpaAdapter | Initialize", $"TcpServer 생성실패", LogAdpType.Nexpa);
                }

                if (runStatus == Status.WebOnly || runStatus == Status.Full)
                {
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
                    MyTcpNetwork.ReceiveFromPeer -= MyTcpNetwork_ReceiveFromPeer;
                    bResult = MyTcpNetwork.Down();
                    break;
                case Status.WebOnly:
                    MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
                    bResult = MyHttpNetwork.Down();
                    break;
                case Status.Full:
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
        private void MyTcpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, RequestEventArgs pEvent = null)
        {
            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", "주차관제로부터 데이터 수신 ==========", LogAdpType.Nexpa);
            if (!bResponseSuccess)
            {
                arrSelfResponseData = buffer[..(int)size];
                bResponseSuccess = true;
            }
            else
            {
                TargetAdapter.SendMessage(buffer, offset, size);
            }
            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyTcpNetwork_ReceiveFromPeer", "주차관제로부터 데이터 수신 완료 =====", LogAdpType.Nexpa);
        }

        /// <summary>
        /// 웹통합에서 받은 Message를 처리한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="pEvent"></param>
        private void MyHttpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, RequestEventArgs e = null)
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
                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | MyHttpNetwork_ReceiveFromPeer", $"PMS로부터 데이터 수신(전송한 Command : {cmdType})==========", LogAdpType.Nexpa);
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
                byte[]  result = resultPayload.Serialize();

                //int iSec = 500; //5초
                //while (iSec > 0 && !bResponseSuccess)
                //{
                //    Thread.Sleep(10); //0.01초씩..쉰다...
                //    iSec -= 1;
                //}

                //byte[] result;

                //if (bResponseSuccess == false)
                //{
                //    ResponseResultPayload resultPayload = new ResponseResultPayload();
                //    resultPayload.command = cmdType;
                //    resultPayload.Result = ResponseResultPayload.Status.ExceptionERROR;
                //    result = resultPayload.Serialize();
                //}
                //else
                //{
                //    result = resultPayload.Serialize();
                //}


                //정상처리 응답을 보낸다.
                e.Response.Body.Write(result, 0, result.Length);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
        }

        private object lockObj = new object();

        public void SendMessage(IPayload payload)
        {
            lock (lockObj)
            {
                byte[] msgBuffer = payload.Serialize();

                switch (runStatus)
                {

                    case Status.TcpOnly:
                        {
                            MyTcpNetwork.SendToPeer(msgBuffer, 0, msgBuffer.Length);
                        }

                        break;
                    case Status.WebOnly:

                        {
                            MyHttpNetwork.SendToPeer(msgBuffer, 0, msgBuffer.Length);
                        }
                        break;
                    case Status.Full:

                        {
                            MyTcpNetwork.SendToPeer(msgBuffer, 0, msgBuffer.Length);
                            MyHttpNetwork.SendToPeer(msgBuffer, 0, msgBuffer.Length);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Target Adapter에게 받은 Message 를 Peer에 전달한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendMessage(byte[] buffer, long offset, long size)
        {
            lock (lockObj)
            {
                
                CmdType cmd = buffer[..(int)size].GetCommand(SysConfig.Instance.Nexpa_Encoding);
                Log.WriteLog(LogType.Info, "NexpaTcpAdapter | SendMessage", $"Target Adapter에게 받은 Command : {cmd}", LogAdpType.Nexpa);
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
                                MyTcpNetwork.SendToPeer(buffer, offset, size);
                            }

                            break;
                        case Status.WebOnly:

                            {
                                MyHttpNetwork.SendToPeer(buffer, offset, size);
                                //resultPayload.Deserialize(JObject.Parse(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size)));
                                //bResponseSuccess = true;
                            }
                            break;
                        case Status.Full:

                            {
                                MyTcpNetwork.SendToPeer(buffer, offset, size);
                                MyHttpNetwork.SendToPeer(buffer, offset, size);
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
        }
    }
}
