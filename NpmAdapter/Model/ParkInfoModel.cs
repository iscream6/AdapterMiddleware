using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class ParkInfoModel : BaseModel
    {
        public DataTable GetParkInfo()
        {
            try
            {
                StringBuilder sQuery = new StringBuilder();
                sQuery.Append("SELECT ParkNo, ParkName, ParkAddr, RegNo, Admin, Telephone \r");
                sQuery.Append("FROM ParkInfo \r");
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
