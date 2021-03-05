using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using System.Xml;
using NpmAdapter.Model;
using System.Threading.Tasks;
using NpmAdapter.Payload;
using System.Runtime.CompilerServices;

namespace NpmAdapter.Adapter
{
    class ULSNServerAdapter : IAdapter
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam);
        
        //==== Alive Check ====
        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private delegate void SafeCallDelegate();
        private bool isRun;
        //==== Alive Check ====

        //==== Process Thread ====
        private Thread processThread;
        private TimeSpan waitForProcess;
        private ManualResetEventSlim shutdownProcessEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseProcessEvent = new ManualResetEvent(false);
        private delegate void ProcessSafeCallDelegate();
        //==== Process Thread ====

        private bool isProcessing = false;
        private GovInterfaceModel gov;
        private StringBuilder receiveMessageBuffer = new StringBuilder();
        private object lockObj = new object();
        private RequestPayload<AlertInOutCarPayload> currentPayload = new RequestPayload<AlertInOutCarPayload>();
        private Queue<RequestPayload<AlertInOutCarPayload>> quePayload = new Queue<RequestPayload<AlertInOutCarPayload>>();

        public bool IsRuning => isRun;
        public IAdapter TargetAdapter { get; set; }
        private INetwork TcpJavaServer { get; set; }

        public void Dispose()
        {
            if (isRun) TcpJavaServer.Down();
//#if (!DEBUG)
            _pauseEvent.Set();
            shutdownEvent.Set();
            JClientKill();
//#endif
            _pauseProcessEvent.Set();
            shutdownProcessEvent.Set();
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
                aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
                aliveCheckThread.Name = "alive check";
                waitForWork = TimeSpan.FromSeconds(10);
//#endif
                processThread = new Thread(new ThreadStart(ProcessAction));
                processThread.Name = "process";
                waitForProcess = TimeSpan.FromSeconds(1); //1초
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
            if (aliveCheckThread.IsAlive)
            {
                _pauseEvent.Set();
            }
            else
            {
                aliveCheckThread.Start();
                _pauseEvent.Set();
            }
//#endif

            if (processThread.IsAlive)
            {
                _pauseProcessEvent.Set();
            }
            else
            {
                processThread.Start();
                _pauseProcessEvent.Set();
            }

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

                Log.WriteLog(LogType.Info, $"ULSNServerAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
                RequestPayload<AlertInOutCarPayload> alertPayload = new RequestPayload<AlertInOutCarPayload>();
                alertPayload.Deserialize(jobj);
                //미인식 차량은 거른다.
                if (!alertPayload.data.car_number.Contains("0000"))
                {
                    quePayload.Enqueue(alertPayload);
                }
            }
        }

        /// <summary>
        /// DiscountProcess가 돌고 있는지 여부
        /// </summary>
        private bool isProcessRun = false;

        private void ProcessAction()
        {
            do
            {
                if (shutdownProcessEvent.IsSet) return;
                {
                    try
                    {
                        if(isProcessRun == false && quePayload.Count > 0)
                        {
                            currentPayload = quePayload.Dequeue();
                            DiscountProcess(currentPayload.data.car_number);
                        }
                        //여기서 큐를 실행하자..
                    }
                    catch (Exception)
                    {

                    }
                }

                shutdownProcessEvent.Wait(waitForProcess);
            }
            while (_pauseProcessEvent.WaitOne());
        }
        
        private async void DiscountProcess(string carno)
        {
            isProcessRun = true;
            try
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
                    Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"친환경 처리실패", LogAdpType.HomeNet);
                }

                //국가유공자 
                isProcessing = true;
                Task test2 = RequestNationalCarTask(carno);
                waitTask = WaitTask();
                await test2;
                await waitTask;

                if (!isProcessing) //처리완료
                {
                    Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"국가유공자 처리완료", LogAdpType.HomeNet);
                }
                else //처리 실패
                {
                    Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"국가유공자 처리실패", LogAdpType.HomeNet);
                }

                //장애인
                isProcessing = true;
                Task test3 = RequestDisabilityCarTask(carno);
                waitTask = WaitTask();
                await test3;
                await waitTask;

                if (!isProcessing) //처리완료
                {
                    Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"장애인 처리완료", LogAdpType.HomeNet);
                }
                else //처리 실패
                {
                    Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"장애인 처리실패", LogAdpType.HomeNet);
                }

                //경차
                isProcessing = true;
                Task test4 = RequestSamlCarTask(carno);
                waitTask = WaitTask();
                await test4;
                await waitTask;

                if (!isProcessing) //처리완료
                {
                    Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"경차 처리완료", LogAdpType.HomeNet);
                }
                else //처리 실패
                {
                    Log.WriteLog(LogType.Info, $"ULSNServerAdapter | DiscountProcess", $"경차 처리실패", LogAdpType.HomeNet);
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "ULSNServerAdapter | DiscountProcess", $"{ex.Message}");
            }
            isProcessRun = false;
        }

        private void TcpServer_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null)
        {
            string receiveMessage = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Log.WriteLog(LogType.Info, $"ULSNServerAdapter | TcpServer_ReceiveFromPeer", $"{receiveMessage}", LogAdpType.HomeNet);

            if (receiveMessage.Contains("Hello"))
            {
                byte[] data = Encoding.UTF8.GetBytes($"init#{SysConfig.Instance.HW_Domain}#{SysConfig.Instance.AuthToken}");
                TcpJavaServer.SendToPeer(data, 0, data.Length);
                return;
            }

            if (isProcessing) //작업 진행중
            {
                try
                {
                    string xmlData = string.Empty;
                    string command = string.Empty;

                    JObject obj = JObject.Parse(receiveMessage);
                    var jResult = obj["result"];

                    if (Helper.NVL(jResult["status"]) == "200")
                    {
                        command = Helper.NVL(obj["command"]);
                        xmlData = Helper.NVL(obj["data"]).Replace("\\n", "").Replace(" ", ""); //\n과 공백을 제거
                                                                                               //DB 저장
                        if (gov == null) gov = new GovInterfaceModel();
                        Dictionary<string, object> param = new Dictionary<string, object>();
                        param.Add("TkNo", currentPayload.data.reg_no);
                        param.Add("CarNo", currentPayload.data.car_number);
                        param.Add("RequestDateTime", $"{currentPayload.data.date_time}");
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
                            Log.WriteLog(LogType.Info, $"ULSNServerAdapter | TcpServer_ReceiveFromPeer", $"DB저장실패", LogAdpType.HomeNet);
                        }

                        isProcessing = false;
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "ULSNServerAdapter | TcpServer_ReceiveFromPeer", $"{ex.Message}");
                    isProcessing = false;
                }
            }
        }

        public bool StopAdapter()
        {
            isRun = !TcpJavaServer.Down();
            //Alive Check Thread pause
//#if (!DEBUG)
            _pauseEvent.Reset();
//#endif
            _pauseProcessEvent.Reset();
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
                byte[] data = Encoding.UTF8.GetBytes($"samll#{carno}#{json.ToString()}");
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
                int iSec = 10 * 100; //10초
                while (iSec > 0 && isProcessing)
                {
                    Thread.Sleep(10); //0.01초씩..쉰다...
                    iSec -= 1;
                }
            });
        }

        int number = 11;
        int tknum = 12341234;

        public void TestReceive(byte[] buffer)
        {

            lock (lockObj)
            {
                string json = "{\"command\": \"alert_incar\",\"data\": {\"dong\" : \"123\"," +
                            "\"ho\" : \"456\"," +
                            $"\"car_number\" : \"46부5989\"," +
                            "\"date_time\" : \"yyyyMMddHHmmss\"," +
                            "\"kind\" : \"v\"," +
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
            do
            {
                if (shutdownEvent.IsSet) return;
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

                shutdownEvent.Wait(waitForWork);
            }
            while (_pauseEvent.WaitOne());
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
}
