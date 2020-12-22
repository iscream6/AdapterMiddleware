using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FadeFox.Database.MSSQL;

namespace Common
{
    public class DBServer
    {
        #region 지역변수


        private bool mForever = true;
        private MSSQL mMSSQL = null;
        public string mDbServerIp = string.Empty;
        public string mDbServerPort = string.Empty;
        private string mDbName = string.Empty;
        private string mdbUserId = string.Empty;
        private string mDbUserPw = string.Empty;
        private bool mIsDbConnected = false;

        #endregion

        #region 생성자
        public DBServer(string pDbServerIp, string pDbServerPort, string pDbName, string pDbUserId, string pDbUserPw)
        {
            mDbServerIp = pDbServerIp;
            mDbServerPort = pDbServerPort;
            mDbName = pDbName;
            mdbUserId = pDbUserId;
            mDbUserPw = pDbUserPw;


        }
        public DBServer()
        {
        }
        #endregion

        #region 프로퍼티

        public MSSQL MSSql
        {
            set { mMSSQL = value; }
            get { return mMSSQL; }
        }
        /// <summary>
        /// db연결상태
        /// </summary>
        public bool IsDbConnected
        {
            get { return mIsDbConnected; }
            set { mIsDbConnected = value; }
        }
        #endregion

        /// <summary>
        /// DB서버 접속정보를 가져와서 접속
        /// </summary>
        public void SetDBServerConnect()
        {
            GetDbAccess();
        }
        #region DB접속관련
        /// <summary>
        /// DB재연결 Thread
        /// </summary>
        private Thread mDbConnectThread = null;
        public SqlConnection mDbConnection = null;
        private SqlConnection GetDbAccess()
        {
            if (mMSSQL != null)
            {
                try
                {
                    mMSSQL.Disconnect();
                    mMSSQL = null;
                }
                catch
                {
                }
            }
            try
            {
                mMSSQL = new MSSQL();
                if (mDbServerIp.Trim().Equals(string.Empty) || mDbServerPort.Trim().Equals(string.Empty)
                    || mDbName.Trim().Equals(string.Empty) || mdbUserId.Trim().Equals(string.Empty)
                    || mDbUserPw.Trim().Equals(string.Empty))
                {
                    mIsDbConnected = false;
                    return null;
                }
                else
                {
                    mMSSQL.Server = mDbServerIp;
                    mMSSQL.Port = mDbServerPort;
                    mMSSQL.Database = mDbName;
                    mMSSQL.UserID = mdbUserId;
                    mMSSQL.Password = mDbUserPw;
                    bool l_isConnect = mMSSQL.Connect();
                    mIsDbConnected = l_isConnect;
                    return mDbConnection = mMSSQL.GetConnection;
                }
            }
            catch
            {
                mIsDbConnected = false;
                return null;
            }
        }
        /// <summary>
        /// db 재연결 쓰래드를 생성한다
        /// </summary>
        public void StartDbThread()
        {
            mDbConnectThread = new Thread(DBReConnect);
            mDbConnectThread.IsBackground = true;
            mDbConnectThread.Start();
        }
        /// <summary>
        /// db 재연결 쓰래드를 종료한다
        /// </summary>
        public void EndDbThread()
        {
            mForever = false;
            try
            {
                mDbConnectThread.Abort();
                mMSSQL.Disconnect();
            }
            catch (Exception ex)
            {
            }
        }
        private bool SQLServerConnect()
        {
            try
            {
                if (!mIsDbConnected)
                {
                    mMSSQL.Server = mDbServerIp;
                    mMSSQL.Port = mDbServerPort;
                    mMSSQL.Database = mDbName;
                    mMSSQL.UserID = mdbUserId;
                    mMSSQL.Password = mDbUserPw;

                    bool l_connected = mMSSQL.Connect();
                    mIsDbConnected = l_connected;

                    return l_connected;
                }
                return true;
            }
            catch (Exception ex)
            {
                mIsDbConnected = false;
                return false;
            }
        }

        private void DBReConnect()
        {
            while (mForever)
            {
                SQLServerConnect();
                Thread.Sleep(10000);
            }
        }



        #endregion
    }
}
