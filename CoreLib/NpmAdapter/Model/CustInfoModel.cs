using NpmCommon;
using NpmDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class CustInfoModel : BaseModel
    {
        public DataTable SelectPaging(Dictionary<ColName, object> dicParam)
        {
            QueryString query = new QueryString();
            query.Append($"SELECT ParkNo, TkType, GroupNo, TKNo, Name, TelNo, CarNo, CompName, DeptName, Address, IssueDate, ExpDateF, ExpDateT, Reserve1");
            query.Append($"FROM");
            query.Append($"(");
            query.Append($" SELECT ROW_NUMBER() OVER(ORDER BY CompName, DeptName) AS RowNum, ParkNo, TkType, GroupNo, TKNo, Name, TelNo, CarNo, CompName, DeptName, Address, IssueDate, ExpDateF, ExpDateT, Reserve1");
            query.Append($" FROM CustInfo");
            query.Append($" WHERE ParkNo = {Helper.NVL(dicParam[ColName.ParkNo], "0")}");
            //regId
            if(dicParam.ContainsKey(ColName.TKNo) && Helper.NVL(dicParam[ColName.TKNo]) != "")
            {
                query.Append($" AND TKNo = '{Helper.NVL(dicParam[ColName.TKNo])}'");
            }
            //dateTimeFilterType, dateTimeFrom, dateTimeTo
            if (dicParam.ContainsKey(ColName.DateFilterType) && Helper.NVL(dicParam[ColName.DateFilterType]) != "")
            {
                var StartDt = Helper.NVL(dicParam[ColName.StartDt]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                var EndDt = Helper.NVL(dicParam[ColName.EndDt]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                string colName = "";
                if (Helper.NVL(dicParam[ColName.DateFilterType]) == "StartDateTime")
                {
                    //입차 가능 시작일시
                    colName = "ExpDateF";
                }
                else
                {
                    //출차 가능 시작일시
                    colName = "ExpDateT";
                }
                query.Append($" AND {colName} BETWEEN '{StartDt}' AND '{EndDt}'");
            }
            //deviceId
            if(dicParam.ContainsKey(ColName.UnitNo) && Helper.NVL(dicParam[ColName.UnitNo]) != "")
            {
                query.Append($" AND LastUnitNo = {Helper.NVL(dicParam[ColName.UnitNo])}");
            }
            //carNo
            if (dicParam.ContainsKey(ColName.CarNo) && Helper.NVL(dicParam[ColName.CarNo]) != "")
            {
                query.Append($" AND CarNo = '{Helper.NVL(dicParam[ColName.CarNo])}'");
            }
            //mobileNo
            if (dicParam.ContainsKey(ColName.TelNo) && Helper.NVL(dicParam[ColName.TelNo]) != "")
            {
                query.Append($" AND TelNo = '{Helper.NVL(dicParam[ColName.TelNo])}'");
            }
            //metaData1
            if (dicParam.ContainsKey(ColName.Dong) && Helper.NVL(dicParam[ColName.Dong]) != "")
            {
                query.Append($" AND CompName = '{Helper.NVL(dicParam[ColName.Dong])}'");
            }
            //metaData2
            if (dicParam.ContainsKey(ColName.Ho) && Helper.NVL(dicParam[ColName.Ho]) != "")
            {
                query.Append($" AND DeptName = '{Helper.NVL(dicParam[ColName.Ho])}'");
            }

            query.Append($") A");
            query.Append($"WHERE RowNum BETWEEN ({Helper.NVL(dicParam[ColName.PageNumber], "0")} * {Helper.NVL(dicParam[ColName.PageSize], "0")}) + 1 AND (({Helper.NVL(dicParam[ColName.PageNumber], "0")} + 1) * {Helper.NVL(dicParam[ColName.PageSize], "0")})");

            Log.WriteLog(LogType.Info, "", query.ToString(), LogAdpType.DataBase);

            var selectTable = DA.ExecuteDataTable(query.ToString());

            return selectTable;
        }

        public ModelResult Insert(Dictionary<ColName, object> dicParam)
        {
            ModelResult result = new ModelResult();

            try
            {
                IDbDataParameter[] oSelectParams = new IDbDataParameter[5];
                oSelectParams[0] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
                oSelectParams[0].Value = Helper.NVL(dicParam[ColName.CarNo]);

                oSelectParams[1] = DataAccessFactory.Instance.GetParameter("Dong", NPDBType.VarChar);
                oSelectParams[1].Value = Helper.NVL(dicParam[ColName.Dong], "0");

                oSelectParams[2] = DataAccessFactory.Instance.GetParameter("Ho", NPDBType.VarChar);
                oSelectParams[2].Value = Helper.NVL(dicParam[ColName.Ho], "0");

                oSelectParams[3] = DataAccessFactory.Instance.GetParameter("ExpDateF", NPDBType.Char, 10);
                oSelectParams[3].Value = Helper.NVL(dicParam[ColName.ExpDateF]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");

                //yyyy-MM-dd
                oSelectParams[4] = DataAccessFactory.Instance.GetParameter("ExpDateT", NPDBType.Char, 10);
                oSelectParams[4].Value = Helper.NVL(dicParam[ColName.ExpDateT]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");

                QueryString query = new QueryString();
                query.Append("SELECT TKNo");
                query.Append("FROM CustInfo");
                query.Append("WHERE CarNo = @CarNo");
                query.Append("AND CompName = @Dong");
                query.Append("AND DeptName = @Ho");
                query.Append("AND (@ExpDateF BETWEEN ExpDateF AND ExpDateT OR @ExpDateT BETWEEN ExpDateF AND ExpDateT)");
                query.Append("OR (ExpDateF > @ExpDateF AND ExpDateT < @ExpDateT)");

                WriteLog("CustInfoModel | Insert", query.ToString(), dicParam);

                var selectTable = DA.ExecuteDataTable(query.ToString(), oSelectParams);

                if (selectTable != null && selectTable.Rows.Count > 0)
                {
                    result.code = ModelCode.AlreadyRegisted;
                    result.Message = "이미 등록된 차량입니다.";
                    result.Description = selectTable.Rows[0]["TKNo"].ToString();
                }
                else
                {
                    Transaction = DA.GetTransaction();

                    IDbDataParameter[] oCustParams = new IDbDataParameter[12];
                    oCustParams[0] = DataAccessFactory.Instance.GetParameter("ParkNo", NPDBType.Integer);
                    oCustParams[0].Value = int.Parse(Helper.NVL(dicParam[ColName.ParkNo], "0"));
                     
                    oCustParams[1] = DataAccessFactory.Instance.GetParameter("Name", NPDBType.VarChar);
                    oCustParams[1].Value = Helper.NVL(dicParam[ColName.Name]);
                     
                    oCustParams[2] = DataAccessFactory.Instance.GetParameter("TelNo", NPDBType.VarChar);
                    oCustParams[2].Value = Helper.NVL(dicParam[ColName.TelNo]);
                     
                    oCustParams[3] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
                    oCustParams[3].Value = Helper.NVL(dicParam[ColName.CarNo]);
                     
                    oCustParams[4] = DataAccessFactory.Instance.GetParameter("Dong", NPDBType.VarChar);
                    oCustParams[4].Value = Helper.NVL(dicParam[ColName.Dong], "0");
                     
                    oCustParams[5] = DataAccessFactory.Instance.GetParameter("Ho", NPDBType.VarChar);
                    oCustParams[5].Value = Helper.NVL(dicParam[ColName.Ho], "0");
                     
                    //yyyy-MM-dd
                    oCustParams[6] = DataAccessFactory.Instance.GetParameter("IssueDate", NPDBType.Char, 10);
                    oCustParams[6].Value = DateTime.Now.ToString("yyyy-MM-dd");
                     
                    //yyyy-MM-dd
                    oCustParams[7] = DataAccessFactory.Instance.GetParameter("ExpDateF", NPDBType.Char, 10);
                    oCustParams[7].Value = Helper.NVL(dicParam[ColName.ExpDateF]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                     
                    //yyyy-MM-dd
                    oCustParams[8] = DataAccessFactory.Instance.GetParameter("ExpDateT", NPDBType.Char, 10);
                    oCustParams[8].Value = Helper.NVL(dicParam[ColName.ExpDateT]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                     
                    oCustParams[9] = DataAccessFactory.Instance.GetParameter("UnitNo", NPDBType.Integer);
                    oCustParams[9].Value = int.Parse(Helper.NVL(dicParam[ColName.UnitNo], "0"));
                     
                    oCustParams[10] = DataAccessFactory.Instance.GetParameter("Memo", NPDBType.VarChar);
                    oCustParams[10].Value = Helper.NVL(dicParam[ColName.Memo]);
                     
                    oCustParams[11] = DataAccessFactory.Instance.GetParameter("TKNo", NPDBType.VarChar);
                    oCustParams[11].Value = Helper.NVL(dicParam[ColName.TKNo]);

                    query.Clear();
                    query.Append($"INSERT INTO CustInfo");
                    query.Append($"(ParkNo, TKType, GroupNo, Name, TelNo, CarNo, CompName,");
                    query.Append($" DeptName, Address, IssueDate, IssueAmt, Status, ExpDateF,");
                    query.Append($" ExpDateT, WPNo, LastParkNo, LastUnitNo, LastUseDate, LastUseTime,");
                    query.Append($" IOStatusNo, CurrAmt, TKNo, APB, CarType, Reserve1, HIType, HISTime, HIETime, BujaeType)");
                    query.Append($"VALUES (@ParkNo, 2, {SysConfig.Instance.DBCustGroupNo}, @Name, @TelNo, @CarNo, @Dong,");
                    query.Append($"        @Ho, '', @IssueDate, 0, 1, @ExpDateF,");
                    query.Append($"        @ExpDateT, 1, @ParkNo, @UnitNo, '', '',");
                    query.Append($"        1, 0, @TKNo, 3, 1, @Memo, 1, '00:00', '23:59', 0)");

                    WriteLog("CustInfoModel | Insert", query.ToString(), dicParam);

                    int iRet = DA.ExecuteNonQuery(query.ToString(), oCustParams, Transaction);

                    if (iRet > 0)
                    {
                        IDbDataParameter[] oIssueParams = new IDbDataParameter[7];
                        oIssueParams[0] = DataAccessFactory.Instance.GetParameter("ParkNo", NPDBType.Integer);
                        oIssueParams[0].Value = int.Parse(Helper.NVL(dicParam[ColName.ParkNo], "0"));
                        oIssueParams[1] = DataAccessFactory.Instance.GetParameter("IssueDate", NPDBType.Char, 10);
                        oIssueParams[1].Value = DateTime.Now.ToString("yyyy-MM-dd");
                        oIssueParams[2] = DataAccessFactory.Instance.GetParameter("IssueTime", NPDBType.Char, 8);
                        oIssueParams[2].Value = DateTime.Now.ToString("HH:mm:ss");
                        oIssueParams[3] = DataAccessFactory.Instance.GetParameter("TKNo", NPDBType.VarChar);
                        oIssueParams[3].Value = Helper.NVL(dicParam[ColName.TKNo]);
                        oIssueParams[4] = DataAccessFactory.Instance.GetParameter("ExpDateF", NPDBType.Char, 10);
                        oIssueParams[4].Value = Helper.NVL(dicParam[ColName.ExpDateF]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                        oIssueParams[5] = DataAccessFactory.Instance.GetParameter("ExpDateT", NPDBType.Char, 10);
                        oIssueParams[5].Value = Helper.NVL(dicParam[ColName.ExpDateT]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd");
                        oIssueParams[6] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
                        oIssueParams[6].Value = Helper.NVL(dicParam[ColName.CarNo]);

                        query.Clear();
                        query.Append("INSERT INTO IssueTK");
                        query.Append("(ParkNo, IssueDate, IssueTime, MNo, TKType, TKNo, CarType, IssueType,");
                        query.Append(" IssueUnit, ExpDateF, ExpDateT, FValue, ChkClosing, CarNo, WPNo) ");
                        query.Append("VALUES (@ParkNo, @IssueDate, @IssueTime, 0, 2, @TKNo, 1, 1,");
                        query.Append("        0, @ExpDateF, @ExpDateT, 0, 0, @CarNo, 1)");

                        WriteLog("CustInfoModel | Insert", query.ToString(), dicParam);

                        iRet = DA.ExecuteNonQuery(query.ToString(), oIssueParams, Transaction);
                    }

                    if (iRet > 0)
                    {
                        Transaction?.Commit();
                        result.code = ModelCode.OK;
                        result.Message = "OK";
                    }
                    else
                    {
                        Transaction?.Rollback();
                        result.code = ModelCode.Fail;
                        result.Message = "등록실패";
                    }
                }
            }
            catch (Exception ex)
            {
                result.code = ModelCode.Exception;
                result.Message = ex.Message;
                Transaction?.Rollback();
            }

            return result;
        }

        public ModelResult Delete(Dictionary<ColName, object> dicParam)
        {
            ModelResult result = new ModelResult();

            try
            {
                QueryString sQuery = new QueryString();
                sQuery.Append("DELETE CustInfo");
                sQuery.Append("WHERE TKNo = @TKNo");
                sQuery.Append("AND CompName = @Dong");
                sQuery.Append("AND DeptName = @Ho");

                WriteLog("VisitInfoMode | Delete", sQuery.ToString(), dicParam);

                IDbDataParameter[] oParams = new IDbDataParameter[3];
                oParams[0] = DataAccessFactory.Instance.GetParameter("Dong", NPDBType.Integer);
                oParams[0].Value = int.Parse(Helper.NVL(dicParam[ColName.Dong], "0"));

                oParams[1] = DataAccessFactory.Instance.GetParameter("Ho", NPDBType.Integer);
                oParams[1].Value = int.Parse(Helper.NVL(dicParam[ColName.Ho], "0"));

                oParams[2] = DataAccessFactory.Instance.GetParameter("TKNo", NPDBType.VarChar);
                oParams[2].Value = Helper.NVL(dicParam[ColName.TKNo]);

                int iRet = DA.ExecuteNonQuery(sQuery.ToString(), oParams, Transaction);

                if (iRet > 0)
                {
                    Transaction?.Commit();
                    result.code = ModelCode.OK;
                    result.Message = "OK";
                }
                else if (iRet == 0)
                {
                    Transaction?.Commit();
                    result.code = ModelCode.NotFound;
                    result.Message = "삭제 할 정기차량차량 데이터가 없습니다.";
                }
                else
                {
                    Transaction?.Rollback();
                    result.code = ModelCode.Fail;
                    result.Message = "삭제실패";
                }
            }
            catch (Exception)
            {
                Transaction?.Rollback();
            }

            return result;
        }
    }
}
