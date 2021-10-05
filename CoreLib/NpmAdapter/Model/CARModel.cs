using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class CARModel : BaseModel
    {
        public DataTable GetCars(Dictionary<ColName, object> param)
        {
            QueryString strQuery = new QueryString();
            strQuery.Append("SELECT * FROM Car");
            return DA.ExecuteDataTable(strQuery.ToString());
        }
    }
}
