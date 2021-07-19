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

            StringBuilder sSelectQuery = new StringBuilder();
            sSelectQuery.Append("SELECT InterfaceData \r");
            sSelectQuery.Append("FROM GovInterface \r");
            sSelectQuery.Append("WHERE TkNo = @TkNo \r");
            sSelectQuery.Append("AND CarNo = @CarNo \r");
            sSelectQuery.Append("AND InterfaceCode = @InterfaceCode ");

            IDbDataParameter[] oSelectParams = new IDbDataParameter[3];
            oSelectParams[0] = DataAccessFactory.Instance.GetParameter("TkNo", NPDBType.VarChar);
            oSelectParams[0].Value = dicParam["TkNo"];
            oSelectParams[1] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
            oSelectParams[1].Value = dicParam["CarNo"];
            oSelectParams[2] = DataAccessFactory.Instance.GetParameter("InterfaceCode", NPDBType.VarChar);
            oSelectParams[2].Value = dicParam["InterfaceCode"];

            var selectTable = DA.ExecuteDataTable(sSelectQuery.ToString(), oSelectParams);

            this.Transaction = DA.GetTransaction();

            if (selectTable != null && selectTable.Rows.Count > 0)
            {
                if(selectTable.Rows[0]["InterfaceData"].ToString().Trim() != "")
                {
                    Log.WriteLog(LogType.Info, $"GovInterfaceModel | Save", "이미 저장된 데이터", LogAdpType.HomeNet);
                    Transaction?.Commit();
                    return true;
                }

                //이미 존재함.. Update 해야 함....
                StringBuilder sQuery = new StringBuilder();
                sQuery.Append("UPDATE GovInterface \r");
                sQuery.Append("SET InterfaceData = @InterfaceData \r");
                sQuery.Append("WHERE TkNo = @TkNo \r");
                sQuery.Append("AND CarNo = @CarNo \r");
                sQuery.Append("AND InterfaceCode = @InterfaceCode ");

                IDbDataParameter[] oParams = new IDbDataParameter[4];
                oParams[0] = DataAccessFactory.Instance.GetParameter("InterfaceData", NPDBType.Text);
                oParams[0].Value = dicParam["InterfaceData"];
                oParams[1] = DataAccessFactory.Instance.GetParameter("TkNo", NPDBType.VarChar);
                oParams[1].Value = dicParam["TkNo"];
                oParams[2] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
                oParams[2].Value = dicParam["CarNo"];
                oParams[3] = DataAccessFactory.Instance.GetParameter("InterfaceCode", NPDBType.VarChar);
                oParams[3].Value = dicParam["InterfaceCode"];

                Log.WriteLog(LogType.Info, $"GovInterfaceModel | Save", $"Query : UPDATE GovInterface \r" +
                    $"SET InterfaceData = '{dicParam["InterfaceData"]}' \r" +
                    $"WHERE TkNo = '{dicParam["TkNo"]}' \r" +
                    $"AND CarNO = {dicParam["CarNo"]}' \r" +
                    $"AND InterfaceCode = '{dicParam["InterfaceCode"]}'", LogAdpType.HomeNet);

                iRet = DA.ExecuteNonQuery(sQuery.ToString(), oParams, Transaction);
            }
            else
            {
                //신규
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

                Log.WriteLog(LogType.Info, $"GovInterfaceModel | Save", $"Query : INSERT INTO GovInterface(TkNo, CarNo, RequestDateTime, InterfaceCode, InterfaceData) \r" +
                    $"VALUES('{dicParam["TkNo"]}','{dicParam["CarNo"]}','{dicParam["RequestDateTime"]}','{dicParam["InterfaceCode"]}','{dicParam["InterfaceData"]}')", LogAdpType.HomeNet);

                iRet = DA.ExecuteNonQuery(sQuery.ToString(), oParams, Transaction);
            }

            if (iRet > 0)
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
