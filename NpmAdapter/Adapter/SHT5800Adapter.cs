using HttpServer;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace NpmAdapter.Adapter
{
    class SHT5800Adapter : IAdapter
    {
        private bool hasReceiveEvent = false;
        private BackgroundWorker worker = null;
        private List<byte> nexpaJson;
        private Dictionary<byte[], int> transmittedData;
        private Queue<byte[]> qSendMessage;
        private bool isRun = false;
        /// <summary>
        /// 최초 1Byte
        /// </summary>
        private const byte FirstPacket = 0xBB;
        private const byte NotRetryPacket = 0x0A;
        private const byte RetryPacket = 0x0B;

        private enum DataPacket : byte
        {
            InCar = 0x51,
            OutCar = 0x5C,
            InitAck = 0x71,
            AliveACK = 0x33,
            InCarFailAck = 0x52,
            OutCarFailAck = 0x5D
        }

        private enum ErrorPacket : byte
        {
            /// <summary>
            /// 세대 통신 불가
            /// </summary>
            NetworkError = 0x01, 
            /// <summary>
            /// 세대 번호 없음
            /// </summary>
            NotFoundDongHo = 0x02
        }

        private string _strBaudRate, _strPortName, _strParity = "";
        private string Section { get => "HomeNet_SerialConfig"; }
        public INetwork MyNetwork { get; set; }
        public IAdapter TargetAdapter { get; set; }
        public bool IsRuning { get=>isRun; }

        public bool StartAdapter()
        {
            Log.WriteLog(LogType.Info, "SHT5800Adapter | StartAdapter", $"SHT5800 485 Serial 통신 시작", LogAdpType.HomeNet);
            if (!hasReceiveEvent)
            {
                MyNetwork.ReceiveFromPeer += MyNetwork_ReceiveFromPeer;
                hasReceiveEvent = true;
                isRun = true;
            }
            return MyNetwork.Run();
        }

        public bool StopAdapter()
        {
            Log.WriteLog(LogType.Info, "SHT5800Adapter | StopAdapter", $"SHT5800 485 Serial 통신 중지", LogAdpType.HomeNet);
            bool bResult;
            try
            {
                if (hasReceiveEvent)
                {
                    MyNetwork.ReceiveFromPeer -= MyNetwork_ReceiveFromPeer;
                    hasReceiveEvent = false;
                    isRun = false;
                    qSendMessage.Clear(); 
                    transmittedData.Clear();
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                bResult = MyNetwork.Down();
            }
            
            return bResult;
        }

        public bool Initialize()
        {
            try
            {
                //config.ini를 읽어 변수를 셋팅하자
                Log.WriteLog(LogType.Info, $"SHT5800Adapter | Initialize", $"Read Config : {Section}", LogAdpType.HomeNet);
                _strBaudRate = SysConfig.Instance.HS_BaudRate;
                _strPortName = SysConfig.Instance.HS_PortName; 
                _strParity = SysConfig.Instance.HS_Parity;
                Log.WriteLog(LogType.Info, $"SHT5800Adapter | Initialize", $"BaudRate:{_strBaudRate}, PortName:{_strPortName}, Parity:{_strParity}", LogAdpType.HomeNet);

                //Param 순서 : BaudRateString, PortNameString, ParityString
                MyNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.SerialPort, _strBaudRate, _strPortName, _strParity);

                //속도땜에 Nexpa쪽 Send는 BackgroundWorker를 쓴다.
                worker = new BackgroundWorker();
                worker.WorkerReportsProgress = true;
                worker.WorkerSupportsCancellation = true;
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);

                nexpaJson = new List<byte>();
                transmittedData = new Dictionary<byte[], int>();
                qSendMessage = new Queue<byte[]>();
                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"SHT5800Adapter | Initialize", $"{ex.Message}");
                return false;
            }
        }

        

        /// <summary>
        /// Nexpa에서 받은 데이터를 처리하여 5800으로 전송한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public unsafe void SendMessage(byte[] buffer, long offset, long size)
        {
            List<byte> lstBytes = new List<byte>();
            Log.WriteLog(LogType.Info, "SHT5800Adapter | SendMessage", $"Nexpa Adapter ---> SHT_5800 Adapter =======", LogAdpType.HomeNet);
            try
            {
                List<byte> tempBytes = new List<byte>();
                //Nextpa Server로부터 받은 자료를 Parsing 하여 SHT_5800 Format으로 변경 후 MyNetwork에 전달한다.
                string strJson = Encoding.Default.GetString(buffer, (int)offset, (int)size);
                var jobj = JObject.Parse(strJson);
                string cmd = jobj["command"].ToString();

                CmdType cmdType = CmdType.none;
                cmdType = (CmdType)Enum.Parse(typeof(CmdType), cmd);

                Log.WriteLog(LogType.Info, $"SHT5800Adapter | SendMessage", $"Command : {cmdType}", LogAdpType.HomeNet);

                //Data 및 SubData 생성
                switch (cmdType)
                {
                    case CmdType.status_ack:
                        //MyNetwork?.SendToPeer(MakeStatusAck(), 1, 4);
                        return;
                    case CmdType.alert_incar:
                    case CmdType.alert_outcar:
                        //1. Try Packet 셋팅
                        lstBytes.Add(NotRetryPacket);

                        if (cmdType == CmdType.alert_incar) tempBytes.Add((byte)DataPacket.InCar);
                        else tempBytes.Add((byte)DataPacket.OutCar);

                        JArray array = jobj["data"] as JArray;
                        
                        foreach (var json in array)
                        {
                            InOutCarPayload payload = new InOutCarPayload();
                            payload.Deserialize(json as JObject);

                            //동호정보 + 차량번호4자리 + 차량입차시간
                            //2-2. 동 2byte = 그냥 decimal hexa다..
                            //2020-09-11 : 하.... 삼성 이놈들이... 100단위 처리가 아니라 1단위 처리만한다..
                            //int로 변환 후 100으로 나눈 나머지를 넣어주도록 하자....
                            int tempDong = 0;
                            if(int.TryParse(payload.dong, out tempDong))
                            {
                                tempDong = tempDong % 100; //ex : 101동이면 1동으로 표시됨.
                                tempBytes.AddRange(tempDong.ToString().FourStringTo2Byte());
                            }
                            else
                            {
                                tempBytes.AddRange(payload.dong.FourStringTo2Byte());
                            }

                            Log.WriteLog(LogType.Info, $"SHT5800Adapter | SendMessage", $"동변환 AS-IS : {payload.dong}, TO-BE : {tempDong.ToString()}", LogAdpType.HomeNet);

                            //2-3. 호는 2byte Hexa 변환...
                            tempBytes.AddRange(payload.ho.FourStringTo2Byte());

                            //2-4. 차량번호 뒤에 4자리만 송신...
                            string number = payload.car_number.Substring(payload.car_number.Length - 4);
                            tempBytes.AddRange(number.FourStringTo2Byte());

                            //2-5. 입차시간 월,일,시,분 1byte씩... 월일을 묶고 시분을 묶으면 될듯..
                            //yyyy MMdd HHmm ss 형태로 날라올것이다....
                            string monthday = payload.date_time.Substring(4, 4);
                            tempBytes.AddRange(monthday.FourStringTo2Byte());
                            string hourmin = payload.date_time.Substring(8, 4);
                            tempBytes.AddRange(hourmin.FourStringTo2Byte());
                        }
                        break;
                } //switch (cmdType)

                //2. Header Byte BB 셋팅
                lstBytes.Add(FirstPacket); //BB

                //3. Frame 길이 Heigh Nibble 값 셋팅
                byte length = 0xA0;
                length += (byte)3;
                length += (byte)tempBytes.Count;
                lstBytes.Add(length);

                //4. 2. Data 및 SubData 셋팅
                lstBytes.AddRange(tempBytes);

                //5. 맨마지막에 CheckSum 1byte 추가
                lstBytes.Add(lstBytes.CalCheckSum(1));
                Log.WriteLog(LogType.Info, $"SHT5800Adapter | SendMessage", $"SHT5800 Make Data : {lstBytes.ToArray().ToHexString()}", LogAdpType.HomeNet);
                //6. 큐에 저장.. Alive 신호가 들어오면 q를 뺀다.
                //MyNetwork?.SendToPeer(lstBytes.ToArray(), 1, lstBytes.Count - 1);
                if (isRun)
                {
                    //같은 Key가 아니면 Dictionary에 Add 한다.
                    if (!transmittedData.ContainsKey(lstBytes.ToArray()))
                    {
                        transmittedData.Add(lstBytes.ToArray(), 0);
                    }
                }
                Log.WriteLog(LogType.Info, "SHT5800Adapter | SendMessage", $"Nexpa Adapter ---> SHT_5800 Adapter =======", LogAdpType.HomeNet);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"SHT5800Adapter | SendMessage", $"Exception Error Message : {ex.Message}");
            }
            finally
            {
            }
        }

        public void TestReceive(byte[] buffer)
        {
            MyNetwork_ReceiveFromPeer(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 5800에서 받은 데이터를 처리한다.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        private void MyNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size , RequestEventArgs pEvent = null)
        {
            try
            {
                if(buffer[0] == 0xE8 && buffer[1] == 0xE8)
                {
                    //Log.WriteLog(LogType.Info, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"SHT-5800으로부터 데이터 수신 ==========", LogAdpType.HomeNet);
                    OperateMessage(buffer);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"Command : {ex.Message}");
            }
        }

        private void OperateMessage(byte[] buffer)
        {
            nexpaJson.Clear();

            //Alive 신호는 너무 많이 찍는다....
            if(buffer[3] != (byte)DataPacket.AliveACK)
            {
                Log.WriteLog(LogType.Info, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"ReceiveMessge :  {buffer.ToHexString()}", LogAdpType.HomeNet);
            }
            
            if (buffer.CalCheckSum(0, buffer.Length - 1) != buffer[buffer.Length - 1])
            {
                Log.WriteLog(LogType.Error, "SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"CheckSum 오류");
                Log.WriteLog(LogType.Error, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"계산한 CheckSum : {buffer.CalCheckSum(0, buffer.Length - 1)} : 데이터 CheckSum : {buffer[buffer.Length - 1]}", LogAdpType.HomeNet);
                return;
            }
            if (buffer[0] != 0xE8 && buffer[1] != 0xE8)
            {
                Log.WriteLog(LogType.Error, "SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"Header 2 Byte 오류");
                Log.WriteLog(LogType.Error, "SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"{buffer[0]} {buffer[1]}");
                return;
            }
            var framelength = buffer[2] ^ 0xA0;
            if (buffer.Length != framelength)
            {
                Log.WriteLog(LogType.Error, "SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"Frame 길이 오류");
                Log.WriteLog(LogType.Error, "SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"{buffer.Length} : {framelength}");
                return;
            }

            byte[] bData;
            switch (buffer[3])
            {
                case (byte)DataPacket.InitAck: //통신초기화
                    bData = MakeInitAck();
                    Log.WriteLog(LogType.Info, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"InitAck", LogAdpType.HomeNet);
                    MyNetwork.SendToPeer(bData, 1, 4);
                    break;
                case (byte)DataPacket.AliveACK: //Polling Status Check 
                    //Status Check가 들어오면 Queue 가 있는지 없는지 보고 있으면 Q를 보내고 없으면 ACk를 보낸다.
                    //주석처리... 너무 많이 찍는다.
                    //Log.WriteLog(LogType.Info, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"AliveACK", LogAdpType.HomeNet);

                    //전송할 데이터가 있으면 q에 넣어주자.
                    if (transmittedData.Count > 0)
                    {
                        List<byte[]> overKeys = (from items in transmittedData
                                                 where items.Value > 2
                                                 select items.Key).ToList();

                        for (int i = 0; i < overKeys.Count; i++)
                        {
                            transmittedData.Remove(overKeys[i]);
                        }

                        foreach (KeyValuePair<byte[], int> item in transmittedData)
                        {
                            qSendMessage.Enqueue(item.Key);
                        }
                    }

                    //전송할 Data가 있다.
                    if (qSendMessage.Count > 0)
                    {
                        byte[] bytes = qSendMessage.Dequeue();
                        MyNetwork.SendToPeer(bytes, 1, bytes.Length - 1);
                        //전송 한 Data의 Count 값을 늘려준다.
                        if (transmittedData.ContainsKey(bytes)) transmittedData[bytes] += 1;
                    }
                    else //전송할 데이터가 없다.
                    {
                        MyNetwork?.SendToPeer(MakeStatusAck(), 1, 4);
                    }
                    break;
                case (byte)DataPacket.InCar: //차량입차 ACK
                case (byte)DataPacket.OutCar: //차량출차 ACK
                    bData = MakeStatusAck();
                    Log.WriteLog(LogType.Info, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"In/OutCar", LogAdpType.HomeNet);
                    MyNetwork.SendToPeer(bData, 1, 4);

                    //응답이 왔으면 전송할 Data에서 찾아 지워주자...
                    //transmittedData
                    byte[] checkBytes = buffer[3..(buffer.Length - 2)]; //동호...
                    foreach (var key in transmittedData.Keys)
                    {
                        if (StructuralComparisons.StructuralEqualityComparer.Equals(checkBytes, key[3..7]))
                        {
                            transmittedData.Remove(key);
                            break;
                        }
                    }
                    break;
                case (byte)DataPacket.InCarFailAck: //차량입차 통보 실패
                case (byte)DataPacket.OutCarFailAck: //차량출차 통보 실패
                    //byte[] packet1 = MakeFailAck(buffer.Range(3, buffer.Length - 3));
                    byte[] packet1 = MakeFailAck(buffer[3..(buffer.Length - 2)]);
                    
                    Log.WriteLog(LogType.Info, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"FailAck_입/출차 통보 실패", LogAdpType.HomeNet);
                    MyNetwork.SendToPeer(packet1, 1, packet1.Length - 1);
                    byte[] errorPacket1 = MakeTransError(buffer[8]);
                    nexpaJson = errorPacket1.ToList();
                    worker.RunWorkerAsync();
                    break;
            }

            //Log.WriteLog(LogType.Info, $"SHT5800Adapter | MyNetwork_ReceiveFromPeer", $"SHT-5800으로부터 데이터 수신완료 ======", LogAdpType.HomeNet);
        }

        // Worker Thread가 실제 하는 일
        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            TargetAdapter.SendMessage(nexpaJson.ToArray(), 0, nexpaJson.Count);
        }

        // 작업 완료 - UI Thread
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log.WriteLog(LogType.Info, $"SHT5800Adapter | worker_RunWorkerCompleted", $"Nexpa로 데이터 전송 완료", LogAdpType.HomeNet);
        }

        private byte[] MakeTransError(byte error)
        {
            string errorCode = "" ;
            string errorMesgae = "";

            if(error == (byte)ErrorPacket.NetworkError)
            {
                errorCode = "-1";
                errorMesgae = "세대 통신 불가";
            }
            else if (error == (byte)ErrorPacket.NotFoundDongHo)
            {
                errorCode = "-2";
                errorMesgae = "세대 번호 없음";
            }

            Payload<StatusPayload> payload = new Payload<StatusPayload>();
            payload.command = CmdType.trans_error.ToString();
            payload.data = new List<StatusPayload>
            {
                new StatusPayload
                {
                    code = errorCode,
                    message = errorMesgae
                }
            };

            return payload.Serialize();
        }
        
        private byte[] MakeStatusRequest()
        {
            Payload<StatusPayload> payload = new Payload<StatusPayload>();
            payload.command = CmdType.status_check.ToString();
            payload.data = new List<StatusPayload>();
            StatusPayload subPayload = new StatusPayload();
            subPayload.code = "";
            subPayload.message = "";
            payload.data.Add(subPayload);
            return payload.Serialize();
        }

        private byte[] MakeInitAck()
        {
            List<byte> result = new List<byte>();
            result.Add(NotRetryPacket);
            result.Add(FirstPacket);
            result.Add(0xA4);
            result.Add((byte)DataPacket.InitAck);
            result.Add(result.CalCheckSum(1));
            return result.ToArray();
        }

        private byte[] MakeStatusAck()
        {
            List<byte> result = new List<byte>();
            result.Add(NotRetryPacket);
            result.Add(FirstPacket);
            result.Add(0xA4);
            result.Add((byte)DataPacket.AliveACK);
            result.Add(result.CalCheckSum(1));
            return result.ToArray();
        }

        private byte[] MakeFailAck(byte[] data)
        {
            List<byte> result = new List<byte>();
            result.Add(NotRetryPacket);
            result.Add(FirstPacket);
            byte length = 0xA0;
            length += (byte)3;
            length += (byte)data.Length;
            result.Add(length);
            result.AddRange(data);
            result.Add(result.CalCheckSum(1));
            return result.ToArray();
        }

        public void Dispose()
        {
            if (worker != null && worker.IsBusy) worker.CancelAsync();
        }
    }
}
