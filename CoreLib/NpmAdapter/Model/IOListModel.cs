using NpmCommon;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class IOListModel : BaseModel
    {
        /// <summary>
        /// Lpr정보를 DB로부터 가져온다.
        /// </summary>
        /// <returns></returns>
        public DataTable SelectPaging(Dictionary<ColName, object> dicParam)
        {
            QueryString sQuery = new QueryString();
            var parkNo = Helper.NVL(dicParam[ColName.ParkNo]);
            var startDt = Helper.NVL(dicParam[ColName.StartDt]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
            var endDt = Helper.NVL(dicParam[ColName.EndDt]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
            var pageNumber = Helper.NVL(dicParam[ColName.PageNumber], "0");
            var pageSize = Helper.NVL(dicParam[ColName.PageSize], "0");

            sQuery.Append($"SELECT TKNo, CarNo, UnitNo, OutChk, WhiteListType, IODateTime, Name, Dong, Ho, TelNo");
            sQuery.Append($"FROM");
            sQuery.Append($"(");
            sQuery.Append($" SELECT ROW_NUMBER() OVER(ORDER BY Dong, ho, TKNo, IODateTime) AS RowNum, TKNo, CarNo, UnitNo, OutChk, WhiteListType, IODateTime, Name, Dong, Ho, TelNo");
            sQuery.Append($" FROM (");
            sQuery.Append($"   SELECT TKNo, InCarNo1 CarNo, UnitNo, 'In' AS OutChk");
            sQuery.Append($"     CASE Reserve1");
            sQuery.Append($"	 WHEN '방문객차량' THEN 'Visitor'");
            sQuery.Append($"	 ELSE 'Guest'");
            sQuery.Append($"	 END WhiteListType,");
            sQuery.Append($"	 ProcDate + ' ' + ProcTime AS IODateTime,");
            sQuery.Append($"	 '' Name,");
            sQuery.Append($"	 Reserve2 Dong,");
            sQuery.Append($"	 Reserve3 Ho,");
            sQuery.Append($"	 '' TelNo");
            sQuery.Append($"    FROM IONDATA");
            sQuery.Append($"    WHERE ParkNo = {parkNo}");
            sQuery.Append($"    AND OutChk IN (0, 1, 4, 7)");
            sQuery.Append($"    AND ProcDate BETWEEN '{startDt}' AND '{endDt}'");

            //mobileNo
            if (dicParam.ContainsKey(ColName.TelNo) && Helper.NVL(dicParam[ColName.TelNo]) != "")
            {
                sQuery.Append($"    AND  1 = 2"); //ION에는 전화번호가 없음.
            }
            //carNo
            if (dicParam.ContainsKey(ColName.CarNo) && Helper.NVL(dicParam[ColName.CarNo]) != "")
            {
                sQuery.Append($"    AND InCarNo1 = '{Helper.NVL(dicParam[ColName.CarNo])}'");
            }
            //metaData1
            if (dicParam.ContainsKey(ColName.Dong) && Helper.NVL(dicParam[ColName.Dong]) != "")
            {
                sQuery.Append($"    AND Reserve2 = '{Helper.NVL(dicParam[ColName.Dong])}'");
            }
            //metaData2
            if (dicParam.ContainsKey(ColName.Ho) && Helper.NVL(dicParam[ColName.Ho]) != "")
            {
                sQuery.Append($"    AND Reserve3 = '{Helper.NVL(dicParam[ColName.Ho])}'");
            }

            sQuery.Append($"    UNION ALL");

            sQuery.Append($"    SELECT TKNo, InCarNo1 CarNo, UnitNo, 'Out' OutChk,");
            sQuery.Append($"    	CASE Reserve1");
            sQuery.Append($"    	WHEN '방문객차량' THEN 'Visitor'");
            sQuery.Append($"    	ELSE 'Guest'");
            sQuery.Append($"    	END WhiteListType,");
            sQuery.Append($"    	OutDate + ' ' + OutTime AS IODateTime,");
            sQuery.Append($"    	'' Name,");
            sQuery.Append($"    	Reserve2 Dong,");
            sQuery.Append($"    	Reserve3 Ho,");
            sQuery.Append($"    	'' TelNo");
            sQuery.Append($"    FROM IONDATA");
            sQuery.Append($"    WHERE ParkNo = {parkNo}");
            sQuery.Append($"    AND OutChk IN (1, 4)");
            sQuery.Append($"    AND OutDate BETWEEN '{startDt}' AND '{endDt}'");
            //mobileNo
            if (dicParam.ContainsKey(ColName.TelNo) && Helper.NVL(dicParam[ColName.TelNo]) != "")
            {
                sQuery.Append($"    AND  1 = 2"); //ION에는 전화번호가 없음.
            }
            //carNo
            if (dicParam.ContainsKey(ColName.CarNo) && Helper.NVL(dicParam[ColName.CarNo]) != "")
            {
                sQuery.Append($"    AND InCarNo1 = '{Helper.NVL(dicParam[ColName.CarNo])}'");
            }
            //metaData1
            if (dicParam.ContainsKey(ColName.Dong) && Helper.NVL(dicParam[ColName.Dong]) != "")
            {
                sQuery.Append($"    AND Reserve2 = '{Helper.NVL(dicParam[ColName.Dong])}'");
            }
            //metaData2
            if (dicParam.ContainsKey(ColName.Ho) && Helper.NVL(dicParam[ColName.Ho]) != "")
            {
                sQuery.Append($"    AND Reserve3 = '{Helper.NVL(dicParam[ColName.Ho])}'");
            }

            sQuery.Append($"    UNION ALL");
            
            sQuery.Append($"    SELECT S.TKNo, InCarNo1 CarNo, UnitNo,");
            sQuery.Append($"    	'In' OutChk,");
            sQuery.Append($"    	'Regular' WhiteListType,");
            sQuery.Append($"    	ProcDate + ' ' + ProcTime IODateTime,");
            sQuery.Append($"    	S.Name,");
            sQuery.Append($"    	S.CompName Dong,");
            sQuery.Append($"    	S.DeptName Ho,");
            sQuery.Append($"    	C.TelNo");
            sQuery.Append($"    FROM IOSDATA S, ");
            sQuery.Append($"        (SELECT ParkNo, TelNo, CarNo, CompName, DeptName");
            sQuery.Append($"    	 FROM CUSTINFO B");
            sQuery.Append($"    	 WHERE ExpDateT >= '{endDt}'");
            sQuery.Append($"    	 ) C");
            sQuery.Append($"    WHERE S.ParkNo = {parkNo}");
            sQuery.Append($"    AND S.ParkNo = C.ParkNo");
            sQuery.Append($"    AND S.InCarNo1 = C.CarNo");
            sQuery.Append($"    AND S.CompName = C.CompName");
            sQuery.Append($"    AND S.DeptName = C.DeptName");
            sQuery.Append($"    AND InIOStatusNo IN (1, 2)");
            sQuery.Append($"    AND ProcDate BETWEEN '{startDt}' AND '{endDt}'");
            //mobileNo
            if (dicParam.ContainsKey(ColName.TelNo) && Helper.NVL(dicParam[ColName.TelNo]) != "")
            {
                sQuery.Append($"    AND C.TelNo = '{Helper.NVL(dicParam[ColName.TelNo])}");
            }
            //carNo
            if (dicParam.ContainsKey(ColName.CarNo) && Helper.NVL(dicParam[ColName.CarNo]) != "")
            {
                sQuery.Append($"    AND InCarNo1 = '{Helper.NVL(dicParam[ColName.CarNo])}'");
            }
            //metaData1
            if (dicParam.ContainsKey(ColName.Dong) && Helper.NVL(dicParam[ColName.Dong]) != "")
            {
                sQuery.Append($"    AND S.CompName = '{Helper.NVL(dicParam[ColName.Dong])}'");
            }
            //metaData2
            if (dicParam.ContainsKey(ColName.Ho) && Helper.NVL(dicParam[ColName.Ho]) != "")
            {
                sQuery.Append($"    AND S.DeptName = '{Helper.NVL(dicParam[ColName.Ho])}'");
            }

            sQuery.Append($"    UNION ALL");
            
            sQuery.Append($"    SELECT S.TKNo, InCarNo1 CarNo, UnitNo,");
            sQuery.Append($"    	'Out' OutChk,");
            sQuery.Append($"    	'Regular' WhiteListType,");
            sQuery.Append($"    	OutDate + ' ' + OutTime IODateTime,");
            sQuery.Append($"    	S.Name,");
            sQuery.Append($"    	S.CompName Dong,");
            sQuery.Append($"    	S.DeptName Ho,");
            sQuery.Append($"    	C.TelNo");
            sQuery.Append($"    FROM IOSDATA S, ");
            sQuery.Append($"        (SELECT ParkNo, TelNo, CarNo, CompName, DeptName");
            sQuery.Append($"    	 FROM CUSTINFO B");
            sQuery.Append($"    	 WHERE ExpDateT >= '{endDt}'");
            sQuery.Append($"    	 ) C");
            sQuery.Append($"    WHERE S.ParkNo = {parkNo}");
            sQuery.Append($"    AND S.ParkNo = C.ParkNo");
            sQuery.Append($"    AND S.InCarNo1 = C.CarNo");
            sQuery.Append($"    AND S.CompName = C.CompName");
            sQuery.Append($"    AND S.DeptName = C.DeptName");
            sQuery.Append($"    AND InIOStatusNo = 2");
            sQuery.Append($"    AND OutDate BETWEEN '{startDt}' AND '{endDt}'");
            //mobileNo
            if (dicParam.ContainsKey(ColName.TelNo) && Helper.NVL(dicParam[ColName.TelNo]) != "")
            {
                sQuery.Append($"    AND C.TelNo = '{Helper.NVL(dicParam[ColName.TelNo])}");
            }
            //carNo
            if (dicParam.ContainsKey(ColName.CarNo) && Helper.NVL(dicParam[ColName.CarNo]) != "")
            {
                sQuery.Append($"    AND InCarNo1 = '{Helper.NVL(dicParam[ColName.CarNo])}'");
            }
            //metaData1
            if (dicParam.ContainsKey(ColName.Dong) && Helper.NVL(dicParam[ColName.Dong]) != "")
            {
                sQuery.Append($"    AND S.CompName = '{Helper.NVL(dicParam[ColName.Dong])}'");
            }
            //metaData2
            if (dicParam.ContainsKey(ColName.Ho) && Helper.NVL(dicParam[ColName.Ho]) != "")
            {
                sQuery.Append($"    AND S.DeptName = '{Helper.NVL(dicParam[ColName.Ho])}'");
            }
            sQuery.Append($"  ) WhiteList");
            sQuery.Append($") A");
            sQuery.Append($"WHERE RowNum BETWEEN ({pageNumber} * {pageSize}) + 1 AND (({pageNumber} + 1) * {pageSize})");

            Log.WriteLog(LogType.Info, "IOListModel | SelectPaging", sQuery.ToString(), LogAdpType.DataBase);

            return DA.ExecuteDataTable(sQuery.ToString());
        }
    }
}
