using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;
using System.Text;
using System.Threading;

namespace NpmAdapter.Adapter
{
    class KakaoMovilAdapter : IAdapter
    {
        private const string GET_IONDATA = "getIONData";
        private const string SET_CUSTINFO = "setCustInfo";
        private const string DEL_CUSTINFO = "delCustInfo";
        private const string GET_CUSTINFO = "getCustInfo";
        private const string GET_ALL_CUSTINFO = "getAllCustInfo";
        private const string GET_IOSDATA = "getIOSData";
        private const string SET_RESERVECAR = "setReserveCar";
        private const string GET_RESERVECAR = "getReserveCar";
        private const string DEL_RESERVECAR = "delReserveCar";
        private const string GET_IORESERVE = "getIOReserve";

        private object lockObj = new object();
        private string webport = "42141";

        private bool isRun = false;
        private bool bResponseSuccess = false;
        private IPayload responsePayload;
        private StringBuilder receiveMessageBuffer = new StringBuilder();

        public IAdapter TargetAdapter { get; set; }
        private INetwork HttpNet { get; set; }
        public bool IsRuning => isRun;

        public void Dispose()
        {

        }

        public bool Initialize()
        {
            webport = SysConfig.Instance.HW_Port;
            HttpNet = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, webport);
            HttpNet.ReceiveFromPeer += HttpServer_ReceiveFromPeer;
            return true;
        }

        private void HttpServer_ReceiveFromPeer(byte[] buffer, long offset, long size, HttpServer.RequestEventArgs e = null)
        {
            lock (lockObj)
            {
                try
                {
                    bResponseSuccess = false;

                    JObject json = JObject.Parse(SysConfig.Instance.HomeNet_Encoding.GetString(buffer[..(int)size]));
                    string urlData = e.Request.Uri.PathAndQuery;

                    Log.WriteLog(LogType.Info, $"NPAutoBoothAdapter | MyHttpNetwork_ReceiveFromPeer", $"URL : {urlData}", LogAdpType.Nexpa);

                    switch (urlData)
                    {
                        case GET_IONDATA: //미등록 일반차량 출입 조회
                            {
                                RequestSearchIONPayload dataPayload = new RequestSearchIONPayload();
                                dataPayload.car_number = Helper.NVL(json["Carno"]);
                                dataPayload.start_date_time = Helper.NVL(json["StartDatetime"]).ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss");
                                dataPayload.end_date_time = Helper.NVL(json["EndDatetime"]).ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss");

                                RequestPayload<RequestSearchIONPayload> payload = new RequestPayload<RequestSearchIONPayload>();
                                payload.command = CmdType.ion_list;
                                payload.data = dataPayload;
                                TargetAdapter.SendMessage(payload);
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
                                TargetAdapter.SendMessage(payload);
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
                                TargetAdapter.SendMessage(payload);
                            }
                            break;
                        case GET_CUSTINFO: //정기차량 세대 목록
                            {
                                RequestCustListPayload dataPayload = new RequestCustListPayload();
                                dataPayload.car_number = Helper.NVL(json["Carno"]);
                                dataPayload.dong = Helper.NVL(json["Dong"]);
                                dataPayload.ho = Helper.NVL(json["Ho"]);

                                RequestPayload<RequestCustListPayload> payload = new RequestPayload<RequestCustListPayload>();
                                payload.command = CmdType.cust_list;
                                payload.data = dataPayload;
                                TargetAdapter.SendMessage(payload);
                            }
                            break;
                        case GET_ALL_CUSTINFO: //정기차량 전체 목록
                            {
                                RequestEmptyPayload payload = new RequestEmptyPayload();
                                payload.command = CmdType.cust_list;
                                TargetAdapter.SendMessage(payload);
                            }
                            break;
                        case GET_IOSDATA: //정기차량 출입조회
                            {
                                RequestSearchIONPayload dataPayload = new RequestSearchIONPayload();
                                dataPayload.car_number = Helper.NVL(json["Carno"]);
                                dataPayload.start_date_time = Helper.NVL(json["StartDatetime"]).ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss");
                                dataPayload.end_date_time = Helper.NVL(json["EndDatetime"]).ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "yyyyMMddHHmmss");

                                RequestPayload<RequestSearchIONPayload> payload = new RequestPayload<RequestSearchIONPayload>();
                                payload.command = CmdType.ios_list;
                                payload.data = dataPayload;
                                TargetAdapter.SendMessage(payload);
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
                                TargetAdapter.SendMessage(payload);
                            }
                            break;
                        case GET_RESERVECAR: //방문신청차량 목록
                            {
                                RequestVisitSingleListPayload dataPayload = new RequestVisitSingleListPayload();
                                dataPayload.car_number = Helper.NVL(json["Carno"]);
                                dataPayload.dong = Helper.NVL(json["Dong"]);
                                dataPayload.ho = Helper.NVL(json["Ho"]);

                                RequestPayload<RequestVisitSingleListPayload> payload = new RequestPayload<RequestVisitSingleListPayload>();
                                payload.command = CmdType.visit_single_list;
                                payload.data = dataPayload;
                                TargetAdapter.SendMessage(payload);
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
                                TargetAdapter.SendMessage(payload);
                            }
                            break;
                        case GET_IORESERVE: //방문신청차량 출입 조회
                            {
                                RequestVisitSingleIOPayload dataPayload = new RequestVisitSingleIOPayload();
                                dataPayload.dong = Helper.NVL(json["Dong"]);
                                dataPayload.ho = Helper.NVL(json["Ho"]);
                                dataPayload.car_number = Helper.NVL(json["Carno"]);

                                RequestPayload<RequestVisitSingleIOPayload> payload = new RequestPayload<RequestVisitSingleIOPayload>();
                                payload.command = CmdType.visit_single_io;
                                payload.data = dataPayload;
                                TargetAdapter.SendMessage(payload);
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
                                byte[] result = payload.Serialize();
                                e.Response.Body.Write(result, 0, result.Length);
                                return;
                            }
                    }

                    //3초 대기 Task
                    int iSec = 5 * 100; //3초
                    while (iSec > 0 && !bResponseSuccess)
                    {
                        Thread.Sleep(10); //0.01초씩..쉰다...
                        iSec -= 1;
                    }

                    if (bResponseSuccess) //응답성공
                    {
                        byte[] result = responsePayload.Serialize();
                        e.Response.Body.Write(result, 0, result.Length);
                    }
                    else
                    {
                        MvlResponsePayload payload = new MvlResponsePayload();
                        payload.resultCode = MvlResponsePayload.SttCode.NotSupportedMethod;
                        payload.resultMessage = "지원하지 않는 http 메소드 입니다";
                        byte[] result = payload.Serialize();
                        e.Response.Body.Write(result, 0, result.Length);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public void SendMessage(IPayload payload)
        {
            //Do Nothing
        }

        public void SendMessage(byte[] buffer, long offset, long size)
        {
            receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
            var jobj = JObject.Parse(receiveMessageBuffer.ToString());
            Thread.Sleep(10);
            receiveMessageBuffer.Clear();

            Log.WriteLog(LogType.Info, $"AptStAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
            JObject data = jobj["data"] as JObject; //응답 데이터

            //결과 Payload 생성 =======
            JObject result = jobj["result"] as JObject; //응답 결과
            ResultPayload resultPayload = result.GetResultPayload();

            if(resultPayload.code == "200")
            {
                string cmd = jobj["command"].ToString();
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
                                payload.enrollType = MvlSingleCustInfoPayload.EnrollType.New;
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
                                        dataPayload.carNo = Helper.NVL(item["car_number"]);
                                        dataPayload.dong = Helper.NVL(item["dong"]);
                                        dataPayload.ho = Helper.NVL(item["ho"]);
                                        dataPayload.name = Helper.NVL(item["name"]);
                                        dataPayload.contact = Helper.NVL(item["tel_number"]);
                                        dataPayload.remark = Helper.NVL(item["remark"]);
                                        dataPayload.effStart = Helper.NVL(item["start_date"]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                        dataPayload.effEnd = Helper.NVL(item["end_date"]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                        dataPayload.chkUse = 0;
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
        }

        public bool StartAdapter()
        {

            isRun = true;
            return isRun;
        }

        public bool StopAdapter()
        {

            isRun = false; 
            return isRun;
        }

        public void TestReceive(byte[] buffer)
        {
        }
    }
}
