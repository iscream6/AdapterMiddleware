using NetworkCore;
//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace NexpaAdapterStandardLib.Network
{
    public delegate void ReceiveAction(byte[] buffer, long offset, long size, Guid guid);

    class NetworkTcpServer : TcpServer, INetwork
    {
        private Dictionary<Guid, TcpSession> _dicClientSession;

        public event SendToPeer ReceiveFromPeer;
        public Encoding MyEncoding { get; set; }

        public NetworkTcpServer(IPAddress address, int port) : base(address, port)
        {
            _dicClientSession = new Dictionary<Guid, TcpSession>();
        }

        public int CurrentSessionCount
        {
            get => _dicClientSession.Count;
        }
        public Action OnConnectionAction { get; set; }
        public NetStatus Status { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #region Implements IHomeNetServerAdapter

        public bool Down()
        {
            if (!IsStarted) return false;
            return Stop();
        }

        /// <summary>
        /// Peer로 Data를 전달한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendToPeer(byte[] buffer, long offset, long size, string id = null, System.Net.EndPoint ep = null)
        {
            try
            {
                if(id == null) //Broad Cast
                {
                    //MultiCast 한다.
                    int cntCast = 0;
                    foreach (var item in _dicClientSession.Values)
                    {
                        Log.WriteLog(LogType.Info, $"TcpServerNetwork | SendToPeer", $"{item.Id}");
                        item.SendAsync(buffer, offset, size);
                        cntCast += 1;
                    }
                    Log.WriteLog(LogType.Info, $"TcpServerNetwork | SendToPeer", $"Broad Cast {cntCast}개 완료");
                }
                else //특정 Target에 Cast
                {
                    Guid guid = Guid.Parse(id);
                    _dicClientSession[guid].SendAsync(buffer, offset, size);
                    Log.WriteLog(LogType.Info, $"TcpServerNetwork | SendToPeer", $"{guid}");
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"TcpServerNetwork | SendToPeer", $"TCP Server 오류 : {ex.Message}");
            }
        }

        public bool Run()
        {
            try
            {
                if (IsStarted)
                {
                    SystemStatus.Instance.SendEventMessage(LogAdpType.Nexpa, "Already started Tcp Server...");
                    return true;
                }
                Log.WriteLog(LogType.Info, $"TcpServerNetwork | Run", $"TCP Server 시작");
                return Start();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"TcpServerNetwork | Run", $"TCP Server 오류 : {ex.Message}");
                return false;
            }
        }

        #endregion


        #region Session Events

        protected override TcpSession CreateSession()
        {
            var session = new TcpNetworkSession(this);
            session.receiveAction += Session_receiveAction;
            session.Session_Disconnected += Session_Session_Disconnected;

            _dicClientSession.Add(session.Id, session);

            Log.WriteLog(LogType.Info, $"TcpServerNetwork | CreateSession", $"Client 접속 ID : {session.Id}");
            if (OnConnectionAction != null) OnConnectionAction();
            return session;
        }

        /// <summary>
        /// Client로부터 온 Message 수신 처리
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        private void Session_receiveAction(byte[] buffer, long offset, long size, Guid guid)
        {
            try
            {
                Log.WriteLog(LogType.Info, "TcpServerNetwork| Session_receiveAction", $"Session_receiveAction Call");
                ReceiveFromPeer?.Invoke(buffer, offset, size, id: guid.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"TcpServerNetwork| Session_receiveAction", $"Session_receiveAction error : {ex.Message}");
            }
        }

        /// <summary>
        /// Client 접속 종료
        /// </summary>
        /// <param name="id"></param>
        private void Session_Session_Disconnected(Guid id)
        {
            if (_dicClientSession.ContainsKey(id))
            {
                ((TcpNetworkSession)_dicClientSession[id]).Session_Disconnected -= Session_Session_Disconnected;
                ((TcpNetworkSession)_dicClientSession[id]).receiveAction -= Session_receiveAction;
                _dicClientSession.Remove(id);
                Log.WriteLog(LogType.Info, $"TcpServerNetwork | Session_Session_Disconnected", $"Client 접속 종료 ID : {id}");
            }
        }

        #endregion
    }
}
