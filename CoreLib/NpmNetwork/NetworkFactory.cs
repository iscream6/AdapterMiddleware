using NpmCommon;
using System.Net;

namespace NpmNetwork
{
    public class NetworkFactory
    {
        private static NetworkFactory instance;

        private NetworkFactory() { }

        public static NetworkFactory GetInstance()
        {
            if (instance == null) instance = new NetworkFactory();
            return instance;
        }

        public enum Adapters
        {
            TcpServer,
            TcpClient,
            UdpServer,
            UdpClient,
            HttpServer,
            SerialPort,
        }

        public INetwork MakeNetworkControl(Adapters adapter, params string[] args)
        {
            INetwork homeNetServer = null;
            switch (adapter)
            {
                case Adapters.TcpServer: //Param : port

                    if (args.Length > 0)
                    {
                        string sPort = args[0];
                        int iPort = -1;
                        if (int.TryParse(sPort, out iPort))
                        {
                            homeNetServer = new NetworkTcpServer(IPAddress.Any, iPort);
                            ((NetworkTcpServer)homeNetServer).MyEncoding = SysConfig.Instance.Nexpa_Encoding;
                        }
                    }
                    break;
                case Adapters.TcpClient: //Param : Server IP, Server Port
                    if (args.Length > 1)
                    {
                        string sPort = args[1];
                        int iPort = -1;
                        if (int.TryParse(sPort, out iPort))
                        {
                            homeNetServer = new NetworkTcpClient(args[0], iPort);
                        }
                    }
                    break;
                case Adapters.UdpServer:
                    if (args.Length > 0)
                    {
                        string sPort = args[0];
                        int iPort = -1;
                        if (int.TryParse(sPort, out iPort))
                        {
                            homeNetServer = new NetworkUdpServer(IPAddress.Any, iPort);
                        }
                    }
                    break;
                case Adapters.UdpClient:
                    if (args.Length > 1)
                    {
                        string sPort = args[1];
                        int iPort = -1;
                        if (int.TryParse(sPort, out iPort))
                        {
                            homeNetServer = new NetworkUdpClient(args[0], iPort);
                        }
                    }
                    break;
                case Adapters.HttpServer: //Param : port

                    if (args.Length > 0)
                    {
                        string sPort = args[0];
                        int iPort = -1;
                        if (int.TryParse(sPort, out iPort))
                        {
                            homeNetServer = new NetworkWebServer(iPort);
                        }
                    }
                    break;
                case Adapters.SerialPort: //Param : BaudRate, PortName, Parity
                    if(args.Length > 2)
                    {
                        NetworkSerial rS485Serial = new NetworkSerial();
                        rS485Serial.BaudRateString = args[0];
                        rS485Serial.PortNameString = args[1];
                        rS485Serial.ParityString = args[2];
                        homeNetServer = rS485Serial;
                    }
                    break;
            }
            return homeNetServer;
        }
    }
}
