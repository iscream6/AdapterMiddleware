using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;

namespace NexpaAdapterStandardLib.Network
{
    class NetworkSerial : AbstractSerialPort<bool>, INetwork
    {
        private const byte NotRetryPacket = 0x0A;
        private const byte RetryPacket = 0x0B;

        private enum ProtocolStep
        {
            Ready,
            DoCommand,
            ReceiveACK,
            SendENQ,
            SendACK,
            ReceiveData,
            RemainCoin
        }

        ProtocolStep mStep = ProtocolStep.Ready;

        //public const byte _STX_ = 0x02;
        //public const byte _ETX_ = 0x03;
        //public const byte _ACK_ = 0x06;         //응답 정상 수신
        //public const byte _NACK_ = 0x15;        //응답 재전송 요청 

        private object lockThis = new object();

        public Action OnConnectionAction { get; set; }

        public event SendToPeer ReceiveFromPeer;

        public NetworkSerial()
        {
            SerialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
            SerialPort.ErrorReceived += new SerialErrorReceivedEventHandler(SerialPort_ErrorReceived);
        }

        #region Implements AbstractSerialPort

        public override bool Connect()
        {
            try
            {
                Log.WriteLog(LogType.Info, $"SerialNetwork | Connect", $"Connect 시작");
                if (SerialPort.IsOpen)
                {
                    SerialPort.Close();
                }

                SerialPort.ReadTimeout = 1000;
                SerialPort.WriteTimeout = 1000;
                SerialPort.DtrEnable = true;
                SerialPort.RtsEnable = true;

                SerialPort.DataBits = 8;
                SerialPort.StopBits = System.IO.Ports.StopBits.One;
                SerialPort.Handshake = System.IO.Ports.Handshake.None;

                SerialPort.Open();
                Initialize();

                Log.WriteLog(LogType.Info, $"SerialNetwork | Connect", $"Connect 완료");
                SystemStatus.Instance.SendEventMessage(LogAdpType.HomeNet, "Success connect serial network!");
                System.Threading.Thread.Sleep(100);
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"SerialNetwork | Connect", $"Error : {ex.Message}");
                return false;
            }
        }

        public override void Disconnect()
        {
            try
            {
                if (!SerialPort.IsOpen)
                {
                    SystemStatus.Instance.SendEventMessage(LogAdpType.HomeNet, "Already disconnected serial network...");
                    return;
                }
                SerialPort.Close();
                Log.WriteLog(LogType.Info, $"SerialNetwork | Disconnect", $"Disconnect 완료");
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"SerialNetwork | Disconnect", $"Error : {ex.Message}");
            }
        }

        public override void Initialize()
        {
            SerialPort.DiscardInBuffer();
            SerialPort.DiscardOutBuffer();
        }

        protected override void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //Thread.Sleep(800); //SerialPort로부터 Data가 모두 전달되기까지 좀 기다려야 함.
            int length = SerialPort.BytesToRead;
            List<byte> mReadBuffer = new List<byte>();

            for (int i = 0; i < length; i++)
            {
                mReadBuffer.Add((byte)SerialPort.ReadByte());
            }

            //Log.WriteLog(LogType.Info, $"SerialNetwork | SerialPort_DataReceived", $"데이터 수신");

            ReceiveFromPeer?.Invoke(mReadBuffer.ToArray(), 0, mReadBuffer.Count);
            mStep = ProtocolStep.Ready;
        }

        protected override void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            //시리얼 실패함....
            SerialError err = e.EventType;
            string strErr = "";

            switch (err)
            {
                case SerialError.Frame: //하드웨어에서 프레이밍 오류를 발견했습니다.
                    strErr = "HardWare Framing Error";
                    break;
                case SerialError.Overrun: //문자 버터 오버런이 발생했습니다. 다음 문자가 손실됩니다.
                    strErr = "Charaters Buffer Over Run";
                    break;
                case SerialError.RXOver: //입력 버퍼 오버플로가 발생했습니다. 입력 버퍼에 공간이 없거나 파일 끝(EOF) 문자 다음에 문자를 받았습니다.
                    strErr = "Input Buffer OverFlow";
                    break;
                case SerialError.RXParity: //하드웨어에서 패리티 오류를 발견했습니다.
                    strErr = "Founded Parity Error";
                    break;
                case SerialError.TXFull: //어플리케이션에서 문자를 전송하려고 했지만 출력 버커가 꽉 찼습니다.
                    strErr = "Write Buffer was Fulled";
                    break;
                default:
                    strErr = "Unkwon Error";
                    break;
            }

            Log.WriteLog(LogType.Error, $"SerialNetwork | SerialPort_ErrorReceived", $"Error : {strErr}");
        }

        #endregion

        #region Implements INetwork

        public bool Run()
        {
            return Connect();
        }

        public bool Down()
        {
            Disconnect();
            return true;
        }

        public void SendToPeer(byte[] buffer, long offset, long size, string id = null)
        {
            byte[] sendBuffer = new byte[size];
            Array.Copy(buffer, offset, sendBuffer, 0, size);
            bool isRetry = false;
            if (buffer[0] == RetryPacket) isRetry = true;
            else isRetry = false;
            SendByte(sendBuffer, reTry: isRetry);
        }

        #endregion

        private bool SendByte(Byte[] pSendData, int pTImeOut = 3000, int checkCount = 5, bool reTry = false)
        {
            lock (new object())
            {
                try
                {
                    if (!SerialPort.IsOpen)
                    {
                        Log.WriteLog(LogType.Error, $"SerialNetwork | SendByte", $"포트가 열려있지 않습니다.");
                        return false;
                    }

                    SerialPort.Write(pSendData, 0, pSendData.Length);

                    if (reTry) mStep = ProtocolStep.DoCommand;
                    else mStep = ProtocolStep.Ready;

                    DateTime startDate = DateTime.Now;
                    while (mStep != ProtocolStep.Ready)
                    {
                        TimeSpan diff = DateTime.Now - startDate;

                        if (Convert.ToInt32(diff.TotalMilliseconds) > pTImeOut)
                        {
                            if (checkCount == 0)
                            {
                                mStep = ProtocolStep.Ready;
                                return false;
                            }
                            SerialPort.Write(pSendData, 0, pSendData.Length);
                            checkCount -= 1;
                            startDate = DateTime.Now;
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, $"SerialNetwork | SendByte", $"Error : {ex.Message}");
                    mStep = ProtocolStep.Ready;
                    return false;
                }
            }
        }
    }
}
