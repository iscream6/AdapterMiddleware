using Newtonsoft.Json.Linq;
using NpmAdapter.Model;
using NpmAdapter.Payload;
using NpmCommon;
using NpmNetwork;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NpmAdapter.Adapter
{
    class ULSNServerAdapter : IAdapter
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam);

        private bool isRun;

        //==== Alive Check ====
        private NpmThread aliveCheckThread;
        //==== Alive Check ====

        //==== Process Thread ====
        private NpmThread processThread;
        //==== Process Thread ====

        private bool isProcessing = false;
        private GovInterfaceModel gov;
        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private object lockObj = new object();
        private RequestPayload<AlertInOutCarPayload> currentPayload = new RequestPayload<AlertInOutCarPayload>();
        private Queue<RequestPayload<AlertInOutCarPayload>> quePayload = new Queue<RequestPayload<AlertInOutCarPayload>>();

        public bool IsRuning => isRun;

        public string reqPid { get; set; }
        public IAdapter TargetAdapter { get; set; }
        private INetwork TcpJavaServer { get; set; }

        public void Dispose()
        {
            if (isRun) TcpJavaServer.Down();
//#if (!DEBUG)
            aliveCheckThread.Dispose();
            JClientKill();
//#endif
            processThread.Dispose();
        }

        public bool Initialize()
        {
            //Config Version Check~!
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "ULSNServerAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.Nexpa);
                return false;
            }

            try
            {
                TcpJavaServer = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpServer, SysConfig.Instance.HT_MyPort);
//#if (!DEBUG)
                JClientKill();
                aliveCheckThread = new NpmThread("alive check", TimeSpan.FromSeconds(10));
                aliveCheckThread.ThreadAction = AliveCheck;
                //#endif
                processThread = new NpmThread("java process", TimeSpan.FromSeconds(1));
                processThread.ThreadAction = ProcessAction;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"ULSNServerAdapter | Initialize", $"{ex.Message}", LogAdpType.Nexpa);
                return false;
            }

            return true;
        }
        
        public bool StartAdapter()
        {
            TcpJavaServer.ReceiveFromPeer += TcpServer_ReceiveFromPeer;
            isRun = TcpJavaServer.Run();

            //Alive Check Thread 시작
            //#if (!DEBUG)
            aliveCheckThread.Start();
            //#endif
            processThread.Start();

            return isRun;
        }

        
        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            lock (lockObj)
            {
                //이리로 alert_incar가 들어올거임....
                receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                var jobj = JObject.Parse(receiveMessageBuffer.ToString());
                Thread.Sleep(10);
                receiveMessageBuffer.Clear();

                //Log.WriteLog(LogType.Info, $"ULSNServerAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
                RequestPayload<AlertInOutCarPayload> alertPayload = new RequestPayload<AlertInOutCarPayload>();
                alertPayload.Deserialize(jobj);
                //미인식 차량은 거르고 일반차량만 적용...
                if (!alertPayload.data.car_number.Contains("0000") && alertPayload.data.kind == "n")
                {
                    quePayload.Enqueue(alertPayload);
                }
            }
        }

        /// <summary>
        /// DiscountProcess가 돌고 있는지 여부
        /// </summary>
        private bool isProcessRun = false;
        private List<Car> lstCar = new List<Car>();

        private void ProcessAction()
        {
            try
            {
                //여기서 큐를 실행하자..
                if (isProcessRun == false && quePayload.Count > 0)
                {
                    currentPayload = quePayload.Dequeue();
                    Log.WriteLog(LogType.Info, $"ULSNServerAdapter | ProcessAction", $"Discount 처리할 Payload : {currentPayload.ToJson()}", LogAdpType.HomeNet);

                    Car car = new Car();
                    car.carNo = currentPayload.data.car_number;

                    if (!lstCar.Contains(car))
                    {
                        car.tkNo = currentPayload.data.reg_no;
                        car.DateTime = currentPayload.data.date_time;
                        lstCar.Add(car);
                    }

                    DiscountProcess(currentPayload);
                }
            }
            catch (Exception)
            {

            }
        }

        private enum DiscountSuccess
        {
            yes,
            no,
            pass
        }

        private DiscountSuccess GetVaildCar(string cmd, string carno, string tkno)
        {
            foreach (var item in lstCar)
            {
                if(item.tkNo == tkno && item.carNo == carno)
                {
                    if(cmd == "disabilityCar")
                    {
                        if (!item.disability)
                        {
                            return DiscountSuccess.no;
                        }
                        else return DiscountSuccess.yes;
                    }
                    else if(cmd == "nationalCar")
                    {
                        if (!item.national)
                        {
                            return DiscountSuccess.no;
                        }
                        else return DiscountSuccess.yes;
                    }
                    else if (cmd == "echoCar")
                    {
                        if (!item.eco)
                        {
                            return DiscountSuccess.no;
                        }
                        else return DiscountSuccess.yes;
                    }
                    else if (cmd == "smallCar")
                    {
                        if (!item.small)
                        {
                            return DiscountSuccess.no;
                        }
                        else return DiscountSuccess.yes;
                    }
                }
            }

            return DiscountSuccess.pass;
        }

        private async void DiscountProcess(RequestPayload<AlertInOutCarPayload> requestPayload)
        {
            isProcessRun = true;
            try
            {
                var carno = requestPayload.data.car_number;
                var tkno = requestPayload.data.reg_no;

                DiscountSuccess succType = GetVaildCar("echoCar", carno, tkno);
                if(succType == DiscountSuccess.no)
                {
                    //친환경
                    isProcessing = true;
                    Task test = RequestEcoCarTask(carno);
                    Task waitTask = WaitTask();
                    await test;
                    await waitTask;

                    if (!isProcessing) //처리완료
                    {
                        Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"친환경 처리완료", LogAdpType.HomeNet);
                    }
                    else //처리 실패
                    {
                        Log.WriteLog(LogType.Error, $"ULSNServerAdapter | DiscountProcess", $"친환경 처리 타임오버 : {carno}", LogAdpType.HomeNet);
                    }
                }

                succType = GetVaildCar("nationalCar", carno, tkno);
                if (succType == DiscountSuccess.no)
                {
                    //국가유공자 
                    isProcessing = true;
                    Task test2 = RequestNationalCarTask(carno);
                    Task waitTask = WaitTask();
                    await test2;
                    await waitTask;

                    if (!isProcessing) //처리완료
                    {
                        Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"국가유공자 처리완료", LogAdpType.HomeNet);
                    }
                    else //처리 실패
                    {
                        Log.WriteLog(LogType.Error, $"ULSNServerAdapter | DiscountProcess", $"국가유공자 처리 타임오버 : {carno}", LogAdpType.HomeNet);
                    }
                }

                succType = GetVaildCar("disabilityCar", carno, tkno);
                if (succType == DiscountSuccess.no)
                {
                    //장애인
                    isProcessing = true;
                    Task test3 = RequestDisabilityCarTask(carno);
                    Task waitTask = WaitTask();
                    await test3;
                    await waitTask;

                    if (!isProcessing) //처리완료
                    {
                        Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"장애인 처리완료", LogAdpType.HomeNet);
                    }
                    else //처리 실패
                    {
                        Log.WriteLog(LogType.Error, $"ULSNServerAdapter | DiscountProcess", $"장애인 처리 타임오버 : {carno}", LogAdpType.HomeNet);
                    }
                }

                succType = GetVaildCar("smallCar", carno, tkno);
                if (succType == DiscountSuccess.no)
                {
                    //경차
                    isProcessing = true;
                    Task test4 = RequestSamlCarTask(carno);
                    Task waitTask = WaitTask();
                    await test4;
                    await waitTask;

                    if (!isProcessing) //처리완료
                    {
                        Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"경차 처리완료", LogAdpType.HomeNet);
                    }
                    else //처리 실패
                    {
                        Log.WriteLog(LogType.Error, $"ULSNServerAdapter | DiscountProcess", $"경차 처리 타임오버 : {carno}", LogAdpType.HomeNet);
                    }
                }

                foreach (var car in lstCar)
                {
                    if (car.carNo == carno && car.tkNo == tkno)
                    {
                        try
                        {
                            if (car.eco && car.disability && car.national && car.small)
                            {
                                lstCar.Remove(car);
                            }
                            else
                            {
                                quePayload.Enqueue(requestPayload);
                            }
                        }
                        catch (Exception)
                        {
                            //Do Nothing...
                        }
                        
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "ULSNServerAdapter | DiscountProcess", $"{ex.Message}");
            }

            isProcessRun = false;

        }

        private void TcpServer_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null, System.Net.EndPoint ep = null)
        {
            string receiveMessage = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Log.WriteLog(LogType.Info, $"ULSNServerAdapter | TcpServer_ReceiveFromPeer", $"{receiveMessage}", LogAdpType.HomeNet);

            if (receiveMessage.Contains("Hello"))
            {
                byte[] data = Encoding.UTF8.GetBytes($"init#{SysConfig.Instance.HW_Domain}#{SysConfig.Instance.AuthToken}");
                TcpJavaServer.SendToPeer(data, 0, data.Length);
                return;
            }

            try
            {
                string xmlData = string.Empty;
                string command = string.Empty;
                string carNo = string.Empty;
                string tkNo = string.Empty;
                string dateTime = string.Empty;

                JObject obj = JObject.Parse(receiveMessage);
                var jResult = obj["result"];

                if (Helper.NVL(jResult["status"]) == "200")
                {
                    command = Helper.NVL(obj["command"]);
                    carNo = Helper.NVL(obj["carno"]);
                    xmlData = Helper.NVL(obj["data"]).Replace("\\n", "").Replace(" ", ""); //\n과 공백을 제거

                    List<Car> tempRemove = new List<Car>();

                    foreach (var car in lstCar)
                    {
                        if (car.carNo == carNo)
                        {
                            tkNo = car.tkNo;
                            dateTime = car.DateTime;

                            switch (command)
                            {
                                case "disability":
                                    car.disability = true;
                                    break;
                                case "national":
                                    car.national = true;
                                    break;
                                case "eco":
                                    car.eco = true;
                                    break;
                                default: //small
                                    car.small = true;
                                    break;
                            }

                            if (car.eco && car.disability && car.national && car.small) tempRemove.Add(car);

                            break;
                        }
                    }

                    foreach (var item in tempRemove)
                    {
                        lstCar.Remove(item);
                    }

                    if (tkNo == string.Empty)
                    {
                        tkNo = currentPayload.data.reg_no;
                        dateTime = currentPayload.data.date_time;
                    }

                    if (gov == null) gov = new GovInterfaceModel();
                    Dictionary<string, object> param = new Dictionary<string, object>();
                    param.Add("TkNo", tkNo);
                    param.Add("CarNo", carNo);
                    param.Add("RequestDateTime", dateTime);
                    param.Add("InterfaceCode", command); //command
                    param.Add("InterfaceData", xmlData);

                    if (gov.Save(param))
                    {
                        //DB저장성공
                        Log.WriteLog(LogType.Info, $"ULSNServerAdapter | TcpServer_ReceiveFromPeer", $"DB저장성공", LogAdpType.HomeNet);
                    }
                    else
                    {
                        //DB저장실패
                        Log.WriteLog(LogType.Error, $"ULSNServerAdapter | TcpServer_ReceiveFromPeer", $"DB저장실패", LogAdpType.HomeNet);
                    }

                    isProcessing = false;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "ULSNServerAdapter | TcpServer_ReceiveFromPeer", $"{ex.Message}");
                isProcessing = true;
            }
        }

        public bool StopAdapter()
        {
            isRun = !TcpJavaServer.Down();
            //Alive Check Thread pause
            //#if (!DEBUG)
            aliveCheckThread.Stop();
            //#endif
            processThread.Stop();

            return !isRun;
        }

        /// <summary>
        /// 친환경차
        /// </summary>
        /// <param name="carno"></param>
        /// <returns></returns>
        private async Task RequestEcoCarTask(string carno)
        {
            await Task.Run( () => 
            {
                JObject json = new JObject();
                json["infoType"] = "echoCar";
                json["carNumber"] = carno;
                Log.WriteLog(LogType.Info, $"ULSNServerAdapter | RequestEcoCarTask", $"친환경차 전송 메시지 : eco#{carno}#{json.ToString()}", LogAdpType.HomeNet);
                byte[] data = Encoding.UTF8.GetBytes($"eco#{carno}#{json.ToString()}");
                TcpJavaServer.SendToPeer(data, 0, data.Length);
            });
        }

        /// <summary>
        /// 국가유공자
        /// </summary>
        /// <param name="carno"></param>
        /// <returns></returns>
        private async Task RequestNationalCarTask(string carno)
        {
            await Task.Run(() =>
            {
                JObject json = new JObject();
                json["infoType"] = "nationalCar";
                json["carNo"] = carno;
                Log.WriteLog(LogType.Info, $"ULSNServerAdapter | RequestEcoCarTask", $"국가유공자 전송 메시지 : national#{carno}#{json.ToString()}", LogAdpType.HomeNet);
                byte[] data = Encoding.UTF8.GetBytes($"national#{carno}#{json.ToString()}");
                TcpJavaServer.SendToPeer(data, 0, data.Length);
            });
        }

        /// <summary>
        /// 장애인
        /// </summary>
        /// <param name="carno"></param>
        /// <returns></returns>
        private async Task RequestDisabilityCarTask(string carno)
        {
            await Task.Run(() =>
            {
                JObject json = new JObject();
                json["infoType"] = "disabilityCar";
                json["CAR_NO"] = carno;
                Log.WriteLog(LogType.Info, $"ULSNServerAdapter | RequestEcoCarTask", $"장애인 전송 메시지 : disability#{carno}#{json.ToString()}", LogAdpType.HomeNet);
                byte[] data = Encoding.UTF8.GetBytes($"disability#{carno}#{json.ToString()}");
                TcpJavaServer.SendToPeer(data, 0, data.Length);
            });
        }

        /// <summary>
        /// 경차
        /// </summary>
        /// <param name="carno"></param>
        /// <returns></returns>
        private async Task RequestSamlCarTask(string carno)
        {
            await Task.Run(() =>
            {
                JObject json = new JObject();
                json["infoType"] = "smallCar";
                json["vhrNo"] = carno;
                Log.WriteLog(LogType.Info, $"ULSNServerAdapter | RequestEcoCarTask", $"경차 전송 메시지 : small#{carno}#{json.ToString()}", LogAdpType.HomeNet);
                byte[] data = Encoding.UTF8.GetBytes($"small#{carno}#{json.ToString()}");
                TcpJavaServer.SendToPeer(data, 0, data.Length);
            });
        }

        private async Task RequestDecriptDataTask(string pe, string pm, string encData)
        {
            await Task.Run(() =>
            {
                string strData = $"{encData}&{pe}&{pm}";
                byte[] data = Encoding.UTF8.GetBytes($"dec#{strData}");
                TcpJavaServer.SendToPeer(data, 0, data.Length);
            });
        }

        private async Task WaitTask()
        {
            await Task.Run(() =>
            {
                int iSec = 30 * 100; //10초
                while (iSec > 0 && isProcessing)
                {
                    Thread.Sleep(10); //0.01초씩..쉰다...
                    iSec -= 1;
                }
            });
        }

        int number = 11;
        int tknum = 12341234;

        public event IAdapter.ShowBallonTip ShowTip;

        public void TestReceive(byte[] buffer)
        {

            lock (lockObj)
            {
                string json = "{\"command\": \"alert_incar\",\"data\": {\"dong\" : \"123\"," +
                            "\"ho\" : \"456\"," +
                            $"\"car_number\" : \"10버1119\"," +
                            $"\"date_time\" : \"{DateTime.Now.ToString("yyyyMMddHHmmss")}\"," +
                            "\"kind\" : \"n\"," +
                            "\"lprid\" : \"Lpr 식별 번호\"," +
                            "\"car_image\" : \"차량 이미지 경로\"," +
                            $"\"reg_no\" : \"{tknum++}\"," +
                            "\"visit_in_date_time\" : \"yyyyMMddHHmmss\"," + //방문시작일시, kind가 v 일 경우 외 빈값
                            "\"visit_out_date_time\" : \"yyyyMMddHHmmss\"" + //방문종료일시, kind가 v 일 경우 외 빈값
                            "}" +
                            "}";
                JObject jObject = JObject.Parse(json);
                RequestPayload<AlertInOutCarPayload> alertPayload = new RequestPayload<AlertInOutCarPayload>();
                alertPayload.Deserialize(jObject);
                quePayload.Enqueue(alertPayload);
            }
        }

        /// <summary>
        /// AliveCheck 를 날린다.
        /// </summary>
        private void AliveCheck()
        {
            try
            {
                Process[] processList = Process.GetProcessesByName("secureclient");
                if (processList == null || processList.Length == 0)
                {
                    JClientKill();
                    ProcessStartInfo startInfo = new ProcessStartInfo($"{System.IO.Directory.GetCurrentDirectory()}\\secureclient.exe");
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    Process.Start(startInfo);
                }
            }
            catch (Exception)
            {

            }
        }

        private void JClientKill()
        {
            Process[] processList = Process.GetProcessesByName("secureclient");
            if (processList != null && processList.Length > 0)
            {
                processList[0].Kill();
            }
            processList = Process.GetProcessesByName("java");
            if (processList != null && processList.Length > 0)
            {
                processList[0].Kill();
            }
        }
    }

    class Fail
    {
        public string cmd;
        public RequestPayload<AlertInOutCarPayload> payload;
    }

    class Car : IComparable<Car>, IEquatable<Car>
    {
        public string tkNo;
        public string DateTime;
        public string carNo;
        public bool eco;
        public bool small;
        public bool disability;
        public bool national;

        public Car()
        {
            eco = false;
            small = false;
            disability = false;
            national = false;
        }

        public int CompareTo(Car other)
        {
            if (this.carNo == other.carNo) return 0;
            else return -1;
        }

        public bool Equals(Car other)
        {
            return this.carNo == other.carNo;
        }
    }
}
