using NpmAdapter.Model;
using System;
using System.Collections.Generic;
using System.Text;
using NpmAdapter.Payload;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace NpmAdapter.Adapter
{
    class ITronHighPassAdapter : IAdapter
    {
        private IPAddress localAddr = null;
        private IPEndPoint remoteEndPoint = null;
        private TcpClient client = new TcpClient();
        private object objLock = new object();

        private Thread AliveCheckThread;
        private TimeSpan waitForAliveCheckProcess;
        private ManualResetEventSlim shutdownAliveCheckEvent = new ManualResetEventSlim(false);
        private ManualResetEvent _pauseFailAliveCheckEvent = new ManualResetEvent(false);
        private delegate void AccessTokenSafeCallDelegate();

        private static byte[] AliveCheckData;
        private StringBuilder receiveMessageBuffer;

        /// <summary>
        /// 하이패스 TCP IP
        /// </summary>
        private string hiIp { get; }
        /// <summary>
        /// 하이패스 TCP Port
        /// </summary>
        private int hiPort { get; }

        public ITronHighPassAdapter(string ip, int port)
        {
            hiIp = ip;
            hiPort = port;
        }

        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => true;

        public string reqPid { get; set; }

        public event IAdapter.ShowBallonTip ShowTip;

        public void Dispose()
        {
            
            
        }

        public bool Initialize()
        {
            try
            {
                objLock = new object();
                receiveMessageBuffer = new StringBuilder();

                AliveCheckData = GetInitAliveCheckData();

                AliveCheckThread = new Thread(new ThreadStart(AliveCheckAction));
                AliveCheckThread.Name = "Alive_Check";
                waitForAliveCheckProcess = TimeSpan.FromMinutes(1); //1분

                Log.WriteLog(LogType.Error, "Hipass | Initialize", "초기화 성공", LogAdpType.HomeNet);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "Hipass | Initialize", ex.StackTrace, LogAdpType.HomeNet);
            }

            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            lock (objLock)
            {
                try
                {
                    receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                    var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
                    Thread.Sleep(10);
                    receiveMessageBuffer.Clear();

                    HipassRequestPayload hipassRequest = new HipassRequestPayload();
                    hipassRequest.Deserialize(jobj);

                    HipassReceivePayload receivePayload = Payment(hipassRequest.fee, hipassRequest.fee, hipassRequest.car_number);

                    byte[] sendBuffer = null;
                    if (receivePayload != null)
                    {
                        sendBuffer = hipassRequest.ResponseSerialize(receivePayload.SerializeJson(), true);
                    }
                    else //뭔가 문제가 생긴거...
                    {
                        sendBuffer = hipassRequest.ResponseSerialize(null, false);
                    }

                    TargetAdapter.SendMessage(sendBuffer, 0, sendBuffer.Length, pid);
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "Hipass | SendMessage", $"{ex.Message}\r\n{ex.StackTrace}", LogAdpType.HomeNet);
                }
            }
        }

        public bool StartAdapter()
        {
            bool bStart = Connect();
            if (AliveCheckThread.IsAlive)
            {
                _pauseFailAliveCheckEvent.Set();
            }
            else
            {
                AliveCheckThread.Start();
                _pauseFailAliveCheckEvent.Set();
            }
            return bStart;
        }

        public bool StopAdapter()
        {
            try
            {
                _pauseFailAliveCheckEvent.Reset();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "KakaoMovilAdapter | StopAdapter", $"{ex.StackTrace}", LogAdpType.HomeNet);
                return false;
            }

            return true;
        }

        private byte[] GetInitAliveCheckData()
        {
            byte[] data = new byte[500];
            data[0] = 0x02; //STX
            MsgToByte(ref data, 1, 4, "0494"); //Length
            MsgToByte(ref data, 5, 4, "0102"); //Code
            MsgToByte(ref data, 9, 3, "000"); //6종OBU할인율
            MsgToByte(ref data, 12, 3, SysConfig.Instance.HIP_Dsc_Normal); //일반
            MsgToByte(ref data, 15, 3, SysConfig.Instance.HIP_Dsc_Disable); //장애인 1~6급
            MsgToByte(ref data, 18, 3, SysConfig.Instance.HIP_Dsc_National15); //국가유공상이자 1~5급
            MsgToByte(ref data, 21, 3, SysConfig.Instance.HIP_Dsc_National67); //국가유공상이자 6~7급
            MsgToByte(ref data, 24, 3, SysConfig.Instance.HIP_Dsc_518Demo15); //5.18민주화운동부상자 1~5급
            MsgToByte(ref data, 27, 3, SysConfig.Instance.HIP_Dsc_518Demo6); //5.18민주화운동부상자 6급 이하
            MsgToByte(ref data, 30, 3, SysConfig.Instance.HIP_Dsc_Independent); //독립유공자
            MsgToByte(ref data, 33, 3, SysConfig.Instance.HIP_Dsc_DeadLeaf); //고엽체후유(의)중환자
            MsgToByte(ref data, 36, 3, "000");
            MsgToByte(ref data, 39, 3, "000");
            MsgToByte(ref data, 42, 3, "000");
            MsgToByte(ref data, 45, 3, "000");
            MsgToByte(ref data, 48, 3, "000");
            MsgToByte(ref data, 51, 3, "000");
            MsgToByte(ref data, 54, 3, "000");
            MsgToByte(ref data, 57, 3, "000");
            MsgToByte(ref data, 60, 439, ""); //DUMMY
            data[499] = 0x03; //ETX

            return data;
        }

        private HipassReceivePayload Payment(string parkfee, string accountfee, string hicarno)
        {
            if (client.Connected != true)
            {
                Log.WriteLog(LogType.Info, "Hipass | Payment", "Server Disconnected...", LogAdpType.HomeNet);
                Log.WriteLog(LogType.Info, "Hipass | Payment", "Retry Connect to Server......", LogAdpType.HomeNet);

                if (!Connect())
                {
                    Log.WriteLog(LogType.Error, "Hipass | Payment", "Connected fail", LogAdpType.HomeNet);
                    return null;
                }
            }

            try
            {
                Log.WriteLog(LogType.Error, "Hipass | Payment", $"전송 : {hiIp} {hiPort}", LogAdpType.HomeNet);

                byte[] data = new byte[500];

                data[0] = 0x02;
                MsgToByte(ref data, 1, 4, "0494"); //length
                MsgToByte(ref data, 5, 4, "0101"); //code
                MsgToByte(ref data, 9, 8, "12345678"); //gamangcode
                MsgToByte(ref data, 17, 20, string.Format("{0:D20}", Convert.ToInt16("5"))); //PosAdminID
                MsgToByte(ref data, 37, 8, string.Format("{0:D8}", Convert.ToInt64(accountfee))); //accountfee (받아올것)
                MsgToByte(ref data, 45, 8, "00000000"); //vat
                MsgToByte(ref data, 53, 8, "00000000"); //servicefee
                MsgToByte(ref data, 61, 20, string.Format("{0:D20}", Convert.ToInt64("00100010010102"))); //TID
                string vanreg = string.Empty;
                if (hicarno.Contains("X"))
                {
                    vanreg = "99" + DateTime.Now.ToString("HHmm");
                }
                else
                {
                    if (hicarno.Length < 8)
                    {
                        vanreg = hicarno.Substring(3) + DateTime.Now.ToString("HHmm");
                    }
                    else
                    {
                        vanreg = hicarno.Substring(5) + DateTime.Now.ToString("HHmm");
                    }
                }
                MsgToByte(ref data, 81, 8, string.Format("{0:D8}", Convert.ToInt64(vanreg))); //단말기 거래일련번호
                MsgToByte(ref data, 89, 14, DateTime.Now.ToString("yyyyMMddHHmmss")); //outtime (받아올것)
                MsgToByte(ref data, 103, 14, DateTime.Now.ToString("yyyyMMddHHmmss")); //intime (받아올것)
                MsgToByte(ref data, 117, 4, "02FD"); //version
                MsgToByte(ref data, 121, 8, string.Format("{0:D8}", Convert.ToInt64(parkfee))); //parkfee (받아올것)
                MsgToByte(ref data, 129, 1, SysConfig.Instance.HIP_Dsc_Use); //disuse 옵션설정 : 1번이면 할인율 적용안하고 2번이면 할인율이 적용됨.
                MsgToByte(ref data, 130, 369, ""); //dummy
                data[499] = 0x03;

                Log.WriteLog(LogType.Info, "Hipass | Payment", $"전송 데이터 : {SysConfig.Instance.HomeNet_Encoding.GetString(data)}", LogAdpType.HomeNet);

                NetworkStream stream = client.GetStream();

                stream.Write(data, 0, data.Length);

                Log.WriteLog(LogType.Info, "Hipass | Payment", $"전송완료", LogAdpType.HomeNet);

                byte[] rdata = new byte[500];

                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                stream.Read(rdata, 0, rdata.Length);

                responseData = Encoding.Default.GetString(rdata, 0, rdata.Length);

                Log.WriteLog(LogType.Info, "Hipass | Payment", $"수신 데이터 : {responseData}", LogAdpType.HomeNet);

                if (rdata[0] != 0x02)
                {
                    Log.WriteLog(LogType.Error, "Hipass | Payment", $"결제 응답 stx 체크 오류", LogAdpType.HomeNet);
                    return null;
                }
                if (rdata[499] != 0x03)
                {
                    Log.WriteLog(LogType.Error, "Hipass | Payment", $"결제 응답 etx 체크 오류", LogAdpType.HomeNet);
                    return null;
                }

                HipassReceivePayload HipassRecvData = new HipassReceivePayload();
                HipassRecvData.Initialize();

                HipassRecvData.stx = System.Text.Encoding.UTF8.GetString(rdata, 0, 1);
                HipassRecvData.length = System.Text.Encoding.UTF8.GetString(rdata, 1, 4);
                HipassRecvData.code = System.Text.Encoding.UTF8.GetString(rdata, 5, 4); //0001
                HipassRecvData.gamangcode = System.Text.Encoding.UTF8.GetString(rdata, 9, 8); //가맹점번호
                HipassRecvData.PosID = System.Text.Encoding.UTF8.GetString(rdata, 17, 20); //포스기 아이디
                HipassRecvData.PosSeq = System.Text.Encoding.UTF8.GetString(rdata, 37, 8); //포스기당 거래 일련번호
                HipassRecvData.viocode = System.Text.Encoding.UTF8.GetString(rdata, 45, 2); //위반코드
                HipassRecvData.receipt_resq = System.Text.Encoding.UTF8.GetString(rdata, 47, 1); //수납여부판단요청
                HipassRecvData.cardsunbul = System.Text.Encoding.UTF8.GetString(rdata, 48, 1); //이용자카드 선/후불 구분
                HipassRecvData.cardno = System.Text.Encoding.UTF8.GetString(rdata, 49, 16); //이용자카드 일련번호
                HipassRecvData.parkaccountfee = System.Text.Encoding.UTF8.GetString(rdata, 65, 8); //지불요금
                HipassRecvData.afterdisaccount = System.Text.Encoding.UTF8.GetString(rdata, 73, 8); //할인 적용 지불요금
                HipassRecvData.accountfee = System.Text.Encoding.UTF8.GetString(rdata, 81, 8); //수납금액
                HipassRecvData.wongum = System.Text.Encoding.UTF8.GetString(rdata, 89, 8); //수납전 카드잔액
                HipassRecvData.OBU_Carkind = System.Text.Encoding.UTF8.GetString(rdata, 97, 1); //OBU 차종
                HipassRecvData.OBU_Kind = System.Text.Encoding.UTF8.GetString(rdata, 98, 2); //OUB 종류
                HipassRecvData.suNap = System.Text.Encoding.UTF8.GetString(rdata, 100, 1); //수납구분

                if (HipassRecvData.viocode == "00" && Convert.ToInt32(HipassRecvData.accountfee) > 0)
                {
                    stream.Close();
                    Log.WriteLog(LogType.Info, "Hipass | Payment", $"Hipass hipass_pay - 응답 : {responseData}", LogAdpType.HomeNet);
                    return HipassRecvData;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "Hipass | Payment", $"{ex.StackTrace}", LogAdpType.HomeNet);
                return null;
            }
        }

        private void AliveCheckAction()
        {
            do
            {
                if (shutdownAliveCheckEvent.IsSet) return;
                {
                    try
                    {
                        if (client.Connected != true)
                        {
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", "Server Disconnected...", LogAdpType.HomeNet);
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", "Retry Connect to Server......", LogAdpType.HomeNet);

                            if (!Connect())
                            {
                                Log.WriteLog(LogType.Error, "Hipass | AliveCheck", "Connected fail", LogAdpType.HomeNet);
                                return;
                            }
                        }

                        Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"전송 : {hiIp} {hiPort}", LogAdpType.HomeNet);
                        Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"AliveCheck Data : {SysConfig.Instance.HomeNet_Encoding.GetString(AliveCheckData)}", LogAdpType.HomeNet);

                        using (NetworkStream stream = client.GetStream())
                        {
                            stream.Write(AliveCheckData, 0, AliveCheckData.Length);
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"전송완료", LogAdpType.HomeNet);

                            byte[] rdata = new byte[500];

                            // Read the first batch of the TcpServer response bytes.
                            stream.Read(rdata, 0, rdata.Length);
                            string responseData = SysConfig.Instance.HomeNet_Encoding.GetString(rdata, 0, rdata.Length);
                            string status = SysConfig.Instance.HomeNet_Encoding.GetString(rdata, 60, 2);
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"AliveCheck Recevie Data : {responseData}, Status Code : {status.Trim('\0')}", LogAdpType.HomeNet);
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"수신완료", LogAdpType.HomeNet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogType.Error, $"KakaoMovilAdapter | AccessTokenAction", $"{ex.StackTrace}");
                    }
                }

                shutdownAliveCheckEvent.Wait(waitForAliveCheckProcess);
            }
            while (_pauseFailAliveCheckEvent.WaitOne());
        }

        public bool Connect()
        {
            try
            {
                localAddr = IPAddress.Parse(hiIp);
                remoteEndPoint = new IPEndPoint(localAddr, hiPort);
                client = K_TimeOutSocket.K_Connect(remoteEndPoint, 4000);
                client.ReceiveTimeout = 4000;
                Log.WriteLog(LogType.Info, "Hipass | Connect", $"== 연결성공 ==", LogAdpType.HomeNet);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "Hipass | Connect", $"{ex.StackTrace}", LogAdpType.HomeNet);
                // 연결실패
                return false;
            }
        }

        private void MsgToByte(ref Byte[] bytes, int start, int len, string msg)
        {
            Byte[] temp_bytes = System.Text.Encoding.UTF8.GetBytes(msg);

            int count = 0;
            for (int i = start; i < start + len; i++)
            {
                if (count < temp_bytes.Length)
                {
                    bytes[i] = temp_bytes[count];
                }
                else
                {
                    bytes[i] = 0x00;
                }
                count++;
            }
        }

        public void TestReceive(byte[] buffer)
        {

        }
    }

    class K_TimeOutSocket
    {
        private static bool IsConnectionSuccessful = false;
        private static Exception socketexception;
        private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);

        public static TcpClient K_Connect(IPEndPoint remoteEndPoint, int timeoutMSec)
        {
            TimeoutObject.Reset();
            socketexception = null;

            string serverip = Convert.ToString(remoteEndPoint.Address);
            int serverport = remoteEndPoint.Port;
            TcpClient tcpclient = new TcpClient();

            tcpclient.BeginConnect(serverip, serverport,
                new AsyncCallback(CallBackMethod), tcpclient);

            if (TimeoutObject.WaitOne(timeoutMSec, false))
            {
                if (IsConnectionSuccessful)
                {
                    return tcpclient;
                }
                else
                {
                    throw socketexception;
                }
            }
            else
            {
                tcpclient.Close();
                throw new TimeoutException("TimeOut Exception");
            }
        }

        private static void CallBackMethod(IAsyncResult asyncresult)
        {
            try
            {
                IsConnectionSuccessful = false;
                TcpClient tcpclient = asyncresult.AsyncState as TcpClient;

                if (tcpclient.Client != null)
                {
                    tcpclient.EndConnect(asyncresult);
                    IsConnectionSuccessful = true;
                }
            }
            catch (Exception ex)
            {
                IsConnectionSuccessful = false;
                socketexception = ex;
            }
            finally
            {
                TimeoutObject.Set();
            }
        }
    }
}
