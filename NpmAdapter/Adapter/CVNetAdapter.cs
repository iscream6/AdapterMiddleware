using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Adapter
{
    class CVNetAdapter : IAdapter
    {
        private const byte STX = 0x02;
        private const byte EXT = 0x03;
        /// <summary>
        /// 메시지의 목적지 장치(홈넷 단지서버)
        /// </summary>
        private const byte DES = 0xB0;
        /// <summary>
        /// 메시지의 출발지 장치(입차 통보서버)
        /// </summary>
        private const byte SOU = 0xD2;
        /// <summary>
        /// 메시지기능(Contents)
        /// </summary>
        private const byte CAT_Contents = 0x05;
        /// <summary>
        /// 메시지기능(System)
        /// </summary>
        private const byte CAT_System = 0x08;
        /// <summary>
        /// 메시지기능(Server)
        /// </summary>
        private const byte CAT_Server = 0x09;
        /// <summary>
        /// 상세기능(Parking MGR)
        /// </summary>
        private const byte ITEM_PMGR = 0x56;
        /// <summary>
        /// 상세기능(Packet Error)
        /// </summary>
        private const byte ITEM_PError = 0x87;
        /// <summary>
        /// 상세기능(Connection State)
        /// </summary>
        private const byte ITEM_ConStt = 0x91;
        /// <summary>
        /// 상세기능(Logon)
        /// </summary>
        private const byte ITEM_Logon = 0x93;
        private const byte CMD_QReq = 0x01;
        private const byte CMD_QRes = 0x02;
        private const byte CMD_CReq = 0x03;
        private const byte CMD_CRes = 0x04;
        private const byte CMD_SttEvntReq = 0x05;
        private const byte CMD_SttEvntRes = 0x06;

        private string tcpServerIp = "172.20.200.200";
        private string tcpPort = "8030";
        private string id = "parkmgr";
        private string pw = "parkmgr0";

        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => throw new NotImplementedException();

        public string reqPid { get; set; }
        private INetwork MyTcpNetwork { get; set; }


        public event IAdapter.ShowBallonTip ShowTip;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            id = SysConfig.Instance.HC_Id;
            pw = SysConfig.Instance.HC_Pw;
            tcpServerIp = SysConfig.Instance.HT_IP;
            tcpPort = SysConfig.Instance.HT_Port;

            MyTcpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpClient, tcpServerIp, tcpPort);

            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            throw new NotImplementedException();
        }

        public bool StartAdapter()
        {
            throw new NotImplementedException();
        }

        public bool StopAdapter()
        {
            throw new NotImplementedException();
        }

        public void TestReceive(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        private byte GetCheckSum(byte[] data)
        {
            byte bResult = 0x00;
            for (int i = 0; i < data.Length; i++)
            {
                bResult += data[i];
            }

            bResult ^= 0xFF;
            bResult += 1;

            return bResult;
        }
    }
}
