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
    /// <summary>
    /// 탑통신
    /// 이상삼 대표 : 010-4738-6223
    /// 프로그램 박용석 이사 : 010-2014-8911
    /// 현장 : 부평 산곡 푸르지오
    /// </summary>
    class STIAdapter : IAdapter
    {
        private bool isRun = false;
        private string tcpServerIp = "0.0.0.0";
        private string tcpport = "29712";

        private StringBuilder receiveMessageBuffer = new StringBuilder();

        private const byte IN = 0x30;
        private const byte OUT = 0x31;
        private const byte COMMA = 0x2C;

        private INetwork TcpClientNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning => isRun;

        public void Dispose()
        {
            StopAdapter();
        }

        public bool Initialize()
        {
            try
            {
                tcpServerIp = SysConfig.Instance.HT_IP;
                tcpport = SysConfig.Instance.HT_Port;
                Log.WriteLog(LogType.Info, $"STIAdapter | Initialize", $"TpcNetwork IP :{tcpServerIp}, Port :{tcpport}", LogAdpType.HomeNet);

                TcpClientNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, tcpServerIp, tcpport);

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "STIAdapter | Initialize", ex.Message);
                return false;
            }
        }

        

        public bool StartAdapter()
        {
            try
            {
#if (DEBUG)
                return true;
#else
                isRun = TcpClientNetwork.Run();
                return isRun;
#endif
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "STIAdapter | StartAdapter", ex.Message);
                return false;
            }
        }

        public bool StopAdapter()
        {
            try
            {
#if (DEBUG)
                return true;
#else
                isRun = !TcpClientNetwork.Down();
                return !isRun;
#endif
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "STIAdapter | StopAdapter", ex.Message);
                return false;
            }
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            receiveMessageBuffer.Append(buffer[..(int)size].ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(receiveMessageBuffer.ToString());
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"STIAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);

            string cmd = jobj["command"].ToString();
            switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
            {
                //입/출차 통보
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    {
                        RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                        payload.Deserialize(jobj);
                        
                        ResponsePayload responsePayload = new ResponsePayload();
                        responsePayload.command = payload.command;

                        //동/호 없으면 PASS
                        if (payload.data == null || payload.data.dong == null || payload.data.ho == null || payload.data.dong == "" || payload.data.ho == "")
                        {
                            responsePayload.result = ResultType.FailFormatError;
                            byte[] errResult = responsePayload.Serialize();
                            TargetAdapter.SendMessage(errResult, 0, errResult.Length);
                            Log.WriteLog(LogType.Info, $"STIAdapter | SendMessage", $"전송메시지 : {responsePayload.ToJson().ToString()}", LogAdpType.Nexpa);
                        }
                        //일반차량 PASS
                        else if (payload.data.kind.ToLower() == "n") return;
                        else
                        {
                            List<byte> tempBytes = new List<byte>();
                            //입/출차 구분
                            if (payload.command == CmdType.alert_incar)
                            {
                                tempBytes.Add(IN);
                            }
                            else if (payload.command == CmdType.alert_outcar)
                            {
                                tempBytes.Add(OUT);
                            }
                            //구분자
                            tempBytes.Add(COMMA);
                            //동
                            tempBytes.AddRange(payload.data.dong.FourStringTo4ByteAscii());
                            //구분자
                            tempBytes.Add(COMMA);
                            //호
                            tempBytes.AddRange(payload.data.ho.FourStringTo4ByteAscii());
                            //구분자
                            tempBytes.Add(COMMA);
                            //차량번호 뒤 4자리
                            string carno = payload.data.car_number;
                            tempBytes.AddRange(carno.StringToAsciiByte());
                            //구분자
                            tempBytes.Add(COMMA);
                            //입차시간(yyyyMMddHHmmss)
                            string time = payload.data.date_time;
                            tempBytes.AddRange(time.StringToAsciiByte());

                            //전송~!
                            byte[] bArrSendMesssage = tempBytes.ToArray();
                            Log.WriteLog(LogType.Info, $"STIAdapter | SendMessage", $"전송 메시지 : {bArrSendMesssage.ToHexString()}", LogAdpType.HomeNet);
                            TcpClientNetwork.SendToPeer(bArrSendMesssage, 0, bArrSendMesssage.Length);
                            
                            if(TcpClientNetwork.Status == NetStatus.Connected)
                            {
                                responsePayload.result = ResultType.OK;
                            }
                            else
                            {
                                responsePayload.result = ResultType.HomenetkDisconnected;
                            }
                            byte[] arrResult = responsePayload.Serialize();
                            TargetAdapter.SendMessage(arrResult, 0, arrResult.Length);
                        }
                    }
                    break;
            }
        }
        
        public void TestReceive(byte[] buffer)
        {
            string strAlert = "{\"command\":\"alert_outcar\",\"data\":{\"dong\":\"103\",\"ho\":\"302\",\"car_number\":\"11가1111\",\"date_time\":\"20210322000005\",\"kind\":\"a\",\"lprid\":\"15\",\"car_image\":\"uB85C2419.jpg\",\"reg_no\":\"\",\"visit_in_date_time\":\"\",\"visit_out_date_time\":\"\"}}";
            byte[] bArr = SysConfig.Instance.Nexpa_Encoding.GetBytes(strAlert);
            SendMessage(bArr, 0, bArr.Length);
        }
    }
}
