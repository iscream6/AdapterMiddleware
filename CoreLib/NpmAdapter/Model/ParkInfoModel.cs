using NpmCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class ParkInfoModel : BaseModel
    {
        public DataTable GetParkInfo(Dictionary<ColName, object> dicParam)
        {
            try
            {
                QueryString sQuery = new QueryString();
                sQuery.Append($"SELECT ParkNo, ParkName, ParkAddr, RegNo, Admin, Telephone");
                sQuery.Append($"FROM ParkInfo");
                sQuery.Append($"WHERE ParkNo = {Helper.NVL(dicParam[ColName.ParkNo])}");

                Log.WriteLog(LogType.Info, "ParkInfoModel | GetParkInfo", sQuery.ToString(), LogAdpType.Biz);

                return DA.ExecuteDataTable(sQuery.ToString());
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "ParkInfoModel | GetParkInfo", ex.Message, LogAdpType.Biz);
                return null;
            }
        }
    }
}
