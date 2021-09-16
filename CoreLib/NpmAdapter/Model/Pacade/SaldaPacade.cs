using NpmCommon;
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using NpmAdapter.Payload;
using Newtonsoft.Json.Linq;

namespace NpmAdapter.Model
{
    public struct SaldaResult
    {
        public IPayload payload;
        public ModelResult result;
    }

    /// <summary>
    /// 잘살아보세-살다 연동 퍼사드 Class
    /// </summary>
    class SaldaPacade : Singleton<SaldaPacade>
    {
        private ParkInfoModel ParkInfo { get; set; }
        private UnitModel UnitInfo { get; set; }
        private VisitInfoModel VisitInfo { get; set; }
        private CustInfoModel CustInfo { get; set; }
        private IOListModel IOList { get; set; }

        protected override void Init()
        {
            UnitInfo = new UnitModel();
            ParkInfo = new ParkInfoModel();
            VisitInfo = new VisitInfoModel();
            CustInfo = new CustInfoModel();
            IOList = new IOListModel();
        }

        /// <summary>
        /// 4.1 Zone 정보(주차장 정보)
        /// </summary>
        /// <param name="parkNo">주차장ID</param>
        /// <returns></returns>
        public IPayload GetZoneInfo(string parkNo)
        {
            ReponseSdParkInfoPayload parkInfoPayload = new ReponseSdParkInfoPayload();

            try
            {
                Dictionary<ColName, object> dicParam = new Dictionary<ColName, object>();
                dicParam.Add(ColName.ParkNo, parkNo);
                var parkInfoDt = ParkInfo.GetParkInfo(dicParam);
                
                if (parkInfoDt == null || parkInfoDt.Rows.Count == 0)
                {
                    Log.WriteLog(LogType.Error, "SaldaAdapter | GetZoneInfo", "ParkInfo 자료 없음");
                }
                else
                {
                    DataRow fRow = parkInfoDt.Rows[0];
                    parkInfoPayload.companyName = Helper.NVL(fRow["Admin"]?.ToString());
                    parkInfoPayload.licenseNo = Helper.NVL(fRow["RegNo"]?.ToString());
                    parkInfoPayload.zoneAddress = Helper.NVL(fRow["ParkAddr"]?.ToString());
                    parkInfoPayload.zoneName = Helper.NVL(fRow["ParkName"]?.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SaldaPacade | GetZoneInfo", ex.Message, LogAdpType.DataBase);
            }

            return parkInfoPayload;
        }

        /// <summary>
        /// 4.2 장치 정보
        /// </summary>
        /// <param name="parkNo">주차장 ID</param>
        /// <returns></returns>
        public IPayload GetDeviceInfo(string parkNo)
        {
            ResponseSdDivPayload resPayload = new ResponseSdDivPayload();

            try
            {
                Dictionary<ColName, object> dicParam = new Dictionary<ColName, object>();
                dicParam.Add(ColName.ParkNo, parkNo);
                var unitInfoDt = UnitInfo.GetLprInfo(dicParam);

                

                if (unitInfoDt == null || unitInfoDt.Rows.Count == 0)
                {
                    Log.WriteLog(LogType.Error, "SaldaAdapter | GetDeviceInfo", "UnitInfo 자료 없음");
                }
                else
                {
                    foreach (DataRow dr in unitInfoDt.Rows)
                    {
                        ResponseSdDivContentPayload content = new ResponseSdDivContentPayload();
                        content.deviceId = Helper.NVL(dr["UnitNo"]);
                        content.deviceName = Helper.NVL(dr["UnitName"]);
                        content.deviceType = Helper.NVL(dr["UnitKind"]) == "8" ? "IN" : "OUT";

                        resPayload.contents.Add(content);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SaldaPacade | GetDeviceInfo", ex.Message, LogAdpType.DataBase);
            }

            return resPayload;
        }

        /// <summary>
        /// 4.4 화이트리스트 조회
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public SaldaResult GetWhiteList(JObject json)
        {
            SaldaResult saldaResult = new SaldaResult();
            ModelResult mResult = new ModelResult();

            ResponseSdWhiteListPayload resPayload = new ResponseSdWhiteListPayload();

            try
            {
                Dictionary<ColName, object> param = new Dictionary<ColName, object>();
                param.Add(ColName.ParkNo, Helper.NVL(json["zoneId"]));
                param.Add(ColName.TKNo, Helper.NVL(json["regId"]));
                param.Add(ColName.DateFilterType, Helper.NVL(json["dateTimeFilterType"]));
                param.Add(ColName.StartDt, Helper.NVL(json["dateTimeFrom"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd"));
                param.Add(ColName.EndDt, Helper.NVL(json["dateTimeTo"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd"));
                param.Add(ColName.UnitNo, Helper.NVL(json["deviceId"]));
                param.Add(ColName.CarNo, Helper.NVL(json["carNo"]));
                param.Add(ColName.TelNo, Helper.NVL(json["mobileNo"]).ConvertTelFormat(TelFormat.Normal));
                param.Add(ColName.Dong, Helper.NVL(json["metaData1"]));
                param.Add(ColName.Ho, Helper.NVL(json["metaData2"]));
                param.Add(ColName.PageNumber, Helper.NVL(json["pageNumber"]));
                param.Add(ColName.PageSize, Helper.NVL(json["pageSize"]));

                DataTable resultDt = null;

                if (Helper.NVL(json["type"]).ToLower() == "regular")
                {
                    resultDt = CustInfo.SelectPaging(param);

                    foreach (DataRow dr in resultDt.Rows)
                    {
                        ResponseSdWhiteListContentPayload content = new ResponseSdWhiteListContentPayload();
                        content.type = "Regular";
                        content.regId = Helper.NVL(dr["TKNo"]);
                        content.carNo = Helper.NVL(dr["CarNo"]);
                        content.startDateTime = (Helper.NVL(dr["ExpDateF"]) + " 00:00:00").ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "ISO8601");
                        content.endDateTime = (Helper.NVL(dr["ExpDateT"]) + " 23:59:00").ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "ISO8601");
                        content.customerName = Helper.NVL(dr["Name"]);
                        content.mobileNo = Helper.NVL(dr["TelNo"]).ConvertTelFormat(TelFormat.Gloval);
                        content.metaData1 = Helper.NVL(dr["CompName"]);
                        content.metaData2 = Helper.NVL(dr["DeptName"]);
                        content.memo = Helper.NVL(dr["Reserve1"]);
                        content.repeatable = true;
                        var CreatedDate = DateTime.Now.ToString("yyyyMMddHHmmss").ConvertDateTimeFormat("yyyyMMddHHmmss", "ISO8601");
                        content.createdAt = CreatedDate;
                        content.updatedAt = CreatedDate;

                        resPayload.contents.Add(content);
                    }
                }
                else if (Helper.NVL(json["type"]).ToLower() == "visitor")
                {
                    resultDt = VisitInfo.SelectPaging(param);

                    foreach (DataRow dr in resultDt.Rows)
                    {
                        ResponseSdWhiteListContentPayload content = new ResponseSdWhiteListContentPayload();
                        content.type = "Visitor";
                        content.regId = Helper.NVL(dr["TKNo"]);
                        content.carNo = Helper.NVL(dr["CarNo"]);
                        content.startDateTime = Helper.NVL(dr["StartDateTime"]).ConvertDateTimeFormat("yyyyMMddHHmmss", "ISO8601");
                        content.endDateTime = Helper.NVL(dr["EndDateTime"]).ConvertDateTimeFormat("yyyyMMddHHmmss", "ISO8601");
                        content.customerName = "";
                        content.mobileNo = "";
                        content.metaData1 = Helper.NVL(dr["dong"]);
                        content.metaData2 = Helper.NVL(dr["ho"]);
                        content.memo = "";
                        content.repeatable = true;
                        var CreatedDate = DateTime.Now.ToString("yyyyMMddHHmmss").ConvertDateTimeFormat("yyyyMMddHHmmss", "ISO8601");
                        content.createdAt = CreatedDate;
                        content.updatedAt = CreatedDate;

                        resPayload.contents.Add(content);
                    }
                }

                mResult.code = ModelCode.OK;
                if (resultDt.Rows.Count > 0) mResult.Message = "조회된 자료가 없습니다.";
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SaldaPacade | GetWhiteList", ex.Message, LogAdpType.DataBase);
                mResult.code = ModelCode.Exception;
                mResult.Message = ex.Message;
            }
            finally
            {
                saldaResult.payload = resPayload;
                saldaResult.result = mResult;
            }
            
            return saldaResult;
        }

        /// <summary>
        /// 4.5 차량등록(정기/방문)
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public SaldaResult RegistCar(JObject json)
        {
            SaldaResult saldaResult = new SaldaResult();
            ModelResult mResult = new ModelResult();
            RequestSdReg resPayload = new RequestSdReg();
            Dictionary<ColName, object> param = new Dictionary<ColName, object>();
            try
            {
                var TkNo = Helper.UTCNansu(DateTime.Now, 10); //TKNo 난수값 생성
                if (Helper.NVL(json["type"]).ToLower() == "regular")
                {
                    param.Add(ColName.TKNo, TkNo);
                    param.Add(ColName.ParkNo, Helper.NVL(json["zoneId"]));
                    param.Add(ColName.Name, Helper.NVL(json["customerName"]));
                    param.Add(ColName.TelNo, Helper.NVL(json["mobileNo"]).ConvertTelFormat(TelFormat.Normal));
                    param.Add(ColName.CarNo, Helper.NVL(json["carNo"]));
                    param.Add(ColName.Dong, Helper.NVL(json["metaData1"]));
                    param.Add(ColName.Ho, Helper.NVL(json["metaData2"]));
                    param.Add(ColName.ExpDateF, Helper.NVL(json["startDateTime"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd"));
                    param.Add(ColName.ExpDateT, Helper.NVL(json["endDateTime"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd"));
                    param.Add(ColName.UnitNo, "10");
                    param.Add(ColName.Memo, Helper.NVL(json["memo"]));

                    mResult = CustInfo.Insert(param);

                    if (mResult.code == ModelCode.OK)
                    {
                        //신규 발행 된 TKNo를 설정 해 준다.
                        json["regId"] = TkNo;
                    }
                    else if (mResult.code == ModelCode.AlreadyRegisted)
                    {
                        //이미 등록된 차량 TKNo 를 설정 해 준다.
                        json["regId"] = mResult.Description;
                    }
                }
                else if (Helper.NVL(json["type"]).ToLower() == "visitor") //== 완료
                {
                    //방문차량등록
                    param.Add(ColName.TKNo, TkNo);
                    param.Add(ColName.Dong, Helper.NVL(json["metaData1"]));
                    param.Add(ColName.Ho, Helper.NVL(json["metaData2"]));
                    param.Add(ColName.CarNo, Helper.NVL(json["carNo"]));
                    param.Add(ColName.StartDt, Helper.NVL(json["startDateTime"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd") + "000000");
                    param.Add(ColName.EndDt, Helper.NVL(json["endDateTime"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd") + "235959");

                    mResult = VisitInfo.Insert(param);

                    if (mResult.code == ModelCode.OK)
                    {
                        //신규 발행 된 TKNo를 설정 해 준다.
                        json["regId"] = TkNo;
                    }
                    else if (mResult.code == ModelCode.AlreadyRegisted)
                    {
                        //이미 등록된 차량 TKNo 를 설정 해 준다.
                        json["regId"] = mResult.Description;
                    }
                }

                json.Remove("repeatable");
                var CreatedDate = DateTime.Now.ToString("yyyyMMddHHmmss").ConvertDateTimeFormat("yyyyMMddHHmmss", "ISO8601");
                json.Add("createdAt", CreatedDate);
                json.Add("updatedAt", CreatedDate);
                json.Add("createdAt", "");

                resPayload.Deserialize(json);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SaldaPacade | GetWhiteList", ex.Message, LogAdpType.DataBase);
                mResult.code = ModelCode.Exception;
                mResult.Message = ex.Message;
            }
            finally
            {
                saldaResult.payload = resPayload;
                saldaResult.result = mResult;
            }

            return saldaResult;
        }

        /// <summary>
        /// 4.7 차량삭제(정기/방문)
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public SaldaResult RemoveCar(JObject json)
        {
            SaldaResult saldaResult = new SaldaResult();
            ModelResult mResult = new ModelResult();
            Dictionary<ColName, object> param = new Dictionary<ColName, object>();
            try
            {
                if (Helper.NVL(json["type"]).ToLower() == "regular")
                {
                    param.Add(ColName.Dong, Helper.NVL(json["metaData1"]));
                    param.Add(ColName.Ho, Helper.NVL(json["metaData2"]));
                    param.Add(ColName.TKNo, Helper.NVL(json["regId"]));

                    mResult = CustInfo.Delete(param);
                }
                else if (Helper.NVL(json["type"]).ToLower() == "visitor")
                {
                    param.Add(ColName.Dong, Helper.NVL(json["metaData1"]));
                    param.Add(ColName.Ho, Helper.NVL(json["metaData2"]));
                    param.Add(ColName.TKNo, Helper.NVL(json["regId"]));

                    mResult = VisitInfo.Delete(param);
                }
                else
                {
                    mResult.code = ModelCode.NotAcceptable;
                    mResult.Message = "지원하지 않는 type 입니다.";
                }

            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SaldaPacade | GetWhiteList", ex.Message, LogAdpType.DataBase);
                mResult.code = ModelCode.Exception;
                mResult.Message = ex.Message;
            }
            finally
            {
                saldaResult.result = mResult;
            }

            return saldaResult;
        }

        /// <summary>
        /// 4.8 입출차내역 조회
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public SaldaResult GetIOList(JObject json)
        {
            SaldaResult saldaResult = new SaldaResult();
            ModelResult mResult = new ModelResult();
            ResponseSdIOPayload resPayload = new ResponseSdIOPayload();
            Dictionary<ColName, object> param = new Dictionary<ColName, object>();
            try
            {
                param.Add(ColName.ParkNo, Helper.NVL(json["zoneId"]));
                param.Add(ColName.CarNo, Helper.NVL(json["carNo"]));
                param.Add(ColName.TelNo, Helper.NVL(json["mobileNo"]).ConvertTelFormat(TelFormat.Normal));
                param.Add(ColName.Dong, Helper.NVL(json["metaData1"]));
                param.Add(ColName.Ho, Helper.NVL(json["metaData2"]));
                param.Add(ColName.PageNumber, Helper.NVL(json["pageNumber"]));
                param.Add(ColName.PageSize, Helper.NVL(json["pageSize"]));
                param.Add(ColName.StartDt, Helper.NVL(json["dateTimeFrom"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd"));
                param.Add(ColName.EndDt, Helper.NVL(json["dateTimeTo"]).ConvertDateTimeFormat("ISO8601", "yyyyMMdd"));

                using (DataTable resultDt = IOList.SelectPaging(param))
                {
                    foreach (DataRow dr in resultDt.Rows)
                    {
                        ResponseSdIOContentPayload content = new ResponseSdIOContentPayload();
                        content.eventId = Helper.NVL(dr["TKNo"]);
                        content.carNo = Helper.NVL(dr["CarNo"]);
                        content.deviceId = Helper.NVL(dr["UnitNo"]);
                        content.whitelistType = Helper.NVL(dr["WhiteListType"]);
                        content.eventType = Helper.NVL(dr["OutChk"]);
                        content.eventDateTime = Helper.NVL(dr["IODateTime"]).ConvertDateTimeFormat("yyyy-MM-dd HH:mm:ss", "ISO8601");
                        content.customerName = Helper.NVL(dr["Name"]);
                        content.mobileNo = Helper.NVL(dr["TelNo"]);
                        content.metaData1 = Helper.NVL(dr["Dong"]);
                        content.metaData2 = Helper.NVL(dr["Ho"]);
                        content.link = "";

                        resPayload.contents.Add(content);
                    }

                    mResult.code = ModelCode.OK;
                    if (resultDt.Rows.Count > 0) mResult.Message = "조회된 자료가 없습니다.";
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, "SaldaPacade | GetIOList", ex.Message, LogAdpType.DataBase);
                mResult.code = ModelCode.Exception;
                mResult.Message = ex.Message;
            }
            finally
            {
                saldaResult.payload = resPayload;
                saldaResult.result = mResult;
            }

            return saldaResult;
        }
    }
}
