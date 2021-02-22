using NexpaAdapterStandardLib;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace NpmAdapter.Adapter
{
    class IControlsAdpater : IAdapter
    {
        private bool isRun = false;
        
        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => isRun;

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(IPayload payload)
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

        /// <summary>
        /// 입출차 통보
        /// </summary>
        /// <param name="io">입차:in, 출차:out</param>
        /// <param name="dong"></param>
        /// <param name="ho"></param>
        /// <param name="car_num"></param>
        /// <returns></returns>
        private byte[] RequestInOutCar(string io, string dong, string ho, string car_num)
        {
            XmlDocument xDoc = GetDeclareDocument();

            //0-1 : destination
            XmlElement xDestination = xDoc.CreateElement("destination");
            xDestination.SetAttribute("homedev", "homedev");
            xDestination.SetAttribute("id_high", dong);
            xDestination.SetAttribute("id_low", ho);

            //0-2 : move info
            XmlElement xMoveInfo = xDoc.CreateElement("move_info");
            xMoveInfo.InnerText = $"{io}";

            //0-3 : params
            XmlElement xParams = xDoc.CreateElement("params");
            xParams.SetAttribute("car_num", car_num);
            xParams.SetAttribute("loc_info", "정문");
            xParams.SetAttribute("message", "null");

            //1 : service
            XmlElement xService = xDoc.CreateElement("service");
            xService.SetAttribute("type", "request");
            xService.SetAttribute("name", "car_move_info");
            xService.AppendChild(xDestination); //0-1 Append
            xService.AppendChild(xMoveInfo); //0-2 Append
            xService.AppendChild(xParams); //0-3 Append

            //2 : imp
            XmlElement xImap = xDoc.CreateElement("imap");
            xImap.SetAttribute("ver", "1.0");
            xImap.SetAttribute("address", $"{Helper.GetLocalIP()}");
            xImap.SetAttribute("sender", $"주차관제 미들웨어");
            xImap.AppendChild(xService); //1 Append

            //Last
            xDoc.AppendChild(xImap); //2 Append


            byte[] dataBytes = null;
            using (StringWriter stringWriter = new StringWriter())
            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter))
            {
                xDoc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"전송메시지 : {stringWriter.GetStringBuilder().ToString()}", LogAdpType.HomeNet);
                dataBytes = stringWriter.GetStringBuilder().ToString().ToByte(SysConfig.Instance.HomeNet_Encoding);
            }

            return dataBytes;
        }

        private XmlDocument GetDeclareDocument()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration xmlDecl;
            xmlDecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmlDecl.Encoding = "utf-8";

            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmlDecl, root);

            return doc;
        }
    }
}
