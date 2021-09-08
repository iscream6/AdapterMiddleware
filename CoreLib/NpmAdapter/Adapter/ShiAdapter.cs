using Newtonsoft.Json.Linq;
using NpmAdapter.Payload;
using NpmCommon;
using NpmNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 삼성중공업
    /// </summary>
    class ShiAdapter : IAdapter
    {
        private enum STT : byte
        {
            Alive = 0xA0,
            Dead = 0xA1,
            InitErr = 0xA2,
            FuncErr = 0xA3,
            InterErr = 0xA4,
            NetErr = 0xA5
        }

        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private STT _Status;
        private bool isRun;
        private byte[] _STX;
        private byte _AlivePacket = 0xA0;
        private byte _AlertPacket = 0xA1;
        private IPEndPoint _EndPoint;
        private INetwork networkUdp { get; set; }
        public IAdapter TargetAdapter { get; set; }
        
        public bool IsRuning => isRun;

        public string reqPid { get; set; }


        public event IAdapter.ShowBallonTip ShowTip;

        Dictionary<byte, CmdType> _DicAlertBuffer;

        public void Dispose()
        {
            StopAdapter();
        }

        public bool Initialize()
        {
            try
            {
                
                int iPort = 5160;
                int.TryParse(SysConfig.Instance.HT_Port, out iPort);
                _EndPoint = new IPEndPoint(IPAddress.Parse(SysConfig.Instance.HT_IP), iPort);
                _STX = new byte[] {0xAA, 0xAA, 0xAA, 0xA0};
                _DicAlertBuffer = new Dictionary<byte, CmdType>();
                networkUdp = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.UdpServer, SysConfig.Instance.HT_MyPort);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "ShiAdapter | Initialize", ex.Message);
                return false;
            }
        }

        public bool StartAdapter()
        {
            try
            {
                networkUdp.ReceiveFromPeer += NetworkUdp_ReceiveFromPeer;
                isRun = networkUdp.Run();
                _Status = STT.Alive;
                return isRun;
            }
            catch (Exception ex)
            {
                _Status = STT.Dead;
                Log.WriteLog(LogType.Error, "ShiAdapter | StartAdapter", ex.Message);
                return false;
            }
        }

        public bool StopAdapter()
        {
            try
            {
                _Status = STT.Dead;
                networkUdp.ReceiveFromPeer -= NetworkUdp_ReceiveFromPeer;
                isRun = !networkUdp.Down();
                return !isRun;
            }
            catch (Exception ex)
            {
                _Status = STT.InterErr;
                Log.WriteLog(LogType.Error, "STIAdapter | StopAdapter", ex.Message);
                return false;
            }
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(receiveMessageBuffer.ToString());
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"ShiAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
            JObject data = jobj["data"] as JObject;
            string cmd = jobj["command"].ToString();
            CmdType command = (CmdType)Enum.Parse(typeof(CmdType), cmd);
            switch (command)
            {
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    {
                        try
                        {
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);

                            List<byte> chamResponse = new List<byte>();
                            List<byte> tempData = new List<byte>();
                            chamResponse.AddRange(_STX);
                            tempData.Add(0xBB);
                            tempData.Add(0xBB);
                            tempData.Add(0xA1);
                            //Data1 : 입차 0x01, 출차 0x02
                            if(command == CmdType.alert_incar)
                            {
                                tempData.Add(0x01);
                            }
                            else if(command == CmdType.alert_outcar)
                            {
                                tempData.Add(0x02);
                            }
                            //Data2 : 정기 0x01, 방문 0x02, RF카드 발급이 없는 경우 0x01로만 입력
                            tempData.Add(0x01);
                            //동
                            tempData.AddRange(payload.data.dong.FourStringTo4ByteAscii());
                            //호
                            tempData.AddRange(payload.data.ho.FourStringTo4ByteAscii());
                            //차량번호
                            tempData.AddRange(payload.data.car_number.StringToAsciiByte());
                            //체크섬
                            byte chs = tempData.CalCheckSum();
                            if (_DicAlertBuffer.ContainsKey(chs))
                            {
                                //이미 존재하는 체크섬..삭제하자..
                                _DicAlertBuffer.Remove(chs);
                            }

                            _DicAlertBuffer.Add(chs, command);

                            tempData.Add(chs);

                            chamResponse.AddRange(tempData);

                            byte[] responseBytes = chamResponse.ToArray();
                            networkUdp.SendToPeer(responseBytes, 0, responseBytes.Length, ep: _EndPoint);
                        }
                        catch (Exception ex)
                        {
                            Log.WriteLog(LogType.Error, $"ShiAdapter | SendMessage", $"{ex.Message}");
                        }
                    }
                    break;
            }
        }

        private void NetworkUdp_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null, System.Net.EndPoint ep = null)
        {
            try
            {
                if (_STX.SequenceEqual(buffer[..4]))
                {
                    //Packet Type 가져오기
                    byte[] packetType = buffer[6..7];
                    byte[] chss = buffer[7..];
                    if (packetType.Length == 0) return;
                    else
                    {
                        switch (packetType[0]) 
                        {
                            case 0xA0: //상태요청
                                //들어온 패킷 그대로 바로 응답을 보낸다. 
                                networkUdp.SendToPeer(buffer, offset, size, ep: _EndPoint);
                                break;
                            case 0xA1: //입출차통보 응답
                                ResponsePayload responsePayload = new ResponsePayload();
                                byte[] responseBuffer;
                                byte chs = chss[0];
                                if (_DicAlertBuffer.ContainsKey(chs))
                                {
                                    responsePayload.command = _DicAlertBuffer[chs];
                                    _DicAlertBuffer.Remove(chs);
                                }
                                else
                                {
                                    responsePayload.command = CmdType.alert_incar;
                                }
                                
                                responsePayload.result = ResultType.OK;
                                responseBuffer = responsePayload.Serialize();
                                TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "STIAdapter | StopAdapter", ex.Message);
            }
        }

        public void TestReceive(byte[] buffer)
        {
            
        }
    }
}
