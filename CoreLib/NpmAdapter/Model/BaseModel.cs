using NpmCommon;
using NpmDA.DataAccess;
using System.Data;

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
    }
}
