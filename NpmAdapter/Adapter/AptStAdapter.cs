using HttpServer;
using HttpServer.Headers;
using Newtonsoft.Json.Linq;
using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.Network;
using NLog.Targets;
using NpmAdapter.Payload;
using NpmAdapter.Payload.CommaxDaelim.Response;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 아파트스토리 아답터
    /// </summary>
    class AptStAdapter : IAdapter
    {
        private string hostDomain = "";
        private static string APT_INCAR_POST = "/parking/enterance-vehicles";
        private static string APT_OUTCAR_POST = "/parking/leaving-vehicles";
        private static string REQ_POST_STATUS = "/nxmdl/cmx";
        private bool isRun;
        private string webport = "42141";
        private string aptId = "";
        private object lockObj = new object();
        private ResponseCmdPayload responsePayload = null;
        private SyncResonseWait ResonseWait = null;
        private StringBuilder receiveMessageBuffer = new StringBuilder();

        private INetwork MyHttpNetwork { get; set; }

        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning { get => isRun; }

        public void Dispose()
        {
            
        }

        public bool Initialize()
        {
            if (!SysConfig.Instance.ValidateConfig)
            {
                Log.WriteLog(LogType.Error, "AptStAdapter | Initialize", $"Config Version이 다릅니다. 프로그램버전 : {SysConfig.Instance.ConfigVersion}", LogAdpType.HomeNet);
                return false;
            }

            aptId = SysConfig.Instance.AptId;
            hostDomain = SysConfig.Instance.HW_Domain;
            webport = SysConfig.Instance.HW_Port;
            MyHttpNetwork = NetworkFactory.GetInstance().MakeNetworkControl(NetworkFactory.Adapters.HttpServer, webport);

            ResonseWait = new SyncResonseWait();
            responsePayload = new ResponseCmdPayload();

            return true;
        }

        public void SendMessage(byte[] buffer, long offset, long size)
        {
            try
            {
                if (ResonseWait.bResponseSuccess) return;

                receiveMessageBuffer.Append(buffer.ToString(SysConfig.Instance.Nexpa_Encoding, size));
                var jobj = JObject.Parse(receiveMessageBuffer.ToString());
                Thread.Sleep(10);
                receiveMessageBuffer.Clear();

                Log.WriteLog(LogType.Info, $"CmxDLAdapter | SendMessage", $"넥스파에서 받은 메시지 : {jobj}", LogAdpType.HomeNet);
                JObject data = jobj["data"] as JObject;
                string cmd = jobj["command"].ToString();
                switch ((CmdType)Enum.Parse(typeof(CmdType), cmd))
                {
                    //입/출차 통보
                    case CmdType.alert_incar:
                    case CmdType.alert_outcar:
                        {
                            RequestPayload<AlertInOutCarPayload> payload = new RequestPayload<AlertInOutCarPayload>();
                            payload.Deserialize(jobj);
                            
                            //동호가 없으면 PASS시킨다.
                            if (payload.data.dong == null || payload.data.ho == null || payload.data.dong == "" || payload.data.ho == "")
                            {
                                ResponseResultPayload resultPayload = new ResponseResultPayload();
                                resultPayload.command = payload.command;
                                resultPayload.Result = ResponseResultPayload.Status.FailFormatError;
                                byte[] result = resultPayload.Serialize();
                                TargetAdapter.SendMessage(result, 0, result.Length);
                                Log.WriteLog(LogType.Info, $"AptStAdapter | SendMessage", $"전송메시지 : {resultPayload.ToJson().ToString()}", LogAdpType.Nexpa);
                            }
                            else
                            {
                                Uri uri = null;
                                byte[] requestData;

                                if (payload.command == CmdType.alert_incar)
                                {
                                    uri = new Uri(hostDomain + APT_INCAR_POST);

                                    RequestApsInCarPayload inCarPayload = new RequestApsInCarPayload();
                                    inCarPayload.apt_id = aptId;
                                    inCarPayload.car_number = payload.data.car_number;
                                    inCarPayload.dong = payload.data.dong;
                                    inCarPayload.ho = payload.data.ho;
                                    inCarPayload.parking_in_datetime = payload.data.date_time.ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                    requestData = inCarPayload.Serialize();
                                }
                                else
                                {
                                    uri = new Uri(hostDomain + APT_OUTCAR_POST);

                                    RequestApsOutCarPayload outCarPayload = new RequestApsOutCarPayload();
                                    outCarPayload.apt_id = aptId;
                                    outCarPayload.car_number = payload.data.car_number;
                                    outCarPayload.parking_in_datetime = payload.data.date_time.ConvertDateTimeFormat("yyyyMMddHHmmss", "yyyy-MM-dd HH:mm:ss");
                                    requestData = outCarPayload.Serialize();
                                }

                                string responseData = string.Empty;
                                if (NetworkWebClient.Instance.SendData(uri, NetworkWebClient.RequestType.POST, ContentType.Json, requestData, ref responseData))
                                {
                                    try
                                    {
                                        Log.WriteLog(LogType.Info, "AptStAdapter | SendMessage | WebClientResponse", $"==응답== {responseData}", LogAdpType.Nexpa);
                                        ResponseResultPayload responsePayload = new ResponseResultPayload();
                                        byte[] responseBuffer;

                                        var responseJobj = JObject.Parse(responseData);
                                        if (Helper.NVL(responseJobj["code"]) == "0")
                                        {
                                            responsePayload.command = payload.command;
                                            responsePayload.Result = ResponseResultPayload.Status.OK;
                                            responseBuffer = responsePayload.Serialize();
                                        }
                                        else
                                        {
                                            responsePayload.command = payload.command;
                                            responsePayload.Result = ResponseResultPayload.Status.ExceptionERROR;
                                            responseBuffer = responsePayload.Serialize();
                                        }

                                        TargetAdapter.SendMessage(responseBuffer, 0, responseBuffer.Length);
                                    }
                                    catch (Exception ex)
                                    {
                                        Log.WriteLog(LogType.Error, "NexpaTcpAdapter | RequestUDO_LocationMap", $"{ex.Message}", LogAdpType.Nexpa);
                                    }
                                }
                            }
                        }
                        break;
                    #region 방문자 응답 처리

                    case CmdType.visit_list: //방문자 리스트 응답
                        {
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdListData<ResponseCmdVisitListData> dataPayload = new ResponseCmdListData<ResponseCmdVisitListData>();
                                dataPayload.page = data["page"].ToString();
                                dataPayload.count = data["list_count"].ToString();
                                int nPage = 0;
                                if (int.TryParse(data["remain_page"].ToString(), out nPage))
                                {
                                    if (nPage > 0) dataPayload.next_page = "y";
                                    else dataPayload.next_page = "n";
                                }
                                else
                                {
                                    dataPayload.next_page = "n";
                                }

                                JArray list = data["list"] as JArray;
                                if (list != null)
                                {
                                    foreach (JObject item in list)
                                    {
                                        ResponseCmdVisitListData visitlst = new ResponseCmdVisitListData();
                                        visitlst.dong = item["dong"]?.ToString();
                                        visitlst.ho = item["ho"]?.ToString();
                                        visitlst.reg_num = item["reg_no"].ToString();
                                        visitlst.car_num = item["car_number"].ToString();
                                        visitlst.reg_date = item["date"].ToString().ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                        visitlst.term = item["term"].ToString();
                                        visitlst.status = item["visit_flag"].ToString() == "y" ? "방문완료" : "미방문";
                                        dataPayload.list.Add(visitlst);
                                    }
                                }
                                responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());
                            }

                            //Response 완료~!
                            ResonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.visit_reg:
                        {
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdRegNumData dataPayload = new ResponseCmdRegNumData();
                                dataPayload.reg_num = data["reg_no"].ToString();
                                responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());
                            }
                            else
                            {
                                responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notinterface_kwanje);
                            }

                            //Response 완료~!
                            ResonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.visit_modify:
                        {
                            ResonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.visit_del:
                        {
                            ResonseWait.bResponseSuccess = true;
                        }
                        break;
                    #endregion

                    case CmdType.blacklist_list:
                        {
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdListData<ResponseCmdBlackListData> dataPayload = new ResponseCmdListData<ResponseCmdBlackListData>();
                                dataPayload.page = data["page"].ToString();
                                dataPayload.count = data["list_count"].ToString();
                                int nPage = 0;
                                if (int.TryParse(data["remain_page"].ToString(), out nPage))
                                {
                                    if (nPage > 0) dataPayload.next_page = "y";
                                    else dataPayload.next_page = "n";
                                }
                                else
                                {
                                    dataPayload.next_page = "n";
                                }

                                JArray list = data["list"] as JArray;
                                if (list != null)
                                {
                                    foreach (JObject item in list)
                                    {
                                        ResponseCmdBlackListData visitlst = new ResponseCmdBlackListData();
                                        visitlst.reg_num = item["reg_no"].ToString();
                                        visitlst.car_num = item["car_number"].ToString();
                                        visitlst.reg_date = item["date"].ToString().ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                                        visitlst.reason = item["reason"].ToString();
                                        dataPayload.list.Add(visitlst);
                                    }
                                }
                                responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());
                            }
                            //Response 완료~!
                            ResonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.blacklist_reg:
                        {
                            if (data != null && data.HasValues)
                            {
                                ResponseCmdRegNumData dataPayload = new ResponseCmdRegNumData();
                                dataPayload.reg_num = data["reg_no"].ToString();
                                responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());
                            }
                            else
                            {
                                responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notinterface_kwanje);
                            }
                            //Response 완료~!
                            ResonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.blacklist_del:
                        {
                            //Response 완료~!
                            ResonseWait.bResponseSuccess = true;
                        }
                        break;
                    case CmdType.blacklist_car:
                        {
                            ResponseCmdBlackListData dataPayload = new ResponseCmdBlackListData();
                            dataPayload.code = data.NPGetValue(NPElements.Code);
                            dataPayload.reg_num = data.NPGetValue(NPElements.Reg_No);
                            dataPayload.car_num = data.NPGetValue(NPElements.Car_Number);
                            dataPayload.reg_date = data.NPGetValue(NPElements.Date).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                            dataPayload.reason = data.NPGetValue(NPElements.Reason);

                            responsePayload.DeserializeData(responsePayload.header.type, dataPayload.ToJson());

                            ResonseWait.bResponseSuccess = true;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"CmxDLAdapter | SendMessage", $"Nexpa Adpater로부터 온 Message를 처리 중 오류 : {ex.Message}", LogAdpType.HomeNet);
                throw;
            }
        }

        public bool StartAdapter()
        {
            try
            {
                MyHttpNetwork.ReceiveFromPeer += MyHttpNetwork_ReceiveFromPeer;
                isRun = MyHttpNetwork.Run();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "AptStAdapter | StartAdapter", $"{ex.Message}", LogAdpType.HomeNet);
            }
            
            return isRun;
        }

        public bool StopAdapter()
        {
            bool bResult = false;
            MyHttpNetwork.ReceiveFromPeer -= MyHttpNetwork_ReceiveFromPeer;
            bResult = MyHttpNetwork.Down();
            isRun = !bResult;
            return bResult;
        }

        private void MyHttpNetwork_ReceiveFromPeer(byte[] buffer, long offset, long size, RequestEventArgs e)
        {
            lock (lockObj)
            {
                responsePayload.Initialize();
                JObject json = JObject.Parse(SysConfig.Instance.HomeNet_Encoding.GetString(buffer));

                //DefaultURL을 만들어야 함.....
                //http://localhost:42142/nxmdl/cmx/... 까지..
                string urlData = e.Request.Uri.PathAndQuery;
                Log.WriteLog(LogType.Info, $"CmxDLAdapter | MyHttpNetwork_ReceiveFromPeer", $"URL : {urlData}", LogAdpType.HomeNet);
                if (urlData != REQ_POST_STATUS)
                {
                    e.Response.Connection.Type = ConnectionType.Close;
                    e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                    e.Response.Status = System.Net.HttpStatusCode.BadRequest;
                    e.Response.Reason = "Bad Request";

                    //에러 메시지를 만들어 대림으로 보낸다.
                    CmdHeader responseHeader = new CmdHeader();
                    responseHeader.Deserialize(json["header"] as JObject);
                    responsePayload.header = responseHeader;
                    responsePayload.data = null;
                    responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.bad_request);
                    byte[] result = responsePayload.Serialize();

                    e.Response.Body.Write(result, 0, result.Length);
                }
                else
                {
                    e.Response.Connection.Type = ConnectionType.Close;
                    e.Response.ContentType = new ContentTypeHeader("application/json;charset=UTF-8");
                    e.Response.Reason = "OK";

                    //IPayload response = RequestToNexpa(json);
                    RequestToNexpa(json);
                    byte[] result = responsePayload.Serialize();
                    //응답을 보낸다.
                    e.Response.Body.Write(result, 0, result.Length);

                }
            }
        }

        /// <summary>
        /// Nexpa로 데이터를 전달하여 응답값을 받아 리턴한다.
        /// </summary>
        private void RequestToNexpa(JObject json)
        {
            // ResponsePayload의 Header를 만든다.
            CmdHeader responseHeader = new CmdHeader();
            responseHeader.Deserialize(json["header"] as JObject);
            responsePayload.header = responseHeader;

            ResonseWait.bResponseSuccess = false;
            //넥스파 Payload를 가져온다.
            CmdHelper.ResultReqInfo result = CmdHelper.ConvertNexpaRequestPayload(json);

            if (result.type != CmdType.none)
            {
                //넥스파로 payload를 보낸다.
                byte[] sendMsg = result.payload.Serialize();
               
                TargetAdapter.SendMessage(sendMsg, 0, sendMsg.Length);

                //3초 대기 Task
                int iSec = 3 * 100; //3초
                while (iSec > 0 && !ResonseWait.bResponseSuccess)
                {
                    Thread.Sleep(10); //0.01초씩..쉰다...
                    iSec -= 1;
                }

                receiveMessageBuffer.Clear();

                if (ResonseWait.bResponseSuccess) //응답성공
                {
                    //응답이 왔으므로.... Data는 채워져 있을거임...
                    if (responsePayload.result == null)
                    {
                        responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.ok);
                    }
                }
                else //응답 실패
                {
                    //에러 Payload를 만들어 보낸다.
                    switch (responsePayload.header.type)
                    {
                        case CmdHelper.Type.visitor_car_book_add:
                        case CmdHelper.Type.visitor_car_book_delete:
                        case CmdHelper.Type.location_nick:
                        case CmdHelper.Type.blacklist_book_list:
                        case CmdHelper.Type.blacklist_book_add:
                        case CmdHelper.Type.blacklist_book_delete:
                        case CmdHelper.Type.visitor_car_book_update:
                            responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notinterface_kwanje);
                            break;
                        case CmdHelper.Type.location_list:
                        case CmdHelper.Type.location_map:
                            responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notinterface_udo);
                            break;
                        default:
                            responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notresponse);
                            break;
                    }
                }
            }
            else //요청 페이로드를 못만들었다.
            {
                //에러 Payload를 만들어 보낸다.
                responsePayload.result = CmdHelper.MakeResponseResultPayload(CmdHelper.StatusCode.notresponse);
            }
        }

        public void TestReceive(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
