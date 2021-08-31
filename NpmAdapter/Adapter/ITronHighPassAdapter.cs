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

        private INetwork tcpClient { get; set; }

        private static HipassRequestPayload CurrentRequest;
        private static HipassReceivePayload HipassRecvData;

        private bool bResponseSuccess = false;

        public void Dispose()
        {
            tcpClient.Down();
            _pauseFailAliveCheckEvent.Reset();
            AliveCheckThread.Abort();
        }

        public bool Initialize()
        {
            try
            {
                objLock = new object();
                receiveMessageBuffer = new StringBuilder();

                tcpClient = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, hiIp, hiPort.ToString());
                tcpClient.ReceiveFromPeer += Clients_ReceiveFromPeer;

                CurrentRequest = new HipassRequestPayload();
                HipassRecvData = new HipassReceivePayload();

                AliveCheckData = GetInitAliveCheckData();

                AliveCheckThread = new Thread(new ThreadStart(AliveCheckAction));
                AliveCheckThread.Name = "Alive_Check";
                waitForAliveCheckProcess = TimeSpan.FromMinutes(1); //1분

                Log.WriteLog(LogType.Info, "Hipass | Initialize", "초기화 성공", LogAdpType.HomeNet);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "Hipass | Initialize", ex.StackTrace, LogAdpType.HomeNet);
            }

            return true;
        }

        public bool StartAdapter()
        {
            bool bStart = tcpClient.Run();

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
            bool bStart = false;

            try
            {
                bStart = !tcpClient.Down();
                _pauseFailAliveCheckEvent.Reset();

            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "KakaoMovilAdapter | StopAdapter", $"{ex.StackTrace}", LogAdpType.HomeNet);
                return false;
            }

            return bStart;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            lock (objLock)
            {
                try
                {
                    byte[] sendBuffer = null;
                    bResponseSuccess = false;

                    receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                    var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));
                    Thread.Sleep(10);
                    receiveMessageBuffer.Clear();

                    CurrentRequest.Initialize();
                    CurrentRequest.Deserialize(jobj);

                    //하이패스 장비로 전송
                    if(Payment(CurrentRequest.fee, CurrentRequest.fee, CurrentRequest.car_number))
                    {
                        // === 응답이 올때까지 대기 ===
                        int iSec = 5 * 100; //10초
                        while (iSec > 0 && !bResponseSuccess)
                        {
                            Thread.Sleep(10); //0.01초씩..쉰다...
                            iSec -= 1;
                        }
                        // === 응답이 올때까지 대기 ===

                        if (bResponseSuccess)
                        {
                            Log.WriteLog(LogType.Info, "Hipass | SendMessage", $"응답성공 : {HipassRecvData.SerializeJson()}", LogAdpType.HomeNet);

                            if (HipassRecvData.viocode == "00" && Convert.ToInt32(HipassRecvData.accountfee) > 0)
                            {
                                sendBuffer = CurrentRequest.ResponseSerialize(HipassRecvData.SerializeJson(), 200);
                            }
                            else
                            {
                                //ERROR : 하이패스 결제 실패
                                sendBuffer = CurrentRequest.ResponseSerialize(null, 501);
                            }
                        }
                        else
                        {
                            //ERROR : Request Time Out
                            sendBuffer = CurrentRequest.ResponseSerialize(null, 408);
                        }

                        CurrentRequest.Initialize();
                        TargetAdapter.SendMessage(sendBuffer, 0, sendBuffer.Length, pid);
                    }
                    else
                    {
                        //ERROR : 하이패스 장비 연동 실패
                        sendBuffer = CurrentRequest.ResponseSerialize(null, 400);
                        CurrentRequest.Initialize();
                        TargetAdapter.SendMessage(sendBuffer, 0, sendBuffer.Length, pid);
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "Hipass | SendMessage", $"{ex.Message}\r\n{ex.StackTrace}", LogAdpType.HomeNet);
                    receiveMessageBuffer.Clear();
                }
            }
        }

        private void Clients_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null, EndPoint ep = null)
        {
            var responseData = buffer.ToString(SysConfig.Instance.HomeNet_Encoding, size);

            if (buffer[0] != 0x02)
            {
                Log.WriteLog(LogType.Error, "Hipass | Payment", $"결제 응답 stx 체크 오류", LogAdpType.HomeNet);
                return;
            }
            if (buffer[499] != 0x03)
            {
                Log.WriteLog(LogType.Error, "Hipass | Payment", $"결제 응답 etx 체크 오류", LogAdpType.HomeNet);
                return;
            }

            HipassRecvData.Initialize();

            var code = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 5, 4);

            if(code == "0002")
            {
                //AliveCheck 응답
                string status = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 60, 2);
                Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"AliveCheck Recevie Data : {responseData}, Status Code : {status.Trim('\0')}", LogAdpType.HomeNet);
                Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"수신완료", LogAdpType.HomeNet);
            }
            else if(code == "0001")
            {
                //Payment 응답
                HipassRecvData.stx = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 0, 1);
                HipassRecvData.length = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 1, 4);
                HipassRecvData.code = code; //0001
                HipassRecvData.gamangcode = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 9, 8); //가맹점번호
                HipassRecvData.PosID = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 17, 20); //포스기 아이디
                HipassRecvData.PosSeq = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 37, 8); //포스기당 거래 일련번호
                HipassRecvData.viocode = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 45, 2); //위반코드
                HipassRecvData.receipt_resq = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 47, 1); //수납여부판단요청
                HipassRecvData.cardsunbul = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 48, 1); //이용자카드 선/후불 구분
                HipassRecvData.cardno = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 49, 16); //이용자카드 일련번호
                HipassRecvData.parkaccountfee = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 65, 8); //지불요금
                HipassRecvData.afterdisaccount = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 73, 8); //할인 적용 지불요금
                HipassRecvData.accountfee = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 81, 8); //수납금액
                HipassRecvData.wongum = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 89, 8); //수납전 카드잔액
                HipassRecvData.OBU_Carkind = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 97, 1); //OBU 차종
                HipassRecvData.OBU_Kind = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 98, 2); //OUB 종류
                HipassRecvData.suNap = SysConfig.Instance.HomeNet_Encoding.GetString(buffer, 100, 1); //수납구분

                Log.WriteLog(LogType.Info, "Hipass | Payment", $"응답코드 : {HipassRecvData.viocode} : {GetDescriptionCode(HipassRecvData.viocode)} \r\nHipass hipass_pay - 응답 : {responseData}", LogAdpType.HomeNet);

                bResponseSuccess = true;
            }
        }

        private string GetDescriptionCode(string code)
        {
            switch (code)
            {
                case "00":
                    return "정상";
                case "02":
                    return "Many OBU";
                case "14":
                    return "OBU 유효기간초과";
                case "15":
                    return "OBU 인증이상";
                case "42":
                    return "카드 S3인증후에러";
                case "10":
                    return "OBU기본정보이상";
                case "13":
                    return "OBUB/L";
                case "01":
                    return "OBU 미부착";
                case "11":
                    return "OBU 전원이상";
                case "33":
                    return "카드 초기화 이상";
                case "39":
                    return "카드 S1인증 이상";
                case "40":
                    return "카드 S2인증실패";
                case "41":
                    return "카드 S3인증 이상";
                case "43":
                    return "카드 S3 미수신";
                case "32":
                    return "OBU 정보 미수신";
                case "30":
                    return "카드미삽입";
                case "31":
                    return "카드오삽입";
                case "34":
                    return "카드 종류이상";
                case "37":
                    return "카드 B/L";
                case "38":
                    return "카드 유효기간 초과";
                case "81":
                    return "카드번호이상";
                case "35":
                    return "카드 잔액없음";
                case "36":
                    return "카드 잔액부족";
                default:
                    return "";
            }
        }

        private bool Payment(string parkfee, string accountfee, string hicarno)
        {
            bool bConnet = false;

            if (tcpClient.Status == NetStatus.Disconnected)
            {
                Log.WriteLog(LogType.Info, "Hipass | Payment", "Server Disconnected...", LogAdpType.HomeNet);
                Log.WriteLog(LogType.Info, "Hipass | Payment", "Retry Connect to Server......", LogAdpType.HomeNet);

                if (tcpClient.Run())
                {
                    bConnet = true;
                }
                else
                {
                    Log.WriteLog(LogType.Error, "Hipass | Payment", "Connected fail", LogAdpType.HomeNet);
                    bConnet = false;
                }
            }
            else
            {
                bConnet = true;
            }

            try
            {
                if (bConnet)
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
                    tcpClient.SendToPeer(data, 0, data.Length);
                    Log.WriteLog(LogType.Info, "Hipass | Payment", $"전송완료", LogAdpType.HomeNet);
                }

                return bConnet;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "Hipass | Payment", $"{ex.StackTrace}", LogAdpType.HomeNet);
                return false;
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
                        if (tcpClient.Status == NetStatus.Disconnected)
                        {
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", "Server Disconnected...", LogAdpType.HomeNet);
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", "Retry Connect to Server......", LogAdpType.HomeNet);

                            if (tcpClient.Run())
                            {
                                Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"전송 : {hiIp} {hiPort}", LogAdpType.HomeNet);
                                Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"AliveCheck Data : {SysConfig.Instance.HomeNet_Encoding.GetString(AliveCheckData)}", LogAdpType.HomeNet);

                                tcpClient.SendToPeer(AliveCheckData, 0, AliveCheckData.Length);
                                Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"전송완료", LogAdpType.HomeNet);
                            }
                            else
                            {
                                Log.WriteLog(LogType.Error, "Hipass | AliveCheck", "Connected fail", LogAdpType.HomeNet);
                            }
                        }
                        else
                        {
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"전송 : {hiIp} {hiPort}", LogAdpType.HomeNet);
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"AliveCheck Data : {SysConfig.Instance.HomeNet_Encoding.GetString(AliveCheckData)}", LogAdpType.HomeNet);

                            tcpClient.SendToPeer(AliveCheckData, 0, AliveCheckData.Length);
                            Log.WriteLog(LogType.Info, "Hipass | AliveCheck", $"전송완료", LogAdpType.HomeNet);
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

        private byte[] GetInitAliveCheckData()
        {
            byte[] data = new byte[500];
            data[0] = 0x02; //STX
            MsgToByte(ref data, 1, 4, "0494"); //Length
            MsgToByte(ref data, 5, 4, "0102"); //Code
            MsgToByte(ref data, 9, 3, SysConfig.Instance.HIP_Dsc_Small); //6종OBU할인율
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

        private void MsgToByte(ref Byte[] bytes, int start, int len, string msg)
        {
            Byte[] temp_bytes = SysConfig.Instance.HomeNet_Encoding.GetBytes(msg);

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
}
