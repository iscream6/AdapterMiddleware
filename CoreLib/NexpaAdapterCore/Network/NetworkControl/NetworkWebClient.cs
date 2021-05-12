using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading;
using System.Runtime;

namespace NexpaAdapterStandardLib.Network
{
    //http://qa.nexpa.co.kr:42142
    public delegate void WebSocketConnected();
    public delegate void WebSocketReceive(string strData);
    public delegate void WebSocketClose();

    public enum ContentType
    {
        Json,
        FormData,
        Multipart_FormData,
        Text,
        Xml
    }

    public class NetworkWebClient : Singleton<NetworkWebClient>
    {
        public static event WebSocketConnected OnWebSocketConnected;
        public static event WebSocketReceive OnWebSocketReceive;
        public static event WebSocketClose OnWebSocketClose;

        private const string ContentTypeMultiFormData = "multipart/form-data";
        private const string ContentTypeFormData = "application/x-www-form-urlencoded;charset=UTF-8";
        private const string ContentTypeJson = "application/json;charset=UTF-8";
        private const string ContentTypeText = "text/x-json;charset=UTF-8";
        private const string ContentTypeXml = "text/xml;charset=UTF-8";
        public enum RequestType
        {
            GET,
            POST,
            PUT,
            DELETE
        }

        public bool SendData(Uri uri, RequestType requestType, ContentType contentType, byte[] sendData, ref string strData, ref string strHeader, Dictionary<string, string> header = null, Dictionary<string, string> content = null)
        {
            bool bResult = true;

            strData = string.Empty;

            try
            {
                Log.WriteLog(LogType.Info, "NetworkWebClient| SendData", $"Request Type : {requestType}, Request URI : {uri}");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = requestType.ToString();

                //ContentType 설정 ===============
                StringBuilder sContentType = new StringBuilder();
                if (contentType == ContentType.FormData)
                {
                    sContentType.Append(ContentTypeFormData);
                }
                else if (contentType == ContentType.Json)
                {
                    sContentType.Append(ContentTypeJson);
                }
                else if (contentType == ContentType.Multipart_FormData)
                {
                    sContentType.Append(ContentTypeMultiFormData);
                }
                else if (contentType == ContentType.Text)
                {
                    sContentType.Append(ContentTypeText);
                }
                else if(contentType == ContentType.Xml)
                {
                    sContentType.Append(ContentTypeXml);
                }
                else
                {
                    sContentType.Append(ContentTypeJson);
                }

                if(content != null)
                {
                    foreach (var item in content)
                    {
                        sContentType.Append($"; {item.Key}={item.Value}");
                    }
                }

                if (header != null)
                {
                    foreach (var item in header)
                    {
                        request.Headers.Add(item.Key, item.Value);
                    }
                }

                //ContentType 설정 ===============
                request.ContentType = sContentType.ToString();
                request.ContentLength = (long)sendData.Length;

                if (request.ContentLength != 0)
                {
                    using (Stream requestStream = request.GetRequestStream())
                    {
                        requestStream.Write(sendData, 0, Convert.ToInt32(request.ContentLength));
                    }
                }

                // Response 처리
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response != null)
                    {
                        //Header를 Key, Value로 Json 형태로 만들어준다.
                        JObject jHeader = new JObject();
                        for (int i = 0; i < response.Headers.Count; i++)
                        {
                            jHeader[response.Headers.Keys[i].ToString()] = response.Headers[i].ToString();
                        }

                        if (jHeader.HasValues)
                        {
                            strHeader = jHeader.ToString();
                        }

                        switch (response.StatusCode)
                        {
                            case HttpStatusCode.Created:
                            case HttpStatusCode.OK:
                                using (Stream respStream = response.GetResponseStream())
                                using (StreamReader sr = new StreamReader(respStream))
                                {
                                    int nCount = 0;
                                    while (strData == string.Empty)
                                    {
                                        strData = sr.ReadToEnd();

                                        if (nCount > 20)
                                        {
                                            bResult = false;
                                            break;
                                        }
                                        nCount++;
                                        Thread.Sleep(100);
                                    }

                                    if (bResult)
                                    {
                                        //성공                            
                                        Log.WriteLog(LogType.Info, "NetworkWebClient| SendData", $"응답성공 : {strData}", LogAdpType.Nexpa);
                                    }
                                    else
                                    {
                                        //실패
                                        Log.WriteLog(LogType.Info, "NetworkWebClient| SendData", $"응답실패 : {strData}");
                                    }
                                }

                                break;
                            default:
                                //에러
                                Log.WriteLog(LogType.Info, "NetworkWebClient| SendDataPost", $"response is null");
                                strData = $"ERR,{(int)response.StatusCode},{response.StatusCode.ToString()}";
                                break;
                        }
                    }
                    else
                    {
                            
                    }
                        
                }
            }
            catch (WebException exWeb)
            {
                Log.WriteLog(LogType.Error, "NetworkWebClient| SendDataPost", $"{exWeb.ToString()}");

                //strData
                using (HttpWebResponse response = (HttpWebResponse)exWeb.Response)
                {
                    if (response == null) return false;

                    using (Stream resGetStream = response.GetResponseStream())
                    using (StreamReader resGetRead = new StreamReader(resGetStream, Encoding.UTF8, true))
                    {
                        strData = $"ERR,{(int)response.StatusCode},{response.StatusCode.ToString()},{resGetRead.ReadToEnd()}";
                    }

                    if (strData != null && strData != "")
                        bResult = true;
                    else
                        bResult = false;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "NetworkWebClient| SendData", $"{ex.Message}");
                bResult = false;
                OnWebSocketClose?.Invoke();
            }

            return bResult;
        }


        public bool SendDataPost(Uri uri, byte[] sendData, ref string strJsonData, ContentType contentType, Dictionary<string, string> content = null)
        {
            bool bResult = true;
            HttpWebResponse response = null;
            StreamReader resGetRead = null;
            Stream resGetStream = null;

            strJsonData = string.Empty;

            try
            {
                Log.WriteLog(LogType.Info, "NetworkWebClient| SendDataPost", $"Request URI : {uri}");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.Method = "POST";
                //request.ContinueTimeout = 3000;

                //ContentType 설정 ===============
                StringBuilder sContentType = new StringBuilder();
                if (contentType == ContentType.FormData)
                {
                    sContentType.Append(ContentTypeFormData);
                }
                else if (contentType == ContentType.Json)
                {
                    sContentType.Append(ContentTypeJson);
                }
                else if (contentType == ContentType.Multipart_FormData)
                {
                    sContentType.Append(ContentTypeMultiFormData);
                }
                else
                {
                    sContentType.Append(ContentTypeJson);
                }

                if (content != null)
                {
                    foreach (var item in content)
                    {
                        sContentType.Append($"; {item.Key}={item.Value}");
                    }
                }

                //ContentType 설정 ===============

                request.ContentType = sContentType.ToString();
                request.ContentLength = (long)sendData.Length;

                Stream requestPostStream = request.GetRequestStream();

                requestPostStream.Write(sendData, 0, Convert.ToInt32(request.ContentLength));
                requestPostStream.Close();

                response = (HttpWebResponse)request.GetResponse();

                if (response != null)
                {

                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        Stream resPostStream = response.GetResponseStream();
                        StreamReader resPostReader = new StreamReader(resPostStream, Encoding.UTF8, true);

                        int nCount = 0;
                        while (strJsonData == string.Empty)
                        {
                            strJsonData = resPostReader.ReadToEnd();

                            if (nCount > 20)
                            {
                                bResult = false;
                                break;
                            }
                            nCount++;
                            Thread.Sleep(100);
                        }

                        if (bResult)
                        {
                            //성공                            
                            Log.WriteLog(LogType.Info, "NetworkWebClient| SendDataPost", $"응답성공 : {strJsonData}", LogAdpType.Nexpa);
                        }
                        else
                        {
                            //실패
                            Log.WriteLog(LogType.Info, "NetworkWebClient| SendDataPost", $"응답실패 : {strJsonData}");
                        }

                        resPostReader.Close();
                        resPostStream.Close();
                    }
                    else if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Stream resPostStream = response.GetResponseStream();
                        StreamReader resPostReader = new StreamReader(resPostStream, Encoding.UTF8, true);

                        int nCount = 0;
                        while (strJsonData == string.Empty)
                        {
                            strJsonData = resPostReader.ReadToEnd();
                            Log.WriteLog(LogType.Info, "NetworkWebClient| SendDataPost", $"응답성공 : {strJsonData}", LogAdpType.Nexpa);
                            if (nCount > 20)
                            {
                                bResult = false;
                                break;
                            }
                            nCount++;
                            Thread.Sleep(100);
                        }
                    }
                    else
                    {
                        //에러                        
                        Log.WriteLog(LogType.Info, "NetworkWebClient| SendDataPost", $"response is null : {strJsonData}");
                    }
                    response.Close();
                }
                else
                {
                    //response is null
                }

            }
            catch (WebException exWeb)
            {
                Log.WriteLog(LogType.Error, "NetworkWebClient| SendDataPost", $"{exWeb.ToString()}");
                response = (HttpWebResponse)exWeb.Response;
                if (response == null) return false;
                resGetStream = response.GetResponseStream();

                resGetRead = new StreamReader(resGetStream, Encoding.UTF8, true);

                strJsonData = resGetRead.ReadToEnd();

                resGetRead.Close();
                resGetStream.Close();
                response.Close();

                if (strJsonData != null && strJsonData != "")
                    bResult = true;
                else
                    bResult = false;
            }
            catch (Exception e)
            {
                bResult = false;
                OnWebSocketClose?.Invoke();
            }

            return bResult;
        }
    }
}
