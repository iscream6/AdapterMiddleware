using HttpServer;
using Newtonsoft.Json.Linq;
using NpmAdapter.Payload;
using NpmCommon;
using NpmNetwork;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class NexpaHipassAdapter : IAdapter
    {
        private bool isRun = false;
        private Dictionary<string, IAdapter> dicHipass;
        private StringBuilder receiveMessageBuffer;
        private object objLock;
        private INetwork MyTcpNetwork { get; set; }
        
        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning { get => isRun; }

        public string reqPid { get; set; }

        public event IAdapter.ShowBallonTip ShowTip;

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            receiveMessageBuffer = new StringBuilder();
            objLock = new object();

            int port = 30542;
            int.TryParse(SysConfig.Instance.Nexpa_TcpPort, out port);
            MyTcpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpServer, port.ToString());
            List<SysConfig.HIP> HipList = SysConfig.Instance.HIP_List;
            dicHipass = new Dictionary<string, IAdapter>();

            foreach (var hip in HipList)
            {
                IAdapter adapter = new ITronHighPassAdapter(hip.IP, hip.Port);
                adapter.TargetAdapter = this;
                adapter.Initialize();
                dicHipass.Add(hip.UnitNo, adapter);
            }

            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            //바로 전송
            MyTcpNetwork.SendToPeer(buffer, offset, size, pid);
        }

        public bool StartAdapter()
        {
            MyTcpNetwork.ReceiveFromPeer += MyTcpNetwork_ReceiveFromPeer;
            isRun = MyTcpNetwork.Run();

            foreach (var item in dicHipass)
            {
                item.Value.StartAdapter();
            }

#if (DEBUG)
            //JObject TestJson = new JObject();
            //TestJson["unit_no"] = "10";
            //TestJson["tk_no"] = "1234567890";
            //TestJson["car_number"] = "11가1111";
            //TestJson["fee"] = "1500";
            //byte[] buffer = TestJson.ToByteArray(SysConfig.Instance.Nexpa_Encoding);
            //foreach (var item in dicHipass)
            //{
            //    item.Value.SendMessage(buffer, 0, buffer.Length);
            //}
#endif

            return isRun;
        }

        public bool StopAdapter()
        {
            bool bResult = false;
            MyTcpNetwork.ReceiveFromPeer -= MyTcpNetwork_ReceiveFromPeer;
            bResult = MyTcpNetwork.Down();

            foreach (var item in dicHipass)
            {
                item.Value.StopAdapter();
            }

            isRun = !bResult;
            return bResult;
        }

        private void MyTcpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, RequestEventArgs pEvent, string id, EndPoint ep)
        {
            lock (objLock)
            {
                try
                {
                    receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));

                    Log.WriteLog(LogType.Info, "NexpaLocalAdapter | MyTcpNetwork_ReceiveFromPeer", $"ReceiveMessage : {receiveMessageBuffer.ToString()}", LogAdpType.Nexpa);

                    //넥스파 장비쪽 Live Check 로 OK를 보낸다...
                    if(receiveMessageBuffer.ToString() == "OK")
                    {
                        receiveMessageBuffer.Clear();
                        return;
                    }

                    var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
                    Thread.Sleep(10);
                    receiveMessageBuffer.Clear();

                    HipassRequestPayload hipassRequest = new HipassRequestPayload();
                    hipassRequest.Deserialize(jobj);

                    if (dicHipass.ContainsKey(hipassRequest.unit_no))
                    {
                        byte[] sendBuffer = hipassRequest.Serialize();
                        dicHipass[hipassRequest.unit_no].SendMessage(sendBuffer, 0, sendBuffer.Length, id);
                    }
                    else
                    {
                        jobj["result_code"] = "Fail";
                        jobj["result_message"] = "설정된 Unit No 가 아닙니다.";

                        Log.WriteLog(LogType.Error, "NexpaLocalAdapter | MyTcpNetwork_ReceiveFromPeer", $"{jobj}", LogAdpType.Nexpa);

                        byte[] sendBuffer = SysConfig.Instance.Nexpa_Encoding.GetBytes(jobj.ToString());
                        MyTcpNetwork.SendToPeer(sendBuffer, 0, sendBuffer.Length);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "NexpaLocalAdapter | MyTcpNetwork_ReceiveFromPeer", $"{ex.StackTrace}", LogAdpType.Nexpa);
                    receiveMessageBuffer.Clear();
                }
            }
        }

        public void TestReceive(byte[] buffer)
        {
            
        }
    }
}
