using NexpaAdapterStandardLib;
using NexpaAdapterStandardLib.DataAccess;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmStandardLib.DataAccess
{
    class NpgSqlDA : AbstractDA
    {
        private NpgsqlConnection _Connection;

        #region ExecuteDataTable

        public override DataTable ExecuteDataTable(string psQuery)
        {
            return ExecuteDataTable(psQuery, false);
        }

        public override DataTable ExecuteDataTable(string psQuery, bool pbThrowException)
        {
            if (!OpenConnection()) return null;

            NpgsqlDataAdapter oAdapter;
            NpgsqlCommand oCommand;
            DataTable oData;

            oCommand = new NpgsqlCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;

            oCommand.CommandTimeout = 180;

            oAdapter = new NpgsqlDataAdapter(oCommand);
            oData = new DataTable();

            try
            {
                oAdapter.Fill(oData);
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"NpgSqlDA| ExecuteDataTable", $"{ex.Message}\r\n{psQuery}");
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

            NpgsqlDataAdapter oAdapter;
            NpgsqlCommand oCommand;
            DataTable oData;

            oCommand = new NpgsqlCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;

            //Add Parameter
            foreach (var param in poParameters)
            {
                oCommand.Parameters.Add(param);
            }

            oCommand.CommandTimeout = 180;

            //Execute
            oAdapter = new NpgsqlDataAdapter(oCommand);
            oData = new DataTable();

            try
            {
                oAdapter.Fill(oData);
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"MSSqlDA| ExecuteDataTable", $"{ex.Message}\r\n{psQuery}");
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

            NpgsqlDataAdapter oAdapter;
            NpgsqlCommand oCommand;
            DataTable oData;

            oCommand = new NpgsqlCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.Transaction = (NpgsqlTransaction)poTransaction;

            oCommand.CommandTimeout = 180;

            //Execute
            oAdapter = new NpgsqlDataAdapter(oCommand);
            oData = new DataTable();

            try
            {
                oAdapter.Fill(oData);
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"MSSqlDA| ExecuteDataTable", $"{ex.Message}\r\n{psQuery}");
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

            NpgsqlDataAdapter oAdapter;
            NpgsqlCommand oCommand;
            DataTable oData;

            oCommand = new NpgsqlCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.Transaction = (NpgsqlTransaction)poTransaction;

            //Add Parameter
            foreach (var param in poParameters)
            {
                oCommand.Parameters.Add(param);
            }

            oCommand.CommandTimeout = 180;

            //Execute
            oAdapter = new NpgsqlDataAdapter(oCommand);
            oData = new DataTable();

            try
            {
                oAdapter.Fill(oData);
            }
            catch (Exception ex)
            {
                if (!pbThrowException)
                {
                    Log.WriteLog(LogType.Error, $"MSSqlDA| ExecuteDataTable", $"{ex.Message}\r\n{psQuery}");
                    return null;
                }
                else
                {
                    throw ex;
                }

            }

            return oData;
        }

        #endregion

        #region ExecuteNonQuery

        public override int ExecuteNonQuery(string psQuery)
        {
            return ExecuteNonQuery(psQuery, false);
        }

        public override int ExecuteNonQuery(string psQuery, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            NpgsqlCommand oCommand = new NpgsqlCommand(psQuery, _Connection);

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

        public override int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters)
        {
            return ExecuteNonQuery(psQuery, poParameters, false);
        }

        public override int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            NpgsqlCommand oCommand = new NpgsqlCommand(psQuery, _Connection);
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
                    Log.WriteLog(LogType.Error, $"MSSqlDA| ExecuteNonQuery", $"{ex.Message}\r\n{psQuery}");
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

            NpgsqlCommand oCommand = new NpgsqlCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.Transaction = (NpgsqlTransaction)poTransaction;

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

        public override int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction)
        {
            return ExecuteNonQuery(psQuery, poParameters, poTransaction, false);
        }

        public override int ExecuteNonQuery(string psQuery, IDbDataParameter[] poParameters, IDbTransaction poTransaction, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            NpgsqlCommand oCommand = new NpgsqlCommand(psQuery, _Connection);
            oCommand.CommandType = CommandType.Text;
            oCommand.Transaction = (NpgsqlTransaction)poTransaction;

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

        #endregion

        #region ExecuteStoredProcedure

        public override int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters)
        {
            return ExecuteStoredProcedure(psProcedureName, poParameters, false);
        }

        public override int ExecuteStoredProcedure(string psProcedureName, IDbDataParameter[] poParameters, bool pbThrowException)
        {
            if (!OpenConnection()) return -1;

            NpgsqlCommand oCommand = new NpgsqlCommand(psProcedureName, _Connection);
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
                    Log.WriteLog(LogType.Error, $"MSSqlDA| ExecuteStoredProcedure", $"{ex.Message}\r\n{psProcedureName}");
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

            NpgsqlCommand oCommand = new NpgsqlCommand(psProcedureName, _Connection);
            oCommand.CommandType = CommandType.StoredProcedure;
            oCommand.Transaction = (NpgsqlTransaction)poTransaction;

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
                    Log.WriteLog(LogType.Error, $"MSSqlDA| ExecuteStoredProcedure", $"{ex.Message}\r\n{psProcedureName}");
                    return -1;
                }
                else
                {
                    throw ex;
                }
            }
        }

        #endregion

        public override IDbTransaction GetTransaction()
        {
            if (OpenConnection()) return _Connection.BeginTransaction();
            else return null;
        }

        public override bool IsAvaliable()
        {
            return OpenConnection();
        }

        #region Private Methods

        private bool OpenConnection()
        {
            if (_Connection == null) _Connection = new NpgsqlConnection();

            if (_Connection.State == ConnectionState.Closed)
            {
                StringBuilder sConnection = new StringBuilder();
                sConnection.Append("HOST=");
                sConnection.Append(SysConfig.Instance.NpgHost);
                sConnection.Append(";");

                sConnection.Append("PORT=");
                sConnection.Append(SysConfig.Instance.NpgPort);
                sConnection.Append(";");

                sConnection.Append("USERNAME=");
                sConnection.Append(SysConfig.Instance.UserID);
                sConnection.Append(";");

                sConnection.Append("PASSWORD=");
                sConnection.Append(SysConfig.Instance.Password);
                sConnection.Append(";");

                sConnection.Append("DATABASE=");
                sConnection.Append(SysConfig.Instance.InitialCatalog);

                _Connection.ConnectionString = sConnection.ToString();

                try
                {
                    _Connection.Open();
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, $"NpgSqlDA| OpenConnection", $"{ex.Message}");
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
