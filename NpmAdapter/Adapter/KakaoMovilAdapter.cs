using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NpmAdapter.Payload;
using System;

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

                            }
                            break;
                        case DEL_RESERVECAR: //방문신청차량 삭제
                            {

                            }
                            break;
                        case GET_IORESERVE: //방문신청차량 출입 조회
                            {

                            }
                            break;
                        default:
                            //e.Response.Connection.Type = ConnectionType.Close;
                            //e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                            //e.Response.Status = System.Net.HttpStatusCode.BadRequest;
                            //e.Response.Reason = "Bad Request";

                            //ResponsePayload resultPayload = new ResponsePayload();
                            //resultPayload.command = cmdType;
                            //resultPayload.result = ResultType.InvalidURL;
                            //byte[] result = resultPayload.Serialize();

                            //e.Response.Body.Write(result, 0, result.Length);
                            break;
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public void SendMessage(IPayload payload)
        {

        }

        public void SendMessage(byte[] buffer, long offset, long size)
        {

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
