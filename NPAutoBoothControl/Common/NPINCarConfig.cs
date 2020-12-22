using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reflection;



namespace Common
{
    /// <summary>
    /// 설정데이터 관련 클래스
    /// DB설정등의 설정값을 불러오가나 명명한 클래스이다
    /// </summary>
    public class ConfigLoad
    {

        public NPINCarConfig GetConfigLoad(ref NPINCarConfig pNpincarConfig)
        {
            if (pNpincarConfig == null) pNpincarConfig = new NPINCarConfig();

            XMLConfig xmlConfig = new XMLConfig();
            pNpincarConfig.DBServerIp = xmlConfig.GetAppValue(NPINCarConfig.Setting.DBServerIp.ToString()).Trim();

            pNpincarConfig.DBUserID = xmlConfig.GetAppValue(NPINCarConfig.Setting.DBUserID.ToString()).Trim();

            pNpincarConfig.DBPassword = xmlConfig.GetAppValue(NPINCarConfig.Setting.DBPassword.ToString()).Trim();

            string databasePort = xmlConfig.GetAppValue(NPINCarConfig.Setting.DatabasePort.ToString().Trim()) == string.Empty ? "42130" : xmlConfig.GetAppValue(NPINCarConfig.Setting.DatabasePort.ToString()).Trim();
            pNpincarConfig.DatabasePort = Convert.ToInt32(databasePort);


            pNpincarConfig.DBName = xmlConfig.GetAppValue(NPINCarConfig.Setting.DBName.ToString()).Trim();

            string DBConnectionTimeout = xmlConfig.GetAppValue(NPINCarConfig.Setting.DBConnectionTimeout.ToString().Trim()) == string.Empty ? "5" : xmlConfig.GetAppValue(NPINCarConfig.Setting.DBConnectionTimeout.ToString()).Trim();
            pNpincarConfig.DBConnectionTimeout = Convert.ToInt32(DBConnectionTimeout);

            string DBCommandTimeout = xmlConfig.GetAppValue(NPINCarConfig.Setting.DBCommandTimeout.ToString().Trim()) == string.Empty ? "30" : xmlConfig.GetAppValue(NPINCarConfig.Setting.DBCommandTimeout.ToString()).Trim();
            pNpincarConfig.DBCommandTimeout = Convert.ToInt32(DBCommandTimeout);


            pNpincarConfig.ParkCode = xmlConfig.GetAppValue(NPINCarConfig.Setting.ParkCode.ToString()).Trim();

            pNpincarConfig.DevCode = xmlConfig.GetAppValue(NPINCarConfig.Setting.DevCode.ToString()).Trim();

            pNpincarConfig.BarChannelNumber = xmlConfig.GetAppValue(NPINCarConfig.Setting.BarChannelNumber.ToString()).Trim() == string.Empty ? "1" : xmlConfig.GetAppValue(NPINCarConfig.Setting.BarChannelNumber.ToString()).Trim();



            string parkingLevel = xmlConfig.GetAppValue(NPINCarConfig.Setting.ParkingLevel.ToString()).Trim() == string.Empty ? "Parking" : xmlConfig.GetAppValue(NPINCarConfig.Setting.ParkingLevel.ToString()).Trim();
            pNpincarConfig.ParkingLevel = (NPINCarConfig.parkingLevle)Enum.Parse(typeof(NPINCarConfig.parkingLevle), parkingLevel);




            pNpincarConfig.ImageIp = xmlConfig.GetAppValue(NPINCarConfig.Setting.ImageIp.ToString()).Trim() == string.Empty ? "" : xmlConfig.GetAppValue(NPINCarConfig.Setting.ImageIp.ToString()).Trim();


            string CurrentTcpMode = xmlConfig.GetAppValue(NPINCarConfig.Setting.CurrentTcpMode.ToString()).Trim() == string.Empty ? "Local" : xmlConfig.GetAppValue(NPINCarConfig.Setting.CurrentTcpMode.ToString()).Trim();
            pNpincarConfig.CurrentTcpMode = (NPINCarConfig.TcpMode)Enum.Parse(typeof(NPINCarConfig.TcpMode), CurrentTcpMode);


            string CurrentCameraType = xmlConfig.GetAppValue(NPINCarConfig.Setting.CurrentCameraType.ToString()).Trim() == string.Empty ? NPINCarConfig.CameraType.OLD.ToString() : xmlConfig.GetAppValue(NPINCarConfig.Setting.CurrentCameraType.ToString()).Trim();
            pNpincarConfig.CurrentCameraType = (NPINCarConfig.CameraType)Enum.Parse(typeof(NPINCarConfig.CameraType), CurrentCameraType);

            pNpincarConfig.CameraImagePath = xmlConfig.GetAppValue(NPINCarConfig.Setting.CameraImagePath.ToString()).Trim() == @"C:\Image" ? "" : xmlConfig.GetAppValue(NPINCarConfig.Setting.CameraImagePath.ToString()).Trim();
            
            pNpincarConfig.CameraImageIp = xmlConfig.GetAppValue(NPINCarConfig.Setting.CameraImageIp.ToString()).Trim() == "127.0.0.1" ? "" : xmlConfig.GetAppValue(NPINCarConfig.Setting.CameraImageIp.ToString()).Trim();

            pNpincarConfig.CameraDataPath = xmlConfig.GetAppValue(NPINCarConfig.Setting.CameraDataPath.ToString()).Trim() == string.Empty ? "Image" : xmlConfig.GetAppValue(NPINCarConfig.Setting.CameraDataPath.ToString()).Trim();

            string CurrentCameraStream = xmlConfig.GetAppValue(NPINCarConfig.Setting.CurrentCameraStream.ToString()).Trim() == string.Empty ? NPINCarConfig.CameraSTREAM.HD.ToString() : xmlConfig.GetAppValue(NPINCarConfig.Setting.CurrentCameraStream.ToString()).Trim();
            pNpincarConfig.CurrentCameraStream = (NPINCarConfig.CameraSTREAM)Enum.Parse(typeof(NPINCarConfig.CameraSTREAM), CurrentCameraStream);
            //카메라처리 로직분리 적용
            string UseCamera = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseCamera.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseCamera.ToString()).Trim();
            pNpincarConfig.UseCamera = (UseCamera == "True" ? true : false);
            //카메라처리 로직분리 적용완료

            // 신규카메라 관련 변경
            string UseGosiser = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseGosiser.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseGosiser.ToString()).Trim();
            pNpincarConfig.UseGosiser = (UseGosiser == "True" ? true : false);



            string UseNormalBooth = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseNormalBooth.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseNormalBooth.ToString()).Trim();
            pNpincarConfig.UseNormalBooth = (UseNormalBooth == "True" ? true : false);

            string UseInCarBOpenMode = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseInCarBOpenMode.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseInCarBOpenMode.ToString()).Trim();
            pNpincarConfig.UseInCarBOpenMode = (UseInCarBOpenMode == "True" ? true : false);
            //전동어닝 제어 적용
            string UseElecAwning = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseElecAwning.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseElecAwning.ToString()).Trim();
            pNpincarConfig.UseElecAwning = (UseElecAwning == "True" ? true : false);
            //전동어닝 제어 적용완료
            //원격 영수증발행 옵션처리 적용
            string UseReceiptPrint = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseReceiptPrint.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseReceiptPrint.ToString()).Trim();
            pNpincarConfig.UseReceiptPrint = (UseReceiptPrint == "True" ? true : false);
            //원격 영수증발행 옵션처리 적용완료

            //대형차 처리 적용
            string UseBigCarCharge = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseBigCarCharge.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseBigCarCharge.ToString()).Trim();
            pNpincarConfig.UseBigCarCharge = (UseBigCarCharge == "True" ? true : false);
            //사전할인등록 적용
            string UsePreDiscount = xmlConfig.GetAppValue(NPINCarConfig.Setting.UsePreDiscount.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UsePreDiscount.ToString()).Trim();
            pNpincarConfig.UsePreDiscount = (UsePreDiscount == "True" ? true : false);
            //사전할인등록 적용완료
            //무인정산기 개방모드 적용
            string UseOpenMode = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseOpenMode.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseOpenMode.ToString()).Trim();
            pNpincarConfig.UseOpenMode = (UseOpenMode == "True" ? true : false);
            //무인정산기 개방모드 적용완료

            //2020-04 이재영 : 신용카드 취소기능 추가
            string UseCardCancel = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseCardCancel.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseCardCancel.ToString()).Trim();
            pNpincarConfig.UseCardCancel = (UseCardCancel == "True" ? true : false);
            //2020-04 이재영 : 신용카드 취소기능 추가완료

            //2020-06 이재영 : 멀티모니터기능 추가
            string UseMultiMonitor = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseMultiMonitor.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseMultiMonitor.ToString()).Trim();
            pNpincarConfig.UseMultiMonitor = (UseMultiMonitor == "True" ? true : false);
            //2020-06 이재영 : 멀티모니터기능 추가완료

            //2020-06 이재영 : 멀티모니터 카드총액기능 추가
            string UseVanTotal = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseVanTotal.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseVanTotal.ToString()).Trim();
            pNpincarConfig.UseVanTotal = (UseVanTotal == "True" ? true : false);
            //2020-06 이재영 : 멀티모니터 카드총액기능 추가완료

            //수동출차사유입력 적용
            string UseReasonInput = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseReasonInput.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseReasonInput.ToString()).Trim();
            pNpincarConfig.UseReasonInput = (UseReasonInput == "True" ? true : false);
            //수동출차사유입력 적용완료

            //IO신호시 보조차단기제어 관련 적용
            //보조차단기 제어 사용유무
            string UseIObarControl = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseIObarControl.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseIObarControl.ToString()).Trim();
            pNpincarConfig.UseIObarControl = (UseIObarControl == "True" ? true : false);
            //보조차단기 ON/OFF모드 사용유무
            string UseIObarOnOffMode = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseIObarOnOffMode.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseIObarOnOffMode.ToString()).Trim();
            pNpincarConfig.UseIObarOnOffMode = (UseIObarOnOffMode == "True" ? true : false);
            //IO보드 TCP IP
            pNpincarConfig.IOBoardIP = xmlConfig.GetAppValue(NPINCarConfig.Setting.IOBoardIP.ToString()).Trim() == "" ? "192.168.0.100" : xmlConfig.GetAppValue(NPINCarConfig.Setting.IOBoardIP.ToString()).Trim();
            //IO보드 TCP PORT
            pNpincarConfig.IOBoardPORT = xmlConfig.GetAppValue(NPINCarConfig.Setting.IOBoardPORT.ToString()).Trim() == "" ? "5000" : xmlConfig.GetAppValue(NPINCarConfig.Setting.IOBoardPORT.ToString()).Trim();
            //IO채널번호
            string IOChannelNo = xmlConfig.GetAppValue(NPINCarConfig.Setting.IOChannelNo.ToString()).Trim() == string.Empty ? NPINCarConfig.IOChannel.CHANNEL_1.ToString() : xmlConfig.GetAppValue(NPINCarConfig.Setting.IOChannelNo.ToString()).Trim();
            pNpincarConfig.IOChannelNo = (NPINCarConfig.IOChannel)Enum.Parse(typeof(NPINCarConfig.IOChannel), IOChannelNo);
            //IO호출시간
            pNpincarConfig.IOPollTime = xmlConfig.GetAppValue(NPINCarConfig.Setting.IOPollTime.ToString()).Trim() == "" ? "2" : xmlConfig.GetAppValue(NPINCarConfig.Setting.IOPollTime.ToString()).Trim();
            //보조차단기 열기 릴레이번호
            string IOBarOpenRelay = xmlConfig.GetAppValue(NPINCarConfig.Setting.IOBarOpenRelay.ToString()).Trim() == string.Empty ? NPINCarConfig.IORelayNo.RELAY_1.ToString() : xmlConfig.GetAppValue(NPINCarConfig.Setting.IOBarOpenRelay.ToString()).Trim();
            pNpincarConfig.IOBarOpenRelay = (NPINCarConfig.IORelayNo)Enum.Parse(typeof(NPINCarConfig.IORelayNo), IOBarOpenRelay);
            //보조차단기 닫기 릴레이번호
            string IOBarCloseRelay = xmlConfig.GetAppValue(NPINCarConfig.Setting.IOBarCloseRelay.ToString()).Trim() == string.Empty ? NPINCarConfig.IORelayNo.RELAY_1.ToString() : xmlConfig.GetAppValue(NPINCarConfig.Setting.IOBarCloseRelay.ToString()).Trim();
            pNpincarConfig.IOBarCloseRelay = (NPINCarConfig.IORelayNo)Enum.Parse(typeof(NPINCarConfig.IORelayNo), IOBarCloseRelay);
            //IO신호시 보조차단기제어 관련 적용 완료
            //SMS전원제어기능                    
            string UseSMSPowerControl = xmlConfig.GetAppValue(NPINCarConfig.Setting.UseSMSPowerControl.ToString()).Trim() == string.Empty ? "False" : xmlConfig.GetAppValue(NPINCarConfig.Setting.UseSMSPowerControl.ToString()).Trim();
            pNpincarConfig.UseSMSPowerControl = (UseSMSPowerControl == "True" ? true : false);
            //SMS전원제어기능 적용

            return pNpincarConfig;
        }

    }
    [DefaultPropertyAttribute("환경설정")]
    public class NPINCarConfig
    {

        public enum Setting
        {
            /// <summary>
            /// DB서버IP
            /// </summary>
            DBServerIp,
            /// <summary>
            /// DB서버 ID
            /// </summary>
            DBUserID,
            /// <summary>
            /// DB서버 암호
            /// </summary>
            DBPassword,
            /// <summary>
            /// 데이터베이스명
            /// </summary>
            DBName,
            /// <summary>
            /// 데이터베이스포트
            /// </summary>
            DatabasePort,
            DBConnectionTimeout,
            DBCommandTimeout,
            ParkCode,
            DevCode,
            BarChannelNumber,
            ParkingLevel,
            ServerIp,
            ServerPort,
            ImageIp,
            CurrentTcpMode,
            CurrentCameraType,
            /// <summary>
            /// 카메라이미지저장풀경로
            /// </summary>
            CameraImagePath,
            /// <summary>
            /// 카메라이미지IP
            /// </summary>
            CameraImageIp,
            /// <summary>
            /// 카메라이미지데이터저장파일경로
            /// </summary>
            CameraDataPath,
            //카메라처리 로직분리 적용
            UseCamera,
            //카메라처리 로직분리 적용완료
            UseGosiser,
            CurrentCameraStream,
            /// <summary>
            /// 유인정산기 사용
            /// </summary>
            UseNormalBooth,
            /// <summary>
            /// 입차출입통제 사용
            /// </summary>
            UseInCarBOpenMode,
            //전동어닝 제어 적용
            /// <summary>
            /// 전동어닝 제어 사용
            /// </summary>
            UseElecAwning,
            //전동어닝 제어 적용완료
            //원격 영수증발행 옵션처리 적용
            UseReceiptPrint,
            //원격 영수증발행 옵션처리 적용완료

            //대형차 요금 적용
            UseBigCarCharge,
            //사전할인등록 적용
            UsePreDiscount,
            //사전할인등록 적용완료
            //수동출차사유입력
            UseReasonInput,
            //무인정산기 개방모드 적용
            UseOpenMode,
            //무인정사닉 개방모드 적용완료
            //2020-04 이재영 : 신용카드 취소기능 추가
            UseCardCancel,
            //2020-04 이재영 : 신용카드 취소기능 추가완료
            //2020-06 이재영 : 멀티모니터기능 추가
            UseMultiMonitor,
            //2020-06 이재영 : 멀티모니터기능 추가완료
            //2020-06 이재영 : 멀티모니터 카드총액기능 추가
            UseVanTotal,
            //2020-06 이재영 : 멀티모니터 카드총액기능 추가완료
            //IO신호시 보조차단기제어 적용
            UseIObarControl,
            UseIObarOnOffMode,
            IOBoardIP,
            IOBoardPORT,
            IOChannelNo,
            IOPollTime,
            IOBarOpenRelay,
            IOBarCloseRelay,
            //IO신호시 보조차단기제어 적용완료
            //SMS전원제어 적용
            UseSMSPowerControl
            //SMS전원제어 적용완료
        }




        public enum parkingLevle
        {
            /// <summary>
            /// 통합센터
            /// </summary>
            TotalCenter,
            /// <summary>
            /// 주차장
            /// </summary>
            Parking

        }

        public enum TcpMode
        {
            /// <summary>
            /// 로컬에서 원격지원
            /// </summary>
            Local,
            /// <summary>
            /// 인터넷에서 원격지원
            /// </summary>
            Internet
        }

        public enum CameraType
        {
            OLD,
            NEW
        }
        public enum CameraSTREAM
        {
            HD = 0,
            VGA = 1
        }



        public enum IORelayNo
        {
            RELAY_1 = 49,
            RELAY_2,
            RELAY_3,
            RELAY_4,
            RELAY_5,
            RELAY_6,
            RELAY_7,
            RELAY_8,
        }

        public enum IOChannel
        {
            CHANNEL_1 = 1,
            CHANNEL_2,
            CHANNEL_3,
            CHANNEL_4,
            CHANNEL_5,
            CHANNEL_6,
            CHANNEL_7,
            CHANNEL_8,
        }


        private IORelayNo mOpenRelayNo = IORelayNo.RELAY_1;
        private IORelayNo mCloseRelayNo = IORelayNo.RELAY_1;
        private IOChannel mIOChannelNo = IOChannel.CHANNEL_1;

        private parkingLevle mParkingLevel = parkingLevle.Parking;
        private TcpMode mCurrentTcpMode = TcpMode.Local;
        private CameraType mCurrentCameraType = CameraType.OLD;
        private CameraSTREAM mCurrentCameraStream = CameraSTREAM.HD;

        [CategoryAttribute("DB"), DescriptionAttribute("DB서버주소"), ReadOnly(false), DisplayName("DBServerIp")]
        public String DBServerIp { get; set; }
        [CategoryAttribute("DB"), DescriptionAttribute("DB사용자ID"), ReadOnly(false), DisplayName("DBUserID")]
        public String DBUserID { get; set; }
        [CategoryAttribute("DB"), DescriptionAttribute("DB사용자암호"), ReadOnly(false), DisplayName("DBPassword")]
        public String DBPassword { get; set; }
        [CategoryAttribute("DB"), DescriptionAttribute("DB사용자암호"), ReadOnly(false), DisplayName("DatabasePort")]
        public int DatabasePort { get; set; }
        [CategoryAttribute("DB"), DescriptionAttribute("데이터베이스명"), ReadOnly(false), DisplayName("DBName")]
        public String DBName { get; set; }
        [CategoryAttribute("DB"), DescriptionAttribute("DB연결제한시간"), ReadOnly(false), DisplayName("DBConnectionTimeout")]
        public int DBConnectionTimeout { get; set; }
        [CategoryAttribute("DB"), DescriptionAttribute("DB명령어제한시간"), ReadOnly(false), DisplayName("DBCommandTimeout")]
        public int DBCommandTimeout { get; set; }



        [CategoryAttribute("주차모드설정"), DescriptionAttribute("주차장코드"), ReadOnly(false), DisplayName("ParkCode")]
        public String ParkCode { get; set; }

        [CategoryAttribute("주차모드설정"), DescriptionAttribute("주차 장비번호"), ReadOnly(false), DisplayName("DevCode")]
        public String DevCode { get; set; }

        [CategoryAttribute("주차모드설정"), DescriptionAttribute("차단기 채널번호 기본 1"), ReadOnly(false), DisplayName("BarChannelNumber")]
        public String BarChannelNumber { get; set; }

        // 신규카메라 관련 변경

        [CategoryAttribute("카메라설정"), DescriptionAttribute("카메라버젼"), ReadOnly(false), DisplayName("CurrentCameraType")]
        public CameraType CurrentCameraType
        {
            get { return mCurrentCameraType; }
            set { mCurrentCameraType = value; }
        }

        [CategoryAttribute("카메라설정"), DescriptionAttribute(@"카메라이미지저장풀경로 예)c:\Image\"), ReadOnly(false), DisplayName("CameraImagePath")]
        public string CameraImagePath { get; set; }

        [CategoryAttribute("카메라설정"), DescriptionAttribute("카메라이미지저장IP  예)192.168.0.100"), ReadOnly(false), DisplayName("CameraImageIp")]
        public string CameraImageIp { get; set; }

        [CategoryAttribute("카메라설정"), DescriptionAttribute("카메라이미지데이터저장파일경로 예)Image "), ReadOnly(false), DisplayName("CameraDataPath")]
        public string CameraDataPath { get; set; }

        [CategoryAttribute("카메라설정"), DescriptionAttribute("카메라 스티림"), ReadOnly(false), DisplayName("CurrentCameraStream")]
        public CameraSTREAM CurrentCameraStream
        {
            get { return mCurrentCameraStream; }
            set { mCurrentCameraStream = value; }

        }
        //카메라처리 로직분리 적용
        [CategoryAttribute("카메라설정"), DescriptionAttribute("카메라 사용유무"), ReadOnly(false), DisplayName("UseCamera")]
        public bool UseCamera { get; set; }
        //카메라처리 로직분리 적용완료
        [CategoryAttribute("기타설정"), DescriptionAttribute("원격고지서 사용유무 "), ReadOnly(false), DisplayName("UseGosiser")]
        public bool UseGosiser { get; set; }
        // 신규카메라 관련 변경완료


        [CategoryAttribute("주차장레벨"), DescriptionAttribute("통합센터 또는 개별주차장"), ReadOnly(false), DisplayName("ParkingLevel")]

        public parkingLevle ParkingLevel
        {
            get { return mParkingLevel; }
            set { mParkingLevel = value; }
        }


        [CategoryAttribute("이미지관련"), DescriptionAttribute("이미지경로강제변경"), ReadOnly(false), DisplayName("ImageIp")]
        public string ImageIp
        {
            get { return mImageIp; }
            set { mImageIp = value; }

        }
        private string mImageIp = string.Empty;

        [CategoryAttribute("통신모드"), DescriptionAttribute("무인기와 통신연결시 인터넷 또는 로컬설정여부 인터넷이면 UNITINFO에 RESERVE4가IP RESERVE5 PORT가됨"), ReadOnly(false), DisplayName("CurrentTcpMode")]

        public TcpMode CurrentTcpMode
        {
            get { return mCurrentTcpMode; }
            set { mCurrentTcpMode = value; }
        }


        [CategoryAttribute("기타설정"), DescriptionAttribute("유인정산기 사용유무 사용이면 Reserve2에 1값만 사용함 "), ReadOnly(false), DisplayName("UseNormalBooth")]
        public bool UseNormalBooth { get; set; }
        // 신규카메라 관련 변경완료
        [CategoryAttribute("기타설정"), DescriptionAttribute("입차출입통제 사용유무"), ReadOnly(false), DisplayName("UseInCarBOpenMode")]
        public bool UseInCarBOpenMode { get; set; }
        //전동어닝 제어 적용
        [CategoryAttribute("기타설정"), DescriptionAttribute("전동어닝 제어 사용유무"), ReadOnly(false), DisplayName("UseElecAwning")]
        public bool UseElecAwning { get; set; }
        //전동어닝 제어 적용완료
        //원격 영수증발행 옵션처리 적용
        [CategoryAttribute("기타설정"), DescriptionAttribute("원격 영수증발행 사용유무"), ReadOnly(false), DisplayName("UseReceiptPrint")]
        public bool UseReceiptPrint { get; set; }
        //원격 영수증발행 옵션처리 적용완료

        //대형차 처리 적용
        [CategoryAttribute("기타설정"), DescriptionAttribute("대형차 처리기능 사용유무"), ReadOnly(false), DisplayName("UseBigCarCharge")]
        public bool UseBigCarCharge { get; set; }
        //사전할인등록 적용
        [CategoryAttribute("기타설정"), DescriptionAttribute("사전할인기능 사용유무"), ReadOnly(false), DisplayName("UsePreDiscount")]
        public bool UsePreDiscount { get; set; }
        //사전할인등록 적용완료
        //수동출차사유입력 적용
        [CategoryAttribute("기타설정"), DescriptionAttribute("수동출차사유입력 사용유무"), ReadOnly(false), DisplayName("UseReasonInput")]
        public bool UseReasonInput { get; set; }
        //수동출차사유입력 적용완료
        //무인정산기 개방모드 적용
        [CategoryAttribute("기타설정"), DescriptionAttribute("무인정산기 개방모드 유무 "), ReadOnly(false), DisplayName("UseOpenMode")]
        public bool UseOpenMode { get; set; }
        //무인정산기 개방모드 적용완료   


        //2020-04 이재영 : 신용카드 취소기능 추가
        [CategoryAttribute("기타설정"), DescriptionAttribute("무인정산기 카드취소 사용유무 "), ReadOnly(false), DisplayName("UseCardCancel")]
        public bool UseCardCancel { get; set; }
        //2020-04 이재영 : 신용카드 취소기능 추가완료

        //2020-06 이재영 : 멀티모니터기능 추가
        [CategoryAttribute("기타설정"), DescriptionAttribute("다중 부스 모니터 사용유무 "), ReadOnly(false), DisplayName("UseMultiMonitor")]
        public bool UseMultiMonitor { get; set; }
        //2020-06 이재영 : 멀티모니터기능 추가완료

        //2020-06 이재영 : 멀티모니터 카드총액기능 추가
        [CategoryAttribute("기타설정"), DescriptionAttribute("다중 부스 모니터 사용유무 "), ReadOnly(false), DisplayName("UseVanTotal")]
        public bool UseVanTotal { get; set; }
        //2020-06 이재영 : 멀티모니터 카드총액기능 추가완료

        //I.O보드 차단기제어 관련
        [CategoryAttribute("IO차단기제어"), DescriptionAttribute("보조차단기 제어사용 유무"), ReadOnly(false), DisplayName("UseIObarControl")]
        public bool UseIObarControl { get; set; }
        //보조차단기 ON/OFF 모드 사용유무
        [CategoryAttribute("IO차단기제어"), DescriptionAttribute("보조차단기 ON/OFF모드 사용유무"), ReadOnly(false), DisplayName("UseIObarOnOffMode")]
        public bool UseIObarOnOffMode { get; set; }
        //IO보드 TCP IP
        [CategoryAttribute("IO차단기제어"), DescriptionAttribute("IO보드 IP"), ReadOnly(false), DisplayName("IOBoardIP")]
        public string IOBoardIP { get; set; }
        //IO보드 TCP PORT
        [CategoryAttribute("IO차단기제어"), DescriptionAttribute("IO보드 port"), ReadOnly(false), DisplayName("IOBoardPORT")]
        public string IOBoardPORT { get; set; }
        //IO보드 채널번호
        [CategoryAttribute("IO차단기제어"), DescriptionAttribute("채널"), ReadOnly(false), DisplayName("IOChannel")]
        public IOChannel IOChannelNo
        {
            get { return mIOChannelNo; }
            set { mIOChannelNo = value; }
        }
        //호출시간
        [CategoryAttribute("IO차단기제어"), DescriptionAttribute("호출시간 예)1일경우 1초"), ReadOnly(false), DisplayName("IOPollTime")]
        public string IOPollTime { get; set; }
        //차단기열기 릴레이번호
        [CategoryAttribute("IO차단기제어"), DescriptionAttribute("차단기열기 릴레이번호"), ReadOnly(false), DisplayName("IOBarOpenRelay")]
        public IORelayNo IOBarOpenRelay
        {
            get { return mOpenRelayNo; }
            set { mOpenRelayNo = value; }
        }
        //차단기닫기 릴레이번호
        [CategoryAttribute("IO차단기제어"), DescriptionAttribute("차단기닫기 릴레이번호"), ReadOnly(false), DisplayName("IOBarCloseRelay")]
        public IORelayNo IOBarCloseRelay
        {
            get { return mCloseRelayNo; }
            set { mCloseRelayNo = value; }
        }

        //NEXPA SMS전원제어
        [CategoryAttribute("SMS전원제어"), DescriptionAttribute("SMS전원제어 사용유무"), ReadOnly(false), DisplayName("UseSMSPowerControl")]
        public bool UseSMSPowerControl { get; set; }
        //NEXPA SMS전원제어 적용
    }

}
