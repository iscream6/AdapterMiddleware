using NexpaAdapterStandardLib.IO.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace NexpaAdapterStandardLib.Network
{
    public delegate void WebSocketConnected();
    public delegate void WebSocketReceive(string strData);
    public delegate void WebSocketClose();

    public class NetworkWebClient : Singleton<NetworkWebClient>
    {
        public static event WebSocketConnected OnWebSocketConnected;
        public static event WebSocketReceive OnWebSocketReceive;
        public static event WebSocketClose OnWebSocketClose;

        public bool SendDataPost(Uri uri, byte[] sendData, ref string strJsonData)
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
                request.ContentType = "application/json;charset=UTF-8";
                request.Headers.Add("appKey", "fcbfb6b7-f5e6-4a50-a506-f3e45c6523d8");
                request.Headers.Add("Authorization", "Basic bmV4cGEtMDA6ZjNlNDVjNjUyM2Q4");
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
