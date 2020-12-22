using NexpaAdapterStandardLib.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class DCModel : BaseModel
    {
        public DataTable GetDCInfo()
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT \r");
            sQuery.Append(" ParkNo, DCNo, DCName, DCValue, DCType, VisibleCnt, RealUse, ButtonUse, RemoteButtonUse, WebUse, \r");
            sQuery.Append(" FixedUse, DCTypeCode, CamUse, Reserve1, Reserve2, Reserve3, Reserve4, Reserve5, Reserve6, Reserve7, \r");
            sQuery.Append(" Reserve8, Reserve9, Reserve10, PrintName, BarcodeUse, MagneticUse, OcsUse, RemoteUse, FeeNo, WebDCUseFunction \r");
            sQuery.Append("FROM DCInfo \r");
            sQuery.Append("WHERE RealUse = @RealUse \r");
            sQuery.Append("ORDER BY ParkNo, DCNo ");

            IDbDataParameter[] oParams = new IDbDataParameter[1];
            oParams[0] = DataAccessFactory.Instance.GetParameter("RealUse", NPDBType.Char);
            oParams[0].Value = 1;

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }
    }
}
