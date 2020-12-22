using NexpaAdapterStandardLib.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class BaseModel
    {
        protected AbstractDA DA;
        protected IDbTransaction Transaction;

        public BaseModel()
        {
            DA = DataAccessFactory.Instance.GetDAInstance(NPDBkind.MSSQL);
        }
    }
}
