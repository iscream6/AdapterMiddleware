using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class UnitModel : BaseModel
    {
        /// <summary>
        /// Lpr정보를 DB로부터 가져온다.
        /// </summary>
        /// <returns></returns>
        public DataTable GetLprInfo()
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT \r");
            sQuery.Append(" ParkNo, UnitNo, UnitName, UnitKind, MyNo, IPNo, PortNo \r");
            sQuery.Append("FROM UnitInfo A \r");
            sQuery.Append("WHERE UnitKind = 9 \r");
            sQuery.Append("AND MyNo NOT IN (SELECT UnitNo \r");
            sQuery.Append("                 FROM UnitInfo B \r");
            sQuery.Append("				    WHERE B.UnitKind = 20 \r");
            sQuery.Append("				    AND B.MyNo = 0) \r");
            sQuery.Append("ORDER BY ParkNo, MyNo, UnitNo ");

            return DA.ExecuteDataTable(sQuery.ToString());
        }

        public DataTable GetBoothInfo()
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT \r");
            sQuery.Append(" ParkNo, UnitNo, UnitName, UnitKind, MyNo, IPNo, PortNo \r");
            sQuery.Append("FROM UnitInfo A \r");
            sQuery.Append("WHERE UnitKind = 13 \r");
            sQuery.Append("AND MyNo NOT IN (SELECT UnitNo \r");
            sQuery.Append("                 FROM UnitInfo B \r");
            sQuery.Append("				    WHERE B.UnitKind = 20 \r");
            sQuery.Append("				    AND B.MyNo = 0) \r");
            sQuery.Append("ORDER BY ParkNo, MyNo, UnitNo ");

            return DA.ExecuteDataTable(sQuery.ToString());
        }
    }
}
