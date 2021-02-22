using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

namespace NpmAdapter.Adapter
{
    class ULSNServerAdapter : IAdapter
    {
        private Queue<string> peer;
        //==== Alive Check ====
        private Thread aliveCheckThread;
        private TimeSpan waitForWork;
        private ManualResetEventSlim shutdownEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private delegate void SafeCallDelegate();
        private bool isRun;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam);

        //==== Alive Check ====
        public bool IsRuning => isRun;
        public IAdapter TargetAdapter { get; set; }
        private INetwork TcpServer { get; set; }


        public void Dispose()
        {
            if (isRun) TcpServer.Down();
#if (!DEBUG)
            _pauseEvent.Set();
            shutdownEvent.Set();
            JClientKill();
#endif
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
                peer = new Queue<string>();
                TcpServer = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.TcpServer, SysConfig.Instance.HT_MyPort);
#if (!DEBUG)
                JClientKill();
#endif
                aliveCheckThread = new Thread(new ThreadStart(AliveCheck));
                aliveCheckThread.Name = "alive check";
                waitForWork = TimeSpan.FromSeconds(10);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"ULSNServerAdapter | Initialize", $"{ex.Message}", LogAdpType.Nexpa);
                return false;
            }

            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            if (pid != null && pid != "") peer.Enqueue(pid);
        }
        
        public bool StartAdapter()
        {
            TcpServer.ReceiveFromPeer += TcpServer_ReceiveFromPeer;
            isRun = TcpServer.Run();

            //Alive Check Thread 시작
#if (!DEBUG)
            if (aliveCheckThread.IsAlive)
            {
                _pauseEvent.Set();
            }
            else
            {
                aliveCheckThread.Start();
                _pauseEvent.Set();
            }
#endif
            return isRun;
        }

        private void TcpServer_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs pEvent = null, string id = null)
        {
            string testMessage = Encoding.UTF8.GetString(buffer, (int)offset, (int)size);
            Log.WriteLog(LogType.Error, $"ULSNServerAdapter | TcpServer_ReceiveFromPeer", $"{testMessage}", LogAdpType.HomeNet);
            Log.WriteLog(LogType.Error, $"ULSNServerAdapter | Initialize", $"Hello", LogAdpType.HomeNet);
            //Client로부터 받은 메시지를 처리한다.'
            peer.Enqueue(id);
        }

        public bool StopAdapter()
        {
            isRun = !TcpServer.Down();
            //Alive Check Thread pause
#if (!DEBUG)
            _pauseEvent.Reset();
#endif
            return !isRun;
        }

        public void TestReceive(byte[] buffer)
        {
            //JObject json = new JObject();
            //json["infoType"] = "disabilityCar";
            //json["CAR_NO"] = "11가1111";
            //string strData = "{\"infoType\":\"echoCar\",\"CAR_NO\":\"11가1111\"}";//json.ToString();
            string privateExponent = "32121578780987823488238876392745049073356503855799731576462257342701047108272294427839700472215763113281677630971377137068058057743355696373629740906087657473217347764355582817477026515679978625458931790426710351924748873922266231334861062186898348419164106275767687575168130415099689337531007099600067822754917898237190255023341394238207790286811897979735675823152723041657179973736230833668988754008937631594613308341063778163612990663717028967130494144902543555334245252765200615005122669618539958158750817518261590359483900590389989639651535717826586899753462521556557321429754075166616004657553442578704260080216222473034150628640374506432152915796693383830525503126746740376704500097584653642456340075095702312592775954563908200064705520068259283354844891588405678212570705123843484721073380127410922476953625115042106767144953453771900091391166832401204927345334557219732902108130658305911369314406852592964778184646684826788674870326104570645431337942349618110682979574156902305863218089980596561858255108567940060515098984242872621360551576210021058878791049062938612395480694062037486141495344944406515349742639417652913335549873414161864556003548129530587108796966071638519702373907505907720825573199163713562870569851489";
            string privateModulus = "551664546270859273571465210207372191069330501362040620630662201118605483316258218007686176584802009212563235561051138215940597728072930365104447675514220861588638710805181297460427643280429444228695496003457892121093361360126719602461422807270114533633846444705185256974264612949263191853713210766899802122612435612361304440111301088624062880510165450182897533129444447060033177132796478025724453870933895587478032596632153257208780023356400138212481969280523584115812481952430019052827757966140003469038273670779431301726807230868026402257296304072117669195267996141313442655802356610113866115629475882148988755995102161375373758046150473600059430321763448126715135580154863225813319407574878152858030137106503262948942900143029688558401221467657070242568641574846725628758563171326743519374209709998736386612164640309449653200915704068710052161608779457402434155085829003661662396492885000336511271184718275595014127174185031557876949402785896910596024026413527311649030839678452780939886045689508101129412919649813484296883649243080091353017317745620374162559872626559466353202762611718072259915109188524876254830578534133762281062683147731404416340194511092928664108111832033366280826955050110148272716685004352723410703737801101";
            string encData = "009ce153f4d40dc839f03b8cee720274146b358a40b98ee0c95e45e9851aa1fc40f44c49812a05746da4eabd4433ae4ef97e822cdaf3b05bfb033e02fe9159f10a1d55e195c527feaa4fe58809c46d74e1e260c3298e13db534cf1c151c67c5a906fc21597f81f5578dd8af2558f160ebb760c71bdf480c4cb41c6c653d752bf0d4e085cbc08108d4daecb65d0c0f05d4fc1af9fb3a6e400d46a9c4fd236736c37dc916583ebfc8cf90ee886a1c4141e651c5c0f1880d74931360049bd768d9eba83c2eb945931323335eb8d1fc35f1df0be0cb644cf9b6ae1c1d275bcb48b0ec85438b1fcc74686cbd60260f01491b22fabdd2547f20d026da2584ed8ba46cbec6d47f937f0a4f1139ff804be77c9c859a678af0ac8320b15b3caea9359dbf987574d6328c4cc062dbe55453a6546581c192aeaf3eac523d5edf72f49bd71e742b0cae30202198b0c6eb067931c5d81212de5d5f7a3fc34cb0faaebd48e67f325bb1e51b211525a8ab40e4c4fdd0c62a2de6ea7a835e12b49d3829954756319cd7828f6bb5cc887e2df6d3f4fc24266088347600b4663f5bcf98e1379910482d342425574335cae76c8b88772ecc3c91199d612e930644b8d51bef1e839e00d642794ba8a503f090b3934ef7221317605a575c06596badb74f53d5c38ec11b607a7150a38575482ef001ec8acd27618056705f6f98f0ed98887fd7437021b1e";
            //string strData = "{\"param\":\"" + encData + "\",\"exponent\":\"" + privateExponent + "\",\"modulus\":\"" + privateModulus + "\"}";
            string strData = $"{encData}&{privateExponent}&{privateModulus}";
            byte[] data = Encoding.UTF8.GetBytes($"D#{strData}");
            TcpServer.SendToPeer(data, 0, data.Length);


            //if (gov == null) gov = new GovInterfaceModel();
            //Dictionary<string, object> param = new Dictionary<string, object>();
            //param.Add("TkNo", "2020032613341710");
            //param.Add("CarNo", "11가1111");
            //param.Add("RequestDateTime", "20210219180312");
            //param.Add("InterfaceCode", "TEST");
            //param.Add("InterfaceData", "<faultcode>soap:Server</faultcode><faultstring>Fault occured</faultstring><detail />");

            //bool bResult = gov.Save(param);
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
