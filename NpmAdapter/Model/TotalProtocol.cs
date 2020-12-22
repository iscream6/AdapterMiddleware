using NexpaAdapterStandardLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NpmAdapter.Model
{
    public class TotalContorolProtocol
    {
        public const char _STX_ = (char)0x02;
        public const char _ETX_ = (char)0x03;
        public const char _FS_ = (char)0x1C;
        public const char _GS_ = (char)0x1D;
        public const char _RS_ = (char)0x1E;
        public const char _US_ = (char)0x1F;

        private static string m_UnknowError = "98";
        private static string m_TimeOver = "99";

        private static string m_Bill_MotorFailer = "01";
        private static string m_Bill_ChecksumError = "02";

        private static string m_Bill_BillJamError = "03";

        private static string m_Bill_BillRemoveError = "04";


        private static string m_Bill_StackerOpenError = "05";

        private static string m_Bill_SensorProblemError = "06";

        private static string m_Bill_BillRejectError = "07";

        private static string m_Bill_StackerProblemError = "08";
        private static string m_Bill_InvalidCommandError = "09";

        private static string m_CoinDispensor_MotorProblem = "01";
        private static string m_CoinDispensor_CheckForcoinAvailablity = "02";
        private static string m_CoinDispensor_CoinsSizeVaries = "03";
        private static string m_CoinDispensor_PrismSensorFailureorCoinJammed = "04";
        private static string m_CoinDispensor_ShaftSensorFailure = "05";
        private static string m_CoinDispensor_Lack10Qty = "06";
        private static string m_CoinDispensor_Lack50Qty = "07";
        private static string m_CoinDispensor_Lack100Qty = "08";
        private static string m_CoinDispensor_Lack500Qty = "09";
        private static string m_CoinDispensor_Not10Qty = "10";
        private static string m_CoinDispensor_Not50Qty = "11";
        private static string m_CoinDispensor_Not100Qty = "12";
        private static string m_CoinDispensor_Not500Qty = "13";
        private static string m_CoinDispensor_NotDispense = "97";

        private static string m_BillDispensor_OverBillDispene = "02";
        private static string m_BillDispensor_Lack1000Qty = "03";
        private static string m_BillDispensor_Lack5000Qty = "04";

        private static string m_BillDispensor_Not1000Qty = "07";
        private static string m_BillDispensor_Not5000Qty = "08";
        private static string m_BillDispensor_NotDispense = "97";
        private static string m_BillDispensor_SenSorError = "96";

        private static string m_CardReader_ConnectFail = "01";
        private static string m_CardReader_JAM = "02";
        private static string m_CardReader_Recject_Error = "03";

        private static string m_Receipt_LackPage = "01";
        private static string m_Receipt_NotPage = "02";

        private static string m_Device_OK = "88";

        private static string m_DeivceConnectFaile = "999";
        public const int m_DeivceConnectFail = 154;
        public const int m_Deivce_OK_Int = 154;

        private const string m_KoreaConnectFaile = "연결실패";

        /// <summary>
        /// BRE 지폐리더기, CRE 동전리더기,TRE 교통카드리더기,BCH 지폐방출기,_CC1_동전방출기,CA1 카드리더기 , DID 도어신호 , REP 영수증프린터
        /// </summary>
        public enum DEVICE
        {
            /// <summary>
            /// 지폐리더기
            /// </summary>
            BRE,
            /// <summary>
            /// 동전리더기
            /// </summary>
            CRE,
            /// <summary>
            /// 교통카드리더기
            /// </summary>
            TRE,
            /// <summary>
            /// 지폐방출기
            /// </summary>
            BCH,
            /// <summary>
            /// 50원방출기
            /// </summary>
            CC1,
            /// <summary>
            /// 100원방출기
            /// </summary>
            CC2,
            /// <summary>
            /// 500원방출기
            /// </summary>
            CC3,
            /// <summary>
            /// 카드리더기1
            /// </summary>
            CA1,
            /// <summary>
            /// 카드리더기2
            /// </summary>
            CA2,
            /// <summary>
            /// LED
            /// </summary>
            DID,
            /// <summary>
            /// 영수증
            /// </summary>
            REP,
            /// <summary>
            /// 전광판
            /// </summary>
            DIS,
            /// <summary>
            /// 전체
            /// </summary>
            ALL,
            /// <summary>
            /// 없음
            /// </summary>
            NONE


        }
        /// <summary>
        /// 프로토콜 규칙중 stx etx제외하고 {fs}로 분리했을때 나오는 데이터 
        /// </summary>
        public enum CommandRegulation
        {
            //COMMAND, // 명령어
            SENDPARKNO, // 보내는 주차장번호
            SENDUNITNO, // 보내는 대상의 장비번호
            SENDDATETIME, // 시간(yyyyMMddHHmmss)
            RECEIVEPARKNO, // 받는 주차장번호
            RECEIVEUNITNO, // 받는 대상의 장비번호
            SubCommand, // 

            NONE,
            IPADDRESS,
            BOOTHTYPE,
            COMMAND,
            SUBCOMMAND,
            NONES
        }
        public enum BoothType
        {
            /// <summary>
            /// 유인부스
            /// </summary>
            Booth,
            /// <summary>
            /// 출구무인
            /// </summary>
            AP,
            /// <summary>
            /// 사전무인
            /// </summary>
            PrevAP,
            /// <summary>
            /// LPR
            /// </summary>
            LPR,
            /// <summary>
            /// 전광판
            /// </summary>
            DISPLAY,
            CENTER


        }
        /// <summary>
        /// 명령어
        /// </summary>
        public enum ControlCommand
        {
            PAYCANCLE,

            /// <summary>
            /// 사전할인무료
            /// </summary>
            PREDISCOUNT,
            /// <summary>
            /// 사전정산차량
            /// </summary>
            PRECAR,
            /// <summary>
            /// 회차
            /// </summary>
            FREECAR,
            /// <summary>
            /// 정기권
            /// </summary>
            REG,

            /// <summary>
            /// 시간오버
            /// </summary>
            PAYTIMEOUT,

            BAROPEN,
            /// <summary>
            /// 차량정보
            /// 차량번호,그룹번호,입차일자,입차시간,출차일자,출차시간,정기권유무,인식여부,차단기오픈여부
            /// </summary>
            CARINFO,
            /// 차량번호,그룹번호,입차일자,입차시간,출차일자,출차시간,정기권유무,인식여부,차단기오픈여부
            JUNGSANINFO,
            /// <summary>
            /// LPR상태
            /// LPR상태  (0:비정상/1:정상),차량상태메세지
            /// </summary>
            LPRSTATE,
            /// <summary>
            /// 정산기 상태 
            /// </summary>
            APSTATE,
            /// <summary>
            /// 수동입차  차량번호,TkNo,입차일자,입차시간,입차장비번호
            /// </summary>
            CARINSERT,
            /// <summary>
            /// 현재요청하는 차량요금조회요청 전면차량번호,후면차량번호,주차시간,입차시간,정산시간,출차시간,차종구분,일반/정기여부,출차구분,출차장소,주차요금,할인요금,결제요금,등록된 할인번호
            /// </summary>
            GET_CARPAY,
            /// <summary>
            /// 현재요청하는 차량요금조회요청 전면차량번호,후면차량번호,주차시간,입차시간,정산시간,출차시간,차종구분,일반/정기여부,출차구분,출차장소,주차요금,할인요금,결제요금,등록된 할인번호
            /// </summary>
            MAKE_CARPAY,
            /// <summary>
            /// 일반차량 영수증재출력
            /// </summary>
            RECEIPTNORMAL,

            /// <summary>
            /// 할인 차량번호,TkNo,입차일자,입차시간,현재할인전송
            /// </summary>
            DISCOUNT,
            /// <summary>
            /// 차단기관련
            /// </summary>
            BARRIER,
            /// <summary>
            /// 장비리셋
            /// </summary>
            RESET,
            /// <summary>
            /// 주차요금 강제표출
            /// </summary>
            CHARGE,
            /// <summary>
            /// 주차요금요청(센터에서 요금요청 현재 무인정산기에서 요금을받고있는차량정보)
            /// </summary>
            GETCURRENT_PAYMONEY,
            GETCURRENT_ORIGINAL,
            /// <summary>
            /// 강제 돈방출
            /// </summary>
            DISPENSE,
            /// <summary>
            /// 정상 커맨트카아님
            /// </summary>
            NONCOMMAND,
            /// <summary>
            /// 수입금
            /// </summary>
            INCOME,
            /// <summary>
            /// 수입금
            /// </summary>
            OUTCOME,
            /// <summary>
            /// 보유금
            /// </summary>
            REVERSE,
            /// <summary>
            /// 무인정산기 가동
            /// </summary>
            START,
            /// <summary>
            /// 무인정산기 비가동
            /// </summary>
            END,
            CENTER,
            GOSISER,
            //미,부분인식/일반/정기차량 입차 차단기 개방설정 적용
            /// <summary>
            /// 미,부분인식/일반/정기차량 입차 차단기 개방설정
            /// </summary>
            BAROPENMODE,
            //미,부분인식/일반/정기차량 입차 차단기 개방설정 적용완료
            //전동어닝 제어 적용
            ELECAWNING_CONTROL,
            //전동어닝 제어 적용완료
            //무인정산기 개방모드 적용
            AUTOBOOTH_OPENMODE,
                //무인정산기 개방모드 적용완료

            /// <summary>
            /// 상태요청
            /// </summary>
            STATE,
            ///// <summary>
            ///// 차단기관련
            ///// </summary>
            //BARRIER,
            ///// <summary>
            ///// 장비리셋
            ///// </summary>
            //RESET,
            ///// <summary>
            ///// 주차요금 강제표출
            ///// </summary>
            //CHARGE,
            ///// <summary>
            ///// 강제 돈방출
            ///// </summary>
            //DISPENSE,
            ///// <summary>
            ///// 정상 커맨트카아님
            ///// </summary>
            //NONCOMMAND,
            ///// <summary>
            ///// 수입금
            ///// </summary>
            //INCOME,
            ///// <summary>
            ///// 수입금
            ///// </summary>
            //OUTCOME,
            ///// <summary>
            ///// 보유금
            ///// </summary>
            //REVERSE,
            ///// <summary>
            ///// 무인정산기 가동
            ///// </summary>
            //START,
            ///// <summary>
            ///// 무인정산기 비가동
            ///// </summary>
            //END,
            //CENTER

        }
        /// <summary>
        /// 서브명령어로 차단기 입구인지 출구인지 명명
        /// </summary>
        public enum BarrierType
        {
            /// <summary>
            /// 입구
            /// </summary>
            IGT,
            /// <summary>
            /// 출구
            /// </summary>
            OGT
        }
        /// <summary>
        /// 장비상태 응답 또는 차단기 응답값
        /// </summary>
        public enum Status
        {
            OK,
            ERR
        }


        /// <summary>
        /// 문자열로 들어온 장비이름을 ENUM형태로 리턴
        /// </summary>
        /// <param name="l_Device"></param>
        /// <returns></returns>
        public static TotalContorolProtocol.DEVICE GetDeviceNameAsEnum(string l_Device)
        {
            try
            {
                return (TotalContorolProtocol.DEVICE)Enum.Parse(typeof(TotalContorolProtocol.DEVICE), l_Device);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"TotalContorolProtocol | GetDeviceNameAsEnum", $"{ex.Message}", LogAdpType.Biz);
                return TotalContorolProtocol.DEVICE.NONE;
            }
        }

        /// <summary>
        /// 문자열로 들어온 커맨드를 ENUM형태로 변경
        /// </summary>
        /// <param name="l_CommandName"></param>
        /// <returns></returns>
        public static TotalContorolProtocol.ControlCommand GetCommandNameAsEnum(string l_CommandName)
        {
            try
            {
                return (TotalContorolProtocol.ControlCommand)Enum.Parse(typeof(TotalContorolProtocol.ControlCommand), l_CommandName);
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"TotalContorolProtocol | GetCommandNameAsEnum", $"{ex.Message}", LogAdpType.Biz);
                return TotalContorolProtocol.ControlCommand.NONCOMMAND;
            }
        }


        //public static string INC(NormalCarInfo p_NormalCarInfo)
        //{
        //    string startdate = DateTime.Now.ToString("yyyyMMddHHmmss");
        //    if (startdate.Trim() == string.Empty)
        //    {
        //        startdate = "00000000000000";
        //    }
        //    string Savedqty10 = p_NormalCarInfo.InCome10Qty.ToString();
        //    string Savedqty50 = p_NormalCarInfo.InCome50Qty.ToString();
        //    string Savedqty100 = p_NormalCarInfo.InCome100Qty.ToString();
        //    string Savedqty500 = p_NormalCarInfo.InCome500Qty.ToString();
        //    string Savedqty1000 = p_NormalCarInfo.InCome1000Qty.ToString();
        //    string Savedqty5000 = p_NormalCarInfo.InCome5000Qty.ToString();
        //    string Savedqty10000 = p_NormalCarInfo.InCome10000Qty.ToString();
        //    string Savedqty50000 = p_NormalCarInfo.InCome50000Qty.ToString();

        //    string l_sendMessage = Rev_Inc_Out(TotalContorolProtocol.ControlCommand.INCOME, startdate, DateTime.Now.ToString("yyyyMMddHHmmss"), Savedqty10, Savedqty50, Savedqty100, Savedqty500, Savedqty1000, Savedqty5000, Savedqty10000, Savedqty50000);
        //    string l_startYmd = startdate.Substring(0, 8);
        //    string l_startHms = startdate.Substring(8, 6);
        //    string l_endYmd = l_startYmd;
        //    string l_endHms = l_startHms;
        //    return l_sendMessage;

        //}



        /// <summary>
        /// 보유현금전송
        /// </summary>

        //public static string OUT(NormalCarInfo p_NormalCarInfo)
        //{
        //    string startdate = DateTime.Now.ToString("yyyyMMddHHmmss");
        //    if (startdate.Trim() == string.Empty)
        //    {
        //        startdate = "00000000000000";
        //    }
        //    string Savedqty10 = p_NormalCarInfo.OutCome10Qty.ToString();
        //    string Savedqty50 = p_NormalCarInfo.OutCome50Qty.ToString();
        //    string Savedqty100 = p_NormalCarInfo.OutCome100Qty.ToString();
        //    string Savedqty500 = p_NormalCarInfo.OutCome500Qty.ToString();
        //    string Savedqty1000 = p_NormalCarInfo.OutCome1000Qty.ToString();
        //    string Savedqty5000 = p_NormalCarInfo.OutCome5000Qty.ToString();
        //    string Savedqty10000 = p_NormalCarInfo.OutCome10000Qty.ToString();
        //    string Savedqty50000 = p_NormalCarInfo.OutCome50000Qty.ToString();

        //    string l_sendMessage = (Rev_Inc_Out(TotalContorolProtocol.ControlCommand.OUTCOME, startdate, DateTime.Now.ToString("yyyyMMddHHmmss"), Savedqty10, Savedqty50, Savedqty100, Savedqty500, Savedqty1000, Savedqty5000, Savedqty10000, Savedqty50000));
        //    string l_startYmd = startdate.Substring(0, 8);
        //    string l_startHms = startdate.Substring(8, 6);
        //    string l_endYmd = l_startYmd;
        //    string l_endHms = l_startHms;
        //    return l_sendMessage;

        //}

        /// <summary>
        /// 보유현금전송
        /// </summary>

        //public static string REV()
        //{
        //    string startdate = NPSYS.PreMagamDate.Replace(":", "").Replace("-", "").Replace(" ", "");
        //    if (startdate.Trim() == string.Empty)
        //    {
        //        startdate = "00000000000000";
        //    }
        //    string Savedqty10 = "0";
        //    string Savedqty50 = (NPSYS.Config.GetValue(ConfigID.Cash50SettingQty).Trim() == "" ? "0" : NPSYS.Config.GetValue(ConfigID.Cash50SettingQty));
        //    string Savedqty100 = (NPSYS.Config.GetValue(ConfigID.Cash100SettingQty).Trim() == "" ? "0" : NPSYS.Config.GetValue(ConfigID.Cash100SettingQty));
        //    string Savedqty500 = (NPSYS.Config.GetValue(ConfigID.Cash500SettingQty).Trim() == "" ? "0" : NPSYS.Config.GetValue(ConfigID.Cash500SettingQty));
        //    string Savedqty1000 = (NPSYS.Config.GetValue(ConfigID.Cash1000SettingQty).Trim() == "" ? "0" : NPSYS.Config.GetValue(ConfigID.Cash1000SettingQty));
        //    string Savedqty5000 = (NPSYS.Config.GetValue(ConfigID.Cash5000SettingQty).Trim() == "" ? "0" : NPSYS.Config.GetValue(ConfigID.Cash5000SettingQty));
        //    string Savedqty10000 = "0";
        //    string Savedqty50000 = "0";
        //    string l_sendMessage = Rev_Inc_Out(TotalContorolProtocol.ControlCommand.REVERSE, startdate, DateTime.Now.ToString("yyyyMMddHHmmss"), Savedqty10, Savedqty50, Savedqty100, Savedqty500, Savedqty1000, Savedqty5000, Savedqty10000, Savedqty50000);
        //    string l_startYmd = startdate.Substring(0, 8);
        //    string l_startHms = startdate.Substring(8, 6);
        //    string l_endYmd = DateTime.Now.ToString("yyyyMMddHHmmss").Substring(0, 8);
        //    string l_endHms = DateTime.Now.ToString("yyyyMMddHHmmss").Substring(8, 6);
        //    return l_sendMessage;

        //}
        /// <summary>
        /// 수입금 지출금 보유현금 관련
        /// </summary>
        /// <param name="status"></param>
        /// <param name="startdate"></param>
        /// <param name="enddate"></param>
        /// <param name="p_10Qty"></param>
        /// <param name="p_50Qty"></param>
        /// <param name="p_100Qty"></param>
        /// <param name="p_500Qty"></param>
        /// <param name="p_1000Qty"></param>
        /// <param name="p_5000Qty"></param>
        /// <param name="p_10000Qty"></param>
        /// <param name="p_50000Qty"></param>
        /// <returns></returns>
        //private static string Rev_Inc_Out(ControlCommand p_ControlCommand, string startdate, string enddate, string p_10Qty, string p_50Qty, string p_100Qty, string p_500Qty, string p_1000Qty, string p_5000Qty, string p_10000Qty, string p_50000Qty)
        //{
        //    StringBuilder message = new StringBuilder();
        //    message.Append(_FS_);
        //    message.Append(NPSYS.BoothIp);
        //    message.Append(_FS_);
        //    message.Append(BoothType.AP);
        //    message.Append(_FS_);
        //    message.Append(p_ControlCommand);
        //    message.Append(_FS_);
        //    message.Append(p_10Qty);
        //    message.Append(_GS_);
        //    message.Append(p_50Qty);
        //    message.Append(_GS_);
        //    message.Append(p_100Qty);
        //    message.Append(_GS_);
        //    message.Append(p_500Qty);
        //    message.Append(_GS_);
        //    message.Append(p_1000Qty);
        //    message.Append(_GS_);
        //    message.Append(p_5000Qty);
        //    message.Append(_GS_);
        //    message.Append(p_10000Qty);
        //    message.Append(_GS_);
        //    message.Append(p_50000Qty);
        //    message.Append(_FS_);
        //    return message.ToString();
        //}

        ///// <summary>
        ///// 기본포맷이 STX  fs  IP-Address  fs  장비구분  fs  명령어  fs  Sub 명령어 
        ///// </summary>
        ///// <param name="p_BoothType"></param>
        ///// <param name="p_ControlCommand"></param>
        ///// <param name="p_SubCommand"></param>
        ///// <returns></returns>
        //private static string CommonFormat(string p_DeviceIp, BoothType p_BoothType, ControlCommand p_ControlCommand, string p_SubCommand)
        //{
        //    StringBuilder sendData = new StringBuilder();
        //    sendData.Append(_FS_);
        //    sendData.Append(p_DeviceIp);
        //    sendData.Append(_FS_);
        //    sendData.Append(p_BoothType.ToString());
        //    sendData.Append(_FS_);
        //    sendData.Append(p_ControlCommand.ToString());
        //    sendData.Append(_FS_);
        //    sendData.Append(p_SubCommand);
        //    sendData.Append(_FS_);
        //    return sendData.ToString();

        //}

        /// <summary>
        /// 기본포맷이 STX  fs  IP-Address  fs  장비구분  fs  명령어  fs  Sub 명령어 
        /// </summary>
        /// <param name="p_ProtocolData"></param>
        /// <returns></returns>
        private static string CommonFormat(ProtocolData p_ProtocolData)
        {
            StringBuilder sendData = new StringBuilder();
            sendData.Append(_FS_);
            sendData.Append(p_ProtocolData.IP_Address);
            sendData.Append(_FS_);
            sendData.Append(p_ProtocolData.DeviceType);
            sendData.Append(_FS_);
            sendData.Append(p_ProtocolData.CurrentCommand);
            sendData.Append(_FS_);
            sendData.Append(p_ProtocolData.SubCommand);
            sendData.Append(_FS_);
            return sendData.ToString();

        }

        /// <summary>
        /// fs IP_Address fs DeviceType fs Command fs SubCommand fs 생성
        /// </summary>
        /// <param name="p_ProtocolData"></param>
        /// <returns></returns>
        public static string MakeCommonFormat(ProtocolData p_ProtocolData)
        {
            return CommonFormat(p_ProtocolData);
        }


        //public static string Device_Error(DEVICE devicename, int status)
        //{

        //    return LoaclStatusErrorMessage(BoothType.AP, devicename, false, status);
        //}

        /// <summary>
        /// 현재 로컬장비에 하나의 장비가 이상일시 전송문자열을 만들어준다
        /// </summary>
        /// <param name="p_BoothType">부스타입</param>
        /// <param name="p_DEVICE">장비종류</param>
        /// <param name="p_Status">성공실패유무</param>
        /// <param name="p_ErrorCode">에러코드</param>
        /// <returns></returns>
        //private static string LoaclStatusErrorMessage(BoothType p_BoothType, DEVICE p_DEVICE, bool p_Status, int p_ErrorCode)
        //{
        //    StringBuilder sendData = new StringBuilder();
        //    sendData.Append(_FS_);
        //    sendData.Append(NPSYS.BoothIp);
        //    sendData.Append(_FS_);
        //    sendData.Append(p_BoothType.ToString());
        //    sendData.Append(_FS_);
        //    sendData.Append(ControlCommand.STATE.ToString());
        //    sendData.Append(_FS_);
        //    sendData.Append(p_DEVICE.ToString());
        //    sendData.Append(_RS_);
        //    sendData.Append(Status.ERR.ToString());
        //    sendData.Append(_RS_);
        //    sendData.Append(ErrorCode(p_DEVICE, p_ErrorCode));
        //    sendData.Append(_RS_);
        //    sendData.Append(KoreaErrorCode(p_DEVICE, p_ErrorCode));
        //    sendData.Append(_FS_);
        //    return sendData.ToString();
        //}

        ///// <summary>
        ///// 현재 로컬장비에 하나의 장비가 이상일시 전송문자열을 만들어준다
        ///// </summary>
        ///// <param name="p_BoothType">부스타입</param>
        ///// <param name="p_DEVICE">장비종류</param>
        ///// <param name="p_Status">성공실패유무</param>
        ///// <param name="p_ErrorCode">에러코드</param>
        ///// <returns></returns>
        //public static string LoaclStatusMessage(BoothType p_BoothType, ControlCommand p_ControlCommand,string p_SubCommand)
        //{
        //    StringBuilder sendData = new StringBuilder();
        //    sendData.Append(_FS_);
        //    sendData.Append(NPSYS.BoothIp);
        //    sendData.Append(_FS_);
        //    sendData.Append(p_BoothType.ToString());
        //    sendData.Append(_FS_);
        //    sendData.Append(p_ControlCommand.ToString());
        //    sendData.Append(_FS_);
        //    sendData.Append(p_SubCommand);
        //    sendData.Append(_FS_);
        //    return sendData.ToString();
        //}

        public static string DelimeterGSMake(string p_CurrentStatus, string p_PreSubCommand)
        {
            if (p_CurrentStatus == string.Empty)
            {
                return "";
            }
            else if (p_CurrentStatus != string.Empty && p_PreSubCommand != string.Empty)
            {
                return TotalContorolProtocol._GS_.ToString() + p_CurrentStatus;
            }
            else
            {
                return p_CurrentStatus;
            }
        }

        public static string MakeSubCommand(DEVICE p_DEVICE, bool p_Status, int p_ResultCode)
        {
            StringBuilder l_SubCommand = new StringBuilder();
            l_SubCommand.Append(p_DEVICE);
            l_SubCommand.Append(_RS_);

            if (p_Status == true)
            {
                l_SubCommand.Append(Status.OK);
            }
            else
            {
                l_SubCommand.Append(Status.ERR);
                l_SubCommand.Append(_RS_);
                l_SubCommand.Append(ErrorCode(p_DEVICE, p_ResultCode));
                l_SubCommand.Append(_RS_);
                l_SubCommand.Append(KoreaErrorCode(p_DEVICE, p_ResultCode));

            }
            return l_SubCommand.ToString();
        }

        ///// <summary>
        ///// 전송메시지를 만든다
        ///// </summary>
        ///// <param name="l_Command"></param>
        ///// <param name="p_ProtocolData"></param>
        ///// <returns></returns>
        //public static string SendMessageMake(ref ProtocolData p_ProtocolData )
        //{
        //    string returnMessage = "";
        //    switch (p_ProtocolData.Command)
        //    {
        //        case ControlCommand.BARRIER:
        //            string l_BarrierSubCommand = MakeBarrierSubCommand(p_ProtocolData.SubCommand, p_ProtocolData.IsSuccess);
        //            p_ProtocolData.SubCommand = l_BarrierSubCommand;
        //            if (l_BarrierSubCommand != string.Empty)
        //            {
        //                p_ProtocolData.IsSuccess = true;
        //                returnMessage = CommonFormat(p_ProtocolData);
        //            }
        //            else
        //            {
        //                returnMessage = NotFormatData(p_ProtocolData);
        //            }
        //            break;


        //    }
        //    return returnMessage;
        //}

        /// <summary>
        /// 잘못된 데이터가 왔을때 AJ에 잘못된 데이터 송신 알려줌
        /// </summary>
        /// <param name="p_ProtocolData"></param>
        /// <returns></returns>
        private static string NotFormatData(ProtocolData p_ProtocolData)
        {
            p_ProtocolData.SubCommand = Status.ERR.ToString();
            return CommonFormat(p_ProtocolData);
        }

        private static string MakeBarrierSubCommand(string p_Message, bool isSuccess)
        {
            try
            {
                string[] l_BarrierSubCommand = p_Message.Split(_GS_);
                StringBuilder l_ReturnSubCommand = new StringBuilder();
                l_ReturnSubCommand.Append(l_BarrierSubCommand[0]);
                l_ReturnSubCommand.Append(_GS_);
                l_ReturnSubCommand.Append(l_BarrierSubCommand[1]);
                l_ReturnSubCommand.Append(_GS_);
                if (isSuccess)
                {
                    l_ReturnSubCommand.Append(Status.OK.ToString());
                }
                else
                {
                    l_ReturnSubCommand.Append(Status.ERR.ToString());
                }
                return l_ReturnSubCommand.ToString();
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"TotalContorolProtocol | MakeBarrierSubCommand", $"{ex.Message}", LogAdpType.Biz);
                return "";
            }

        }
        public static string KoreaErrorCode(DEVICE deviceName, int ErrorCode)
        {
            if (deviceName == DEVICE.CA1 || deviceName == DEVICE.CA2)
            {
                switch (ErrorCode)
                {
                    case 1:
                        return "연결안됨";
                    case 2:
                        return "카드걸림";
                    case 3:
                        return "카드배출에러";
                    case 88:
                        return "정상";
                    case 98:
                        return "알수없는 에러";

                    case 99:
                        return "시간초과";
                    case 152:
                        return "정상";
                    case 154:
                        return m_KoreaConnectFaile;

                    case 999:
                        return "연결안됨";
                    default:
                        return "알수없는 에러";

                }
            }
            else if (deviceName == DEVICE.BCH)
            {
                switch (ErrorCode)
                {
                    case 88:
                        return "정상";
                    case 52:
                        return "과다방출됨";
                    case 56:
                        return "5000원 없음  ";                          //트레이 위 수량부족
                    case 1001:
                        return "1000원 부족 ";                          //1000원 최소수량경고

                    case 1002:
                        return "5000원 부족 ";                          //5000원 최소수량경고
                    case 58:                                                         //수량체크에러(CHK3,4센서 ~ DIV센서)
                    case 59:                                                          //Note request error
                    case 60:                                                         //수량체크에러(EJT센서 ~ EXIT센서)
                    case 61:                                                        //수량체크에러(EJT센서 ~ EXIT센서)
                    case 63:                                                         //Reject 트레이 인지불가
                    case 72:                                                         //EJT센서 잼
                    case 73:                                                         //Diverter solenoid or SOL센서 에러
                    case 74:                                                         //SOL센서 에러
                    case 76:                                                         //CHK3,4센서 잼    
                        return "센서에러(지폐걸림)";
                    case 64:
                        return "1000원 없음 "; //트레이 아래 수량부족
                    case 65:
                        return "모터멈춤";
                    case 100:
                        return "지폐불출안됨"; //돈이 안나올때 최희주 새로추가
                    case 55:
                        return "알수없는에러";
                    case 152:
                        return "정상";
                    case 154:
                        return m_KoreaConnectFaile;

                    case -999:   //PortOpenError
                    case 67:
                    case -100: //CommunicationError
                        return "시간초과";
                    case 999:
                        return "연결안됨";
                    default:
                        return "알수없는에러";

                }
            }
            else if (deviceName == DEVICE.REP)
            {
                switch (ErrorCode)
                {
                    case 1:
                        return "용지부족";                         // 용지부족
                    case 2:
                        return "용지없음";                          // 용지없음
                    case 88:
                        return "정상";
                    case 152:
                        return "정상";
                    case 154:
                        return m_KoreaConnectFaile;

                    case 999:
                        return m_KoreaConnectFaile;

                    default:
                        return "알수없는에러";

                }
            }
            else if (deviceName == DEVICE.BRE)
            {
                switch (ErrorCode)
                {
                    case (int)0x20:
                        return "모터이상";
                    case (int)0x21:
                        return "체크섬에러";
                    case (int)0x22:
                        return "지폐걸림에러";
                    case (int)0x23:
                        return "지폐제거에러";

                    case (int)0x24:
                        return "리더기 상단열림";

                    case (int)0x25:
                        return "리더기";

                    case (int)0x28:
                        return "리더기 상단에러";
                    case (int)0x29:
                        return "지폐방출에러";
                    case (int)0x2A:
                        return "잘못된명령어내림";

                    case (int)0x88:
                        return "정상";
                    case 152:
                        return "정상";
                    case 154:
                        return m_KoreaConnectFaile;
                    case (int)0x99:
                        return "시간초과";
                    case 999:
                        return m_KoreaConnectFaile;
                    default:
                        return "알수없는에러";

                }
            }
            else if (deviceName == DEVICE.CC1 || deviceName == DEVICE.CC2 || deviceName == DEVICE.CC3)
            {
                switch (ErrorCode)
                {
                    case 0x01:
                        return "모터이상";
                    case 0x02:
                        return "동전부족";
                    case 0x03:
                        return "이상동전존재";
                    //case 0x04:
                    //    return "Reserved";
                    case 0x04:
                        return "동전걸림 또는 프리즘센서이상";
                    case 0x05:
                        return "쉐프트센서에러-다른크기 동전존재있을수 있음";

                    case (int)0x08:
                        return "10원부족";
                    case (int)0x09:
                        return "50원부족";
                    case (int)0x10:
                        return "100원부족";
                    case (int)0x11:
                        return "500원부족";
                    case (int)0x12:
                        return "10원없음";
                    case (int)0x13:
                        return "50원없음";
                    case (int)0x14:
                        return "100원없음";
                    case (int)0x1A:
                        return "500원없음";

                    case (int)0x88:
                        return "정상";

                    case (int)0x97:
                        return "동전불출안됨";
                    case 152:
                        return "정상";
                    case (int)0x99:
                        return "시간초과";
                    case 154:
                        return m_KoreaConnectFaile;
                    case 999:
                        return m_KoreaConnectFaile;
                    default:
                        return "알수없는에러";

                }
            }
            else
            {
                return "알수없는에러";
            }

        }


        public static string ErrorCode(DEVICE deviceName, int ErrorCode)
        {
            if (deviceName == DEVICE.CA1 || deviceName == DEVICE.CA2)
            {
                switch (ErrorCode)
                {
                    case 1:
                        return m_CardReader_ConnectFail;
                    case 2:
                        return m_CardReader_JAM;
                    case 3:
                        return m_CardReader_Recject_Error;
                    case 88:
                        return m_Device_OK;
                    case 98:
                        return "98";

                    case 99:
                        return "99";
                    case 152:
                        return m_Device_OK;
                    case 154:
                        return m_DeivceConnectFaile;
                    case 999:
                        return m_DeivceConnectFaile;

                    default:
                        return m_UnknowError;

                }
            }
            else if (deviceName == DEVICE.BCH)
            {
                switch (ErrorCode)
                {
                    case 88:
                        return m_Device_OK;
                    case 52:
                        return m_BillDispensor_OverBillDispene;
                    case 56:
                        return m_BillDispensor_Not5000Qty;                          //트레이 위 수량부족
                    case 1001:
                        return m_BillDispensor_Lack1000Qty;                          //1000원 최소수량경고

                    case 1002:
                        return m_BillDispensor_Lack5000Qty;                          //5000원 최소수량경고
                    case 58:                                                         //수량체크에러(CHK3,4센서 ~ DIV센서)
                    case 59:                                                          //Note request error
                    case 60:                                                         //수량체크에러(EJT센서 ~ EXIT센서)
                    case 61:                                                        //수량체크에러(EJT센서 ~ EXIT센서)
                    case 63:                                                         //Reject 트레이 인지불가
                    case 72:                                                         //EJT센서 잼
                    case 73:                                                         //Diverter solenoid or SOL센서 에러
                    case 74:                                                         //SOL센서 에러
                    case 76:                                                         //CHK3,4센서 잼    
                        return m_BillDispensor_SenSorError;
                    case 64:
                        return m_BillDispensor_Not1000Qty; //트레이 아래 수량부족
                    case 65:
                        return m_Bill_MotorFailer;
                    case 100:
                        return m_BillDispensor_NotDispense; //돈이 안나올때 최희주 새로추가
                    case 55:
                        return m_UnknowError;
                    case -999:   //PortOpenError
                    case 67:
                    case -100: //CommunicationError
                        return m_TimeOver;
                    case 152:
                        return m_Device_OK;
                    case 154:
                        return m_DeivceConnectFaile;

                    case 999:
                        return m_DeivceConnectFaile;
                    default:
                        return m_UnknowError;

                }
            }
            else if (deviceName == DEVICE.REP)
            {
                switch (ErrorCode)
                {
                    case 1:
                        return m_Receipt_LackPage;                         // 용지부족
                    case 2:
                        return m_Receipt_NotPage;                          // 용지없음
                    case 88:
                        return m_Device_OK;
                    case 152:
                        return m_Device_OK;
                    case 154:
                        return m_DeivceConnectFaile;

                    case 999:
                        return m_DeivceConnectFaile;

                    default:
                        return m_UnknowError;

                }
            }
            else if (deviceName == DEVICE.BRE)
            {
                switch (ErrorCode)
                {
                    case (int)0x20:
                        return m_Bill_MotorFailer;
                    case (int)0x21:
                        return m_Bill_ChecksumError;
                    case (int)0x22:
                        return m_Bill_BillJamError;
                    case (int)0x23:
                        return m_Bill_BillRemoveError;

                    case (int)0x24:
                        return m_Bill_StackerOpenError;

                    case (int)0x25:
                        return m_Bill_SensorProblemError;

                    case (int)0x28:
                        return m_Bill_StackerProblemError;
                    case (int)0x29:
                        return m_Bill_BillRejectError;
                    case (int)0x2A:
                        return m_Bill_InvalidCommandError;

                    case (int)0x88:
                        return m_Device_OK;
                    case (int)0x99:
                        return m_TimeOver;
                    case 152:
                        return m_Device_OK;
                    case 154:
                        return m_DeivceConnectFaile;
                    default:
                        return m_UnknowError;

                }
            }
            else if (deviceName == DEVICE.CC1 || deviceName == DEVICE.CC2 || deviceName == DEVICE.CC3)
            {
                switch (ErrorCode)
                {
                    case (int)0x01:
                        return m_CoinDispensor_MotorProblem;
                    case (int)0x02:
                        return m_CoinDispensor_CheckForcoinAvailablity;
                    case (int)0x03:
                        return m_CoinDispensor_CoinsSizeVaries;
                    //case 0x04:
                    //    return "Reserved";
                    case (int)0x04:
                        return m_CoinDispensor_PrismSensorFailureorCoinJammed;
                    case (int)0x05:
                        return m_CoinDispensor_ShaftSensorFailure;

                    case (int)0x08:
                        return m_CoinDispensor_Lack10Qty;
                    case (int)0x09:
                        return m_CoinDispensor_Lack50Qty;
                    case (int)0x10:
                        return m_CoinDispensor_Lack100Qty;
                    case (int)0x11:
                        return m_CoinDispensor_Lack500Qty;
                    case (int)0x12:
                        return m_CoinDispensor_Not10Qty;
                    case (int)0x13:
                        return m_CoinDispensor_Not50Qty;
                    case (int)0x14:
                        return m_CoinDispensor_Not100Qty;
                    case (int)0x1A:
                        return m_CoinDispensor_Not500Qty;

                    case (int)0x88:
                        return m_Device_OK;

                    case (int)0x97:
                        return m_CoinDispensor_NotDispense;
                    case 152:
                        return m_Device_OK;
                    case 154:
                        return m_DeivceConnectFaile;
                    case (int)0x99:
                        return m_TimeOver;
                    case (int)999:
                        return m_DeivceConnectFaile;
                    default:
                        return m_UnknowError;

                }
            }
            else
            {
                return "98";
            }

        }

        public class ProtocolData
        {

            #region 주석처리

            /// <summary>
            /// 프로토콜 규칙중 stx etx제외하고 {fs}로 분리했을때 나오는 데이터 
            /// </summary>
            //public enum CommandRegulation
            //{
            //    COMMAND, // 명령어
            //    SENDPARKNO, // 보내는 주차장번호
            //    SENDUNITNO, // 보내는 대상의 장비번호
            //    SENDDATETIME, // 시간(yyyyMMddHHmmss)
            //    RECEIVEPARKNO, // 받는 주차장번호
            //    RECEIVEUNITNO, // 받는 대상의 장비번호
            //    SubCommand // 
            //}

            /// <summary>
            /// 명령어
            /// </summary>
            //public enum Command
            //{
            //    PAYCANCLE,

            //    /// <summary>
            //    /// 사전할인무료
            //    /// </summary>
            //    PREDISCOUNT,
            //    /// <summary>
            //    /// 사전정산차량
            //    /// </summary>
            //    PRECAR,
            //    /// <summary>
            //    /// 회차
            //    /// </summary>
            //    FREECAR,
            //    /// <summary>
            //    /// 정기권
            //    /// </summary>
            //    REG,

            //    /// <summary>
            //    /// 시간오버
            //    /// </summary>
            //    PAYTIMEOUT,

            //    BAROPEN,
            //    /// <summary>
            //    /// 차량정보
            //    /// 차량번호,그룹번호,입차일자,입차시간,출차일자,출차시간,정기권유무,인식여부,차단기오픈여부
            //    /// </summary>
            //    CARINFO,
            //    /// 차량번호,그룹번호,입차일자,입차시간,출차일자,출차시간,정기권유무,인식여부,차단기오픈여부
            //    JUNGSANINFO,
            //    /// <summary>
            //    /// LPR상태
            //    /// LPR상태  (0:비정상/1:정상),차량상태메세지
            //    /// </summary>
            //    LPRSTATE,
            //    /// <summary>
            //    /// 정산기 상태 
            //    /// </summary>
            //    APSTATE,
            //    /// <summary>
            //    /// 수동입차  차량번호,TkNo,입차일자,입차시간,입차장비번호
            //    /// </summary>
            //    CARINSERT,
            //    /// <summary>
            //    /// 현재요청하는 차량요금조회요청 전면차량번호,후면차량번호,주차시간,입차시간,정산시간,출차시간,차종구분,일반/정기여부,출차구분,출차장소,주차요금,할인요금,결제요금,등록된 할인번호
            //    /// </summary>
            //    GET_CARPAY,
            //    /// <summary>
            //    /// 현재요청하는 차량요금조회요청 전면차량번호,후면차량번호,주차시간,입차시간,정산시간,출차시간,차종구분,일반/정기여부,출차구분,출차장소,주차요금,할인요금,결제요금,등록된 할인번호
            //    /// </summary>
            //    MAKE_CARPAY,
            //    /// <summary>
            //    /// 일반차량 영수증재출력
            //    /// </summary>
            //    RECEIPTNORMAL,

            //    /// <summary>
            //    /// 할인 차량번호,TkNo,입차일자,입차시간,현재할인전송
            //    /// </summary>
            //    DISCOUNT,
            //    /// <summary>
            //    /// 차단기관련
            //    /// </summary>
            //    BARRIER,
            //    /// <summary>
            //    /// 장비리셋
            //    /// </summary>
            //    RESET,
            //    /// <summary>
            //    /// 주차요금 강제표출
            //    /// </summary>
            //    CHARGE,
            //    /// <summary>
            //    /// 주차요금요청(센터에서 요금요청 현재 무인정산기에서 요금을받고있는차량정보)
            //    /// </summary>
            //    GETCURRENT_PAYMONEY,
            //    GETCURRENT_ORIGINAL,
            //    /// <summary>
            //    /// 강제 돈방출
            //    /// </summary>
            //    DISPENSE,
            //    /// <summary>
            //    /// 정상 커맨트카아님
            //    /// </summary>
            //    NONCOMMAND,
            //    /// <summary>
            //    /// 수입금
            //    /// </summary>
            //    INCOME,
            //    /// <summary>
            //    /// 수입금
            //    /// </summary>
            //    OUTCOME,
            //    /// <summary>
            //    /// 보유금
            //    /// </summary>
            //    REVERSE,
            //    /// <summary>
            //    /// 무인정산기 가동
            //    /// </summary>
            //    START,
            //    /// <summary>
            //    /// 무인정산기 비가동
            //    /// </summary>
            //    END,
            //    CENTER,
            //    GOSISER,
            //    //미,부분인식/일반/정기차량 입차 차단기 개방설정 적용
            //    /// <summary>
            //    /// 미,부분인식/일반/정기차량 입차 차단기 개방설정
            //    /// </summary>
            //    BAROPENMODE,
            //    //미,부분인식/일반/정기차량 입차 차단기 개방설정 적용완료
            //    //전동어닝 제어 적용
            //    ELECAWNING_CONTROL,
            //    //전동어닝 제어 적용완료
            //    //무인정산기 개방모드 적용
            //    AUTOBOOTH_OPENMODE
            //    //무인정산기 개방모드 적용완료
            //}

            #endregion

            /// <summary>
            /// BRE 지폐리더기, CRE 동전리더기,TRE 교통카드리더기,BCH 지폐방출기,_CC1_동전방출기,CA1 카드리더기 , DID 도어신호 , REP 영수증프린터
            /// </summary>
            public enum DEVICE
            {
                /// <summary>
                /// 지폐리더기
                /// </summary>
                BRE,
                /// <summary>
                /// 동전리더기
                /// </summary>
                CRE,
                /// <summary>
                /// 교통카드리더기
                /// </summary>
                TRE,
                /// <summary>
                /// 지폐방출기
                /// </summary>
                BCH,
                /// <summary>
                /// 50원방출기
                /// </summary>
                CC1,
                /// <summary>
                /// 100원방출기
                /// </summary>
                CC2,
                /// <summary>
                /// 500원방출기
                /// </summary>
                CC3,
                /// <summary>
                /// 카드리더기1
                /// </summary>
                CA1,
                /// <summary>
                /// 카드리더기2
                /// </summary>
                CA2,
                /// <summary>
                /// LED
                /// </summary>
                DID,
                /// <summary>
                /// 영수증
                /// </summary>
                REP,
                /// <summary>
                /// 전광판
                /// </summary>
                DIS,
                /// <summary>
                /// 전체
                /// </summary>
                ALL,
                /// <summary>
                /// 없음
                /// </summary>
                NONE
            }
            //미,부분인식/일반/정기차량 차단기 개방설정 적용
            public enum BAROPENMODE
            {
                //미,부분인식 차량 미개방
                NOT_PART_NOPEN,
                //미,부분인식 차량 개방
                NOT_PART_OPEN,
                //일반차량 미개방
                NORMAL_OPEN,
                //일반차량 개방
                NORMAL_NOPEN,
                //정기차량 개방
                REGIST_OPEN,
                //정기차량 미개방
                REGIST_NOPEN
            }
            //미,부분인식/일반/정기차량 차단기 개방설정 적용완료
            //전동어닝 제어 적용
            public enum ELECAWNING_CONTROL
            {
                //전동어닝 열기
                OPEN,
                //전동어닝 닫기
                CLOSE,
                //전동어닝 멈춤
                STOP
            }
            //전동어닝 제어 적용완료

            // 주차장번호,전송하는장비번호(프로그램장비번호) , 발생장비번호,발생장비IP,   받을장비UNITNO  , 받을장비IP
            private ControlCommand mCurrentCommand = ControlCommand.NONCOMMAND;
            private string mSendParkNO = string.Empty;
            private string mSendUnitNo = string.Empty;
            private string mSendDateTIme = string.Empty;
            private string mReceiveParkNO = string.Empty;
            private string mReceiveUnitNo = string.Empty;

            private string m_IP_Address = "";
            private string m_DeviceType = "";
            private string mSubCommand = string.Empty;
            private bool m_IsSuccess = false;

            public const char _STX_ = (char)0x02;
            public const char _ETX_ = (char)0x03;
            public const char _FS_ = (char)0x1C;
            public const char _GS_ = (char)0x1D;
            public const char _RS_ = (char)0x1E;
            public const char _US_ = (char)0x1F;

            public ProtocolData()
            {
                m_IP_Address = "";
                m_DeviceType = "";

                mCurrentCommand = ControlCommand.NONCOMMAND;
                mSendParkNO = string.Empty;
                mSendDateTIme = string.Empty;
                mReceiveParkNO = string.Empty;
                mReceiveUnitNo = string.Empty;
                mSubCommand = string.Empty;
                m_IsSuccess = false;

            }
            public ProtocolData(string p_message)
            {
                GetProtocolDataInfo(p_message);
            }
            /// <summary>
            /// 들어온데이터가 정상적이면 true반환
            /// </summary>
            /// <returns></returns>
            public bool isCommandRight()
            {
                if (CurrentCommand == ControlCommand.NONCOMMAND)
                {
                    return false;
                }
                return true;
            }
            /// <summary>
            /// 받은 문자메세지를 ProtocolData 멤버변수 형식으로 변환
            /// </summary>
            /// <param name="p_message"></param>
            private void GetProtocolDataInfo(string p_message)
            {
                try
                {

                    p_message = p_message.Replace(TotalContorolProtocol._STX_.ToString(), "").Replace(TotalContorolProtocol._ETX_.ToString(), "").Trim();
                    string[] l_SplitReceiveMessage = p_message.Split(TotalContorolProtocol._FS_); // 데이터 분리
                    CurrentCommand = TotalContorolProtocol.GetCommandNameAsEnum(l_SplitReceiveMessage[(int)CommandRegulation.COMMAND]);
                    mSendParkNO = l_SplitReceiveMessage[(int)CommandRegulation.SENDPARKNO];
                    mSendUnitNo = l_SplitReceiveMessage[(int)CommandRegulation.SENDUNITNO];
                    mSendDateTIme = l_SplitReceiveMessage[(int)CommandRegulation.SENDDATETIME];
                    mReceiveParkNO = l_SplitReceiveMessage[(int)CommandRegulation.RECEIVEPARKNO];
                    mReceiveUnitNo = l_SplitReceiveMessage[(int)CommandRegulation.RECEIVEUNITNO];
                    SubCommand = l_SplitReceiveMessage[(int)CommandRegulation.SubCommand];

                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, "TotalContorolProtocol | GetProtocolDataInfo", $"{ex.Message}", LogAdpType.Biz);
                    CurrentCommand = ControlCommand.NONCOMMAND;
                }

            }
            /// <summary>
            /// Protocoldata 복사
            /// </summary>
            /// <param name="pProtocolData"></param>
            public void Clone(ProtocolData pProtocolData)
            {
                mCurrentCommand = pProtocolData.CurrentCommand;
                mSendParkNO = pProtocolData.SendParkNo;
                mSendUnitNo = pProtocolData.SendUnitNO;
                mSendDateTIme = pProtocolData.SendDateTime;
                mReceiveParkNO = pProtocolData.ReceiveParkNO;
                mReceiveUnitNo = pProtocolData.ReceiveUnitNo;
                SubCommand = pProtocolData.SubCommand;
            }

            public string[] GetSubCommandSplit()
            {
                return SubCommand.Split(_RS_);
            }

            /// <summary>
            /// 명령어
            /// </summary>
            public ControlCommand CurrentCommand
            {
                set { mCurrentCommand = value; }
                get { return mCurrentCommand; }
            }

            public string SendParkNo
            {
                set { mSendParkNO = value; }
                get { return mSendParkNO; }

            }

            public string SendUnitNO
            {
                set { mSendUnitNo = value; }
                get { return mSendUnitNo; }
            }



            public string SendDateTime
            {
                set { mSendDateTIme = value; }
                get { return mSendDateTIme; }

            }




            public string ReceiveParkNO
            {
                set { mReceiveParkNO = value; }
                get { return mReceiveParkNO; }
            }

            public string ReceiveUnitNo
            {
                set { mReceiveUnitNo = value; }
                get { return mReceiveUnitNo; }
            }



            /// <summary>
            /// 보조 명령어
            /// </summary>
            public string SubCommand
            {
                set { mSubCommand = value; }
                get { return mSubCommand; }
            }

            public string SendMakeMessage
            {
                get
                {
                    return (_STX_.ToString() + mCurrentCommand.ToString() + _FS_.ToString()
                                             + mSendParkNO + _FS_.ToString()
                                             + mSendUnitNo + _FS_.ToString()
                                             + mSendDateTIme + _FS_.ToString()
                                             + mReceiveParkNO + _FS_.ToString()
                                             + mReceiveUnitNo + _FS_.ToString()
                                             + SubCommand + _ETX_.ToString()).ToString();
                }
            }

            /// <summary>
            /// 성공여부
            /// </summary>
            public bool IsSuccess
            {
                set { m_IsSuccess = value; }
                get { return m_IsSuccess; }
            }

            /// <summary>
            /// 장비IP
            /// </summary>
            public string IP_Address
            {
                set { m_IP_Address = value; }
                get { return m_IP_Address; }
            }

            /// <summary>
            /// 장비구분
            /// </summary>
            public string DeviceType
            {
                set { m_DeviceType = value; }
                get { return m_DeviceType; }
            }

            /// <summary>
            /// 요금문의전문
            /// </summary>
            /// <param name="pCarInfo"></param>
            /// <returns></returns>
            //public void SubRequestGET_CARPAY(NormalCarInfo pCarInfo)
            //{
            //    StringBuilder sendGetPay = new StringBuilder();
            //    sendGetPay.Append(pCarInfo.ParkNo.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.TkNO);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InCarNumber);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.TkType);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InYMD.Replace("-", ""));
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InHMS.Replace(":", ""));
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(DateTime.Now.ToString("yyyyMMdd"));
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(DateTime.Now.ToString("HHmmss"));
            //    SubCommand = sendGetPay.ToString();

            //}

            public void SubRequestGET_CARPAY(DataRow pCarInfoDatarow)
            {
                StringBuilder sendGetPay = new StringBuilder();
                sendGetPay.Append(pCarInfoDatarow["parkNO"].ToString());
                sendGetPay.Append(_RS_.ToString());
                sendGetPay.Append(pCarInfoDatarow["tkno"].ToString());
                sendGetPay.Append(_RS_.ToString());
                sendGetPay.Append(pCarInfoDatarow["incarNO1"].ToString());
                sendGetPay.Append(_RS_.ToString());
                sendGetPay.Append(pCarInfoDatarow["TkType"].ToString());
                sendGetPay.Append(_RS_.ToString());
                sendGetPay.Append(pCarInfoDatarow["Procdate"].ToString().Replace("-", ""));
                sendGetPay.Append(_RS_.ToString());
                sendGetPay.Append(pCarInfoDatarow["ProcTime"].ToString().Replace(":", ""));
                sendGetPay.Append(_RS_.ToString());
                sendGetPay.Append(DateTime.Now.ToString("yyyyMMdd"));
                sendGetPay.Append(_RS_.ToString());
                sendGetPay.Append(DateTime.Now.ToString("HHmmss"));
                SubCommand = sendGetPay.ToString();

            }

            public void SubRequestGET_RECEIPTNORAML(DataRow pCarInfoDatarow)
            {
                StringBuilder receiptnormal = new StringBuilder();
                receiptnormal.Append(pCarInfoDatarow["parkNO"].ToString());
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(pCarInfoDatarow["tkno"].ToString());
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(pCarInfoDatarow["OutDate"].ToString().Replace("-", ""));
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(pCarInfoDatarow["OutTime"].ToString().Replace(":", ""));
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(DateTime.Now.ToString("yyyyMMdd"));
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(DateTime.Now.ToString("HHmmss"));
                SubCommand = receiptnormal.ToString();

            }

            public void SubRequestGET_GosierNORAML(string pParkNo, string pTkno, string pName, string pPhone, string pRemark)
            {
                StringBuilder receiptnormal = new StringBuilder();
                receiptnormal.Append(pParkNo);
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(pTkno);
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(pName);
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(pPhone);
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(pRemark);
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(DateTime.Now.ToString("yyyyMMdd"));
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(DateTime.Now.ToString("HHmmss"));
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(DateTime.Now.ToString("yyyyMMdd"));
                receiptnormal.Append(_RS_.ToString());
                receiptnormal.Append(DateTime.Now.ToString("HHmmss"));

                SubCommand = receiptnormal.ToString();

            }
            /// <summary>
            /// 요금문의 전문 NormalCarInfo클래스로변환
            /// </summary>
            /// <returns></returns>
            //public NormalCarInfo RequestGET_CARPAY_AsNormalCarInfo()
            //{
            //    NormalCarInfo normalCarInfo = new NormalCarInfo();
            //    string[] carPay = SubCommand.Split(_RS_);
            //    int index = 0;
            //    normalCarInfo.ParkNo = Convert.ToInt32(carPay[index]);
            //    index += 1;
            //    normalCarInfo.TkNO = carPay[index];
            //    index += 1;
            //    normalCarInfo.InCarNumber = carPay[index];
            //    index += 1;
            //    normalCarInfo.TkType = carPay[index];
            //    index += 1;
            //    normalCarInfo.InYMD = carPay[index];
            //    index += 1;
            //    normalCarInfo.InHMS = carPay[index];
            //    index += 1;
            //    normalCarInfo.OutYmd = carPay[index];
            //    index += 1;
            //    normalCarInfo.OutHms = carPay[index];

            //    return normalCarInfo;

            //}

            /// <summary>
            /// 요금문의 응답전문
            /// </summary>
            /// <param name="pCarInfo"></param>
            /// <returns></returns>
            //public void SubResponeGET_CARPAY(NormalCarInfo pCarInfo)
            //{
            //    StringBuilder sendGetPay = new StringBuilder();
            //    sendGetPay.Append(pCarInfo.ParkNo.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InUnitNo.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InCarPath.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InCarNumber.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InRCarPath.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InRCarNumber.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InYMD);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InHMS);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.PreInYmd);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.PreInHms);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.OutYmd);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.OutHms);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.OutChk);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.TkType);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.CarType);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.OutCarPath);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.ParkMoney.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.DiscountMoney.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.ParkTime.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.TkNO.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    for (int i = 0; i < pCarInfo.ListDcDetail.Count; i++)
            //    {
            //        sendGetPay.Append(pCarInfo.ListDcDetail[i].DcNo);
            //        if (i + 1 != pCarInfo.ListDcDetail.Count)
            //        {
            //            sendGetPay.Append(_RS_.ToString());
            //        }
            //    }
            //    SubCommand = sendGetPay.ToString();

            //}

            //public NormalCarInfo GetCarInfo(ProtocolData pProtocolData)
            //{
            //    string pasingData = string.Empty;

            //    pasingData += pProtocolData.CurrentCommand.ToString();
            //    string[] rsData = pProtocolData.SubCommand.Split(_RS_);
            //    NormalCarInfo pCarInfo = new NormalCarInfo();
            //    int index = 0;
            //    pCarInfo.ParkNo = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += "[주차장번호]" + pCarInfo.ParkNo;
            //    index += 1;
            //    pCarInfo.OutCarNumber = rsData[index].ToString();
            //    pasingData += " [차량번호]" + pCarInfo.OutCarNumber.ToString();
            //    index += 1;
            //    pCarInfo.OutCarNumber1 = rsData[index].ToString();
            //    pasingData += " [OutCarNumber1]" + pCarInfo.OutCarNumber1.ToString();
            //    index += 1;
            //    pCarInfo.IO = rsData[index].ToString();
            //    pasingData += " [IO]" + pCarInfo.IO.ToString();
            //    index += 1;
            //    pCarInfo.OutYmd = rsData[index].ToString();
            //    pasingData += " [OutYmd]" + pCarInfo.OutYmd.ToString();
            //    index += 1;
            //    pCarInfo.OutHms = rsData[index].ToString();
            //    pasingData += " [OutHms]" + pCarInfo.OutHms.ToString();
            //    index += 1;
            //    pCarInfo.OutCarPath = rsData[index].ToString();
            //    pasingData += " [OutCarPath]" + pCarInfo.OutCarPath.ToString();
            //    index += 1;
            //    pCarInfo.CarKind = (rsData[index].ToString() == "0" ? "N" : "R");
            //    pasingData += " [CarKind]" + pCarInfo.CarKind.ToString();
            //    index += 1;
            //    pCarInfo.GroupName = rsData[index].ToString();
            //    pasingData += " [GroupName]" + pCarInfo.GroupName.ToString();
            //    return pCarInfo;

            //}
            /// <summary>
            /// 받은 요금문의 전문 클래스로 변환
            /// </summary>
            /// <param name="pSubCommand"></param>
            /// <returns></returns>
            //public NormalCarInfo GET_CARPAYAsNormalCarInfo(ProtocolData pProtocolData)
            //{
            //    //string[] rsData =pSubCommand.Split(_RS_);
            //    //pSubCommand = @"121\\192.168.0.10\MSIMAGE\2016\04\19\CH1_20160419153630_32머9076.JPG32머9076201604191536302016041915531901\\192.168.0.20\MSIMAGE\2016\04\19\CH1_20160419155319_32머9076.JPG6000172016041915363020";
            //    string pasingData = string.Empty;

            //    pasingData += pProtocolData.CurrentCommand.ToString();
            //    string[] rsData = pProtocolData.SubCommand.Split(_RS_);
            //    NormalCarInfo pCarInfo = new NormalCarInfo();
            //    int index = 0;
            //    pCarInfo.ParkNo = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += "[주차장번호]" + pCarInfo.ParkNo;
            //    index += 1;
            //    pCarInfo.InUnitNo = rsData[index].ToString();
            //    pasingData += " [InUnitNo]" + pCarInfo.InUnitNo.ToString();
            //    index += 1;
            //    pCarInfo.InCarPath = rsData[index].ToString();
            //    pasingData += " [InCarPath]" + pCarInfo.InCarPath.ToString();
            //    index += 1;
            //    pCarInfo.InCarNumber = rsData[index].ToString();
            //    pasingData += " [InCarNumber]" + pCarInfo.InCarNumber.ToString();
            //    index += 1;
            //    pCarInfo.InRCarPath = rsData[index].ToString();
            //    pasingData += " [InRCarPath]" + pCarInfo.InRCarPath.ToString();
            //    index += 1;
            //    pCarInfo.InRCarNumber = rsData[index].ToString();
            //    pasingData += " [InRCarNumber]" + pCarInfo.InRCarNumber.ToString();
            //    index += 1;
            //    pCarInfo.InYMD = NPSYS.ConvetYears_Dash(rsData[index].ToString());
            //    pasingData += " [InYMD]" + pCarInfo.InYMD.ToString();
            //    index += 1;
            //    pCarInfo.InHMS = NPSYS.ConvetDay_Dash(rsData[index].ToString());
            //    pasingData += " [InHMS]" + pCarInfo.InHMS.ToString();
            //    index += 1;
            //    pCarInfo.PreInYmd = rsData[index].ToString();
            //    pasingData += " [PreInYmd]" + pCarInfo.PreInYmd.ToString();
            //    index += 1;
            //    pCarInfo.PreInHms = rsData[index].ToString();
            //    pasingData += " [PreInHms]" + pCarInfo.PreInHms.ToString();
            //    index += 1;
            //    pCarInfo.OutYmd = NPSYS.ConvetYears_Dash(rsData[index].ToString());
            //    pasingData += " [OutYmd]" + pCarInfo.OutYmd.ToString();
            //    index += 1;
            //    pCarInfo.OutHms = NPSYS.ConvetDay_Dash(rsData[index].ToString());
            //    pasingData += " [OutHms]" + pCarInfo.OutHms.ToString();
            //    index += 1;
            //    pCarInfo.OutChk = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += " [OutChk]" + pCarInfo.OutChk.ToString();
            //    index += 1;
            //    pCarInfo.TkType = rsData[index].ToString();
            //    pasingData += " [TkType]" + pCarInfo.TkType.ToString();
            //    index += 1;
            //    pCarInfo.CarType = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += " [CarType]" + pCarInfo.CarType.ToString();
            //    index += 1;
            //    pCarInfo.OutCarPath = rsData[index].ToString();
            //    pasingData += " [OutCarPath]" + pCarInfo.OutCarPath.ToString();
            //    index += 1;
            //    pCarInfo.ParkMoney = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += " [ParkMoney]" + pCarInfo.ParkMoney.ToString();
            //    index += 1;
            //    pCarInfo.DiscountMoney = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += " [DiscountMoney]" + pCarInfo.DiscountMoney.ToString();
            //    index += 1;
            //    pCarInfo.ParkTime = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += " [ParkTime]" + pCarInfo.ParkTime.ToString();
            //    index += 1;
            //    pCarInfo.TkNO = rsData[index].ToString();
            //    pasingData += " [TkNO]" + pCarInfo.TkNO.ToString();
            //    index += 1;

            //    pCarInfo.CurrentMoney = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += " [CurrentMoney]" + pCarInfo.CurrentMoney.ToString();
            //    index += 1;
            //    pCarInfo.CardPay = Convert.ToInt32(rsData[index].ToString());
            //    pasingData += " [CardPay]" + pCarInfo.CardPay.ToString();
            //    index += 1;
            //    for (int i = index; i < rsData.Length; i++)
            //    {
            //        if (rsData[i].ToString().Trim() != string.Empty)
            //        {
            //            DcDetail dcdetail = new DcDetail();
            //            dcdetail.DcNo = rsData[i].ToString();
            //            pasingData += " [DCNO]" + dcdetail.DcNo.ToString();
            //            pCarInfo.ListDcDetail.Add(dcdetail);
            //        }
            //    }
            //    TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "TotalContorolProtocol | GET_CARPAYAsNormalCarInfo", "[받은데이터분석]" + pasingData);
            //    return pCarInfo;

            //}

            /// <summary>
            /// 할인요청
            /// </summary>
            /// <param name="pCarInfo"></param>
            /// <returns></returns>
            //public void SubRequestGET_DISCOUNT(NormalCarInfo pCarInfo, string pDcNo, string pIamgePath, string pRemark)
            //{
            //    StringBuilder sendGetPay = new StringBuilder();
            //    sendGetPay.Append(pCarInfo.ParkNo.ToString());
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.TkNO);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InCarNumber);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.TkType);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InYMD);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pCarInfo.InHMS);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pDcNo);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pIamgePath);
            //    sendGetPay.Append(_RS_.ToString());
            //    sendGetPay.Append(pRemark);
            //    SubCommand = sendGetPay.ToString();

            //}



            /// <summary>
            /// 할인요청
            /// </summary>
            /// <param name="pCarInfo"></param>
            /// <returns></returns>
            public void GET_DiscountAsNormalCarInfo(string pSubCommand, ref string pCarNumber, ref string pDcno, ref string pResult)
            {
                string[] rsData = pSubCommand.Split(_RS_);
                int index = 0;
                string ParkNo = rsData[index].ToString();
                index += 1;
                string TkNO = rsData[index].ToString();
                index += 1;
                string InCarNumber = rsData[index].ToString();
                pCarNumber = InCarNumber;
                index += 1;
                string TkType = rsData[index].ToString().Replace(":", "");
                index += 1;
                string InYMD = rsData[index].ToString().Replace("-", "");
                index += 1;
                string InHMS = rsData[index].ToString().Replace(":", "");
                index += 1;
                pDcno = rsData[index].ToString().Replace(":", "");
                index += 1;
                pResult = rsData[index].ToString().Replace(":", "");


            }

            //public void SetCarInfo(NormalCarInfo pCarInfo)
            //{
            //    SubCommand = pCarInfo.OutCarNumber + _RS_.ToString()
            //                + NPSYS.ConvetYears_Dash(pCarInfo.InYMD) + _RS_.ToString()
            //                + NPSYS.ConvetDay_Dash(pCarInfo.InHMS) + _RS_.ToString()
            //                + NPSYS.ConvetYears_Dash(pCarInfo.OutYmd) + _RS_.ToString()
            //                + NPSYS.ConvetDay_Dash(pCarInfo.OutHms) + _RS_.ToString()
            //                + pCarInfo.TkNO + _RS_.ToString()
            //                + pCarInfo.GroupName + _RS_.ToString()
            //                + NPSYS.ConvetYears_Dash(pCarInfo.InYMD) + " " + NPSYS.ConvetDay_Dash(pCarInfo.InHMS) + _RS_.ToString()
            //                + (pCarInfo.CarKind == "N" ? "0" : "1") + _RS_.ToString()
            //                + (pCarInfo.OutRecog1 == 1 ? "1" : "0") + _RS_.ToString()
            //                + (pCarInfo.IsBarOpen == true ? "1" : "0") + _RS_.ToString()
            //                + pCarInfo.InCarPath + _RS_.ToString()
            //                + pCarInfo.OutCarPath + _RS_.ToString();


            //    //차량번호	입차일자	입차시간	출차일자	출차시간	TKNO	그룹명	출차시간	정기권유무	LPR판별상태	차단기열기상태
            //}
            /// <summary>
            /// 
            /// </summary>
            /// <param name="pUnitNo"></param>
            /// <param name="pBarStatus">0:OPEN,1:CLOSE,2:OPENLOCK,3:UNLOCK</param>
            public void SetBarControl(string pUnitNo, string pBarStatus)
            {
                SubCommand = pUnitNo + _RS_.ToString() + pBarStatus;
            }
            
            
        }

        ///// <summary>
        ///// 약속된 프로토콜 정보를 변수로 가지고 있을 클래스 IP,장비구분,명령어,섭명령어 포함
        ///// </summary>
        //public class ProtocolData
        //{
        //    private string m_IP_Address = "";
        //    private string m_DeviceType = "";

        //    private string m_SubCommand = "";
        //    private TotalContorolProtocol.ControlCommand m_ControlCommand = ControlCommand.NONCOMMAND;
        //    private bool m_IsSuccess = false;
        //    private string m_Command = "";

        //    public ProtocolData()
        //    {
        //        m_IP_Address = "";
        //        m_DeviceType = "";
        //        m_SubCommand = "";
        //        m_ControlCommand = ControlCommand.NONCOMMAND;
        //        m_IsSuccess = false;
        //        m_Command = "";
        //    }
        //    public ProtocolData(string p_message)
        //    {
        //        GetProtocolDataInfo(p_message);
        //    }
        //    /// <summary>
        //    /// 들어온데이터가 정상적이면 true반환
        //    /// </summary>
        //    /// <returns></returns>
        //    public bool isCommandRight()
        //    {
        //        if (Command == ControlCommand.NONCOMMAND)
        //        {
        //            return false;
        //        }
        //        return true;
        //    }
        //    /// <summary>
        //    /// 받은 문자메세지를 ProtocolData 멤버변수 형식으로 변환
        //    /// </summary>
        //    /// <param name="p_message"></param>
        //    private void GetProtocolDataInfo(string p_message)
        //    {
        //        try
        //        {
        //            p_message = p_message.Replace(TotalContorolProtocol._STX_.ToString(), "").Replace(TotalContorolProtocol._ETX_.ToString(), "").Trim();
        //            string[] l_SplitReceiveMessage = p_message.Split(TotalContorolProtocol._FS_); // 데이터 분리
        //            Command = TotalContorolProtocol.GetCommandNameAsEnum(l_SplitReceiveMessage[(int)TotalContorolProtocol.CommandRegulation.COMMAND]);
        //            IP_Address = l_SplitReceiveMessage[(int)TotalContorolProtocol.CommandRegulation.IPADDRESS];
        //            DeviceType = l_SplitReceiveMessage[(int)TotalContorolProtocol.CommandRegulation.BOOTHTYPE];
        //            CommandString = l_SplitReceiveMessage[(int)TotalContorolProtocol.CommandRegulation.COMMAND];
        //            SubCommand = l_SplitReceiveMessage[(int)TotalContorolProtocol.CommandRegulation.SUBCOMMAND];
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.WriteLog(LogType.Error, $"TotalContorolProtocol | GetProtocolDataInfo", $"{ex.Message}", LogAdpType.Biz);
        //            Command = TotalContorolProtocol.ControlCommand.NONCOMMAND;
        //        }

        //    }
        //    /// <summary>
        //    /// 장비IP
        //    /// </summary>
        //    public string IP_Address
        //    {
        //        set { m_IP_Address = value; }
        //        get { return m_IP_Address; }
        //    }

        //    /// <summary>
        //    /// 장비구분
        //    /// </summary>
        //    public string DeviceType
        //    {
        //        set { m_DeviceType = value; }
        //        get { return m_DeviceType; }
        //    }

        //    /// <summary>
        //    /// 명령어
        //    /// </summary>
        //    public TotalContorolProtocol.ControlCommand Command
        //    {
        //        set { m_ControlCommand = value; }
        //        get { return m_ControlCommand; }
        //    }

        //    public string CommandString
        //    {
        //        set { m_Command = value; }
        //        get { return m_Command; }
        //    }

        //    /// <summary>
        //    /// 보조 명령어
        //    /// </summary>
        //    public string SubCommand
        //    {
        //        set { m_SubCommand = value; }
        //        get { return m_SubCommand; }
        //    }

        //    public string SendMessage
        //    {
        //        get { return (_STX_.ToString() + _FS_.ToString() + IP_Address + _FS_.ToString() + DeviceType + _FS_.ToString() + CommandString + _FS_.ToString() + SubCommand + _FS_.ToString() + _ETX_).ToString(); }
        //    }

        //    /// <summary>
        //    /// 성공여부
        //    /// </summary>
        //    public bool IsSuccess
        //    {
        //        set { m_IsSuccess = value; }
        //        get { return m_IsSuccess; }
        //    }

        //    public void ClearData()
        //    {
        //        m_IP_Address = "";
        //        m_DeviceType = "";
        //        m_SubCommand = "";
        //        m_Command = "";
        //        m_ControlCommand = ControlCommand.NONCOMMAND;
        //        m_IsSuccess = false;
        //    }
        //}
    }
}
