using System;
using System.Text;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Threading;

namespace Common
{
    public class NPDB
    {
        #region 변수설정

        public enum DBConnectState { Disconnected = 0, Connected = 1, }

        private String sServer = String.Empty;
        private String sConStr = String.Empty;
        private DBConnectState ConnectDB;

        private SqlConnection ConnSQLServer = null;
        private NPLog oLog = new NPLog();
        #endregion


        public DBConnectState ConnectSQLServer()
        {
            #region 데이터베이스 연결

            NPShare Share = new NPShare();
            sServer = Share.GetAppValue("Server");
            sConStr = String.Format("Server={0}; Database={1}; uid={2}; pwd={3}; Connection Timeout={4};", sServer
                                                                                                         , Share.GetAppValue("Database")
                                                                                                         , Share.GetAppValue("UserID")
                                                                                                         , Share.GetAppValue("Password")
                                                                                                         , Share.GetAppValue("ConnectionTimeout")
                                                                                                         );
            try
            {
                ConnSQLServer = new SqlConnection(sConStr);
                ConnSQLServer.Open();

                ConnectDB = DBConnectState.Connected;
                return ConnectDB;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error : " + ex.Message);
                ConnectDB = DBConnectState.Disconnected;
                return ConnectDB;
            }

            #endregion
        }

        public DBConnectState ConnectSQLServerSet()
        {
            #region 데이터베이스 연결
            sServer = NPManager.Properties.Settings.Default.Server.ToString();
            sConStr = String.Format("Server={0}; Database={1}; uid={2}; pwd={3}; Connection Timeout={4}"
                                   , sServer
                                   , NPManager.Properties.Settings.Default.Database.ToString()
                                   , NPManager.Properties.Settings.Default.UserID.ToString()
                                   , NPManager.Properties.Settings.Default.Password.ToString()
                                   , NPManager.Properties.Settings.Default.ConnectionTimeout
                                   );
            try
            {
                ConnSQLServer = new SqlConnection(sConStr);
                ConnSQLServer.Open();

                ConnectDB = DBConnectState.Connected;
                return ConnectDB;
            }
            catch (Exception ex)
            {
                ConnectDB = DBConnectState.Disconnected;
                return ConnectDB;
            }

            #endregion
        }

        public void DisconnectSQLServer()
        {
            #region 데이터베이스 연결 해제
            if (GetDBConnectState == DBConnectState.Connected)
            {
                ConnSQLServer.Close();
                ConnSQLServer = null;
            }
            ConnectDB = DBConnectState.Disconnected;
            #endregion
        }


        #region 연결문자열을 얻음
        public String GetConnectString { get { return sConStr; } }
        #endregion

        #region 데이터베이승 접속된 상태를 얻음
        public DBConnectState GetDBConnectState { get { return ConnectDB; } set { ConnectDB = value; } }
        #endregion


        #region 콘넥션 개체를 얻음
        public SqlConnection GetCurrentConnection { get { return ConnSQLServer; } }
        #endregion


        #region 서버명을 얻음
        public string ServerName { get { return GetDBConnectState == DBConnectState.Connected ? sServer : ""; } }
        #endregion





        public Exception RunDMLExcute(StringBuilder sSQL)
        {
            try
            {
                SqlCommand sCmd = new SqlCommand();
                sCmd.Connection = GetCurrentConnection;
                sCmd.CommandText = sSQL.ToString();
                sCmd.CommandTimeout = 30;
                int nResult = sCmd.ExecuteNonQuery();

                Exception ex = new Exception();
                if (nResult == 0)
                {
                    ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_NoEffect;
                }
                else
                {
                    ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_NoError;
                }

                return ex;
            }
            catch (Exception ex)
            {
                ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_Error;
                return ex;
            }
        }

        public Exception RunDMLExcute(String sSQL)
        {
            try
            {
                SqlCommand sCmd = new SqlCommand();
                sCmd.Connection = GetCurrentConnection;
                sCmd.CommandText = sSQL;
                sCmd.CommandTimeout = 30;
                int nResult = sCmd.ExecuteNonQuery();

                Exception ex = new Exception();
                if (nResult == 0)
                {
                    ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_NoEffect;
                }
                else
                {
                    ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_NoError;
                }

                return ex;
            }
            catch (Exception ex)
            {
                ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_Error;
                return ex;
            }
        }
        public class Result
        {
            public Result(bool pIsSuccess, string pMessage)
            {
                mIsSuccescs = pIsSuccess;
                mErrorMessage = pMessage;
            }
            private bool mIsSuccescs = false;
            private string mErrorMessage = string.Empty;
            public bool IsSuccess
            {
                get { return mIsSuccescs; }
            }
            public string ErrorMessage
            {
                get { return mErrorMessage; }
            }
        }
        public Result RunDMLExcuteHeeJu(String sSQL)
        {
            try
            {
                SqlCommand sCmd = new SqlCommand();
                sCmd.Connection = GetCurrentConnection;
                sCmd.CommandText = sSQL;
                sCmd.CommandTimeout = 30;
                sCmd.ExecuteNonQuery();
                return new Result(true, "성공");
            }
            catch (Exception ex)
            {

                return new Result(false, ex.ToString());

            }
        }

        public Exception RunDMLExcute(StringBuilder sSQL, int Timeout)
        {
            try
            {
                SqlCommand sCmd = new SqlCommand();
                sCmd.Connection = GetCurrentConnection;
                sCmd.CommandText = sSQL.ToString();
                sCmd.CommandTimeout = Timeout;
                int nResult = sCmd.ExecuteNonQuery();

                Exception ex = new Exception();
                if (nResult == 0)
                {
                    ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_NoEffect;
                }
                else
                {
                    ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_NoError;
                }

                return ex;
            }
            catch (Exception ex)
            {
                ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_Error;
                return ex;
            }
        }

        public Exception RunDMLExcute(String sSQL, int Timeout)
        {
            try
            {
                SqlCommand sCmd = new SqlCommand();
                sCmd.Connection = GetCurrentConnection;
                sCmd.CommandText = sSQL;
                sCmd.CommandTimeout = Timeout;
                int nResult = sCmd.ExecuteNonQuery();

                Exception ex = new Exception();
                if (nResult == 0)
                {
                    ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_NoEffect;
                }
                else
                {
                    ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_NoError;
                }

                return ex;
            }
            catch (Exception ex)
            {
                ex.Data[NPDefine.DEF_Result] = NPDefine.DEF_Error;
                return ex;
            }
        }

        public void Sleep(int nWait)
        {
            Thread.Sleep(nWait);
        }
    }
}
