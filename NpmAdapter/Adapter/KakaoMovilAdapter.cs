using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class KakaoMovilAdapter : IAdapter
    {
        private const string GET_GATESTATUS = "/getGateStatus";
        private const string GET_IONDATA = "/getIONData";
        private const string SET_CUSTINFO = "/setCustInfo";
        private const string DEL_CUSTINFO = "/delCustInfo";
        private const string GET_CUSTINFO = "/getCustInfo";
        private const string GET_ALL_CUSTINFO = "/getAllCustInfo";
        private const string GET_IOSDATA = "/getIOSData";
        private const string SET_RESERVECAR = "/setReserveCar";
        private const string GET_RESERVECAR = "/getReserveCar";
        private const string DEL_RESERVECAR = "/delReserveCar";
        private const string GET_IORESERVE = "/getIOReserve";

        private const string APT_INCAR_POST = "/v2/car-manager/LPR/in/car/info";
        private const string APT_OUTCAR_POST = "/v2/car-manager/LPR/out/car/info";

        private const string AuthorizationSeed = "NexpaRegistryHashStoreValidationSeed";
        private string ValidationAuthoData = string.Empty;

        private object lockObj = new object();

        private string webport = "42141";
        private string aptId = "";
        private string hostDomain = "";
        private string reqPid = null;

        Dictionary<string, string> dicHeader = new Dictionary<string, string>();

        private bool isRun = false;
        private bool bResponseSuccess = false;

        private IPayload responsePayload;
        private StringBuilder receiveMessageBuffer = new StringBuilder();

        public event IAdapter.ShowBallonTip ShowTip;

        //==== Access Token Thread ====
        private int _AccesExpireSec = 0; //Access Token 만료 시간(초)

        private Thread AccessTokenThread;
        private TimeSpan waitForAccessTokenProcess;
        private ManualResetEventSlim shutdownAccessTokenEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseFailAccessTokenEvent = new ManualResetEvent(false);
        private delegate void AccessTokenSafeCallDelegate();
        //==== Fail Process Thread ====

        public IAdapter TargetAdapter { get; set; }
        private INetwork HttpNet { get; set; }
        public bool IsRuning => isRun;

        public void Dispose()
        {

        }

        public bool Initialize()
        {
            ValidationAuthoData = Helper.Base64Encode(AuthorizationSeed);
            aptId = SysConfig.Instance.ParkId;
            hostDomain = SysConfig.Instance.HW_Domain;
            webport = SysConfig.Instance.HW_Port;
            HttpNet = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, webport);

            AccessTokenThread = new Thread(new ThreadStart(AccessTokenAction));
            AccessTokenThread.Name = "access token";
            waitForAccessTokenProcess = TimeSpan.FromSeconds(1); //1초

            return true;
        }

        public bool StartAdapter()
        {
            HttpNet.ReceiveFromPeer += HttpServer_ReceiveFromPeer;
            isRun = HttpNet.Run();

            if (isRun)
            {
                //Access Token을 가져온다. 실패 시 서버가 죽었다고 판단함.
                try
                {
                    dicHeader.Add("Authorization",GetAccessToken());

                    if (AccessTokenThread.IsAlive)
                    {
                        _pauseFailAccessTokenEvent.Set();
                    }
                    else
                    {
                        AccessTokenThread.Start();
                        _pauseFailAccessTokenEvent.Set();
                    }
                }
                catch (Exception)
                {
                    isRun = false;
                }
            }

            return isRun;
        }

        public bool StopAdapter()
        {
            bool bResult = false;
            try
            {
                _pauseFailAccessTokenEvent.Reset();
                _AccesExpireSec = 0;

                HttpNet.ReceiveFromPeer -= HttpServer_ReceiveFromPeer;
                bResult = HttpNet.Down();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "KakaoMovilAdapter | StopAdapter", $"{ex.StackTrace}", LogAdpType.HomeNet);
                return false;
            }
            isRun = !bResult;
            return bResult;
        }

        /// <summary>
        /// 카카오 모빌로부터 수신
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <param name="e"></param>
        private void HttpServer_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs e = null, string id = null, System.Net.EndPoint ep = null)
        {
            lock (lockObj)
            {
                bResponseSuccess = false;

                Dictionary<string, string> dicParams = null;
                JObject json = null;

                string urlData = e.Request.Uri.LocalPath;
                string sMethod = e.Request.Method;
                //Header에 Authorization을 추가하여 보낸다.
                HeaderFactory hf = new HeaderFactory();
                IHeader iHeader = hf.Parse("Authorization", dicHeader["Authorization"]);
                Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | MyHttpNetwork_ReceiveFromPeer", $"METHOD : {sMethod}, URL : {urlData}", LogAdpType.HomeNet);

                try
                {
                    if (e.Request.Headers["Authorization"].HeaderValue != ValidationAuthoData)
                    {
                        e.Response.Connection.Type = ConnectionType.Close;
                        e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                        e.Response.Status = System.Net.HttpStatusCode.Unauthorized;
                        e.Response.Reason = "Faild Authorization";

                        MvlResponsePayload resultPayload = new MvlResponsePayload();
                        resultPayload.resultCode = MvlResponsePayload.SttCode.InvalidToken;
                        resultPayload.resultMessage = "권한이 없습니다. 토큰을 확인 바랍니다";
                        resultPayload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        byte[] result = resultPayload.Serialize();

                        Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | 응답(권한오류)", $"{resultPayload.ToJson()}", LogAdpType.HomeNet);

                        e.Response.Body.Write(result, 0, result.Length);
                    }
                    else
                    {
                        if (sMethod == "GET")
                        {
                            dicParams = new Dictionary<string, string>();
                            foreach (var item in e.Request.Parameters)
                            {
                                dicParams.Add(item.Name.ToUpper(), item.Value);
                            }
                        }
                        else
                        {
                            json = JObject.Parse(SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]));
                        }

                        switch (urlData)
                        {
                            case GET_GATESTATUS: //차단기 상태조회, Alive Check~!
                                {
                                    MvlResponsePayload payload = new MvlResponsePayload();
                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = "SUCCESS";
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{GET_GATESTATUS}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] result = payload.Serialize();
                                    e.Response.Encoding = SysConfig.Instance.HomeNet_Encoding;
                                    e.Response.ContentType = new ContentTypeHeader("application/json");
                                    e.Response.Add(iHeader);
                                    e.Response.Body.Write(result, 0, result.Length);
                                    return;
                                }
                            case GET_IONDATA: //미등록 일반차량 출입 조회(GET)
                                {
                                    RequestSearchIONPayload dataPayload = new RequestSearchIONPayload();
                                    dataPayload.car_number = dicParams.GetValue("CARNO");
                                    dataPayload.start_date_time = dicParams.GetValue("STARTDATETIME").ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss");
                                    dataPayload.end_date_time = dicParams.GetValue("ENDDATETIME").ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss");

                                    RequestPayload<RequestSearchIONPayload> payload = new RequestPayload<RequestSearchIONPayload>();
                                    payload.command = CmdType.ion_list;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{GET_IONDATA}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case SET_CUSTINFO: //정기차량 등록
                                {
                                    RequestCustRegPayload dataPayload = new RequestCustRegPayload();
                                    dataPayload.car_number = Helper.NVL(json["Carno"]);
                                    dataPayload.dong = Helper.NVL(json["Dong"]);
                                    dataPayload.ho = Helper.NVL(json["Ho"]);
                                    dataPayload.name = Helper.NVL(json["Name"]);
                                    dataPayload.start_date = Helper.NVL(json["EffStart"]).ConvertDateTimeFormat("yyyy-MM-dd", "yyyyMMdd");
                                    dataPayload.end_date = Helper.NVL(json["EffEnd"]).ConvertDateTimeFormat("yyyy-MM-dd", "yyyyMMdd");
                                    dataPayload.tel_number = Helper.NVL(json["Contact"]);
                                    dataPayload.remark = Helper.NVL(json["Remark"]);

                                    RequestPayload<RequestCustRegPayload> payload = new RequestPayload<RequestCustRegPayload>();
                                    payload.command = CmdType.cust_reg;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{SET_CUSTINFO}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case DEL_CUSTINFO: //정기차량 삭제
                                {
                                    RequestCustDelPayload dataPayload = new RequestCustDelPayload();
                                    dataPayload.car_number = Helper.NVL(json["Carno"]);
                                    dataPayload.dong = Helper.NVL(json["Dong"]);
                                    dataPayload.ho = Helper.NVL(json["Ho"]);
                                    dataPayload.reg_no = Helper.NVL(json["TKNo"]);

                                    RequestPayload<RequestCustDelPayload> payload = new RequestPayload<RequestCustDelPayload>();
                                    payload.command = CmdType.cust_del;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{DEL_CUSTINFO}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case GET_CUSTINFO: //정기차량 세대 목록(GET)
                                {
                                    RequestCustListPayload dataPayload = new RequestCustListPayload();
                                    dataPayload.car_number = dicParams.GetValue("CARNO");
                                    dataPayload.dong = dicParams.GetValue("DONG");
                                    dataPayload.ho = dicParams.GetValue("HO");

                                    RequestPayload<RequestCustListPayload> payload = new RequestPayload<RequestCustListPayload>();
                                    payload.command = CmdType.cust_list;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{GET_CUSTINFO}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case GET_ALL_CUSTINFO: //정기차량 전체 목록(GET)
                                {
                                    RequestEmptyPayload payload = new RequestEmptyPayload();
                                    payload.command = CmdType.cust_list;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{GET_ALL_CUSTINFO}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case GET_IOSDATA: //정기차량 출입조회(GET)
                                {
                                    RequestSearchIONPayload dataPayload = new RequestSearchIONPayload();
                                    dataPayload.car_number = dicParams.GetValue("CARNO");
                                    dataPayload.start_date_time = dicParams.GetValue("STARTDATETIME").ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss");
                                    dataPayload.end_date_time = dicParams.GetValue("ENDDATETIME").ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss");

                                    RequestPayload<RequestSearchIONPayload> payload = new RequestPayload<RequestSearchIONPayload>();
                                    payload.command = CmdType.cust_io_list;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{GET_IOSDATA}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case SET_RESERVECAR: //방문신청차량 등록
                                {
                                    RequestVisitRegPayload dataPayload = new RequestVisitRegPayload();
                                    dataPayload.car_number = Helper.NVL(json["Carno"]);
                                    dataPayload.dong = Helper.NVL(json["Dong"]);
                                    dataPayload.ho = Helper.NVL(json["Ho"]);
                                    dataPayload.date = Helper.NVL(json["Reservestart"]).ConvertDateTimeFormat("yyyy-MM-dd", "yyyyMMdd");
                                    DateTime startDate = Convert.ToDateTime(Helper.NVL(json["Reservestart"]));
                                    DateTime endDate = Convert.ToDateTime(Helper.NVL(json["Reserveend"]));

                                    TimeSpan dateDiff = endDate - startDate;
                                    dataPayload.term = dateDiff.Days.ToString();
                                    //TODO : Remark 추가해야함... 2021-01-14
                                    RequestPayload<RequestVisitRegPayload> payload = new RequestPayload<RequestVisitRegPayload>();
                                    payload.command = CmdType.visit_reg;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{SET_RESERVECAR}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case GET_RESERVECAR: //방문신청차량 목록(GET)
                                {
                                    RequestVisitSingleListPayload dataPayload = new RequestVisitSingleListPayload();
                                    dataPayload.car_number = dicParams.GetValue("CARNO");
                                    dataPayload.dong = dicParams.GetValue("DONG");
                                    dataPayload.ho = dicParams.GetValue("HO");

                                    RequestPayload<RequestVisitSingleListPayload> payload = new RequestPayload<RequestVisitSingleListPayload>();
                                    payload.command = CmdType.visit_single_list;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{GET_RESERVECAR}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case DEL_RESERVECAR: //방문신청차량 삭제
                                {
                                    RequestVisitDelPayload dataPayload = new RequestVisitDelPayload();
                                    dataPayload.dong = Helper.NVL(json["Dong"]);
                                    dataPayload.ho = Helper.NVL(json["Ho"]);
                                    dataPayload.reg_no = Helper.NVL(json["Belong"]);
                                    dataPayload.car_number = Helper.NVL(json["Carno"]);

                                    RequestPayload<RequestVisitDelPayload> payload = new RequestPayload<RequestVisitDelPayload>();
                                    payload.command = CmdType.visit_del;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{DEL_RESERVECAR}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            case GET_IORESERVE: //방문신청차량 출입 조회(GET)
                                {
                                    RequestVisitSingleIOPayload dataPayload = new RequestVisitSingleIOPayload();
                                    dataPayload.dong = dicParams.GetValue("DONG");
                                    dataPayload.ho = dicParams.GetValue("HO");
                                    dataPayload.car_number = dicParams.GetValue("CARNO");

                                    RequestPayload<RequestVisitSingleIOPayload> payload = new RequestPayload<RequestVisitSingleIOPayload>();
                                    payload.command = CmdType.visit_single_io;
                                    payload.data = dataPayload;
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | HttpServer_ReceiveFromPeer", $"{GET_IORESERVE}: {payload.ToJson()}", LogAdpType.HomeNet);

                                    byte[] responseBuffer = payload.Serialize();
                                    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, reqPid);
                                }
                                break;
                            default:
                                e.Response.Connection.Type = ConnectionType.Close;
                                e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                                e.Response.Status = System.Net.HttpStatusCode.MethodNotAllowed;
                                e.Response.Reason = "Bad Request";

                                {
                                    MvlResponsePayload payload = new MvlResponsePayload();
                                    payload.resultCode = MvlResponsePayload.SttCode.NotSupportedMethod;
                                    payload.resultMessage = "지원하지 않는 http 메소드 입니다";
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    byte[] result = payload.Serialize();
                                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | 응답(메소드오류)", $"{payload.ToJson()}", LogAdpType.HomeNet);
                                    e.Response.Add(iHeader);
                                    e.Response.Body.Write(result, 0, result.Length);
                                    return;
                                }
                        }

                        //Task waitTask = WaitTask(15);
                        //await waitTask;
                        //5초 대기 Task
                        int iSec = 5 * 100; //10초
                        while (iSec > 0 && !bResponseSuccess)
                        {
                            Thread.Sleep(10); //0.01초씩..쉰다...
                            iSec -= 1;
                        }

                        if (bResponseSuccess) //응답성공
                        {
                            byte[] result = responsePayload.Serialize();
                            e.Response.Encoding = SysConfig.Instance.HomeNet_Encoding;
                            e.Response.ContentType = new ContentTypeHeader("application/json");
                            e.Response.Add(iHeader);
                            Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | 응답(성공)", $"{responsePayload.ToJson()}", LogAdpType.HomeNet);
                            e.Response.Body.Write(result, 0, result.Length);
                        }
                        else
                        {
                            MvlResponsePayload payload = new MvlResponsePayload();
                            payload.resultCode = MvlResponsePayload.SttCode.NotSupportedMethod;
                            payload.resultMessage = "주차 시스템으로 부터 응답이 없습니다";
                            payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            byte[] result = payload.Serialize();
                            e.Response.Encoding = SysConfig.Instance.HomeNet_Encoding;
                            e.Response.ContentType = new ContentTypeHeader("application/json");
                            e.Response.Add(iHeader);
                            //응답시간이 초과되었다면 responsePayload 는 값이 없는 상태이므로 Null Reference Exception이 발생함.
                            Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | 응답(TimeOut)", $"응답시간 초과", LogAdpType.HomeNet);
                            e.Response.Body.Write(result, 0, result.Length);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MvlResponsePayload payload = new MvlResponsePayload();
                    payload.resultCode = MvlResponsePayload.SttCode.InternalServerError;
                    payload.resultMessage = MvlResponsePayload.SttCode.InternalServerError.GetDescription();
                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | 응답(Exception:{ex.StackTrace})", $"{payload.ToJson()}", LogAdpType.HomeNet);
                    byte[] result = payload.Serialize();
                    e.Response.Encoding = SysConfig.Instance.HomeNet_Encoding;
                    e.Response.ContentType = new ContentTypeHeader("application/json");
                    e.Response.Add(iHeader);
                    e.Response.Body.Write(result, 0, result.Length);
                }
            }
        }

        private void Test()
        {
            //Test
            JObject responseTest = new JObject();
            responseTest["command"] = "cust_reg";
            JObject responseDataTest = new JObject();
            responseDataTest["park_no"] = "1";
            responseDataTest["reg_no"] = "12345";
            responseDataTest["car_number"] = "11가1111";
            responseDataTest["type"] = "1";
            responseTest["data"] = responseDataTest;
            JObject responseResultTest = new JObject();
            responseResultTest["status"] = "200";
            responseResultTest["message"] = "OK";
            responseTest["result"] = responseResultTest;

            byte[] test = responseTest.ToByteArray(SysConfig.Instance.Nexpa_Encoding);
            this.SendMessage(test, 0, test.Length);
        }

        /// <summary>
        /// Nexpa 관제로부터 수신
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(Helper.ValidateJsonParseingData(receiveMessageBuffer.ToString()));

            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | SendMessage", $"Adapter 수신\n{jobj}", LogAdpType.HomeNet);
            
            string cmd = jobj["command"].ToString();
            JObject data = jobj["data"] as JObject;
            switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
            {
                case CmdType.hello:
                    string handShakeData = jobj["data"].ToString();
                    if (Helper.NVL(handShakeData) == "biz") reqPid = pid;
                    break;
                case CmdType.alert_incar:
                case CmdType.alert_outcar:
                    {
                        ResponsePayload responsePayload = new ResponsePayload();
                        byte[] responseBuffer;
                        RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                        payload.Deserialize(jobj);

                        //if (payload.data.kind.Equals("n"))
                        //{
                        //    responsePayload.result = ResultType.NonContent; //처리할게 없음.
                        //    responsePayload.command = payload.command;
                        //    responseBuffer = responsePayload.Serialize();
                        //    TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, pid);
                        //    break;
                        //}

                        //car_imgae 의 car_number 와 car_number 간 비교를 통해 두 차량번호가 다르다면
                        //car_image 의 car_number 로 car_number 필드를 update 한다.
                        string car_img = payload.data.car_image;
                        var imgIdx = car_img.LastIndexOf('_') + 1;
                        if(imgIdx != 0)
                        {
                            var jpg = car_img.Substring(imgIdx);
                            var jIdx = jpg.LastIndexOf('.');
                            var valid_Car_number = jpg.Substring(0, jIdx);

                            if(payload.data.car_number != valid_Car_number)
                            {
                                Log.WriteLog(LogType.Info, "KakaoMovilAdapter | SendMessage", $"== 차량 번호 보정 : {payload.data.car_number} -> {valid_Car_number}", LogAdpType.HomeNet);
                                payload.data.car_number = valid_Car_number;
                            }
                        }

                        MvlInOutCarPayload ioPayload = new MvlInOutCarPayload(payload.command)
                        {
                            apt_idx = int.Parse(aptId),
                            car_number = payload.data.car_number,
                            lpr_number = payload.data.lprID,
                            date = Helper.GetUTCMillisecond(payload.data.date_time)
                        };
                        
                        byte[] requestData = ioPayload.Serialize();
                        Uri uri = payload.command == CmdType.alert_incar ? new Uri(string.Concat(hostDomain, APT_INCAR_POST)) : new Uri(string.Concat(hostDomain, APT_OUTCAR_POST));
                        string responseData = string.Empty;
                        string responseHeader = string.Empty;

                        Log.WriteLog(LogType.Info, "KakaoMovilAdapter | SendMessage", $"Web 송신\n{ioPayload.ToJson()}", LogAdpType.HomeNet);

                        if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Json, requestData, ref responseData, ref responseHeader, header: dicHeader))
                        {
                            try
                            {
                                if (responseData.StartsWith("ERR"))
                                {
                                    Log.WriteLog(LogType.Error, $"KakaoMovilAdapter | SendMessage", $"Web 수신\n{responseData}", LogAdpType.HomeNet);
                                    string[] errMsgs = responseData.Split(',');
                                    //Error 처리...
                                    switch (errMsgs[1])
                                    {
                                        case "400":
                                            responsePayload.result = ResultType.BadRequest;
                                            break;
                                        case "401":
                                            responsePayload.result = ResultType.Unauthorized;
                                            break;
                                        case "404":
                                            responsePayload.result = ResultType.NotFound;
                                            break;
                                        case "405":
                                            responsePayload.result = ResultType.MethodNotAllowed;
                                            break;
                                        default:
                                            responsePayload.result = ResultType.Fail;
                                            break;
                                    }
                                }
                                else
                                {
                                    responsePayload.result = ResultType.OK;
                                }

                                var responseJobj = JObject.Parse(responseData);
                                if (responseJobj != null && Helper.NVL(responseJobj["code"]) == "0000")
                                {
                                    responsePayload.command = payload.command;
                                    responseBuffer = responsePayload.Serialize();
                                }
                                else
                                {
                                    responsePayload.command = payload.command;
                                    responsePayload.result = ResultType.ExceptionERROR;
                                    responseBuffer = responsePayload.Serialize();
                                }

                                TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length, pid);
                            }
                            catch (Exception ex)
                            {
                                Log.WriteLog(LogType.Error, "KakaoMovilAdapter | SendMessage", $"{ex.StackTrace}", LogAdpType.HomeNet);
                            }
                        }
                    }
                    break;

                default:
                    //결과 Payload 생성 =======
                    JObject result = jobj["result"] as JObject; //응답 결과
                    ResultPayload resultPayload = result.GetResultPayload();

                    if (resultPayload.code == "200")
                    {
                        switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
                        {
                            case CmdType.ion_list:
                                {
                                    MvlValuePayload<MvlIONDataPayload> payload = new MvlValuePayload<MvlIONDataPayload>();

                                    if (data != null && data.HasValues)
                                    {
                                        JArray list = data["list"] as JArray;
                                        if (list != null)
                                        {
                                            foreach (JObject item in list)
                                            {
                                                MvlIONDataPayload dataPayload = new MvlIONDataPayload();
                                                dataPayload.carNo = Helper.NVL(item["car_number"]);
                                                dataPayload.parkNo = Helper.NVL(item["park_no"]);
                                                dataPayload.tkNo = Helper.NVL(item["reg_no"]);
                                                dataPayload.indatetime = Helper.NVL(item["in_date_time"]).ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                                dataPayload.outdatetime = Helper.NVL(item["out_date_time"]).ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                                payload.list.Add(dataPayload);
                                            }
                                        }
                                    }

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                            case CmdType.cust_reg:
                                {
                                    MvlSingleCustInfoPayload payload = new MvlSingleCustInfoPayload();

                                    if (data != null && data.HasValues)
                                    {
                                        payload.carNo = Helper.NVL(data["car_number"]);
                                        if (Helper.NVL(data["car_number"]) == "1" || Helper.NVL(data["car_number"]) == "")
                                        {
                                            payload.enrollType = MvlSingleCustInfoPayload.EnrollType.New;
                                        }
                                        else
                                        {
                                            payload.enrollType = MvlSingleCustInfoPayload.EnrollType.Modify;
                                        }
                                        payload.tkNo = Helper.NVL(data["reg_no"]);
                                    }

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                            case CmdType.cust_del:
                                {
                                    MvlResponsePayload payload = new MvlResponsePayload();

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                            case CmdType.cust_list:
                                {
                                    MvlValuePayload<MvlCustInfoPayload> payload = new MvlValuePayload<MvlCustInfoPayload>();

                                    if (data != null && data.HasValues)
                                    {
                                        JArray list = data["list"] as JArray;
                                        if (list != null)
                                        {
                                            foreach (JObject item in list)
                                            {
                                                MvlCustInfoPayload dataPayload = new MvlCustInfoPayload();
                                                dataPayload.tkNo = Helper.NVL(item["reg_no"]);
                                                dataPayload.groupNo = "1"; //Optional
                                                dataPayload.carNo = Helper.NVL(item["car_number"]);
                                                dataPayload.dong = Helper.NVL(item["dong"]);
                                                dataPayload.ho = Helper.NVL(item["ho"]);
                                                dataPayload.name = Helper.NVL(item["name"]);
                                                dataPayload.contact = Helper.NVL(item["tel_number"]);
                                                dataPayload.remark = Helper.NVL(item["remark"]);
                                                dataPayload.effStart = Helper.NVL(item["start_date"]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                                dataPayload.effEnd = Helper.NVL(item["end_date"]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                                dataPayload.chkUse = 0; //Optional
                                                payload.list.Add(dataPayload);
                                            }
                                        }
                                    }

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                            case CmdType.cust_io_list:
                                {
                                    MvlValuePayload<MvlIOSDataPayload> payload = new MvlValuePayload<MvlIOSDataPayload>();

                                    if (data != null && data.HasValues)
                                    {
                                        JArray list = data["list"] as JArray;
                                        if (list != null)
                                        {
                                            foreach (JObject item in list)
                                            {
                                                MvlIOSDataPayload dataPayload = new MvlIOSDataPayload();
                                                dataPayload.tkNo = Helper.NVL(item["reg_no"]);
                                                dataPayload.parkNo = Helper.NVL(item["park_no"]);
                                                dataPayload.carNo = Helper.NVL(item["car_number"]);
                                                dataPayload.dong = Helper.NVL(item["dong"]);
                                                dataPayload.ho = Helper.NVL(item["ho"]);
                                                dataPayload.indatetime = Helper.NVL(item["in_date_time"]).ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                                dataPayload.outdatetime = Helper.NVL(item["out_date_time"]).ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                                payload.list.Add(dataPayload);
                                            }
                                        }
                                    }

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                            case CmdType.visit_reg:
                                {
                                    MvlSingleReserveCarPayload payload = new MvlSingleReserveCarPayload();

                                    if (data != null && data.HasValues)
                                    {
                                        payload.belong = Helper.NVL(data["reg_no"]);
                                    }

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                            case CmdType.visit_single_list:
                                {
                                    MvlValuePayload<MvlReserveCarPayload> payload = new MvlValuePayload<MvlReserveCarPayload>();

                                    if (data != null && data.HasValues)
                                    {
                                        JArray list = data["list"] as JArray;
                                        if (list != null)
                                        {
                                            foreach (JObject item in list)
                                            {
                                                MvlReserveCarPayload dataPayload = new MvlReserveCarPayload();
                                                dataPayload.Belong = Helper.NVL(item["reg_no"]);
                                                dataPayload.carNo = Helper.NVL(item["car_number"]);
                                                dataPayload.dong = Helper.NVL(item["dong"]);
                                                dataPayload.ho = Helper.NVL(item["ho"]);
                                                dataPayload.reserveStart = Helper.NVL(item["start_date"]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                                dataPayload.reserveEnd = Helper.NVL(item["end_date"]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                                dataPayload.remark = Helper.NVL(item["remark"]);
                                                payload.list.Add(dataPayload);
                                            }
                                        }
                                    }

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                            case CmdType.visit_del:
                                {
                                    MvlResponsePayload payload = new MvlResponsePayload();

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                            case CmdType.visit_single_io:
                                {
                                    MvlValuePayload<MvlIOReservePayload> payload = new MvlValuePayload<MvlIOReservePayload>();

                                    if (data != null && data.HasValues)
                                    {
                                        JArray list = data["list"] as JArray;
                                        if (list != null)
                                        {
                                            foreach (JObject item in list)
                                            {
                                                MvlIOReservePayload dataPayload = new MvlIOReservePayload();
                                                dataPayload.parkNo = Helper.NVL(item["park_no"]);
                                                dataPayload.carNo = Helper.NVL(item["car_number"]);
                                                dataPayload.dong = Helper.NVL(item["dong"]);
                                                dataPayload.ho = Helper.NVL(item["ho"]);
                                                dataPayload.indatetime = Helper.NVL(item["in_date_time"]).ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                                dataPayload.outdatetime = Helper.NVL(item["out_date_time"]).ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                                payload.list.Add(dataPayload);
                                            }
                                        }
                                    }

                                    payload.resultCode = MvlResponsePayload.SttCode.OK;
                                    payload.resultMessage = MvlResponsePayload.SttCode.OK.GetDescription();
                                    payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                    responsePayload = payload;
                                    bResponseSuccess = true;
                                }
                                break;
                        }
                    }
                    else
                    {
                        MvlResponsePayload payload = new MvlResponsePayload();

                        payload.resultCode = MvlResponsePayload.SttCode.InternalServerError;
                        payload.resultMessage = resultPayload.message;
                        payload.responseTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        responsePayload = payload;
                        bResponseSuccess = true;
                    }
                    break;
            }
        }

        public void TestReceive(byte[] buffer)
        {
            string json = "{\"command\": \"alert_incar\",\"data\": {\"dong\" : \"101\"," +
                            "\"ho\" : \"101\"," +
                            $"\"car_number\" : \"46부5989\"," +
                            "\"date_time\" : \"20210305042525\"," +
                            "\"kind\" : \"v\"," +
                            "\"lprid\" : \"Lpr 식별 번호\"," +
                            "\"car_image\" : \"차량 이미지 경로\"," +
                            $"\"reg_no\" : \"111111\"," +
                            "\"visit_in_date_time\" : \"yyyyMMddHHmmss\"," + //방문시작일시, kind가 v 일 경우 외 빈값
                            "\"visit_out_date_time\" : \"yyyyMMddHHmmss\"" + //방문종료일시, kind가 v 일 경우 외 빈값
                            "}" +
                            "}";
            byte[] test = SysConfig.Instance.Nexpa_Encoding.GetBytes(json);
            SendMessage(test, 0, test.Length);
        }

        private void AccessTokenAction()
        {
            do
            {
                if (shutdownAccessTokenEvent.IsSet) return;
                {
                    try
                    {
                        if (_AccesExpireSec < 1) //새 Access Token 을 발급 받는다.
                        {
                            //Access Token 발급
                            string accessToken = GetAccessToken();
                            if (dicHeader.ContainsKey("Authorization"))
                            {
                                dicHeader["Authorization"] = accessToken;
                            }
                            else
                            {
                                dicHeader.Add("Authorization", accessToken);
                            }
                            //Alive Check 서버로 전달....
                            Log.WriteLog(LogType.Info, $"KakaoMovilAdapter | AccessTokenAction", $"AccessToken : {accessToken}, AcceptSecond : {_AccesExpireSec}");
                        }
                        else
                        {
                            _AccesExpireSec -= 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogType.Error, $"KakaoMovilAdapter | AccessTokenAction", $"{ex.StackTrace}");
                    }
                }

                shutdownAccessTokenEvent.Wait(waitForAccessTokenProcess);
            }
            while (_pauseFailAccessTokenEvent.WaitOne());
        }

        private string GetAccessToken()
        {
            //개발 - https://dev-openapi.themovill.com/facility/parking/oauth2/token
            //운영 - https://openapi.themovill.com/facility/parking/oauth2/token
            string id = SysConfig.Instance.HC_Id;
            string secret = SysConfig.Instance.HC_Pw;
            string authDomain = SysConfig.Instance.HW_Domain2;
            Uri uri = new Uri(authDomain);

            //Response Valiable
            string accessToken = string.Empty;
            string responseData = string.Empty;
            string responseHeader = string.Empty;

            try
            {
                //Request
                string postMessage = "grant_type=client_credentials&scope=read";
                Dictionary<string, string> dicAccHeader = new Dictionary<string, string>();
                string auth = $"{id}:{secret}";
                string authHeader = $"Basic {auth.Base64Encode()}";
                dicAccHeader.Add("Authorization", authHeader);
                Log.WriteLog(LogType.Info, "KakaoMovilAdapter | GetAccessToken", $"인증토큰 : {authHeader}", LogAdpType.HomeNet);

                if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.FormData, SysConfig.Instance.HomeNet_Encoding.GetBytes(postMessage), ref responseData, ref responseHeader, header: dicAccHeader))
                {
                    JObject jobj = JObject.Parse(responseData);
                    accessToken = $"{Helper.NVL(jobj["token_type"])} {Helper.NVL(jobj["access_token"])}";
                    //Token 만료 시간 설정
                    int.TryParse(Helper.NVL(jobj["expires_in"]), out _AccesExpireSec);
                    _AccesExpireSec -= 300; //5분전 Access Token 재발급...
                }

            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "KakaoMovilAdapter | GetAccessToken", $"Exception Message : {ex.StackTrace}", LogAdpType.HomeNet);
            }

            return accessToken;
        }
    }
}
