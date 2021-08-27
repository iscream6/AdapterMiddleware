using HttpServer;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 정기차량에 관한 DB 연동이 있어 권한이 필요함.
    /// </summary>
    class ChamAdapter : IAdapter
    {
        private class ChamData
        {
            bool isFirst = true;

            public string cmd { get; set; }
            public string opt { get; set; }
            public string ip { get; set; }
            public string curtime { get; set; }
            public string ret { get; set; }
            public string plate { get; set; }
            public string dong { get; set; }
            public string ho { get; set; }
            public string time { get; set; }
            public string type { get; set; }

            public override string ToString()
            {
                StringBuilder dataMsg = new StringBuilder();
                dataMsg.Append($"CMD={cmd}");
                dataMsg.Append($"&OPT={opt}");
                if (!string.IsNullOrEmpty(ip))
                {
                    dataMsg.Append($"&IP={ip}");
                }

                if (!string.IsNullOrEmpty(curtime))
                {
                    dataMsg.Append($"&CURTIME={curtime}");
                }

                if (!string.IsNullOrEmpty(curtime))
                {
                    dataMsg.Append($"&CURTIME={curtime}");
                }

                if (!string.IsNullOrEmpty(ret))
                {
                    dataMsg.Append($"&RET={ret}");
                }

                if (!string.IsNullOrEmpty(plate))
                {
                    dataMsg.Append($"&PLATE={plate}");
                }

                if (!string.IsNullOrEmpty(dong))
                {
                    dataMsg.Append($"&DONG={dong}");
                }

                if (!string.IsNullOrEmpty(ho))
                {
                    dataMsg.Append($"&HO={ho}");
                }

                if (!string.IsNullOrEmpty(time))
                {
                    dataMsg.Append($"&TIME={time}");
                }

                if (!string.IsNullOrEmpty(type))
                {
                    dataMsg.Append($"&TYPE={type}");
                }

                if(dataMsg.ToString() == string.Empty)
                {
                    return base.ToString();
                }
                else
                {
                    string fmt = "00000000.##"; //00000000 형태로 만들기
                    string msg = dataMsg.ToString();
                    string len = msg.Length.ToString(fmt);
                    
                    return len + dataMsg.ToString();
                }
            }
        }

        /// <summary>
        /// 넥스파 요청에 대한 응답이 왔는지 여부
        /// </summary>
        private bool bResponseSuccess = false;
        private bool isRun;
        private StringBuilder receiveMessageBuffer;
        private object lockObj;

        public IAdapter TargetAdapter { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsRuning { get => isRun; }

        public event IAdapter.ShowBallonTip ShowTip;

        private INetwork MyTcpServer { get; set; }
        public string reqPid { get; set; }

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            isRun = false;
            lockObj = new object();
            receiveMessageBuffer = new StringBuilder();

            MyTcpServer = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, SysConfig.Instance.HT_MyPort);
            return true;
        }

        public bool StartAdapter()
        {
            try
            {
                MyTcpServer.ReceiveFromPeer += MyTcpServer_ReceiveFromPeer;
                isRun = MyTcpServer.Run();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "ChamAdapter | StartAdapter", $"{ex.Message}", LogAdpType.HomeNet);
            }

            return isRun;
        }

        public bool StopAdapter()
        {
            bool bResult = false;

            try
            {

                MyTcpServer.ReceiveFromPeer -= MyTcpServer_ReceiveFromPeer;
                bResult = MyTcpServer.Down();
                isRun = !bResult;
                
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "ChamAdapter | StopAdapter", $"{ex.Message}", LogAdpType.HomeNet);
            }

            return bResult;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            bResponseSuccess = false;

            receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"ChamAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);

            JObject data = jobj["data"] as JObject;
            string cmd = jobj["command"].ToString();
            CmdType command = (CmdType)Enum.Parse(typeof(CmdType), cmd);

            switch (command)
            {
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    {
                        ChamData chamdata = new ChamData();
                        RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                        payload.Deserialize(jobj);

                        chamdata.cmd = "REQ";
                        chamdata.opt = "EVENT";
                        chamdata.plate = payload.data.car_number;
                        
                        if(payload.data.kind.ToLower() == "n")
                        {
                            chamdata.dong = "0";
                            chamdata.ho = "0";
                        }
                        else
                        {
                            chamdata.dong = payload.data.dong;
                            chamdata.ho = payload.data.ho;
                        }

                        chamdata.time = DateTime.Now.ToString("yyMMddHHmmss");
                        if (payload.command == CmdType.alert_incar)
                        {
                            chamdata.type = "ARR";
                        }
                        else
                        {
                            chamdata.type = "DEP";
                        }

                        string sendMsg = data.ToString();
                        byte[] msgs = SysConfig.Instance.HomeNet_Encoding.GetBytes(sendMsg);

                        Log.WriteLog(LogType.Info, "ChamAdapter | Send To 참슬테크", $"전송메시지 : {sendMsg}", LogAdpType.HomeNet);

                        MyTcpServer.SendToPeer(msgs, 0, msgs.Length);

                        int iSec = 2 * 100; //2초
                        while (iSec > 0 && !bResponseSuccess)
                        {
                            Thread.Sleep(10); //0.01초씩..쉰다...
                            iSec -= 1;
                        }

                        ResponsePayload responsePayload = new ResponsePayload();
                        byte[] responseBuffer;

                        if (bResponseSuccess) //응답성공
                        {
                            
                            responsePayload.command = command;
                            responsePayload.result = ResultType.OK;
                        }
                        else
                        {
                            responsePayload.command = command;
                            responsePayload.result = ResultType.Fail;
                        }

                        responseBuffer = responsePayload.Serialize();
                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                    }
                    break;
            }
        }

        private void MyTcpServer_ReceiveFromPeer(byte[] buffer, long offset, long size, RequestEventArgs pEvent, string id, EndPoint ep)
        {
            lock (lockObj)
            {
                string receiveMsg = SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]);
                Log.WriteLog(LogType.Info, "ChamAdapter | From 참슬테크", $"받은메시지 : {receiveMsg}", LogAdpType.HomeNet);
                //앞 8자리 빼고 뒤로부터 Data 처리
                receiveMsg = receiveMsg.Substring(8);
                var dicData = receiveMsg.DoubleSplit('&', '=');

                switch (dicData["OPT"].ToUpper())
                {
                    case "CONNECT":
                        {
                            ChamData data = new ChamData();
                            data.cmd = "RES";
                            data.opt = "CONNECT";
                            data.curtime = DateTime.Now.ToString("yyMMddHHmmss");
                            data.ret = "OK";
                            string sendMsg = data.ToString();
                            byte[] msgs = SysConfig.Instance.HomeNet_Encoding.GetBytes(sendMsg);

                            Log.WriteLog(LogType.Info, "ChamAdapter | Send To 참슬테크", $"전송메시지 : {sendMsg}", LogAdpType.HomeNet);

                            MyTcpServer.SendToPeer(msgs, 0, msgs.Length); 
                        }
                        break;
                    case "ALIVE":
                        {
                            ChamData data = new ChamData();
                            data.cmd = "RES";
                            data.opt = "ALIVE";
                            data.curtime = DateTime.Now.ToString("yyMMddHHmmss");
                            data.ret = "OK";
                            string sendMsg = data.ToString();
                            byte[] msgs = SysConfig.Instance.HomeNet_Encoding.GetBytes(sendMsg);

                            Log.WriteLog(LogType.Info, "ChamAdapter | Send To 참슬테크", $"전송메시지 : {sendMsg}", LogAdpType.HomeNet);

                            MyTcpServer.SendToPeer(msgs, 0, msgs.Length);
                        }
                        break;
                    case "EVENT":
                        if (dicData["RET"] == "OK") bResponseSuccess = true;
                        break;
                }
            }
        }

        public void TestReceive(byte[] buffer)
        {
            
        }
    }
}
