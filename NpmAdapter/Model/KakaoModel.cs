using NexpaAdapterStandardLib.DataAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    class KakaoModel : BaseModel
    {
        public DataTable EVENT_SELECT_QUERY(Dictionary<string, object> dicParam)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT * ");
            sQuery.Append("FROM PARKKAKAOEVENT ");
            sQuery.Append("WHERE parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND ReadStat = '1' ");
            sQuery.Append("AND COMPLETE = '0' ");
            sQuery.Append("AND (TK_NO is not NULL AND TK_NO <> '') ");
            sQuery.Append("AND (EventName IS NOT NULL AND EventName <> '') ");
            sQuery.Append("ORDER BY id ASC");

            IDbDataParameter[] oParams = new IDbDataParameter[1];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        public DataTable EVENT_SELECT_QUERY1(Dictionary<string, object> dicParam)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT TOP 100 * ");
            sQuery.Append("FROM PARKKAKAOEVENT ");
            sQuery.Append("WHERE parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND ReadStat = '0' ");
            sQuery.Append("AND COMPLETE = '0' ");
            sQuery.Append("AND (TK_NO is not NULL AND TK_NO <> '') ");
            sQuery.Append("AND (EventName IS NOT NULL AND EventName <> '') ");
            sQuery.Append("ORDER BY id ASC");

            IDbDataParameter[] oParams = new IDbDataParameter[1];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        public DataTable FIELD_ID_SELECT_QUERY(Dictionary<string, object> dicParam, bool isDesc = false)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT TOP 1 FieldID, KAKAOID, TKTYPE ");
            sQuery.Append("FROM PARKKAKAOEVENT ");
            sQuery.Append("WHERE parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND TK_NO = @tk_no ");
            sQuery.Append("AND FieldID <> '' ");
            if (isDesc)
            {
                sQuery.Append("ORDER BY EVENTDATE DESC");
            }

            IDbDataParameter[] oParams = new IDbDataParameter[2];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];

            oParams[1] = DataAccessFactory.Instance.GetParameter("tk_no", NPDBType.Integer);
            oParams[1].Value = dicParam["tk_no"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        /// <summary>
        /// 카카오 등록이 가능한 차량 조회
        /// </summary>
        /// <param name="dicParam"></param>
        /// <param name="isUseKakao"></param>
        /// <returns></returns>
        public DataTable KAKAO_CAR_SELEC_QUERY(Dictionary<string, object> dicParam)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT TOP 1 TK_NO ");
            sQuery.Append("FROM PARKKAKAOEVENT ");
            sQuery.Append("WHERE parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND  FieldID = @fieldid ");
            sQuery.Append("AND  TKTYPE = '1'");

            IDbDataParameter[] oParams = new IDbDataParameter[2];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];

            oParams[1] = DataAccessFactory.Instance.GetParameter("fieldid", NPDBType.VarChar);
            oParams[1].Value = dicParam["fieldid"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        public DataTable KAKAO_CAR_SELEC_QUERY1(Dictionary<string, object> dicParam)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT TOP 1 TK_NO ");
            sQuery.Append("FROM PARKKAKAOEVENT ");
            sQuery.Append("WHERE parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND  FieldID = @fieldid ");
            sQuery.Append("AND  UseKakao = '1'");

            IDbDataParameter[] oParams = new IDbDataParameter[2];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];

            oParams[1] = DataAccessFactory.Instance.GetParameter("fieldid", NPDBType.VarChar);
            oParams[1].Value = dicParam["fieldid"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        /// <summary>
        /// 입출차 내역 조회
        /// </summary>
        /// <param name="dicParam"></param>
        /// <returns></returns>
        public DataTable CAR_SELEC_QUERY(Dictionary<string, object> dicParam)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT FieldID, KAKAOID, TKTYPE, EVENTDATE, EVENTNAME, EVENTVALUE ");
            sQuery.Append("FROM PARKKAKAOEVENT ");
            sQuery.Append("WHERE  parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND  CAR_NO = @car_no ");
            sQuery.Append("AND  EVENTDATE BETWEEN @start_date AND @end_date ");
            sQuery.Append("AND EVENTNAME IN ('car_entered', 'car_exited', 'kakao_exited') ");
            sQuery.Append("ORDER BY EVENTDATE DESC");

            IDbDataParameter[] oParams = new IDbDataParameter[4];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];
            oParams[1] = DataAccessFactory.Instance.GetParameter("car_no", NPDBType.VarChar);
            oParams[1].Value = dicParam["car_no"];
            oParams[2] = DataAccessFactory.Instance.GetParameter("start_date", NPDBType.Char);
            oParams[2].Value = dicParam["start_date"];
            oParams[3] = DataAccessFactory.Instance.GetParameter("end_date", NPDBType.Char);
            oParams[3].Value = dicParam["end_date"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        public DataTable CAR_SELEC_QUERY1(Dictionary<string, object> dicParam)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT TOP 1 FieldID, TKTYPE ");
            sQuery.Append("FROM PARKKAKAOEVENT ");
            sQuery.Append("WHERE parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND CAR_NO = @CAR_NO ");
            sQuery.Append("AND CAREXIT = '0' ");
            sQuery.Append("ORDER BY EVENTDATE DESC");

            IDbDataParameter[] oParams = new IDbDataParameter[2];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];

            oParams[1] = DataAccessFactory.Instance.GetParameter("tk_no", NPDBType.Integer);
            oParams[1].Value = dicParam["tk_no"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        public DataTable SERIAL_SELECT_QUERY(Dictionary<string, object> dicParam)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT TOP 1 FieldID ");
            sQuery.Append("FROM PARKKAKAOEVENT ");
            sQuery.Append("WHERE parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND FieldID IS NOT NULL ");
            sQuery.Append("AND EVENTDATE  BETWEEN @start_date AND @end_date ");
            sQuery.Append("AND EventName = 'car_entered' ");
            sQuery.Append("ORDER BY FieldID DESC");

            IDbDataParameter[] oParams = new IDbDataParameter[3];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];
            oParams[1] = DataAccessFactory.Instance.GetParameter("start_date", NPDBType.Char);
            oParams[1].Value = dicParam["start_date"];
            oParams[2] = DataAccessFactory.Instance.GetParameter("end_date", NPDBType.Char);
            oParams[2].Value = dicParam["end_date"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        public DataTable PARKING_LEVEL_SELECT_QUERY()
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT CODE ");
            sQuery.Append("FROM TB_CODEMASTER ");
            sQuery.Append("WHERE CODE_GROUP = 'FLOOR' ");
            sQuery.Append("AND USE_YN = 'Y'");

            return DA.ExecuteDataTable(sQuery.ToString());
        }

        public DataTable OCCUPANCY_SELECT_QUERY(Dictionary<string, object> dicParam)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("SELECT A.TOTAL, B.PARKING ");
            sQuery.Append("FROM (SELECT COUNT(*) TOTAL ");
            sQuery.Append("      FROM TB_PARKING_AREA ");
            sQuery.Append("      WHERE PARKINGLOT_NO = @parkinglot_num ");
            sQuery.Append("      AND PARKING_LEVEL_CODE IN (@code1, @code2) ");
            sQuery.Append("      AND DELETE_YN = 'N') A, ");
            sQuery.Append("     (SELECT COUNT(*) PARKING ");
            sQuery.Append("      FROM TB_PARKING_AREA ");
            sQuery.Append("      WHERE PARKINGLOT_NO = @parkinglot_num ");
            sQuery.Append("      AND PARKING_LEVEL_CODE IN (@code1, @code2) ");
            sQuery.Append("      AND DELETE_YN = 'N' ");
            sQuery.Append("      AND CAR_STATUS = 'Y') B ");

            IDbDataParameter[] oParams = new IDbDataParameter[3];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parkinglot_num", NPDBType.VarChar);
            oParams[0].Value = dicParam["parkinglot_num"];
            oParams[1] = DataAccessFactory.Instance.GetParameter("code1", NPDBType.VarChar);
            oParams[1].Value = dicParam["code1"];
            oParams[2] = DataAccessFactory.Instance.GetParameter("code2", NPDBType.VarChar);
            oParams[2].Value = dicParam["code2"];

            return DA.ExecuteDataTable(sQuery.ToString(), oParams);
        }

        public int READ_STAT_UPDATE_QUERY(Dictionary<string, object> dicParam, bool isDesc = false)
        {
            StringBuilder sQuery = new StringBuilder();
            sQuery.Append("UPDATE PARKKAKAOEVENT ");
            sQuery.Append("SET ReadStat = '1' ");
            sQuery.Append("WHERE parking_lot_id = @parking_lot_id ");
            sQuery.Append("AND TK_NO = @tk_no ");
            sQuery.Append("AND EVENTNAME = @eventname");

            IDbDataParameter[] oParams = new IDbDataParameter[3];
            oParams[0] = DataAccessFactory.Instance.GetParameter("parking_lot_id", NPDBType.VarChar);
            oParams[0].Value = dicParam["parking_lot_id"];
            oParams[1] = DataAccessFactory.Instance.GetParameter("tk_no", NPDBType.Integer);
            oParams[1].Value = dicParam["tk_no"];
            oParams[2] = DataAccessFactory.Instance.GetParameter("eventname", NPDBType.VarChar);
            oParams[2].Value = dicParam["eventname"];

            return DA.ExecuteNonQuery(sQuery.ToString(), oParams);
        }
    }
}
