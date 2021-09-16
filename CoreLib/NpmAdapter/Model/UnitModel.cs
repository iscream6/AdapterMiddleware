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
            sQuery.Append("WHERE UnitKind IN (8, 9) \r");
            sQuery.Append("AND MyNo NOT IN (SELECT UnitNo \r");
            sQuery.Append("                 FROM UnitInfo B \r");
            sQuery.Append("				    WHERE B.UnitKind = 20 \r");
            sQuery.Append("				    AND B.MyNo = 0) \r");
            sQuery.Append("ORDER BY ParkNo, MyNo, UnitNo ");

            WriteLog("UnitModel | GetLprInfo", sQuery.ToString());

            return DA.ExecuteDataTable(sQuery.ToString());
        }

        public DataTable GetLprInfo(Dictionary<ColName, object> dicParam)
        {
            QueryString sQuery = new QueryString();
            sQuery.Append($"SELECT");
            sQuery.Append($" ParkNo, UnitNo, UnitName, UnitKind, MyNo, IPNo, PortNo");
            sQuery.Append($"FROM UnitInfo A");
            sQuery.Append($"WHERE ParkNo = {Helper.NVL(dicParam[ColName.ParkNo])}");
            sQuery.Append($"AND UnitKind IN (8, 9)");
            sQuery.Append($"AND MyNo NOT IN (SELECT UnitNo");
            sQuery.Append($"                 FROM UnitInfo B");
            sQuery.Append($"				    WHERE B.UnitKind = 20");
            sQuery.Append($"				    AND B.MyNo = 0)");
            sQuery.Append($"ORDER BY ParkNo, MyNo, UnitNo");

            WriteLog("UnitModel | GetLprInfo", sQuery.ToString());

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

            WriteLog("UnitModel | GetBoothInfo", sQuery.ToString());

            return DA.ExecuteDataTable(sQuery.ToString());
        }
    }
}
