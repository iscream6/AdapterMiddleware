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

        public DataTable SelectKM_VisitList(Dictionary<ColName, object> dicParam)
        {
            try
            {
                //모든 param이 빈값이면 안됨. Dong, Ho, CarNumber
                int paramCheckCnt = 3;

                QueryString sQuery = new QueryString();
                sQuery.Append($"SELECT dong, ho, CarNo, FORMAT(StartDateTime, 'yyyy-MM-dd') StartDate, FORMAT(EndDateTime, 'yyyy-MM-dd') EndDate, TKNo, visit_flag");
                sQuery.Append($"FROM VisitInfo");
                sQuery.Append($"WHERE 1 = 1");

                //dong
                if (dicParam.ContainsKey(ColName.Dong) && Helper.NVL(dicParam[ColName.Dong]) != "")
                {
                    sQuery.Append($"AND dong = {Helper.NVL(dicParam[ColName.Dong])}");
                    paramCheckCnt -= 1;
                }
                //ho
                if (dicParam.ContainsKey(ColName.Ho) && Helper.NVL(dicParam[ColName.Ho]) != "")
                {
                    sQuery.Append($"AND ho = {Helper.NVL(dicParam[ColName.Ho])}");
                    paramCheckCnt -= 1;
                }
                //Carno
                if (dicParam.ContainsKey(ColName.CarNo) && Helper.NVL(dicParam[ColName.CarNo]) != "")
                {
                    sQuery.Append($"AND CarNo = '{Helper.NVL(dicParam[ColName.CarNo])}'");
                    paramCheckCnt -= 1;
                }

                sQuery.Append($"AND sDelete_YN = 'N'");
                sQuery.Append($"ORDER BY TKNo");

                Log.WriteLog(LogType.Info, "VisitInfoModel | SelectKM_VisitList", sQuery.ToString(), LogAdpType.DataBase);

                //Param값이 모두 없을경우
                if (paramCheckCnt == 0)
                {
                    Log.WriteLog(LogType.Error, "VisitInfoModel | SelectPaging", "Param값이 없음.", LogAdpType.DataBase);
                    return null;
                }
                else
                {
                    return DA.ExecuteDataTable(sQuery.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "VisitInfoModel | SelectPaging", ex.Message + "\r\n" + ex.StackTrace, LogAdpType.DataBase);
                return null;
            }
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
        /// 방문차량 등록(카카오모빌)
        /// </summary>
        /// <param name="dicParam"></param>
        /// <returns></returns>
        public ModelResult InsertKM(Dictionary<ColName, object> dicParam)
        {
            ModelResult result = new ModelResult();

            try
            {
                var invParam = InvaildParameter(dicParam);

                if (invParam != "")
                {
                    result.code = ModelCode.InvalidParameter;
                    result.Message = $"Invalid Parameter : {invParam}";
                }
                else
                {
                    //기 등록된 차량이 존재하는지 조회...
                    QueryString sQuery = new QueryString();
                    sQuery.Append("SELECT TkNo, sDelete_YN, FORMAT(StartDateTime, 'yyyyMMddHHmmss') StartDateTime, FORMAT(EndDateTime, 'yyyyMMddHHmmss') EndDateTime");
                    sQuery.Append("FROM VisitInfo");
                    sQuery.Append("WHERE dong = @Dong");
                    sQuery.Append("AND ho = @Ho");
                    sQuery.Append("AND CarNo = @CarNo");
                    //sQuery.Append("AND sDelete_YN = 'N'");
                    sQuery.Append("AND (@StartDt BETWEEN StartDateTime AND EndDateTime OR @EndDt BETWEEN StartDateTime AND EndDateTime)");
                    sQuery.Append("OR (StartDateTime > @StartDt AND EndDateTime < @EndDt)");
                    sQuery.Append("Order by StartDateTime DESC");

                    WriteLog("VisitInfoMode | SaveVisitInfo", sQuery.ToString(), dicParam);
                    //FORMAT(StartDateTime, 'yyyyMMddHHmmss')
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

                    string sttCode = "N"; //N : 그냥 Insert, U : Update, D : 삭제 후 Insert, C : 겹치는게 있다.
                    string tempTkNo = "";

                    if (selectTable != null && selectTable.Rows.Count > 0)
                    {
                        foreach (DataRow dr in selectTable.Rows)
                        {
                            var tk = Helper.NVL(dr["TkNo"]);
                            var delYN = Helper.NVL(dr["sDelete_YN"]);
                            var startDt = Helper.NVL(dr["StartDateTime"]);
                            var endDt = Helper.NVL(dr["EndDateTime"]);

                            if(startDt == Helper.NVL(dicParam[ColName.StartDt]) && endDt == Helper.NVL(dicParam[ColName.EndDt]) && delYN == "Y")
                            {
                                //같은 날짜에 삭제 된 항목이 존재함.
                                sttCode = "D";
                                tempTkNo = tk;
                                break;
                            }
                            else if(delYN == "N")
                            {
                                //날짜 중복이 발생함.
                                sttCode = "C";
                                break;
                            }
                        }

                        //if(Helper.NVL(selectTable.Rows[0]["sDelete_YN"]) == "N")
                        //{
                        //    result.code = ModelCode.AlreadyRegisted;
                        //    result.Message = "이미 등록된 차량입니다.";
                        //    result.Description = selectTable.Rows[0]["TKNo"].ToString();

                        //    return result;
                        //}
                        //else
                        //{
                        //    //삭제하고 다시 Insert 하자..
                        //    tempTkNo = Helper.NVL(selectTable.Rows[0]["TkNo"]);
                        //}

                        if(sttCode == "C")
                        {
                            result.code = ModelCode.AlreadyRegisted;
                            result.Message = "이미 등록된 차량입니다.";
                            result.Description = selectTable.Rows[0]["TKNo"].ToString();

                            return result;
                        }
                    }

                    int iRet = -1;
                    Transaction = DA.GetTransaction();

                    if(sttCode == "D")
                    {
                        //삭제를 진행한다.
                        sQuery.Clear();
                        sQuery.Append($"DELETE VisitInfo");
                        sQuery.Append($"WHERE TKNo = '{tempTkNo}'");
                        sQuery.Append($"AND dong = {Helper.NVL(dicParam[ColName.Dong], "0")}");
                        sQuery.Append($"AND ho = {Helper.NVL(dicParam[ColName.Ho], "0")}");
                        sQuery.Append($"AND CarNo = '{Helper.NVL(dicParam[ColName.CarNo])}'");

                        WriteLog("VisitInfoMode | Delete", sQuery.ToString());

                        iRet = DA.ExecuteNonQuery(sQuery.ToString(), Transaction);
                    }

                    string actType = "홈넷.추가";
                    sQuery.Clear();
                    sQuery.Append($"INSERT INTO VisitInfo(dong, ho, CarNo, StartDateTime, EndDateTime, TkNo, s_memo)");
                    sQuery.Append($"Values(@Dong, @Ho, @CarNo, @StartDt, @EndDt, @TKNo, '{actType}')");

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

                    iRet += DA.ExecuteNonQuery(sQuery.ToString(), oInsetParams, Transaction);

                    iRet += InsertKMHistory(Transaction,
                        "추가",
                        "홈넷",
                        Helper.NVL(dicParam[ColName.Dong], "0"),
                        Helper.NVL(dicParam[ColName.Ho], "0"),
                        Helper.NVL(dicParam[ColName.CarNo]),
                        Helper.NVL(dicParam[ColName.TKNo]));


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

        public int InsertKMHistory(IDbTransaction transaction, string prsType, string actor, string dong, string ho, string carno, string tkno)
        {
            QueryString sQuery = new QueryString();
            sQuery.Append($"INSERT INTO VisitInfo_His(");
            sQuery.Append($"dong, ho, CarNo, StartDateTime, EndDateTime, TKNo, visit_flag, s_memo, Inout, RecordTime,");
            sQuery.Append($"period, sDelete_YN, dtReg, s_prs_type, s_actor)");
            sQuery.Append($"SELECT dong, ho, CarNo, StartDateTime, EndDateTime, TKNo, visit_flag, s_memo, Inout, RecordTime,");
            sQuery.Append($"period, sDelete_YN, GETDATE() as dtReg, '{prsType}' as s_prs_type, '{actor}' as s_actor");
            sQuery.Append($"FROM VisitInfo");
            sQuery.Append($"WHERE dong = {dong}");
            sQuery.Append($"AND ho = {ho}");
            sQuery.Append($"AND CarNo = '{carno}'");
            sQuery.Append($"AND TKNo = '{tkno}'");

            Log.WriteLog(LogType.Info, "VisitInfoMode | InsertKMHistory", $"{sQuery.ToString()}", LogAdpType.DataBase);

            return DA.ExecuteNonQuery(sQuery.ToString(), transaction);
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
                var invParam = InvaildParameter(dicParam);

                if (invParam != "")
                {
                    result.code = ModelCode.InvalidParameter;
                    result.Message = $"Invalid Parameter : {invParam}";
                }
                else
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
                    else if (iRet == 0)
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
            }
            catch (Exception ex)
            {
                result.code = ModelCode.Exception;
                result.Message = ex.Message;
                Transaction?.Rollback();
            }

            return result;
        }

        /// <summary>
        /// 방문차량 삭제(카카오모빌)
        /// </summary>
        /// <param name="dicParam"></param>
        /// <returns></returns>
        public ModelResult DeleteKM(Dictionary<ColName, object> dicParam)
        {
            ModelResult result = new ModelResult();

            try
            {

                var invParam = InvaildParameter(dicParam);

                if (invParam != "")
                {
                    result.code = ModelCode.InvalidParameter;
                    result.Message = $"Invalid Parameter : {invParam}";
                }
                else
                {
                    Transaction = DA.GetTransaction();

                    QueryString sQuery = new QueryString();
                    sQuery.Append("UPDATE VisitInfo");
                    sQuery.Append("SET sDelete_YN = 'Y'");
                    sQuery.Append("WHERE dong = @Dong");
                    sQuery.Append("AND ho = @Ho");
                    sQuery.Append("AND TKNo = @TKNo");
                    sQuery.Append("AND CarNo = @CarNo");

                    WriteLog("VisitInfoMode | Delete", sQuery.ToString(), dicParam);

                    IDbDataParameter[] oParams = new IDbDataParameter[4];
                    oParams[0] = DataAccessFactory.Instance.GetParameter("Dong", NPDBType.Integer);
                    oParams[0].Value = int.Parse(Helper.NVL(dicParam[ColName.Dong], "0"));

                    oParams[1] = DataAccessFactory.Instance.GetParameter("Ho", NPDBType.Integer);
                    oParams[1].Value = int.Parse(Helper.NVL(dicParam[ColName.Ho], "0"));

                    oParams[2] = DataAccessFactory.Instance.GetParameter("TKNo", NPDBType.VarChar);
                    oParams[2].Value = Helper.NVL(dicParam[ColName.TKNo]);

                    oParams[3] = DataAccessFactory.Instance.GetParameter("CarNo", NPDBType.VarChar);
                    oParams[3].Value = Helper.NVL(dicParam[ColName.CarNo]);

                    int iRet = DA.ExecuteNonQuery(sQuery.ToString(), oParams, Transaction);

                    iRet += InsertKMHistory(Transaction,
                            "삭제",
                            "홈넷",
                            Helper.NVL(dicParam[ColName.Dong], "0"),
                            Helper.NVL(dicParam[ColName.Ho], "0"),
                            Helper.NVL(dicParam[ColName.CarNo]),
                            Helper.NVL(dicParam[ColName.TKNo]));

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
                        result.Message = "삭제 할 방문차량 데이터가 없습니다.";
                    }
                    else
                    {
                        Transaction?.Rollback();
                        result.code = ModelCode.Fail;
                        result.Message = "삭제실패";
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
    }
}
