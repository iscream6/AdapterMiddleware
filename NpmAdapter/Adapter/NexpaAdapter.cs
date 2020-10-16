using HttpServer;
//using NexpaAdapterStandardLib.IO.Json;
using HttpServer.Headers;
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
        private bool bResponseSuccess = false;
        private bool isRun = false;

        public static string REQ_POST_STATUS = "/nexpa/mdl/";
        
        private INetwork MyTcpNetwork { get; set; }
        private INetwork MyHttpNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning { get=>isRun; }

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

                if (runStatus  == Status.TcpOnly || runStatus == Status.Full)
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
            TargetAdapter.SendMessage(buffer, offset, size);
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
            string sJson = Encoding.UTF8.GetString(buffer);
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
                ResponseResultPayload resultPayload = new ResponseResultPayload();
                resultPayload.command = cmdType;
                resultPayload.Result = ResponseResultPayload.Status.InvalidURL;
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

                //타겟에 메시지를 보낸다.
                TargetAdapter.SendMessage(buffer, offset, size);

                ResponseResultPayload resultPayload = new ResponseResultPayload();
                resultPayload.command = cmdType;
                resultPayload.Result = ResponseResultPayload.Status.OK;
                byte[] result = resultPayload.Serialize();
                //정상처리 응답을 보낸다.
                e.Response.Body.Write(result, 0, result.Length);
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
            Log.WriteLog(LogType.Info, "NexpaTcpAdapter | SendMessage", "Target Adapter에게 받은 Message 를 주차관제로 전송", LogAdpType.Nexpa);
            switch (runStatus)
            {
                case Status.TcpOnly:

                    if (buffer.GetCommand(SysConfig.Instance.Nexpa_Encoding) == CmdType.location_map && SysConfig.Instance.Sys_Option.GetValueToUpper("UseLocationSearch") == "Y")
                    {
                        RequestUDO_LocationMap(buffer);
                    }
                    else
                    {
                        MyTcpNetwork.SendToPeer(buffer, offset, size);
                    }

                    //===================== Nexpa 응답 대기 =====================
                    int iSec = 100; //1초
                    while (iSec > 0 && !bResponseSuccess)
                    {
                        Thread.Sleep(10); //0.01초씩..쉰다...
                        iSec -= 1;
                    }
                    //===================== Nexpa 응답 대기 =====================

                    //1초 내 응답이 안오면 죽은것으로 간주한다.
                    if (bResponseSuccess == false)
                    {
                        bResponseSuccess = true;
                        //응답 지연 에러....
                    }
                    bResponseSuccess = false;

                    break;
                case Status.WebOnly:
                    
                    if (buffer.GetCommand(SysConfig.Instance.Nexpa_Encoding) == CmdType.location_map && SysConfig.Instance.Sys_Option.GetValueToUpper("UseLocationSearch") == "Y")
                    {
                        RequestUDO_LocationMap(buffer);
                    }
                    else
                    {
                        MyHttpNetwork.SendToPeer(buffer, offset, size);
                    }
                    break;
                case Status.Full:
                    if (buffer.GetCommand(SysConfig.Instance.Nexpa_Encoding) == CmdType.location_map && SysConfig.Instance.Sys_Option.GetValueToUpper("UseLocationSearch") == "Y")
                    {
                        RequestUDO_LocationMap(buffer);
                    }
                    else
                    {
                        MyTcpNetwork.SendToPeer(buffer, offset, size);
                        MyHttpNetwork.SendToPeer(buffer, offset, size);
                    }
                    break;
            }
        }

        private void RequestUDO_LocationMap(byte[] buffer)
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

                if (NetworkWebClient.Instance.SendDataPost(new Uri($"http://{_OptionDetail["UdoIP"]}:{_OptionDetail["UdoPort"]}/carfind"), payload.Serialize(), ref responseData))
                {
                    //responseData를 가공하자.....
                    var jobj = JObject.Parse(responseData);
                    ResponseLocationMapPayload dataPayload = null;
                    JArray carFindSub = jobj["carFindSub"] as JArray;
                    if (carFindSub != null)
                    {
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
                        if (dataPayload == null)
                        {
                            response.result = CmdPayloadManager.MakeResponseResultPayload(CmdPayloadManager.StatusCode.notinterface_udo);
                        }
                        else
                        {
                            response.data = dataPayload;
                            response.result = CmdPayloadManager.MakeResponseResultPayload(CmdPayloadManager.StatusCode.ok);
                        }

                        byte[] responseBuffer = response.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                    }
                }
                else
                {
                    if (SysConfig.Instance.Sys_HomeNetAdapter.StartsWith("2"))
                    {
                        response.result = CmdPayloadManager.MakeResponseResultPayload(CmdPayloadManager.StatusCode.notinterface_udo);

                        byte[] responseBuffer = response.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                    }
                }
            }
            else if (json["command"].ToString() == "location_list") //차량리스트를 찾는경우
            {

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
