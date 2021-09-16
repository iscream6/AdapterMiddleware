using NpmCommon;
using NpmDA.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class VisitInfoModel : BaseModel
    {
        /// <summary>
        /// Lpr정보를 DB로부터 가져온다.
        /// </summary>
        /// <returns></returns>
        public DataTable SelectPaging(Dictionary<ColName, object> dicParam)
        {
            QueryString sQuery = new QueryString();
            sQuery.Append($"SELECT dong, ho, CarNo, StartDateTime, EndDateTime, TKNo, visit_flag");
            sQuery.Append($"FROM");
            sQuery.Append($"(");
            sQuery.Append($" SELECT ROW_NUMBER() OVER(ORDER BY dong, ho) AS RowNum, dong, ho, CarNo, FORMAT(StartDateTime, 'yyyyMMddHHmmss') StartDateTime, FORMAT(EndDateTime, 'yyyyMMddHHmmss') EndDateTime, TKNo, visit_flag");
            sQuery.Append($" FROM VisitInfo");
            sQuery.Append($" WHERE 1 = 1");
            //regId
            if (dicParam.ContainsKey(ColName.TKNo) && Helper.NVL(dicParam[ColName.TKNo]) != "")
            {
                sQuery.Append($" AND TKNo = '{Helper.NVL(dicParam[ColName.TKNo])}'");
            }
            //dateTimeFilterType, dateTimeFrom, dateTimeTo
            if (dicParam.ContainsKey(ColName.DateFilterType) && Helper.NVL(dicParam[ColName.DateFilterType]) != "")
            {
                var StartDt = Helper.NVL(dicParam[ColName.StartDt]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd") + " 00:00:00";
                var EndDt = Helper.NVL(dicParam[ColName.EndDt]).ConvertDateTimeFormat("yyyyMMdd", "yyyy-MM-dd") + " 23:59:59";
                string colName = "";
                if (Helper.NVL(dicParam[ColName.DateFilterType]) == "StartDateTime")
                {
                    //입차 가능 시작일시
                    colName = "StartDateTime";
                }
                else
                {
                    //출차 가능 시작일시
                    colName = "EndDateTime";
                }
                sQuery.Append($" AND {colName} BETWEEN CONVERT(datetime, '{StartDt}', 120) AND CONVERT(datetime, '{EndDt}', 120)");
            }
            //carNo
            if (dicParam.ContainsKey(ColName.CarNo) && Helper.NVL(dicParam[ColName.CarNo]) != "")
            {
                sQuery.Append($" AND CarNo = '{Helper.NVL(dicParam[ColName.CarNo])}'");
            }
            //metaData1
            if (dicParam.ContainsKey(ColName.Dong) && Helper.NVL(dicParam[ColName.Dong]) != "")
            {
                sQuery.Append($" AND dong = '{Helper.NVL(dicParam[ColName.Dong])}'");
            }
            //metaData2
            if (dicParam.ContainsKey(ColName.Ho) && Helper.NVL(dicParam[ColName.Ho]) != "")
            {
                sQuery.Append($" AND ho = '{Helper.NVL(dicParam[ColName.Ho])}'");
            }

            sQuery.Append($") A");
            sQuery.Append($"WHERE RowNum BETWEEN ({Helper.NVL(dicParam[ColName.PageNumber], "0")} * {Helper.NVL(dicParam[ColName.PageSize], "0")}) + 1 AND (({Helper.NVL(dicParam[ColName.PageNumber], "0")} + 1) * {Helper.NVL(dicParam[ColName.PageSize], "0")})");

            Log.WriteLog(LogType.Info, "VisitInfoModel | SelectPaging", sQuery.ToString(), LogAdpType.DataBase);

            return DA.ExecuteDataTable(sQuery.ToString());
        }

        public DataTable GetBoothInfo()
        {
            QueryString sQuery = new QueryString();
            sQuery.Append("SELECT");
            sQuery.Append(" ParkNo, UnitNo, UnitName, UnitKind, MyNo, IPNo, PortNo");
            sQuery.Append("FROM UnitInfo A");
            sQuery.Append("WHERE UnitKind = 13");
            sQuery.Append("AND MyNo NOT IN (SELECT UnitNo");
            sQuery.Append("                 FROM UnitInfo B");
            sQuery.Append("				    WHERE B.UnitKind = 20");
            sQuery.Append("				    AND B.MyNo = 0)");
            sQuery.Append("ORDER BY ParkNo, MyNo, UnitNo");

            return DA.ExecuteDataTable(sQuery.ToString());
        }

        public DataTable GetVisitCar(Dictionary<ColName, object> dicParam)
        {
            QueryString sQuery = new QueryString();
            sQuery.Append("SELECT");
            sQuery.Append(" dong, ho, CarNo, StartDateTime, EndDateTime, TKNo, Visit_flag");
            sQuery.Append("FROM VisitInfo");

            if (dicParam.ContainsKey(ColName.TKNo))
            {
                sQuery.Append조건($"TKNo = '{dicParam[ColName.TKNo]}'");
            }

            if (dicParam.ContainsKey(ColName.CarNo))
            {
                sQuery.Append조건($"CarNo = '{dicParam[ColName.CarNo]}'");
            }

            if (dicParam.ContainsKey(ColName.Dong))
            {
                sQuery.Append조건($"dong = '{dicParam[ColName.Dong]}'");
            }

            if (dicParam.ContainsKey(ColName.Ho))
            {
                sQuery.Append조건($"ho = '{dicParam[ColName.Ho]}'");
            }

            return DA.ExecuteDataTable(sQuery.ToString());
        }

        /// <summary>
        /// 방문차량 등록 : VisitInfo Table
        /// </summary>
        /// <param name="payload"></param>
        public ModelResult Insert(Dictionary<ColName, object> dicParam)
        {
            ModelResult result = new ModelResult();

            try
            {
                //기 등록된 차량이 존재하는지 조회...
                QueryString sQuery = new QueryString();
                sQuery.Append("SELECT TkNo");
                sQuery.Append("FROM VisitInfo");
                sQuery.Append("WHERE dong = @Dong");
                sQuery.Append("AND ho = @Ho");
                sQuery.Append("AND CarNo = @CarNo");
                sQuery.Append("AND (@StartDt BETWEEN StartDateTime AND EndDateTime OR @EndDt BETWEEN StartDateTime AND EndDateTime)");
                sQuery.Append("OR (StartDateTime > @StartDt AND EndDateTime < @EndDt)");

                WriteLog("VisitInfoMode | SaveVisitInfo", sQuery.ToString(), dicParam);

                IDbDataParameter[] oSelectParams = new IDbDataParameter[5];
                oSelectParams[0] = DataAccessFactory.Instance.GetParameter("Dong", NPDBType.Integer);
                oSelectParams[0].Value = int.Parse(Helper.NVL(dicParam[ColName.Dong], "0"));

                oSelectParams[1] = DataAccessFactory.Instance.GetParameter("Ho", NPDBType.Integer);
                oSelectParams[1].Value = int.Parse(Helper.NVL(dicParam[ColName.Ho], "0"));

                oSelectParams[2] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
                oSelectParams[2].Value = Helper.NVL(dicParam[ColName.CarNo]);

                oSelectParams[3] = DataAccessFactory.Instance.GetParameter("StartDt", NPDBType.DateTime);
                oSelectParams[3].Value = DateTime.ParseExact(Helper.NVL(dicParam[ColName.StartDt]), "yyyyMMddHHmmss", null);

                oSelectParams[4] = DataAccessFactory.Instance.GetParameter("EndDt", NPDBType.DateTime);
                oSelectParams[4].Value = DateTime.ParseExact(Helper.NVL(dicParam[ColName.EndDt]), "yyyyMMddHHmmss", null);

                var selectTable = DA.ExecuteDataTable(sQuery.ToString(), oSelectParams);

                if (selectTable != null && selectTable.Rows.Count > 0)
                {
                    result.code = ModelCode.AlreadyRegisted;
                    result.Message = "이미 등록된 차량입니다.";
                    result.Description = selectTable.Rows[0]["TKNo"].ToString();
                }
                else
                {
                    Transaction = DA.GetTransaction();

                    sQuery.Clear();
                    sQuery.Append("INSERT INTO VisitInfo(dong, ho, CarNo, StartDateTime, EndDateTime, TkNo)");
                    sQuery.Append("Values(@Dong, @Ho, @CarNo, @StartDt, @EndDt, @TKNo)");

                    WriteLog("VisitInfoMode | SaveVisitInfo", sQuery.ToString(), dicParam);

                    IDbDataParameter[] oInsetParams = new IDbDataParameter[6];
                    oInsetParams[0] = DataAccessFactory.Instance.GetParameter("Dong", NPDBType.Integer);
                    oInsetParams[0].Value = int.Parse(Helper.NVL(dicParam[ColName.Dong], "0"));

                    oInsetParams[1] = DataAccessFactory.Instance.GetParameter("Ho", NPDBType.Integer);
                    oInsetParams[1].Value = int.Parse(Helper.NVL(dicParam[ColName.Ho], "0"));

                    oInsetParams[2] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
                    oInsetParams[2].Value = Helper.NVL(dicParam[ColName.CarNo]);

                    oInsetParams[3] = DataAccessFactory.Instance.GetParameter("StartDt", NPDBType.DateTime);
                    oInsetParams[3].Value = DateTime.ParseExact(Helper.NVL(dicParam[ColName.StartDt]), "yyyyMMddHHmmss", null);

                    oInsetParams[4] = DataAccessFactory.Instance.GetParameter("EndDt", NPDBType.DateTime);
                    oInsetParams[4].Value = DateTime.ParseExact(Helper.NVL(dicParam[ColName.EndDt]), "yyyyMMddHHmmss", null);

                    oInsetParams[5] = DataAccessFactory.Instance.GetParameter("TKNo", NPDBType.VarChar);
                    oInsetParams[5].Value = Helper.NVL(dicParam[ColName.TKNo]);

                    int iRet = DA.ExecuteNonQuery(sQuery.ToString(), oInsetParams, Transaction);

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
            }

            return result;
        }

        /// <summary>
        /// 방문차량 수정
        /// </summary>
        /// <param name="dicParam"></param>
        /// <returns></returns>
        public ModelResult Update(Dictionary<ColName, object> dicParam)
        {
            ModelResult result = new ModelResult();

            try
            {
                Transaction = DA.GetTransaction();

                QueryString sQuery = new QueryString();
                sQuery.Append("UPDATE VisitInfo");
                sQuery.Append("SET dong = @Dong, ho = @Ho, CarNo = @CarNo, StartDateTime = @StartDt, EndDateTime = @EndDt");
                sQuery.Append("WHERE TKNo = @TKNo");

                WriteLog("VisitInfoMode | UpdateVisitInfo", sQuery.ToString(), dicParam);

                IDbDataParameter[] oParams = new IDbDataParameter[6];
                oParams[0] = DataAccessFactory.Instance.GetParameter("Dong", NPDBType.Integer);
                oParams[0].Value = int.Parse(Helper.NVL(dicParam[ColName.Dong], "0"));

                oParams[1] = DataAccessFactory.Instance.GetParameter("Ho", NPDBType.Integer);
                oParams[1].Value = int.Parse(Helper.NVL(dicParam[ColName.Ho], "0"));

                oParams[2] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
                oParams[2].Value = Helper.NVL(dicParam[ColName.CarNo]);

                oParams[3] = DataAccessFactory.Instance.GetParameter("StartDt", NPDBType.DateTime);
                oParams[3].Value = DateTime.ParseExact(Helper.NVL(dicParam[ColName.StartDt]), "yyyyMMddHHmmss", null);

                oParams[4] = DataAccessFactory.Instance.GetParameter("EndDt", NPDBType.DateTime);
                oParams[4].Value = DateTime.ParseExact(Helper.NVL(dicParam[ColName.EndDt]), "yyyyMMddHHmmss", null);

                oParams[5] = DataAccessFactory.Instance.GetParameter("TKNo", NPDBType.VarChar);
                oParams[5].Value = Helper.NVL(dicParam[ColName.TKNo]);

                int iRet = DA.ExecuteNonQuery(sQuery.ToString(), oParams, Transaction);

                if (iRet > 0)
                {
                    Transaction?.Commit();
                    result.code = ModelCode.OK;
                    result.Message = "OK";
                }
                else if(iRet == 0)
                {
                    Transaction?.Commit();
                    result.code = ModelCode.NotFound;
                    result.Message = "수정 할 방문차량 데이터가 없습니다.";
                }
                else
                {
                    Transaction?.Rollback();
                    result.code = ModelCode.Fail;
                    result.Message = "수정실패";
                }
            }
            catch (Exception ex)
            {
                result.code = ModelCode.Exception;
                result.Message = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 방문차량 삭제
        /// </summary>
        /// <param name="dicParam"></param>
        /// <returns></returns>
        public ModelResult Delete(Dictionary<ColName, object> dicParam)
        {
            ModelResult result = new ModelResult();

            try
            {
                Transaction = DA.GetTransaction();

                QueryString sQuery = new QueryString();
                sQuery.Append("DELETE VisitInfo");
                sQuery.Append("WHERE TKNo = @TKNo");
                sQuery.Append("AND dong = @Dong");
                sQuery.Append("AND ho = @Ho");

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
                else if(iRet == 0)
                {
                    Transaction?.Commit();
                    result.code = ModelCode.NotFound;
                    result.Message = "삭제 할 방문차량 데이터가 없습니다.";
                }
                else
                {
                    Transaction?.Rollback();
                    result.code = ModelCode.Fail;
                    result.Message = "삭제실패";
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
    }
}
