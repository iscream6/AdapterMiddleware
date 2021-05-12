using Npgsql;
using NpgsqlTypes;
using NpmStandardLib.DataAccess;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace NexpaAdapterStandardLib.DataAccess
{
    /// <summary>
    /// Nexpipe 표준 DataBase Type
    /// </summary>
    /// <remarks>
    /// 해당 DBType은 실제 DBType으로 변환되어야 함.
    /// </remarks>
    public enum NPDBType
    {
        Integer,
        Char,
        VarChar,
        DateTime,
        Image,
        Text
    }

    public enum NPDBkind
    {
        MSSQL,
        Oracle,
        Postgres
    }

    public class DataAccessFactory
    {
        private static DataAccessFactory _Factory;
        private AbstractDA _Helper;
        private NPDBkind _Kind;

        private DataAccessFactory() { }

        public static DataAccessFactory Instance
        {
            get
            {
                if (_Factory == null) _Factory = new DataAccessFactory();
                return _Factory;
            }
        }

        /// <summary>
        /// 현재 시스템에서 사용하는 data access 개체를 반환한다.
        /// </summary>
        /// <param name="kind"></param>
        /// <returns></returns>
        public AbstractDA GetDAInstance(NPDBkind kind)
        {
            _Kind = kind;

            if (_Helper == null)
            {
                switch (kind)
                {
                    case NPDBkind.MSSQL:
                        _Helper = new MSSqlDA();
                        break;
                    case NPDBkind.Oracle:
                        break;
                    case NPDBkind.Postgres:
                        _Helper = new NpgSqlDA();
                        break;
                }
            }

            return _Helper;
        }

        /// <summary>
        /// IDbDataParameter 인스턴스 생성
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <returns></returns>
        public IDbDataParameter GetParameter(string parameterName, NPDBType dbType)
        {
            switch (_Kind)
            {
                case NPDBkind.MSSQL:
                    return new SqlParameter(ConvertParameterName(parameterName), ConvertMSSqlDbType(dbType));
                case NPDBkind.Oracle:
                    return null;
                case NPDBkind.Postgres:
                    return new NpgsqlParameter(ConvertParameterName(parameterName), ConvertNpgSqlDbType(dbType));
                default:
                    throw new Exception("알 수 없는 DataBase Type 입니다.");
            }
        }

        /// <summary>
        /// IDbDataParameter 인스턴스 생성
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="dbType"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public IDbDataParameter GetParameter(string parameterName, NPDBType dbType, int size)
        {
            switch (_Kind)
            {
                case NPDBkind.MSSQL:
                    return new SqlParameter(ConvertParameterName(parameterName), ConvertMSSqlDbType(dbType), size);
                case NPDBkind.Oracle:
                    return null;
                case NPDBkind.Postgres:
                    return new NpgsqlParameter(ConvertParameterName(parameterName), ConvertNpgSqlDbType(dbType), size);
                default:
                    throw new Exception("알 수 없는 DataBase Type 입니다.");
            }
        }

        private string ConvertParameterName(string parameterName)
        {
            switch (_Kind)
            {
                case NPDBkind.MSSQL:
                case NPDBkind.Postgres:
                    return $"@{parameterName}";
                case NPDBkind.Oracle:
                    return $":{parameterName}";
                default:
                    return parameterName;
            }
        }

        private OracleDbType ConvertOracleDbType(NPDBType dbType)
        {
            switch (dbType)
            {
                case NPDBType.Integer:
                    return OracleDbType.Int32;
                case NPDBType.Char:
                    return OracleDbType.Char;
                case NPDBType.VarChar:
                    return OracleDbType.Varchar2;
                case NPDBType.DateTime:
                    return OracleDbType.Date;
                case NPDBType.Image:
                    return OracleDbType.Blob;
                case NPDBType.Text:
                    return OracleDbType.Clob;
                default:
                    throw new Exception("알 수 없는 DataBase Type 입니다.");
            }
        }

        private SqlDbType ConvertMSSqlDbType(NPDBType dbType)
        {
            switch (dbType)
            {
                case NPDBType.Integer:
                    return SqlDbType.Int;
                case NPDBType.Char:
                    return SqlDbType.Char;
                case NPDBType.VarChar:
                    return SqlDbType.VarChar;
                case NPDBType.DateTime:
                    return SqlDbType.DateTime;
                case NPDBType.Image:
                    return SqlDbType.Image;
                case NPDBType.Text:
                    return SqlDbType.Text;
                default:
                    throw new Exception("알 수 없는 DataBase Type 입니다.");
            }
        }

        private NpgsqlDbType ConvertNpgSqlDbType(NPDBType dbType)
        {
            switch (dbType)
            {
                case NPDBType.Integer:
                    return NpgsqlDbType.Integer;
                case NPDBType.Char:
                    return NpgsqlDbType.Char;
                case NPDBType.VarChar:
                    return NpgsqlDbType.Varchar;
                case NPDBType.DateTime:
                    return NpgsqlDbType.Date;
                case NPDBType.Image:
                    return NpgsqlDbType.Unknown;
                case NPDBType.Text:
                    return NpgsqlDbType.Text;
                default:
                    throw new Exception("알 수 없는 DataBase Type 입니다.");
            }
        }
    }
}
 