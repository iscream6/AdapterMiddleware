using System;
using NexpaAdapterStandardLib.DataAccess;
using System.Data;
using System.Text;
using System.Collections.Generic;
using NexpaAdapterStandardLib;

namespace NpmAdapter.Model
{
    class GovInterfaceModel : BaseModel
    {
        public bool Save(Dictionary<string, object> dicParam)
        {
            int iRet = 0;
            bool bResult = false;
            this.Transaction = DA.GetTransaction();
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("INSERT INTO GovInterface(TkNo, CarNo, RequestDateTime, InterfaceCode, InterfaceData) \r");
            sQuery.Append("VALUES(@TkNo, @CarNo, @RequestDateTime, @InterfaceCode, @InterfaceData)");

            IDbDataParameter[] oParams = new IDbDataParameter[5];
            oParams[0] = DataAccessFactory.Instance.GetParameter("TkNo", NPDBType.VarChar);
            oParams[0].Value = dicParam["TkNo"];
            oParams[1] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
            oParams[1].Value = dicParam["CarNo"];
            oParams[2] = DataAccessFactory.Instance.GetParameter("RequestDateTime", NPDBType.Char);
            oParams[2].Value = dicParam["RequestDateTime"];
            oParams[3] = DataAccessFactory.Instance.GetParameter("InterfaceCode", NPDBType.VarChar);
            oParams[3].Value = dicParam["InterfaceCode"];
            oParams[4] = DataAccessFactory.Instance.GetParameter("InterfaceData", NPDBType.Text);
            oParams[4].Value = dicParam["InterfaceData"];

            Log.WriteLog(LogType.Info, $"GovInterfaceModel | Save", $"Query : INSERT INTO GovInterface(TkNo, CarNo, RequestDateTime, InterfaceCode, InterfaceData) " +
                $"VALUES('{dicParam["TkNo"]}','{dicParam["CarNo"]}','{dicParam["RequestDateTime"]}','{dicParam["InterfaceCode"]}','{dicParam["InterfaceData"]}')", LogAdpType.HomeNet);

            iRet = DA.ExecuteNonQuery(sQuery.ToString(), oParams, Transaction);

            if(iRet > 0)
            {
                Transaction?.Commit();
                bResult = true;
            }
            else
            {
                Transaction?.Rollback();
                bResult = false;
            }

            return bResult;
        }
    }
}
