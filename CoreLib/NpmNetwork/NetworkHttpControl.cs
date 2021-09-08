//using NexpaAdapterStandardLib.IO.Json;
using Newtonsoft.Json.Linq;
using NpmCommon;
using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace NpmNetwork
{
    class NetworkHttpControl
    {
        private ManualResetEvent mSendLock = new ManualResetEvent(true);
        public struct HEADER_INFO
        {
            public string Uri;

            public string Method;

            public string Authkey;

            public string Contenttype;
        }

        // HEADER
        HEADER_INFO mheader;

        // PROTOCOL
        StringBuilder mDataPacket = new StringBuilder();

        // SEND
        HttpWebRequest mRequest;

        // RECV
        HttpWebResponse mResponse;

        // SEND DATA
        JObject mSendData = new JObject();

        // RECV DATA
        //JToken mParseData;


        string _defaultURL = string.Empty;

        /// <summary>
        /// 생성자
        /// </summary>
        public NetworkHttpControl(string url)
        {
            _defaultURL = url;
        }


        /// <summary>
        /// 초기화
        /// </summary>
        public void Initialize()
        {
            // 헤더
            mheader.Uri = string.Empty;

            mheader.Method = string.Empty;

            mheader.Authkey = string.Empty;

            mheader.Contenttype = string.Empty;

            // 데이터
            mDataPacket.Remove(0, mDataPacket.Length);
        }

        /// <summary>
        /// 헤더 설정 URL,IP,PORT 정보등을 자동으로 잡아줌 전송전에 사용해야함
        /// </summary>
        /// <param name="Header"></param>
        public void SetHeader(UrlCmdType pUrlInfo, params string[] param)
        {
            // 시티벨리 패치예정
            Initialize();
            System.Net.ServicePointManager.Expect100Continue = false;
            //string frontData = string.Empty;
            //frontData = @"http://" + NPSYS.gRESTfulServerIp + ":" + NPSYS.gRESTfulServerPort + "/" + "nxpms/" + NPSYS.gRESTfulVersion;
            switch (pUrlInfo)
            {
                case UrlCmdType.certs:
                    //mheader.Uri = _defaultURL + "/" + "unit/" + NPSYS.ParkCode + "-" + NPSYS.BoothID + "/" + UrlCmdType.certs.ToString();
                    break;
                case UrlCmdType.cars:
                    //mheader.Uri = _defaultURL + "/" + "unit/" + NPSYS.ParkCode + "-" + NPSYS.BoothID + "/" + UrlCmdType.cars.ToString() + "?outchk=0,1&carno=" + pCarNumber + "&return-type=summury";
                    break;
                case UrlCmdType.TimeSearch:
                    //mheader.Uri = _defaultURL + "/" + "unit/" + NPSYS.ParkCode + "-" + NPSYS.BoothID + "/cars?outchk=0,1&startdt=" + pInTime + "&enddt=" + pOutTIme + "&return-type=summury";
                    break;
                case UrlCmdType.payments:
                    //mheader.Uri = _defaultURL + "/" + "unit/" + NPSYS.ParkCode + "-" + NPSYS.BoothID + "/" + "serial/" + pTkNO + "/" + UrlCmdType.payments.ToString();

                    break;
                case UrlCmdType.close:
                    //mheader.Uri = _defaultURL + "/" + UrlCmdType.close.ToString() + "s";

                    break;
                case UrlCmdType.closeInfo:
                    //mheader.Uri = _defaultURL + "/" + "unit/" + NPSYS.ParkCode + "-" + NPSYS.BoothID + "/" + UrlCmdType.close.ToString() + "s";

                    break;
                case UrlCmdType.payment_details:
                case UrlCmdType.season_car_payment_details:
                    //mheader.Uri = _defaultURL + "/" + pUrlInfo.ToString().Replace("_", "-");

                    break;

                case UrlCmdType.discount_tickets:
                    //mheader.Uri = _defaultURL + "/" + pUrlInfo.ToString().Replace("_", "-");

                    break;
                case UrlCmdType.discount_ticketsCANCLE:
                    //mheader.Uri = _defaultURL + "/" + "serial/" + pCarNumber + "/" + pUrlInfo.ToString().Replace("_", "-").Replace("CANCLE", "");
                    break;
                case UrlCmdType.remote_cacnel_payments:
                    //mheader.Uri = _defaultURL + "/" + "payment-details/" + pId.ToString();

                    break;
                case UrlCmdType.status_devices:
                    //mheader.Uri = _defaultURL + "/" + pUrlInfo.ToString().Replace("_", "-");
                    break;
                case UrlCmdType.petty_cashes:
                    //mheader.Uri = _defaultURL + "/" + pUrlInfo.ToString().Replace("_", "-");
                    break;
                case UrlCmdType.interworking_cars:
                    //mheader.Uri = _defaultURL + "/" + "unit/" + NPSYS.ParkCode + "-" + NPSYS.BoothID + "/" + UrlCmdType.interworking_cars.ToString().Replace("_", "-") + "?outchk=0,1&carno=" + pCarNumber + "&return-type=summury";
                    break;
                case UrlCmdType.interworking_payments:
                    //mheader.Uri = _defaultURL + "/" + "unit/" + NPSYS.ParkCode + "-" + NPSYS.BoothID + "/" + "serial/" + pTkNO + "/" + UrlCmdType.interworking_payments.ToString().Replace("_", "-");
                    break;
            }
        }


        public enum UrlCmdType
        {
            /// <summary>
            /// 주차면찾기
            /// </summary>
            parking_spaces,
            /// <summary>
            /// 개국
            /// </summary>
            certs,
            /// <summary>
            /// 차량4자리조회
            /// </summary>
            cars,
            onecar,
            /// <summary>
            /// 원격또는 인식후 
            /// </summary>
            car,
            None,
            /// <summary>
            /// 결제정보 읽어오기
            /// </summary>
            payments,
            /// <summary>
            /// 마감
            /// </summary>
            close,
            /// <summary>
            /// 마감정보확인
            /// </summary>
            closeInfo,
            /// <summary>
            /// 결제액션
            /// </summary>
            payment_details,
            /// <summary>
            /// 정기권연장액션
            /// </summary>
            season_car_payment_details,
            /// <summary>
            /// 할인요청
            /// </summary>
            discount_tickets,
            /// <summary>
            /// 할인취소
            /// </summary>
            discount_ticketsCANCLE,
            remote_cacnel_payments,
            /// <summary>
            /// 에러전송
            /// </summary>
            status_devices,
            /// <summary>
            /// 시제설정
            /// </summary>
            petty_cashes,
            TimeSearch,
            //TMAP연동
            interworking_payments,
            interworking_cars,
            //TMAP연동완료
        }

        public UrlCmdType mCurrentUri = UrlCmdType.None;
        /// <summary>
        /// 전문을 get방식으로 전송
        /// </summary>
        /// <returns></returns>
        public string SendMethodGet()
        {
            // 헤더
            try
            {
                mSendLock.WaitOne(100000);
                mSendLock.Reset();
                System.Net.ServicePointManager.Expect100Continue = false;
                Uri sendUri = new Uri(mheader.Uri);

                mRequest = (HttpWebRequest)WebRequest.Create(sendUri);
                mRequest.Method = "GET";
                mRequest.ContentType = "application/json;charset=UTF-8";

                // 수신
                HttpWebResponse response = null;
                try
                {
                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpControl| SendMethodGet", "데이터 전송시작  URI:" + mheader.Uri);
                    response = (HttpWebResponse)mRequest.GetResponse();
                }
                catch (WebException exWeb)
                {
                    response = (HttpWebResponse)exWeb.Response;
                    Stream RespStream = response.GetResponseStream();
                    StreamReader sr = new StreamReader(RespStream, Encoding.UTF8, true);
                    String RecvData = sr.ReadToEnd();
                    sr.Close();
                    RespStream.Close();



                    int statusCode = (int)response.StatusCode;
                    if (RecvData.Trim() == string.Empty) // 서버접속안됨
                    {
                        //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                        //makestatus.command = "TRANS_ERROR";
                        //makestatus.data = new List<HttpStatusPayload>
                        //{
                        //    new HttpStatusPayload
                        //    {
                        //        code = statusCode.ToString(),
                        //        Success = false,
                        //        description = response.StatusCode.ToString(),
                        //        message = response.StatusCode.ToString()
                        //    }
                        //};

                        //HttpStatusPayload httpStatus = new HttpStatusPayload();
                        //makestatus.status.code = statusCode.ToString();
                        //makestatus.status.Success = false;
                        //makestatus.status.description = response.StatusCode.ToString();
                        //makestatus.status.message = response.StatusCode.ToString();

                        //var sendErrorObject = JObject.FromObject(makestatus);
                        //RecvData = sendErrorObject.ToString();

                    }
                    //TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "NPHttpControl | SendMessagePost", "[응답데이터오류] " + RecvData);

                    return RecvData;

                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream RespStream = response.GetResponseStream();

                    StreamReader sr = new StreamReader(RespStream, Encoding.UTF8, true);

                    String RecvData = sr.ReadToEnd();

                    sr.Close();

                    RespStream.Close();

                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpControl| SendMethodGet", RecvData);

                    return RecvData;
                }
                else
                {

                    //TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "NPHttpControl| SendMethodGet", "[response.StatusCode]" + response.StatusCode.ToString());
                    //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                    //makestatus.command = "TRANS_ERROR";
                    //makestatus.data = new List<HttpStatusPayload>
                    //{
                    //    new HttpStatusPayload
                    //    {
                    //        code = "500",
                    //        Success = false,
                    //        description = "Other Reply",
                    //        message = "Other Reply"
                    //    }
                    //};

                    //var sendErrorObject = JObject.FromObject(makestatus);
                    string RecvData = "";//sendErrorObject.ToString();
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    return RecvData;
                }
            }
            catch (Exception ex)
            {
                //TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "NPHttpControl| SendMethodGet", ex.ToString());
                //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                //makestatus.command = "TRANS_ERROR";
                //makestatus.data = new List<HttpStatusPayload>
                //{
                //    new HttpStatusPayload
                //    {
                //        code = "500",
                //        Success = false,
                //        description = "except error",
                //        message = "except error"
                //    }
                //};

                //var sendErrorObject = JObject.FromObject(makestatus);
                string RecvData = "";/*sendErrorObject.ToString();*/


                return RecvData;


            }
            finally
            {
                mSendLock.Set();
            }
        }


        public string SendMessagePost(string pToken, string pData)
        {
            try
            {
                mSendLock.WaitOne(100000);
                mSendLock.Reset();
                System.Net.ServicePointManager.Expect100Continue = false;
                // 초기화
                Uri sendUri = new Uri(mheader.Uri);
                mRequest = (HttpWebRequest)WebRequest.Create(sendUri);

                if (pToken != string.Empty)
                {
                    mRequest.Headers.Add("Authorization", "Token " + pToken);
                }
                mRequest.Method = "POST";

                mRequest.ContentType = "application/json;charset=UTF-8";

                byte[] sendData = Encoding.UTF8.GetBytes(pData);
                mRequest.ContentLength = (long)sendData.Length;
                Stream requestStream = mRequest.GetRequestStream();
                Log.WriteLog(LogType.Info, $"HttpControlNetwork | SendMessagePost", $"데이터 전송시작 URI : {mRequest.RequestUri}");
                Log.WriteLog(LogType.Info, $"HttpControlNetwork | SendMessagePost", $"데이터 전송시작 전송데이터 : {pData}");
                requestStream.Write(sendData, 0, Convert.ToInt32(mRequest.ContentLength));
                requestStream.Close();

                HttpWebResponse response = null;
                try
                {
                    Log.WriteLog(LogType.Info, $"HttpControlNetwork | SendMessagePost" , "데이터 수신시작");
                    response = (HttpWebResponse)mRequest.GetResponse();
                }
                catch (WebException exWeb)
                {
                    response = (HttpWebResponse)exWeb.Response;

                    Stream RespStream = response.GetResponseStream();

                    StreamReader sr = new StreamReader(RespStream, Encoding.UTF8, true);

                    String RecvData = sr.ReadToEnd();

                    sr.Close();

                    RespStream.Close();

                    int statusCode = (int)response.StatusCode;
                    if (RecvData.Trim() == string.Empty) // 서버접속안됨
                    {
                        //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                        //makestatus.command = "TRANS_ERROR";
                        //makestatus.data = new List<HttpStatusPayload>
                        //{
                        //    new HttpStatusPayload
                        //    {
                        //        code = statusCode.ToString(),
                        //        Success = false,
                        //        description = response.StatusCode.ToString(),
                        //        message = response.StatusCode.ToString()
                        //    }
                        //};

                        //var sendErrorObject = JObject.FromObject(makestatus);
                        RecvData = "";/*sendErrorObject.ToString();*/

                    }
                    //TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "NPHttpControl | SendMessagePost", "[응답데이터오류] " + RecvData);
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    return RecvData;

                }


                if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                {
                    Stream RespStream = response.GetResponseStream();

                    StreamReader sr = new StreamReader(RespStream, Encoding.UTF8, true);

                    String RecvData = sr.ReadToEnd();

                    sr.Close();

                    RespStream.Close();
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpControl| SendMessagePost", RecvData);


                    return RecvData;
                }
                else
                {
                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpControl| SendMessagePost", "[response.StatusCode CREATE가아님]" + response.StatusCode.ToString());
                    //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                    //makestatus.command = "TRANS_ERROR";
                    //makestatus.data = new List<HttpStatusPayload>
                    //{
                    //    new HttpStatusPayload
                    //    {
                    //        code = "500",
                    //        Success = false,
                    //        description = "statuscode not create",
                    //        message = "statuscode not create"
                    //    }
                    //};

                    //var sendErrorObject = JObject.FromObject(makestatus);
                    string RecvData = "";/*sendErrorObject.ToString();*/
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    return RecvData;

                }
            }
            catch (Exception ex)
            {
                //TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "NPHttpControl| SendMessagePost", ex.ToString());
                //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                //makestatus.command = "TRANS_ERROR";
                //makestatus.data = new List<HttpStatusPayload>
                //{
                //    new HttpStatusPayload
                //    {
                //        code = "500",
                //        Success = false,
                //        description = "exception error",
                //        message = "exception error"
                //    }
                //};

                //var sendErrorObject = JObject.FromObject(makestatus);
                string RecvData = "";/*sendErrorObject.ToString();*/


                return RecvData;
            }
            finally
            {


                mSendLock.Set();
            }


        }



        public string SendMessagePut(string pToken, string pData)
        {
            try
            {
                mSendLock.WaitOne(100000);
                mSendLock.Reset();
                System.Net.ServicePointManager.Expect100Continue = false;
                // 초기화
                Uri sendUri = new Uri(mheader.Uri);
                mRequest = (HttpWebRequest)WebRequest.Create(sendUri);
                if (pToken != string.Empty)
                {
                    mRequest.Headers.Add("Authorization", "Token " + pToken);
                }


                mRequest.Method = "PUT";

                mRequest.ContentType = "application/json;charset=UTF-8";


                // 송신


                byte[] sendData = Encoding.UTF8.GetBytes(pData);
                mRequest.ContentLength = (long)sendData.Length;
                Stream requestStream = mRequest.GetRequestStream();
                //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpControl| SendMessagePut", "데이터 전송시작  URI:" + mheader.Uri + " 전송데이터:" + pData);
                requestStream.Write(sendData, 0, Convert.ToInt32(mRequest.ContentLength));
                requestStream.Close();

                HttpWebResponse response = null;
                try
                {
                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpControl| SendMessagePut", "[데이터응답시작]");
                    response = (HttpWebResponse)mRequest.GetResponse();
                }
                catch (WebException exWeb)
                {
                    response = (HttpWebResponse)exWeb.Response;

                    Stream RespStream = response.GetResponseStream();

                    StreamReader sr = new StreamReader(RespStream, Encoding.UTF8, true);

                    String RecvData = sr.ReadToEnd();

                    sr.Close();

                    RespStream.Close();

                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "HTTP| SendMethodGet", RecvData);

                    int statusCode = (int)response.StatusCode;
                    if (RecvData.Trim() == string.Empty) // 서버접속안됨
                    {
                        //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                        //makestatus.command = "TRANS_ERROR";
                        //makestatus.data = new List<HttpStatusPayload>
                        //{
                        //    new HttpStatusPayload
                        //    {
                        //        code = statusCode.ToString(),
                        //        Success = false,
                        //        description = response.StatusCode.ToString(),
                        //        message = response.StatusCode.ToString()
                        //    }
                        //};

                        //var sendErrorObject = JObject.FromObject(makestatus);
                        RecvData = ""; /*sendErrorObject.ToString()*/;

                    }
                    //TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "NPHttpControl | SendMessagePut", "[응답데이터오류] " + RecvData);
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    return RecvData;

                }

                if (response.StatusCode == HttpStatusCode.Created || response.StatusCode == HttpStatusCode.OK)
                {
                    Stream RespStream = response.GetResponseStream();

                    StreamReader sr = new StreamReader(RespStream, Encoding.UTF8, true);

                    String RecvData = sr.ReadToEnd();

                    sr.Close();

                    RespStream.Close();

                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpControl| SendMessagePut", RecvData);

                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    return RecvData;
                }
                else
                {

                    //TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NPHttpControl| SendMessagePut", "[response.StatusCode CREATE가아님]" + response.StatusCode.ToString());
                    //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                    //makestatus.command = "TRANS_ERROR";
                    //makestatus.data = new List<HttpStatusPayload>
                    //{
                    //    new HttpStatusPayload
                    //    {
                    //        code = "500",
                    //        Success = false,
                    //        description = "statuscode가 create가아님",
                    //        message = "statuscode가 create가아님"
                    //    }
                    //};

                    //var sendErrorObject = JObject.FromObject(makestatus);
                    string RecvData = ""; /*sendErrorObject.ToString();*/
                    try
                    {
                        response.Close();
                    }
                    catch
                    {
                    }

                    return RecvData;
                }
            }
            catch (Exception ex)
            {

                //TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "NPHttpControl| SendMessagePut", ex.ToString());
                //Payload<HttpStatusPayload> makestatus = new Payload<HttpStatusPayload>();
                //makestatus.command = "TRANS_ERROR";
                //makestatus.data = new List<HttpStatusPayload>
                //{
                //    new HttpStatusPayload
                //    {
                //        code = "500",
                //        Success = false,
                //        description = "exception error",
                //        message = "exception error"
                //    }
                //};

                //var sendErrorObject = JObject.FromObject(makestatus);
                string RecvData = ""; /*sendErrorObject.ToString();*/
                return RecvData;
            }
            finally
            {
                mSendLock.Set();
            }


        }

        /// <summary>
        /// 응답 상태 코드 취득
        /// </summary>
        /// <returns></returns>
        public HttpStatusCode GetHttpStatusCode()
        {
            return mResponse.StatusCode;
        }


        /// <summary>
        /// 응답 데이터 설정
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        //public void SetRecvData(JToken Token)
        //{
        //    mParseData = Token;
        //}

        ////////////////////////////////////////////////////////

        public void InitData()
        {
            mSendData.RemoveAll();
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void SetData(String Key, int Value)
        {
            mSendData[Key] = Value;
            //Console.WriteLine(mSendData.ToString());
        }

        /// <summary>
        /// 데이터 설정
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        public void SetData(String Key, String Value)
        {
            mSendData[Key] = Value;

            //Console.WriteLine(mSendData.ToString());
        }

        /// <summary>
        /// SHA256 암호화
        /// </summary>
        /// <param name="Param"></param>
        /// <returns></returns>
        private String SHA256_Encrypt(String Param)
        {
            SHA256Managed sha256managed = new SHA256Managed();
            return Convert.ToBase64String(sha256managed.ComputeHash(Encoding.UTF8.GetBytes(Param)));
        }
    }
}
