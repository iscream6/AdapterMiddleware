using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.DataAccess;
using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace NpmStandardLib.DataAccess
{
    class SQLiteDA : AbstractDA
    {
        private string mServer = string.Empty;              // 서버명, 사용안함
        private string mPort = string.Empty;                // 포트, 사용안함
        private string mDatabase = string.Empty;            // 데이터베이스명
        private string mUserID = string.Empty;              // 접속자 아이디, 사용안함
        private string mPassword = string.Empty;			// 접속 패스워드
        private SQLiteConnection _Connection = null;           // 연결 객체
                                                               // 트렌젝션을 사용하기 위함.
        private bool mBeginTrans = false;                   // 트렌잭션 중임.
        private SQLiteCommand mTransCmd = null;               // 명령 객체
        private SQLiteTransaction mTrans = null;               // 트렌잭션 객체
        private bool mWindowsAuthority = false;             // 윈도우즈 인증 사용, 사용안함

        #region ExecuteDataTable

        public override DataTable ExecuteDataTable(string psQuery)
        {
            return ExecuteDataTable(psQuery, false);
        }

        public override DataTable ExecuteDataTable(string psQuery, bool pbThrowException)
        {
            if (!OpenConnection()) return null;

            SQLiteDataAdapter oAdapter;
            SQLiteCommand oCommand;
            DataTable oData;

            oCommand = new SQLiteCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.CommandTimeout = 180;

            oAdapter = new SQLiteDataAdapter(oCommand);
            oData = new DataTable();

            try
            {
                oAdapter.Fill(oData);
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteDataTable", $"{ex.Message}\r\n{psQuery}");
                    return null;
                }
                else
                {
                    throw ex;
                }

            }

            return oData;
        }

        public override DataTable ExecuteDataTable(string psQuery, IDbDataParameter[] poParameters)
        {
            return ExecuteDataTable(psQuery, poParameters, false);
        }

        public override DataTable ExecuteDataTable(string psQuery, IDbDataParameter[] poParameters, bool pbThrowException)
        {
            if (!OpenConnection()) return null;

            SQLiteDataAdapter oAdapter;
            SQLiteCommand oCommand;
            DataTable oData;

            oCommand = new SQLiteCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;

            //Add Parameter
            foreach (var param in poParameters)
            {
                oCommand.Parameters.Add(param);
            }

            oCommand.CommandTimeout = 180;

            //Execute
            oAdapter = new SQLiteDataAdapter(oCommand);
            oData = new DataTable();

            try
            {
                oAdapter.Fill(oData);
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteDataTable", $"{ex.Message}\r\n{psQuery}");
                    return null;
                }
                else
                {
                    throw ex;
                }

            }

            return oData;
        }

        public override DataTable ExecuteDataTable(string psQuery, IDbTransaction poTransaction)
        {
            return ExecuteDataTable(psQuery, poTransaction, false);
        }

        public override DataTable ExecuteDataTable(string psQuery, IDbTransaction poTransaction, bool pbThrowException)
        {
            if (!OpenConnection()) return null;

            SQLiteDataAdapter oAdapter;
            SQLiteCommand oCommand;
            DataTable oData;

            oCommand = new SQLiteCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.Transaction = (SQLiteTransaction)poTransaction;

            oCommand.CommandTimeout = 180;

            //Execute
            oAdapter = new SQLiteDataAdapter(oCommand);
            oData = new DataTable();

            try
            {
                oAdapter.Fill(oData);
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteDataTable", $"{ex.Message}\r\n{psQuery}");
                    return null;
                }
                else
                {
                    throw ex;
                }

            }

            return oData;
        }

        public override DataTable ExecuteDataTable(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction)
        {
            return ExecuteDataTable(psQuery, poParameters, poTransaction, false);
        }

        public override DataTable ExecuteDataTable(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction, bool pbThrowException)
        {
            if (!OpenConnection()) return null;

            SQLiteDataAdapter oAdapter;
            SQLiteCommand oCommand;
            DataTable oData;

            oCommand = new SQLiteCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.Transaction = (SQLiteTransaction)poTransaction;

            //Add Parameter
            foreach (var param in poParameters)
            {
                oCommand.Parameters.Add(param);
            }

            oCommand.CommandTimeout = 180;

            //Execute
            oAdapter = new SQLiteDataAdapter(oCommand);
            oData = new DataTable();

            try
            {
                oAdapter.Fill(oData);
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteDataTable", $"{ex.Message}\r\n{psQuery}");
                    return null;
                }
                else
                {
                    throw ex;
                }

            }

            return oData;
        }

        #endregion ExecuteDataTable

        #region ExecuteNonQuery

        public override int ExecuteNonQuery(string psQuery)
        {
            return ExecuteNonQuery(psQuery, false);
        }

        public override int ExecuteNonQuery(string psQuery, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            SQLiteCommand oCommand = new SQLiteCommand(psQuery, _Connection);

            try
            {
                return oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteNonQuery", $"{ex.Message}\r\n{psQuery}");
                    return -1;
                }
                else
                {
                    throw ex;
                }
            }
        }

        public override int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters)
        {
            return ExecuteNonQuery(psQuery, poParameters, false);
        }

        public override int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            SQLiteCommand oCommand = new SQLiteCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;

            //Add Parameter
            foreach (var param in poParameters)
            {
                oCommand.Parameters.Add(param);
            }

            try
            {
                return oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteNonQuery", $"{ex.Message}\r\n{psQuery}");
                    return -1;
                }
                else
                {
                    throw ex;
                }
            }
        }

        public override int ExecuteNonQuery(string psQuery, IDbTransaction poTransaction)
        {
            return ExecuteNonQuery(psQuery, poTransaction, false);
        }

        public override int ExecuteNonQuery(string psQuery, IDbTransaction poTransaction, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            SQLiteCommand oCommand = new SQLiteCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.Transaction = (SQLiteTransaction)poTransaction;

            try
            {
                return oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteNonQuery", $"{ex.Message}\r\n{psQuery}");
                    return -1;
                }
                else
                {
                    throw ex;
                }
            }
        }

        public override int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction)
        {
            return ExecuteNonQuery(psQuery, poParameters, poTransaction, false);
        }

        public override int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            SQLiteCommand oCommand = new SQLiteCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.Transaction = (SQLiteTransaction)poTransaction;

            //Add Parameter
            foreach (var param in poParameters)
            {
                oCommand.Parameters.Add(param);
            }

            try
            {
                return oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"MSSqlDA| ExecuteNonQuery", $"{ex.Message}\r\n{psQuery}");
                    return -1;
                }
                else
                {
                    throw ex;
                }
            }
        }

        #endregion ExecuteNonQuery

        #region ExecuteStoredProcedure

        public override int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters)
        {
            return ExecuteStoredProcedure(psProcedureName, poParameters, false);
        }

        public override int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            SQLiteCommand oCommand = new SQLiteCommand(psProcedureName, _Connection);
            oCommand.CommandType = CommandType.StoredProcedure;

            //Add Parameter
            foreach (var param in poParameters)
            {
                oCommand.Parameters.Add(param);
            }

            //Execute
            try
            {
                return oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteStoredProcedure", $"{ex.Message}\r\n{psProcedureName}");
                    return -1;
                }
                else
                {
                    throw ex;
                }
            }
        }

        public override int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters, IDbTransaction poTransaction)
        {
            return ExecuteStoredProcedure(psProcedureName, poParameters, poTransaction, false);
        }

        public override int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters, IDbTransaction poTransaction, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            SQLiteCommand oCommand = new SQLiteCommand(psProcedureName, _Connection);
            oCommand.CommandType = CommandType.StoredProcedure;
            oCommand.Transaction = (SQLiteTransaction)poTransaction;

            //Add Parameter
            foreach (var param in poParameters)
            {
                oCommand.Parameters.Add(param);
            }

            //Execute
            try
            {
                return oCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"SQLiteDA| ExecuteStoredProcedure", $"{ex.Message}\r\n{psProcedureName}");
                    return -1;
                }
                else
                {
                    throw ex;
                }
            }
        }

        #endregion ExecuteStoredProcedure

        public override IDbTransaction GetTransaction()
        {
            if (OpenConnection()) return _Connection.BeginTransaction();
            else return null;
        }

        public override bool IsAvaliable()
        {
            return OpenConnection();
        }

        public bool OpenConnection()
        {
            Disconnect();

            if(_Connection == null) _Connection = new SQLiteConnection();

            if (_Connection.State == ConnectionState.Closed)
            {
                StringBuilder sConnection = new StringBuilder();
                sConnection.Append($"Data Source={System.Environment.CurrentDirectory}\\config\\mw_config.db3; ");

                //if (!string.IsNullOrEmpty(SysConfig.Instance.Password))
                //{
                //    sConnection.Append("Password=");
                //    sConnection.Append(SysConfig.Instance.Password);
                //    sConnection.Append("; ");
                //}

                sConnection.Append("FailIfMissing=False; Pooling=False;");

                _Connection.ConnectionString = sConnection.ToString();
            }

            try
            {
                if (!File.Exists(System.Environment.CurrentDirectory + @"\config\mw_config.db3"))
                {
                    SQLiteConnection.CreateFile(System.Environment.CurrentDirectory + @"\config\mw_config.db3");
                }
                _Connection.Open();
            }
            catch (Exception ex)
            {
                Disconnect();
                Log.WriteLog(LogType.Error, $"SQLiteDA| OpenConnection", $"{ex.Message}");
                return false;
            }

            return true;
        }

        public void Disconnect()
        {
            if (_Connection != null)
            {
                _Connection.Close();
                _Connection.Dispose();
                _Connection = null;
            }
        }
    }
}
