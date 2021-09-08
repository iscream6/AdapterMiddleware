using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace NpmNetwork
{
    public abstract class AbstractSerialPort<T>
    {
        #region Fields

        private SerialPort mSerialPort = new SerialPort();
        private string mPortNameString = "";
        private string mBaudRateString = "";
        private string mParityString = "";
        private string mDatabitsString = "";
        private string mStopbitsString = "";
        private string mHandshakeString = "";

        #endregion

        #region Properties

        public SerialPort SerialPort { get => mSerialPort; }

        public bool IsConnect { get => mSerialPort.IsOpen; }

        public string PortNameString
        {
            get => mPortNameString;
            set
            {
                mPortNameString = value;

                if (mPortNameString != "")
                    mSerialPort.PortName = mPortNameString;
            }
        }

        public string PortName
        {
            get => mSerialPort.PortName;
            set
            {
                mSerialPort.PortName = value;
                mPortNameString = value;
            }
        }


        public string BaudRateString
        {
            get => mBaudRateString;
            set
            {
                mBaudRateString = value;
                mSerialPort.BaudRate = Convert.ToInt32(mBaudRateString);
            }
        }

        public int BoudRate
        {
            get => mSerialPort.BaudRate;
            set
            {
                mSerialPort.BaudRate = value;
                mBaudRateString = value.ToString();
            }
        }


        public string ParityString
        {
            get => mParityString;
            set
            {
                mParityString = value;

                switch (mParityString.ToUpper())
                {
                    case "NONE":
                        mSerialPort.Parity = Parity.None;
                        break;

                    case "EVEN":
                        mSerialPort.Parity = Parity.Even;
                        break;

                    case "ODD":
                        mSerialPort.Parity = Parity.Odd;
                        break;

                    default:
                        mSerialPort.Parity = Parity.None;
                        break;
                }
            }
        }

        public Parity Parity
        {
            get => mSerialPort.Parity;
            set
            {
                mSerialPort.Parity = value;
                mParityString = value.ToString();
            }
        }


        public string DatabitsString
        {
            get => mDatabitsString;
            set
            {
                mDatabitsString = value;
                if (mDatabitsString.Trim() == "")
                {
                    mDatabitsString = "8";
                }
                mSerialPort.DataBits = Convert.ToInt32(mDatabitsString);
            }
        }

        public int DataBits
        {
            get => mSerialPort.DataBits;
            set
            {
                mSerialPort.DataBits = value;
                mDatabitsString = value.ToString();
            }
        }


        public string StopbitsString
        {
            get => mStopbitsString;
            set
            {
                mStopbitsString = value;

                switch (mStopbitsString)
                {
                    case "1":
                        mSerialPort.StopBits = StopBits.One;
                        break;

                    case "2":
                        mSerialPort.StopBits = StopBits.Two;
                        break;

                    default:
                        mSerialPort.StopBits = StopBits.One;
                        break;
                }
            }
        }

        public StopBits StopBits
        {
            get => mSerialPort.StopBits;
            set
            {
                mSerialPort.StopBits = value;
                mStopbitsString = value.ToString();
            }
        }


        public string HandshakeString
        {
            get => mHandshakeString;
            set
            {
                mHandshakeString = value;

                switch (mHandshakeString.ToUpper())
                {
                    case "NONE":
                        mSerialPort.Handshake = Handshake.None;
                        break;

                    case "RTS":
                        mSerialPort.Handshake = Handshake.RequestToSend;
                        break;

                    case "XON/XOFF":
                        mSerialPort.Handshake = Handshake.XOnXOff;
                        break;

                    case "XONXOFF":
                        mSerialPort.Handshake = Handshake.XOnXOff;
                        break;

                    case "BOTH":
                        mSerialPort.Handshake = Handshake.RequestToSendXOnXOff;
                        break;
                }
            }
        }

        public Handshake Handshake
        {
            get => mSerialPort.Handshake;
            set
            {
                mSerialPort.Handshake = value;
                mHandshakeString = value.ToString();
            }
        }

        #endregion

        #region Abstract Methods

        public abstract void Initialize();

        public abstract T Connect();

        public abstract void Disconnect();

        protected abstract void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e);

        protected abstract void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e);

        #endregion
    }
}
