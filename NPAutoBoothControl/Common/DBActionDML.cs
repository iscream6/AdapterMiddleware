using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NPManager;
using FadeFox.Text;

namespace Common
{
    public class DBActionDML
    {
        #region 조회 쿼리

        /// <summary>
        /// Booth 별 최종 마감 이후 받은 총 카드 금액을 반환한다.
        /// </summary>
        /// <param name="unitNo"></param>
        /// <returns></returns>
        /// <remarks>2020-06-10 이재영 Created</remarks>
        public static string GetCurrentTotalVanAmt(ParkInfo pParkInfo, string unitNo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT MNo, Sum(V.VanAmt) AS TotalVanAmt ");
            sql.Append("FROM VanList V FULL OUTER JOIN ( ");
            sql.Append("				SELECT ParkNo, UnitNo, MAX(ProcDate) AS LatestDate ");
            sql.Append("				FROM ( ");
            sql.Append("					Select ParkNo, UnitNo, CDate, ProcDate + ' ' + ProcTime AS ProcDate ");
            sql.Append("					from TKProcClosing ");
            sql.Append("					 ) K ");
            sql.Append("				GROUP BY ParkNo, UnitNo ");
            sql.Append("				) U ");
            sql.Append("ON V.ParkNo = U.ParkNo ");
            sql.Append("AND V.UnitNo = U.UnitNo ");
            sql.Append("WHERE V.ProcDate + ' ' + V.ProcTime BETWEEN U.LatestDate AND (select convert(varchar, getdate(), 120)) ");
            sql.Append("AND V.VanCheck = 1 ");
            sql.Append($"AND V.MNo = '{unitNo}' ");
            sql.Append("GROUP BY MNo ");

            try
            {
                //TextCore.ACTION(TextCore.ACTIONS.TCPIP, "DBActionDML | GetCurrentTotalVanAmt", sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                DataTable t = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());

                if (t.Rows.Count > 0)
                {
                    return t.Rows[0]["TotalVanAmt"].ToString();
                }
                else
                {
                    return "0";
                }
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return "0";
            }
        }


        //2020-04 이재영 : 신용카드 취소기능 추가
        /// <summary>
        /// 결제취소대상 차량 정보 조회
        /// </summary>
        /// <param name="pParkInfo"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static DataTable GetCardPaiedCars(ParkInfo pParkInfo, Dictionary<string, object> param)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("SELECT T.CarNo, T.TkNo, ");
            sql.Append("       CASE T.TkType WHEN 1 THEN '일반' WHEN 2 THEN '정기' ELSE CONVERT(varchar, T.TkType) END TkType, ");
            sql.Append("	   T.ParkNo, "); //주차장번호
            sql.Append("	   P.ParkName, "); //주차장명
            sql.Append("	   T.UnitNo, "); //기기번호
            sql.Append("	   U.UnitName, "); //기기명
            sql.Append("	   T.InDate, "); //입차일자
            sql.Append("	   T.InTime, "); //입차시간
            sql.Append("	   V.Reserve2 CardName, "); //카드발급사명
            sql.Append("	   V.CardNo, "); //카드번호
            sql.Append("	   V.VanAmt, "); //승인금액
            sql.Append("	   V.VanDate, "); //승인일자
            sql.Append("	   V.VanTime, "); //승인시간
            sql.Append("	   V.VanRegNo, "); //승인번호
            sql.Append("       CASE T.TkType ");
            sql.Append("          WHEN 1 THEN(SELECT TOP(1) OutDate FROM IONData I WHERE I.TKNo = T.TKNo) ");
            sql.Append("          WHEN 2 THEN(SELECT TOP(1) OutDate FROM IOSData S WHERE S.TKNo = T.TKNo) END OutDate, ");
            sql.Append("       CASE T.TkType");
            sql.Append("          WHEN 1 THEN(SELECT TOP(1) OutTime FROM IONData I WHERE I.TKNo = T.TKNo) ");
            sql.Append("          WHEN 2 THEN(SELECT TOP(1) OutTime FROM IOSData S WHERE S.TKNo = T.TKNo) END OutTime ");
            sql.Append("  FROM TkProc T ");
            sql.Append(" INNER JOIN VanList V ");
            sql.Append("    ON T.TKNo = V.Reserve1 ");
            sql.Append("   AND T.ParkNo = V.ParkNo ");
            sql.Append(" INNER JOIN UnitInfo U ");
            sql.Append("    ON T.ParkNo = U.ParkNo ");
            sql.Append("   AND T.UnitNo = U.UnitNo ");
            sql.Append(" INNER JOIN ParkInfo P ");
            sql.Append("    ON T.ParkNo = P.ParkNo ");
            sql.Append(" WHERE T.PayType = 2 ");
            sql.Append("   AND T.RecvAmt != 0 ");
            sql.Append("   AND V.VanCheck = 1 ");

            if (param.ContainsKey("CARNO"))
            {
                sql.Append($"   AND T.CarNo LIKE '%{param["CARNO"]}%' ");
            }

            if (param.ContainsKey("FROMDATE") && param.ContainsKey("TODATE"))
            {
                sql.Append($"   AND CONVERT(DATETIME, V.VanDate + ' ' + V.VanTime) BETWEEN '{param["FROMDATE"]} 00:00:00' AND '{param["TODATE"]} 23:59:59' ");
            }

            if (param.ContainsKey("TKTYPE") && param["TKTYPE"].ToString() != "0")
            {
                sql.Append($"   AND T.TkType = {param["TKTYPE"]} ");
            }

            sql.Append(" ORDER BY V.VanDate DESC, V.VanTime DESC ");

            try
            {
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }


        /// <summary>
        /// 근무자 ID 및 근무자 명 조회
        /// </summary>
        /// <param name="pParkInfo"></param>
        /// <returns></returns>
        public static DataTable GetUserInfo(ParkInfo pParkInfo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" Select MNO,MNAME from Manager where ParkNo=" + pParkInfo.ParkNo + "");
            sql.Append(" UNION ALL ");
            sql.Append(" select '255' MNO,'시스템관리자' MNAME ");
            try
            {
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }




        public static DataTable GetLprFindFromBooth(ParkInfo pParkInfo, string pBoothNo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select * from unitinfo where myno in ");
            sql.Append(" (select UnitNo from dbo.UnitInfo where (UnitKind='12' or UnitKind='13')) and (unitkind<>11 and UnitKind<>10 and unitkind<>22) and myno='" + pBoothNo + "'");

            sql.Append(" order by UnitNo ");
            try
            {
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }

        public static DataTable GetCameraInfo(ParkInfo pParkInfo, string pBoothNo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select UnitNo,ipno,portno,reserve3 AS CameraID,reserve4 AS CameraPW");
            sql.Append("  from UnitInfo ");
            sql.Append(" where UnitKind='22' and myno='" + pBoothNo + "'");

            sql.Append(" order by UnitNo ");
            try
            {
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }


        public static DataTable GetInUnitInfo(ParkInfo pParkInfo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" Select UnitNo,UnitName from UnitInfo where ParkNo=" + pParkInfo.ParkNo + "");
            sql.Append(" and UnitKind=8 ");
            try
            {
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }


        public static DataTable GetValidUser(ParkInfo pParkInfo)
        {

            string sql = String.Format("Select Authority"
                                      + "  from Manager"
                                      + " where ParkNo = '{0}'"
                                      + "   and MNO = '{1}'"
                                      + "   and Passwd = '{2}'", pParkInfo.ParkNo, pParkInfo.CurrentUserInfo.UserId, pParkInfo.CurrentUserInfo.UserPwd);
            try
            {
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return pParkInfo.CurrentDbServer.MSSql.SelectT(sql);
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }

        }

        /// <summary>
        /// 각 주차장의 데이터베이스 정보를 가져온다
        /// </summary>
        /// <param name="pParkInfo"></param>
        /// <returns></returns>
        public static DataTable GetParkDataBaseInfo(ParkInfo pParkInfo)
        {

            StringBuilder sql = new StringBuilder();
            //sql.Append("Select ParkNo,ParkName,Reserve6 DatabaseIp,Reserve7 DatabasePort,Reserve8 DatabaseName,Reserve9 DatabaseUSerId,Reserve10 DatabasePwd");
            sql.Append("Select ParkNo,ParkName");
            sql.Append("  from ParkInfo ");
            sql.Append(" where ParkNo = " + pParkInfo.ParkNo + "");
            try
            {

                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return dtParkDatabaseInfo;
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }

        }

        public static DataTable GetDiscountITable(ParkInfo pParkInfo, int p_ParkId)
        {

            StringBuilder sql = new StringBuilder();


            try
            {

                sql.Append(" select DCNo DiscountCode,DCName DiscountName,0 DiscountPubLimit  ,0 DiscountPubCharge, DCvalue DiscountValue ,");
                //sql.Append("        dctype DiscountType,Reserve1,Reserve2,Reserve3,Reserve4,Reserve5,CamUse from DCInfo ");
                // 무인정산기 3.1.47 이후버젼만처리
                sql.Append("        dctype DiscountType,Reserve1,Reserve2,Reserve3,Reserve4,Reserve5,CamUse,RemarkUse from DCInfo ");
                // 무인정산기 3.1.47 이후버젼만처리 완료
                //sql.Append("  Where Parkno ='" + p_ParkId + "'");
                //카메라 어안카메라 두개처리
                sql.Append("  Where Parkno ='" + p_ParkId + "' and RemoteButtonUse=1 order by VisibleCnt");
                //카메라 어안카메라 두개처리주석완료
                DataTable dtDiscountInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                return dtDiscountInfo;

            }
            catch (Exception ex)
            {
                DataTable resultdatetable = null;
                return resultdatetable;
            }

        }
        public static int GetMaxLevel(ParkInfo pParkInfo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("Select COUNT(ProgramLevel)  ProgramLevel From (Select ProgramLevel From UserProgram group by ProgramLevel ) a");
            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return Convert.ToInt32(dtParkDatabaseInfo.Rows[0]["ProgramLevel"].ToString());

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return 0;
            }

        }

        public static int GetUserProgamInfo(ParkInfo pParkInfo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("Select COUNT(ProgramLevel)  ProgramLevel From (Select ProgramLevel From UserProgram group by ProgramLevel ) a");
            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return Convert.ToInt32(dtParkDatabaseInfo.Rows[0]["ProgramLevel"].ToString());

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return 0;
            }

        }

        public static DataTable ParkingLot(ParkInfo pParkInfo)
        {
            StringBuilder sql = new StringBuilder();


            try
            {


                sql.Append(" select ParkNo,ParkName,ParkAddr  ParkAddress, RegNo ParkBusinessCode, Telephone  ParkPhone ,admin ParkPresident ");
                sql.Append("   from ParkInfo ");
                sql.Append("  where ParkNo =" + pParkInfo.ParkNo + "");

                DataTable resultDt = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());

                return resultDt;
            }
            catch (Exception ex)
            {
                DataTable resultdatetable = null;
                return resultdatetable;
            }

        }

        public static bool GetNexpaChkHoliday(ParkInfo pMainParkInfo, int pParkNo, string pyyyyMMdd)
        {
            StringBuilder sql = new StringBuilder();


            try
            {
                DataTable resultDt = null;

                sql.Append(" Select * from Holiday where ParkNo =" + pParkNo + " and HDate ='" + pyyyyMMdd + "' ");



                resultDt = pMainParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                if (resultDt == null || resultDt.Rows.Count == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public static DataTable GetNexpaParkInfo(ParkInfo pMainParkInfo, int pParkNo)
        {
            StringBuilder sql = new StringBuilder();


            try
            {
                DataTable resultDt = null;

                sql.Append("  SELECT DayMax             ");
                sql.Append("         ,TotMax             ");
                sql.Append("	     ,JunkType             ");
                sql.Append("	     ,RoundType            ");
                sql.Append("	     ,TimeDCType           ");
                sql.Append("	     ,SeekDay            ");
                sql.Append("	     ,Reserve5            ");
                sql.Append("	     ,Reserve3            ");
                sql.Append("	     ,Reserve4            ");
                sql.Append("	 FROM ParkInfo              ");
                sql.Append("   where  ParkNo =" + pParkNo + "");


                resultDt = pMainParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                return resultDt;
            }
            catch (Exception ex)
            {
                DataTable resultdatetable = null;
                return resultdatetable;
            }
        }

        public static DataTable GetNexpaGraceTimeInfo(ParkInfo pMainParkInfo, int pParkNo)
        {
            StringBuilder sql = new StringBuilder();


            try
            {
                DataTable resultDt = null;

                sql.Append("  SELECT GT1             ");
                sql.Append("         ,GT2             ");
                sql.Append("	     ,GT3             ");
                sql.Append("	     ,GT4            ");
                sql.Append("	     ,GT5           ");
                sql.Append("	     ,GT6            ");
                sql.Append("	     ,GT7            ");
                sql.Append("	     ,GT8           ");
                sql.Append("	     ,GT9            ");

                sql.Append("	 FROM Gracetime              ");
                sql.Append("   where  ParkNo = " + pParkNo + " ");

                resultDt = pMainParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());

                return resultDt;
            }
            catch (Exception ex)
            {
                DataTable resultdatetable = null;
                return resultdatetable;
            }
        }
        public static DataTable GetGroupInfo(ParkInfo pParkInfo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append("select groupNo,groupName from ggroup where parkno=" + pParkInfo.ParkNo + "");
            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return dtParkDatabaseInfo;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }

        }


        public static DataTable GetAutoboothUnitInfo(ParkInfo pParkInfo, int pParkNo, NPINCarConfig p_npincarConfig)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select UnitNo,unitname ");
            sql.Append("  from UnitInfo ");
            sql.Append(" where  parkno=" + pParkNo + "");
            sql.Append("  and unitKind in('12','13')");
            if (p_npincarConfig.UseNormalBooth)
            {
                sql.Append(" and Reserve2=1 ");
            }
            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return dtParkDatabaseInfo;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }

        public static DataTable GetAutoBoothIpInfo(ParkInfo pParkInfo, NPINCarConfig pINCarConfig)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select UnitNo,IpNo,PortNo,Reserve4,Reserve5 ");
            sql.Append("  from UnitInfo ");
            sql.Append(" where  parkno=" + pParkInfo.ParkNo.ToString() + " ");
            if (pINCarConfig.UseNormalBooth)
            {
                sql.Append("  and unitKind in('12','13','3') and Reserve2=1");
            }
            else
            {
                sql.Append("  and unitKind in('12','13','3')");
            }
            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return dtParkDatabaseInfo;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }

        }


        public static DataTable GetGroupUnitInfo(ParkInfo pParkInfo, int pParkNo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select parkno,unitkind,SUM(1) Devicecount");
            sql.Append("   from unitinfo where UnitKind in(8,9,3,12,13) ");
            sql.Append("    and parkno=" + pParkNo + "");
            sql.Append("   group by ParkNo,UnitKind ");
            sql.Append(" and parkno=" + pParkNo + "");
            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return dtParkDatabaseInfo;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }

        }

        public static DataTable GetRegMasterCar_Expire(ParkInfo pParkInfo, int pParkNo, string p_Carnumber)
        {
            StringBuilder sql = new StringBuilder();
            try
            {
                sql.Append(" select TKNo,MarkNo,APB,IOStatusNo,LastUseDate,LastUseTime,WPNO ");
                sql.Append("       ,GroupNo,Name,LastUnitNo, CompName,DeptName,ExpDateF,ExpDateT ");
                sql.Append("       ,CARNo ,Reserve1 ,0 OutCHk ");
                sql.Append("   from custinfo ");
                sql.Append("    Where CarNo = '" + p_Carnumber + "' and TKType = 2  AND  Status <3 and parkNo=" + pParkNo + " ");
                DataTable l_Result = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return l_Result;
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;

            }
        }

        public static string GetRegCarnumberSearchSixNumber(ParkInfo pParkInfo, int pParkNo, string p_Carnumber)
        {
            StringBuilder sql = new StringBuilder();
            try
            {
                string lFirstCarnumber = p_Carnumber.Substring(0, 2);
                string lSecondCarnumber = p_Carnumber.Substring(2);
                sql.Append(" select TKNo,MarkNo,APB,IOStatusNo,LastUseDate,LastUseTime,WPNO ");
                sql.Append("       ,GroupNo,Name,LastUnitNo, CompName,DeptName,ExpDateF,ExpDateT ");
                sql.Append("       ,CARNo ,Reserve1,0 OutCHk ");
                sql.Append("   from custinfo ");
                sql.Append("    Where CarNo like '%" + lFirstCarnumber + "%" + lSecondCarnumber + "%' and TKType = 2  AND  Status <3 and ParkNo=" + pParkNo + " ");

                DataTable l_Result = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                if (l_Result == null || l_Result.Rows.Count == 0)
                {
                    return "";
                }
                else
                {
                    string l_Expired = l_Result.Rows[0]["ExpDateT"].ToString().Replace("-", "").Replace(":", "");
                    string l_Start = l_Result.Rows[0]["ExpDateF"].ToString().Replace("-", "").Replace(":", "");
                    if ((Convert.ToInt32(l_Expired) >= Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"))) &&
                         (Convert.ToInt32(l_Start) <= Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"))))
                    {
                        return l_Result.Rows[0]["CARNo"].ToString();
                    }
                    else
                    {
                        return "";
                    }

                }
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return "";

            }
        }

        public static string GetUserAuhonity(ParkInfo pParkInfo, string pProgramName)
        {
            StringBuilder sql = new StringBuilder();
            if (pParkInfo.CurrentUserInfo.CurrentUserLevel == UserInfo.UserLevel.Manager)
            {
                sql.Append("Select UserMainAuthority ");
                sql.Append("  from UserProgram ");
                sql.Append(" where UseYes = 1 and ProgramName='" + pProgramName + "'");

            }
            else if (pParkInfo.CurrentUserInfo.CurrentUserLevel == UserInfo.UserLevel.SubManager)
            {
                sql.Append("Select UserSubAuthority ");
                sql.Append("  from UserProgram ");
                sql.Append(" where UseYes = 1 and ProgramName='" + pProgramName + "'");

            }
            else
            {
                sql.Append("Select UserWorkerAuthority ");
                sql.Append("  from UserProgram ");
                sql.Append(" where UseYes = 1 and ProgramName='" + pProgramName + "'");

            }

            try
            {

                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return dtParkDatabaseInfo.Rows[0][0].ToString();

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return string.Empty;
            }

        }

        public static DataTable GetSqlData(ParkInfo pParkInfo, string pSql)
        {

            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(pSql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return dtParkDatabaseInfo;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }

        }
        /// <summary>
        /// 기존에 차량번호가 있었는지 여부 true면 등록된차량
        /// </summary>
        /// <param name="pParkInfo"></param>
        /// <param name="pSql"></param>
        /// <returns></returns>
        public static bool isAlreadCarNoFromCustInfo(ParkInfo pParkInfo, RegCarData pRegCarData)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" Select Count(*) countCarNo From custInfo where parkNo=" + pParkInfo.ParkNo + " and carno='" + pRegCarData.CarNo + "'");
            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                if (dtParkDatabaseInfo != null)
                {
                    int countCarNo = Convert.ToInt32(dtParkDatabaseInfo.Rows[0]["countCarNo"].ToString());
                    if (countCarNo > 0)
                    {
                        return true;
                    }
                    return false;
                }
                return false;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }

        public static string MakeTkNo(ParkInfo pParkInfo, bool pIsReg)
        {
            StringBuilder sql = new StringBuilder();
            string tkNo = TextCore.ToRightAlignString(3, pParkInfo.ParkNo.ToString(), '0'); // 150930111110  15년09월30일11시11분10초
            tkNo += DateTime.Now.ToString("yyMMddHHmmss");
            // 주차장번호3자리
            tkNo += (pIsReg == true ? "1" : "0"); // 정기권이면 1 정기권이 아니면 0
            tkNo += "1"; // 순번 
            sql.Append(" Select Count(*) countTknoNo From custInfo where parkNo=" + pParkInfo.ParkNo + " and tkno='" + tkNo + "'");
            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                if (dtParkDatabaseInfo != null)
                {
                    int countCarNo = Convert.ToInt32(dtParkDatabaseInfo.Rows[0]["countTknoNo"].ToString());
                    if (countCarNo > 0) // 중복 TKNO가 있다면
                    {
                        StringBuilder sql2 = new StringBuilder();
                        sql2.Append("select max(tkno) maxTkno from custInfo where parkNo=" + pParkInfo.ParkNo + "");
                        DataTable dtMaxTkNo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql2.ToString());
                        long intMaxTkno = Convert.ToInt64(dtMaxTkNo.Rows[0]["maxTkno"].ToString());
                        return tkNo = (intMaxTkno + 1).ToString();

                    }
                    return tkNo;
                }
                return string.Empty;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return string.Empty;
            }

        }
        //미,부분인식/일반/정기차량 입차 차단기개방설정 적용
        public static DataTable GetInCarUnitInfo(ParkInfo pParkInfo, NPINCarConfig p_npincarConfig)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" Select UnitNo,UnitName from UnitInfo where ParkNo=" + pParkInfo.ParkNo + "");
            sql.Append(" and UnitKind=3 ");
            if (p_npincarConfig.UseNormalBooth)
            {
                sql.Append("  and Reserve2=1");
            }
            sql.Append(" order by UnitNo ");
            try
            {
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }
        //미,부분인식/일반/정기차량 입차 차단기개방설정 적용완료
        //사전할인장비인지 체크
        public static bool GetDiscountUnit(ParkInfo pParkInfo, string pUnitNo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" select UnitNo ");
            sql.Append("  from UnitInfo ");
            sql.Append(" where  parkno=" + pParkInfo.ParkNo + "");
            sql.Append("  and unitKind = '3'");
            sql.Append(" and UnitNo= '" + pUnitNo + "' ");
            sql.Append(" and Reserve5=1 ");

            try
            {
                DataTable dtParkDatabaseInfo = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                if (dtParkDatabaseInfo != null && dtParkDatabaseInfo.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }
        }

        public static DataTable GetCarInfo(ParkInfo pParkInfo, string pCarNo, string pProcDate, string pProcTime)
        {
            try
            {
                StringBuilder sql = new StringBuilder();

                sql.Append("Select InImage1 , InCarNo1, InImage2, InCarNo2 ");
                sql.Append("      ,ProcDate, ProcTIme, TkNO ");
                sql.Append("From IOnData ");
                sql.Append("Where  parkno=" + pParkInfo.ParkNo + " ");
                sql.Append("AND ( InCarNo1 ='" + pCarNo + "' OR InCarNo2 = '" + pCarNo + "' ) ");
                sql.Append("AND ProcDate = '" + NPSYS.ConvetYears_Dash(pProcDate) + "' ");
                sql.Append("AND ProcTime = '" + NPSYS.ConvetDay_Dash(pProcTime) + "' ");

                return pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }

        //단기권 등록 차량 개방 적용
        public static bool GetDayCust(ParkInfo pParkInfo, NormalCarInfo pCarData)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT CarNo ");
            sql.Append(" FROM DayCust  ");
            sql.Append(" WHERE (ProcDate +' ' + ProcTime  <=  '" + NPSYS.ConvetYears_Dash(pCarData.OutYmd) + " " + NPSYS.ConvetDay_Dash(pCarData.OutHms) + "' AND '" + NPSYS.ConvetYears_Dash(pCarData.OutYmd) + " " + NPSYS.ConvetDay_Dash(pCarData.OutHms) + "' <= EndDate + ' ' + EndTime ) "); // 입차일
            sql.Append(" AND CarNo IN ('" + pCarData.OutCarNumber + "', '" + pCarData.OutCarNumber + "') ");
            try
            {
                DataTable resultDt = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
                if (resultDt != null && resultDt.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }
        }

        public static string GetBarControlUnit(ParkInfo pParkInfo)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(" SELECT UnitNo ");
                sql.Append(" FROM UnitInfo ");
                sql.Append(" WHERE UnitKind=3 AND Reserve5='1' ");

                return pParkInfo.CurrentDbServer.MSSql.SelectC(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return string.Empty;
            }
        }
        //단기권 등록 차량 개방 적용완료

        //SMS전원제어장비 조회
        public static DataTable GetSMSPowerControlUnit(ParkInfo pParkInfo)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append(" SELECT UnitNo,UnitName,UnitKind,Comport,IPNo,PortNo,Reserve1 ");
            sql.Append(" FROM UnitInfo WHERE UnitKind=23 ");

            try
            {
                DataTable dt = new DataTable();

                return dt = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;

                return null;
            }
        }

        //SMS전원제어장비의 연결장비조회
        public static DataTable GetSMSUnitFind(ParkInfo pParkInfo, string pMyno)
        {
            StringBuilder sql = new StringBuilder();

            sql.Append(" SELECT UnitNo,UnitName,UnitKind,MyNo,Comport,IPNo,PortNo,Reserve1 ");
            sql.Append(" FROM UnitInfo WHERE MyNo=" + pMyno);

            try
            {
                DataTable dt = new DataTable();

                return dt = pParkInfo.CurrentDbServer.MSSql.SelectT(sql.ToString());
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return null;
            }
        }
        #endregion

        #region Delete

        public static bool DeleteCustInfoCar(ParkInfo pParkInfo, string pTkNo, string pCarNumber)
        {

            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("Delete custInfo where ParkNo = " + pParkInfo.ParkNo + " and TkNo = '" + pTkNo + "'");
                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }


        #endregion

        #region Insert
        /// <summary>
        /// 정기권차량등록
        /// </summary>
        /// <param name="pParkInfo"></param>
        /// <param name="pTkNo"></param>
        /// <param name="pCarNumber"></param>
        /// <returns></returns>
        public static bool InsertCustInfoCar(ParkInfo pParkInfo, RegCarData pRegCarData)
        {

            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("Insert into CustInfo ");
                sql.Append("(ParkNo, TKType, GroupNo, Name ");
                sql.Append(" ,TelNo, CarNo, CompName ");
                sql.Append(" ,DeptName, Address, IssueDate");
                sql.Append(" , IssueAmt, Status, ExpDateF ");
                sql.Append(" ,ExpDateT, WPNo, LastParkNo");
                sql.Append(" , LastUnitNo, LastUseDate, LastUseTime");
                sql.Append(" ,IOStatusNo, CurrAmt, TKNo");
                sql.Append(", APB, CarType, Reserve1,Reserve7,Reserve8) ");
                sql.Append(" values(" + pParkInfo.ParkNo + "," + pRegCarData.TkType + "," + pRegCarData.GroupNo + ",'" + pRegCarData.Name + "'");
                sql.Append(" ,'" + pRegCarData.TellNo + "','" + pRegCarData.CarNo + "','" + pRegCarData.CompName + "'");
                sql.Append(" ,'" + pRegCarData.DeptName + "','" + pRegCarData.Address + "','" + pRegCarData.IssueDate + "'");
                sql.Append(" ," + pRegCarData.IssueAmt + "," + pRegCarData.Status + ",'" + pRegCarData.ExpDateF + "'");
                sql.Append(" ,'" + pRegCarData.ExpDateT + "'," + pRegCarData.WPNo + "," + pRegCarData.LastParkNo + "");
                sql.Append(" ," + pRegCarData.LastUnitNo + ",'" + pRegCarData.LastUseDate + "','" + pRegCarData.LastUseTime + "'");
                sql.Append(" ," + pRegCarData.IOStatusNo + "," + pRegCarData.CurrAmt + ",'" + pRegCarData.TkNo + "'");
                sql.Append(" ," + pRegCarData.APB + "," + pRegCarData.CarType + ",'" + pRegCarData.Reserve1 + "','" + pRegCarData.Reserve7 + "','" + pRegCarData.Reserve8 + "')");
                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }

        public static bool InsetCustInfoHistory(ParkInfo pParkInfo, RegCarData pRegCarData)
        {

            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("Insert Into IssueTK ");
                sql.Append("(ParkNo, IssueDate, IssueTime");
                sql.Append(" , MNo, TKType, TKNo");
                sql.Append(" , CarType, IssueType, IssueUnit");
                sql.Append(" , ExpDateF, ExpDateT, FValue");
                sql.Append(" , ChkClosing, CarNo, WPNo,Reserve7,Reserve8) ");
                sql.Append(" Values (" + pRegCarData.ParkNo + ",'" + pRegCarData.LastUseDate + "','" + pRegCarData.LastUseTime + "'");
                sql.Append(" ," + pParkInfo.CurrentUserInfo.UserId + "," + pRegCarData.TkType + ",'" + pRegCarData.TkNo + "'");
                sql.Append(" ," + pRegCarData.CarType + "," + pRegCarData.IssueType + ",1");
                sql.Append(" ,'" + pRegCarData.ExpDateF + "','" + pRegCarData.ExpDateT + "'," + pRegCarData.IssueAmt);
                sql.Append(" ,0,'" + pRegCarData.CarNo + "'," + pRegCarData.WPNo + ",'" + pRegCarData.Reserve7 + "','" + pRegCarData.Reserve8 + "')");

                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }

        public static bool UpdateCustInfoCar(ParkInfo pParkInfo, RegCarData pRegCarData)
        {

            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("Update CustInfo ");
                sql.Append(" set TKType=" + pRegCarData.TkType + "");
                sql.Append("    , GroupNo=" + pRegCarData.GroupNo + "");
                sql.Append("    , Name='" + pRegCarData.Name + "'");
                sql.Append("    , TelNo='" + pRegCarData.TellNo + "'");
                sql.Append("    , CarNo='" + pRegCarData.CarNo + "'");
                sql.Append("    , CompName='" + pRegCarData.CompName + "'");
                sql.Append("    , DeptName='" + pRegCarData.DeptName + "'");
                sql.Append("    , Address='" + pRegCarData.Address + "'");
                sql.Append("    , IssueDate='" + pRegCarData.IssueDate + "'");
                sql.Append("    , IssueAmt=" + pRegCarData.IssueAmt + "");
                sql.Append("    , Status=" + pRegCarData.Status + "");
                sql.Append("    , ExpDateF='" + pRegCarData.ExpDateF + "'");
                sql.Append("    , ExpDateT='" + pRegCarData.ExpDateT + "'");
                sql.Append("    , WPNo=" + pRegCarData.WPNo + "");
                sql.Append("    , LastParkNo=" + pRegCarData.LastParkNo + "");
                sql.Append("    , LastUnitNo=" + pRegCarData.LastUnitNo + "");
                sql.Append("    , LastUseDate='" + pRegCarData.LastUseDate + "'");
                sql.Append("    , LastUseTime='" + pRegCarData.LastUseTime + "'");
                sql.Append("    , IOStatusNo=" + pRegCarData.IOStatusNo + "");
                sql.Append("    , CurrAmt=" + pRegCarData.CurrAmt + "");
                sql.Append("    , APB=" + pRegCarData.APB + "");
                sql.Append("    , CarType=" + pRegCarData.CarType + "");
                sql.Append("    , Reserve1='" + pRegCarData.Reserve1 + "'");
                sql.Append("    , Reserve7='" + pRegCarData.Reserve7 + "'");
                sql.Append("    , Reserve8='" + pRegCarData.Reserve8 + "'");
                sql.Append(" Where Parkno=" + pParkInfo.ParkNo + " and Tkno='" + pRegCarData.TkNo + "'");

                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }

        public static bool UpdateIONCarNumber(ParkInfo pParkInfo, string pTkno, string pInCarNumber, string pInBackCarNumber, string pCarType)
        {

            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("Update IONDATA ");
                sql.Append(" set inCarNo1='" + pInCarNumber + "'");
                //sql.Append("    , InCarNo2='" + pInBackCarNumber + "'");
                sql.Append("    , CarType='" + pCarType + "'");
                sql.Append("    , InRecog1=1,  Reserve5='차량번호수정' ");
                sql.Append("    , Reserve4='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' ");
                sql.Append(" Where Parkno=" + pParkInfo.ParkNo + " and Tkno='" + pTkno + "'");

                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }


        public static bool UpdateBackIONCarNumber(ParkInfo pParkInfo, string pTkno, string pInCarNumber, string pInBackCarNumber, string pCarType)
        {

            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append("Update IONDATA ");
                sql.Append(" set InCarNo2='" + pInBackCarNumber + "'");
                sql.Append("    , CarType='" + pCarType + "'");
                sql.Append("    , InRecog2=1,  Reserve5='차량번호수정' ");
                sql.Append("    , Reserve4='" + DateTime.Now.ToString("yyyyMMddHHmmss") + "' ");
                sql.Append(" Where Parkno=" + pParkInfo.ParkNo + " and Tkno='" + pTkno + "'");

                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }



        public static bool UpdateCarOutProcess(ParkInfo pParkInfo, DataRow[] pNormalCarInfo, string pReasonText)
        {

            try
            {
                StringBuilder sql = new StringBuilder();
                if (pNormalCarInfo[0]["TkType"].ToString() == "2" || pNormalCarInfo[0]["TkType"].ToString() == "4") // 정기차량
                {
                    sql.Append("Update IOSDATA Set outchk=7 ,OutDate ='" + pNormalCarInfo[0]["ProcDate"].ToString() + "' ,OutTime='" + pNormalCarInfo[0]["ProcTime"].ToString() + "'  ");
                }
                else
                {
                    sql.Append("Update IONDATA Set outchk=7 ,OutDate ='" + pNormalCarInfo[0]["ProcDate"].ToString() + "' ,OutTime='" + pNormalCarInfo[0]["ProcTime"].ToString() + "'  ");
                }
                sql.Append(", Reserve10='" + pReasonText + "' ");
                sql.Append(" where ParkNO='" + pParkInfo.ParkNo + "' ");
                sql.Append("     and TkNo='" + pNormalCarInfo[0]["TkNO"].ToString() + "' ");
                sql.Append("     and Procdate='" + pNormalCarInfo[0]["ProcDate"].ToString() + "' ");
                sql.Append("     and Proctime='" + pNormalCarInfo[0]["ProcTime"].ToString() + "' ");

                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }
        public static bool InSertIonData(ParkInfo pParkInfo, FormInCar.InData pInData)
        {
            try
            {
                StringBuilder sql = new StringBuilder();
                sql.Append(" Insert Into Iondata(ParkNo,UnitNo,ProcDate,ProcTime,TKNo,InCarNo1,CarType,tktype,status) ");
                sql.Append(" values('" + pParkInfo.ParkNo + "','" + pInData.InUnit + "','" + pInData.InDate + "','" + pInData.InTime + "','" + MakeTkNo(pParkInfo, false) + "','" + pInData.CarNumber + "',1,1,1)");

                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }

        //public static bool UpdateCarAllOutProcess(ParkInfo pParkInfo, string pInYmd, string pInHms, string pTkno, string pTktype, string pInCarNo)
        public static bool UpdateCarAllOutProcess(ParkInfo pParkInfo, string pStartYMD, string pEndYMD, string pSearch)
        {

            try
            {
                StringBuilder sqlION = new StringBuilder();
                StringBuilder sqlIOS = new StringBuilder();

                string strOutDate;
                string strOutTime;

                strOutDate = System.DateTime.Now.ToString("yyyy-MM-dd");
                strOutTime = System.DateTime.Now.ToString("HH:mm:ss");

                sqlION.Append("UPDATE IONData SET outchk=7 , OutDate ='" + strOutDate + "' ,OutTime='" + strOutTime + "' ");
                sqlION.Append(" WHERE ParkNO='" + pParkInfo.ParkNo + "' ");
                sqlION.Append(" AND Procdate >= '" + pStartYMD + "' AND Procdate <= '" + pEndYMD + "' ");
                switch (pSearch)
                {
                    case "0":
                        sqlION.Append(" AND Outchk=0 ");
                        break;
                    case "1":
                        sqlION.Append(" AND Outchk=0 and inrecog<>1 ");
                        break;
                }


                sqlIOS.Append("UPDATE IOSData SET OutDate='" + strOutDate + "' ,OutTime='" + strOutTime + "' ");
                sqlIOS.Append(" WHERE ParkNO='" + pParkInfo.ParkNo + "' ");
                sqlIOS.Append(" AND Procdate >= '" + pStartYMD + "' AND Procdate <= '" + pEndYMD + "' ");
                switch (pSearch)
                {
                    case "0":
                        sqlIOS.Append(" AND OutDate is NULL");
                        break;
                    case "1":
                        sqlIOS.Append(" AND OutDate is NULL and inrecog<>1 ");
                        break;
                }

                pParkInfo.CurrentDbServer.MSSql.Execute(sqlION.ToString());
                pParkInfo.CurrentDbServer.MSSql.Execute(sqlIOS.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;
                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }

        }


        public static bool UpdateCarType(ParkInfo pParkInfo, string pTKNo, string pYMD, string pHMS, string pInCarNo, bool bCheck)
        {

            try
            {
                StringBuilder sql = new StringBuilder();

                if (bCheck)  // 대형차요금 적용확인
                {
                    sql.Append("Update IONDATA Set CarType='3' ");

                    sql.Append("where (InCarNo1='" + pInCarNo + "' OR InCarNo2='" + pInCarNo + "')");
                    sql.Append("  and TKNo='" + pTKNo + "' ");
                    sql.Append("  and ProcDate='" + pYMD + "' ");
                    sql.Append("  and ProcTime='" + pHMS + "' ");
                    sql.Append("  and OutChk='0'");
                }


                pParkInfo.CurrentDbServer.MSSql.Execute(sql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;

                return true;

            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }
        }


        // 사전할인등록 적용
        /// <summary>
        /// 사전할인등록 
        /// </summary>
        /// <param name="p_mssql"></param>
        /// <param name="p_incarInfo"></param>
        /// <param name="p_dt"></param>
        public static bool InsertRegPreDiscount(ParkInfo pParkInfo, NormalCarInfo pCarInfo, string pUnitNo)
        {


            try
            {
                StringBuilder insertsql = new StringBuilder();
                insertsql.Append(" INSERT INTO tblStoreDiscountPublish");
                insertsql.Append("             (ParkCode ");
                insertsql.Append("             ,PubYMD");
                insertsql.Append("             ,PubHMS");
                insertsql.Append("             ,StoreID");
                insertsql.Append("             ,StoreName");
                insertsql.Append("             ,DiscountName, DiscountCode ,DiscountType,DiscountValue");
                insertsql.Append("             ,DiscountCount");
                insertsql.Append("             ,CarNumber");
                insertsql.Append("             ,InYMD");
                insertsql.Append("             ,InHMS");
                insertsql.Append("             ,InCarPath");
                insertsql.Append("             ,NorKey");
                //insertsql.Append("             ,Reserve12");
                insertsql.Append("             ,UnitNo");
                insertsql.Append("             ,DCTypeCode)");
                insertsql.Append("             SELECT '" + pParkInfo.ParkNo + "'");
                insertsql.Append("            ,'" + NPSYS.ConvetYears_Dash(pCarInfo.InYMD) + "'");
                insertsql.Append("            ,'" + NPSYS.ConvetDay_Dash(pCarInfo.InHMS) + "'");
                insertsql.Append("            ,'" + pUnitNo + "'");
                insertsql.Append("            ,'" + "원격할인" + "'");
                insertsql.Append("            ,DCName, DCNo, DCType, DCValue ");
                insertsql.Append("            ,1");
                insertsql.Append("            ,'" + pCarInfo.InCarNumber + "'");
                insertsql.Append("            ,'" + NPSYS.ConvetYears_Dash(pCarInfo.InYMD) + "'");
                insertsql.Append("            ,'" + NPSYS.ConvetDay_Dash(pCarInfo.InHMS) + "'");
                insertsql.Append("            ,'" + pCarInfo.InCarPath + "'");
                insertsql.Append("            ,'" + pCarInfo.TkNO + "'");
                //insertsql.Append("            ,'사전할인등록' ");
                insertsql.Append("            ,'" + pUnitNo + "'");
                insertsql.Append("            ,2 ");
                insertsql.Append("            FROM DCInfo ");
                insertsql.Append("            WHERE PreDiscountUse = 1 ");

                pParkInfo.CurrentDbServer.MSSql.Execute(insertsql.ToString());
                pParkInfo.CurrentDbServer.IsDbConnected = true;

                return true;
            }
            catch (Exception ex)
            {
                pParkInfo.CurrentDbServer.IsDbConnected = false;
                return false;
            }
            finally
            {

            }
        }
        #endregion
    }
}
