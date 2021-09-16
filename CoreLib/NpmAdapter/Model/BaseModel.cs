using NpmCommon;
using NpmDA.DataAccess;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class BaseModel
    {
        protected AbstractDA ConfigDA;
        protected AbstractDA DA;
        protected IDbTransaction Transaction;

        public BaseModel()
        {
            switch (StdHelper.GetValueFromDescription<NexpaDBType>(SysConfig.Instance.DBType))
            {
                case NexpaDBType.MSSQL:
                    DA = DataAccessFactory.Instance.GetDAInstance(NPDBkind.MSSQL);
                    break;
                case NexpaDBType.POSTGRES:
                    DA = DataAccessFactory.Instance.GetDAInstance(NPDBkind.Postgres);
                    break;
            }
        }

        public void WriteLog(string logTarget, string Query, Dictionary<ColName, object> dicParam = null)
        {
            var LogMessage = Query;

            if(dicParam != null)
            {
                foreach (var key in dicParam.Keys)
                {
                    var findVal = $"@{key}";
                    LogMessage = LogMessage.Replace(findVal, $"'{dicParam[key]}'");
                }
            }
            
            Log.WriteLog(LogType.Info, logTarget, LogMessage, LogAdpType.DataBase);
        }
    }

    class QueryString
    {
        private StringBuilder Query;

        public QueryString()
        {
            Query = new StringBuilder();
        }

        public int Length
        {
            get => Query.Length;
        }

        public void Append(string query)
        {
            if(Query.Length == 0)
            {
                Query.Append(query + " ");
            }
            else
            {
                Query.Append("\r\n" + query + " ");
            }
        }

        public void Append조건(string query)
        {
            if (Query.ToString().IndexOf("WHERE ") > 0)
            {
                //Where절이 존재...
                if (query.ToUpper().StartsWith("WHERE")) query = query.Replace("WHERE", "AND");
                else if (!query.ToUpper().StartsWith("AND")) query = query.Insert(0, "AND ");

                Append(query);
            }
            else
            {
                //Where절 없음.
                if (query.ToUpper().StartsWith("AND")) query = query.Replace("AND", "WHERE");
                else if (!query.ToUpper().StartsWith("WHERE")) query = query.Insert(0, "WHERE ");

                Append(query);
            }
        }

        public void Clear()
        {
            Query.Clear();
        }

        public override string ToString()
        {
            return Query.ToString();
        }
    }

    public enum ColName
    {
        Dong,
        Ho,
        CarNo,
        TKNo,
        StartDt,
        EndDt,
        UpdDt,
        ParkNo,
        Name,
        TelNo,
        CompName,
        DeptName,
        Address,
        UnitNo,
        Memo,
        ExpDateF,
        ExpDateT,
        IssueDate,
        IssueTime,
        PageSize,
        PageNumber,
        DateFilterType
    }

    public enum ModelCode
    {
        OK,
        AlreadyRegisted,
        NotFound,
        Fail,
        NotAcceptable,
        Exception
    }

    public struct ModelResult
    {
        public ModelCode code;
        public string Message;
        public string Description;
    }
}
