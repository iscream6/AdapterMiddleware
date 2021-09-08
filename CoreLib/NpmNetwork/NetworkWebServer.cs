using HttpServer;
using NexpaAdapterStandardLib.IO.Json;
using NpmCommon;
using System;
using System.IO;
using System.Net;
using System.Text;
using HttpListener = HttpServer.HttpListener;

namespace NpmNetwork
{
    class NetworkWebServer : INetwork
    {
        public static string REQ_GET_XVR_LIST = "/v1.0/xvr/list";
        public static string REQ_POST_EVT_BACKUP_STAT = "/v1.0/xvr/evt/video/backup/stat";

        public event SendToPeer ReceiveFromPeer;
        //private NetworkCore.HttpServer m_listener = null;
        private HttpListener m_listener = null;
        private int _port;

        public Action OnConnectionAction { get; set; }
        public NetStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public NetworkWebServer(int port)
        {
            _port = port;
        }

        public bool Run()
        {
            try
            {
                m_listener = HttpListener.Create(IPAddress.Any, _port);
                Log.WriteLog(LogType.Info, $"Address : {m_listener.Address}", $"Port : {_port}", LogAdpType.none);
                m_listener.Start(10);
                m_listener.SocketAccepted += onAccept;
                m_listener.RequestReceived += OnRequestReceive;     // 웹서버에서 데이터가 전달 되면 호출됨
                m_listener.ErrorPageRequested += OnErrorPageRequested;
                m_listener.ContinueResponseRequested += OnContinueResponseRequested;

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "HttpServerNetwork | Run", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 웹서버로 전달.
        /// </summary>
        /// <param name="buffer">전달할 값</param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendToPeer(byte[] buffer, long offset, long size, string id = null, System.Net.EndPoint ep = null)
        {
            string strJson = buffer.ToString(Encoding.UTF8);
            //string cmd = buffer.GetCommand();
            //TODO : 웹서버에서 Peer 서버로 전달 로직을 개발해야함. Peer Web Server To Peer Web Server 용!
            NetworkHttpControl httpControl = new NetworkHttpControl("");
            //cmd에 따라 UrlCmdType 을 지정한다.

            //string data = Newtonsoft.Json.JsonConvert.SerializeObject(pCloseData);
            //NPHttpControl.UrlCmdType currnetUrl = NPHttpControl.UrlCmdType.close;
            //mNPHttpControl.SetHeader(currnetUrl, string.Empty);
            //Close currentClose = new Close();
            ////TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "HttpProcess | SaveClose", "[마감저장처리요청]" + data);
            //currentClose = (Close)restClassParser.SetDataParsing(currnetUrl, mNPHttpControl.SendMessagePost(string.Empty, data));
            //return currentClose;
        }

        public bool Down()
        {
            try
            {
                m_listener.Stop();
                m_listener.RequestReceived -= OnRequestReceive;     // 웹서버에서 데이터가 전달 되면 호출됨
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void OnContinueResponseRequested(object sender, RequestEventArgs e)
        {
            //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpServer | OnErrorPageRequested", "[OnContinueResponseRequested]");
        }

        private void OnErrorPageRequested(object sender, ErrorPageEventArgs e)
        {
            //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpServer | OnErrorPageRequested", "[OnErrorPageRequested]");
        }
        private void onAccept(object sender, SocketFilterEventArgs e)
        {
            IPEndPoint remoteIpEndPoint = e.Socket.RemoteEndPoint as IPEndPoint;
            if (OnConnectionAction != null) OnConnectionAction();
            //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpServer | onAccept", "[서버에 클라이언트 접속]" + remoteIpEndPoint.Address);
        }
        private void OnRequestReceive(object sender, RequestEventArgs e)
        {
            try
            {
                StreamReader reader = new StreamReader(e.Context.Request.Body);
                string readData = reader.ReadToEnd();

                //Path Query를 Param으로 전달하기 위함.
                if ((readData == null || readData == "") && e.Request.Uri.Query != null && e.Request.Uri.Query != "" && e.Request.Uri.ToString().IndexOf('?') > -1)
                {
                    var uriString = e.Request.Uri.ToString();
                    readData = uriString.Substring(e.Request.Uri.ToString().IndexOf('?') + 1);
                }

                byte[] receiveData = Encoding.UTF8.GetBytes(readData);
                ReceiveFromPeer?.Invoke(receiveData, 0, receiveData.Length, e);

                //JObject obj = JObject.Parse(readData);
                //string test = obj["car_no"].ToString().uni_to_kr();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "HttpServerNetwork | OnRequestReceive", ex.StackTrace);
            }
        }
    }
}
