using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Common
{
    public static class NPDefine
    {
        public static NPDB oDB = new NPDB();

        public enum ActionMode
        {
            NONE = 0,
            APPEND = 1,
            UPDATE = 2,
            DELETE = 3,
            PRINT = 4,
            EXCEL = 5,
            DATEUPDATE = 6
        }


        public const String MANUAL_PROCESS = "<수동요금계산>";

        public const int REG_TYPE_NOR = 0;
        public const int REG_TYPE_WHITE = 1;
        public const int REG_TYPE_BLACK = 2;
        public const int REG_TYPE_TIME = 3;
        public const int REG_TYPE_DAY = 4;

        public const int ONE_DAY = 1440;
        public const int ONE_HOUR = 60;

        public const String FireShareEvent = "FireShareEvent";
        public const String SHARE_ACTION_CLOSE = "Close";
        public const String SHARE_ACTION_DISABLE = "Disable";
        public const String SHARE_ACTION_ENABLE = "Enable";


        public const int NO_ROWS = 0;

        public const String USE_NO = "사용안함";
        public const String USE_YES = "사용";
        public const String NO_SELECT = "<선택안함>";
        public const String ALL_SELECT = "전체선택";

        public const String TPLIST_FORMAT = "{0} 건";

        public const int SUCCESS = 0;
        public const int FAIL = -1;

        public const int HAS_NO_CHILD = 0;
        public const int HAS_CHILD = 1;

        public const String DEF_NoError = "No Error";
        public const String DEF_Error = "Error";
        public const String DEF_Result = "DML Result";
        public const String DEF_NoEffect = "No Effect";

        public const String DEF_APPEND = "Append";
        public const String DEF_UPDATE = "Update";

        public const String NP_IN = "IN";
        public const String NP_IN2 = "IN2"; // 2013.06.05 최희주 추가
        public const String NP_OUT = "OUT";
        public const String DISPLAY_IN = "DisplayIN";
        public const String DISPLAY_OUT = "DisplayOUT";
        public const String NP_AP = "AP";
        public const String NP_AP2 = "AP2";


        /// <summary>
        /// NPServer 관련
        /// </summary>
        public const String NP_STX = "CH";
        public const String NP_ETX = ".JPG";
        public const String NP_OK = "OK";
        public const String NP_DIE = "LPR_N";
        public const String NP_ALIVE = "LPR_R";

        public const String NP_NORMAL = "^^";
        public const String NP_BACK = "NW";
        public const String NP_MISMATCH = "UP";
        public const String NP_INCORRECT = "NP";

        public const char NP1400Seps = '#';
        public const int NPSendType1Count = 3;
        public const int NPSendType2Count = 4;
        public const int mMainParkNo = 1;
        /// <summary>
        /// 프로그램 관련
        /// </summary>
        public const String DBCONNECT_FAIL_MESSAGE = "Database에 접속할 수 없습니다.";
        public const String NO_PROGRAM_MESSAGE = "프로그램이 존재하지 않습니다.";
        public const String NODATA_MMESSAGE = "자료가 없습니다.";

        public const String MSG_NODATA_FOR_DELETE = "삭제할 자료가 없습니다.";
        public const String MSG_DELETE_YESNO = "자료를 삭제 하시겠습니까?";
        public const String MSG_SELECT_DATA_FOR_DELETE = "삭제할 자료를 선택하여주십시오.";

        public const String MSG_APPEND_OK = "자료를 추가 하였습니다.";
        public const String MSG_APPEND_FAIL = "자료를 추가할 수 없습니다.";

        public const String MSG_UPDATE_OK = "자료를 변경 하였습니다.";
        public const String MSG_UPDATE_FAIL = "자료를 변경할 수 없습니다.";
        public const String MSG_DELETE_OK = "자료를 삭제 하였습니다.";
        public const String MSG_DELETE_FAIL = "자료를 삭제 할 수 없습니다.";

        public const String MSG_TOBE_DATA = "자료가 이미 존재합니다.";
        public const String CAPTION_DELETE = "삭제선택";
        public const String CAPTION_CONFIRM = "확인";
        public const String CAPTION_ERROR = "에러";

        public const String INVALID_DATA = "잘못된 자료 입니다.";

        public const String ALLOW_APPEND = "A";         // 추가
        public const String ALLOW_UPDATE = "U";         // 수정
        public const String ALLOW_DELETE = "D";         // 삭제
        public const String ALLOW_QUERY = "Q";          // 검색
        public const String ALLOW_PRINT = "P";          // 출력
        public const String ALLOW_EXCEL = "E";          // Excel
        public const String ALLOW_SETUP = "S";          // 설정    
        public const String ALLOW_GRANT = "G";          // 허가

        public const Char SEP_CHAR = ',';
        public const Char PASSWORD_CHAR = '*';

        public const String REG_NEW = "신규";
        public const String REG_EXTEND = "연장";
        public const String REG_STOP = "정지";


        #region // 장비관련
        /// <summary>
        /// 입차LPR
        /// </summary>
        public const string Device_INLPR = "8";
        /// <summary>
        /// 출차LPR
        /// </summary>
        public const string Device_OUTLPR = "9";
        /// <summary>
        /// 출구무인
        /// </summary>
        public const string Device_AP = "13";
        /// <summary>
        /// 사전무인
        /// </summary>
        public const string Device_PAP = "12";
        public const string Device_Booth = "3";
        #endregion
        /// <summary>
        /// 무인정산기 관련
        /// </summary>
        /// 

        public const String AP_CMD_STATE = "STATE";             // 알람정보 //최희주 추가
        public const String AP_CMD_RESET = "RESET";             // 리셋 //최희주 추가
        public const String AP_CMD_INCOME = "INCOME";           // 수입금 //최희주 추가
        public const String AP_CMD_OUTCOME = "OUTCOME";         // 방출금 //최희주 추가
        public const String AP_CMD_REVERSE = "REVERSE";         // 정보 //최희주 추가
        public const string AP_CMD_START = "START";             // 무인정산기장비 가동 //최희주 추가
        public const string AP_CMD_END = "END";                 // 무인정산기장비 비가동 //최희주 추가
        public const String AP_REJ = "REJ";             // 동전,지폐 방출

        public const String AP_BRE = "BRE";             // 지폐 리더기
        public const String AP_CRE = "CRE";             // 동전 리더기
        public const String AP_TRE = "TRE";             // 교통카드 리더기
        public const String AP_BCH = "BCH";             // 지폐방출기
        public const String AP_CC1 = "CC1";             // 
        public const String AP_CC2 = "CC2";             // 동전 방출기 1:50원, 2:100원, 3:500원
        public const String AP_CC3 = "CC3";             //
        public const String AP_CA1 = "CA1";             // 카드리더기 (왼쪽)
        public const String AP_CA2 = "CA2";             // 카드리더기 (오른쪽)
        public const String AP_REP = "REP";             // 영수증 프린터
        public const String AP_DID = "DID";             // 도어신호
        public const String AP_DIS = "DIS";             // 전광판 //최희주 추가
        public const String AP_000 = "000";             // 사용자 해제


        public const string AP_STATUS_OK = "OK";
        public const string AP_STATUS_ERR = "ERR";


        // 이전 버젼 
        public const String AP_ALARM = "A";             // 알람정보
        public const String AP_CONTROL = "C";           // 컨트롤
        public const String AP_INFO = "I";              // 정보

        public const String AP_STA = "STA";
        public const String AP_END = "END";

        public const String AP_CAR = "CAR";

        public const String AP_PAS = "PAS";

        public const String AP_REA = "REA";             // 실시간 수입 정보 확인

        public const String AP_OUT = "OUT";
        public const String AP_INC = "INC";
        public const String AP_REV = "REV";

        public const char STX = (char)0x2;
        public const char ETX = (char)0x3;
        public const char FS = (char)0x1C;
        public const char GS = (char)0x1D;
        public const char RS = (char)0x1E;

        public const String CCx_01 = "모터이상";
        public const String CCx_02 = "동전 부족";
        public const String CCx_03 = "이상한 동전";
        public const String CCx_04 = "동전걸림";
        public const String CCx_05 = "쉬프트센서 에러";
        public const String CCx_06 = "10원부족";
        public const String CCx_07 = "50원부족";
        public const String CCx_08 = "100원부족";
        public const String CCx_09 = "500원부족";
        public const String CCx_10 = "10원없음";
        public const String CCx_11 = "50원없음";
        public const String CCx_12 = "100원없음";
        public const String CCx_13 = "500원없음";
        public const String CCx_88 = "정상작동";
        public const String CCx_97 = "동전불출안됨";
        public const String CCx_98 = "알수없는 에러";
        public const String CCx_99 = "타임오버";

        public const String BCH_01 = "지폐 방출중 걸림";
        public const String BCH_02 = "과다방출";
        public const String BCH_03 = "1000원 부족";
        public const String BCH_04 = "5000원 부족";
        public const String BCH_05 = "10000원 부족";
        public const String BCH_06 = "50000원 부족";
        public const String BCH_07 = "1000원 없음";
        public const String BCH_08 = "5000원 없음";
        public const String BCH_09 = "10000원 없음";
        public const String BCH_10 = "50000원 없음";
        public const String BCH_88 = "정상작동";
        public const String BCH_97 = "지폐불출안됨";
        public const String BCH_98 = "알수없는에러";
        public const String BCH_99 = "타임오버";

        public const String CAx_01 = "연결실패";
        public const String CAx_02 = "카드 걸림";
        public const String CAx_03 = "카드방출에러";
        public const String CAx_88 = "정상작동";
        public const String CAx_98 = "알수없는 에러";
        public const String CAx_99 = "타임오버";

        public const String BRE_01 = "모터이상";
        public const String BRE_02 = "체크섬에러";
        public const String BRE_03 = "지폐걸림에러";
        public const String BRE_04 = "지폐제거에러";
        public const String BRE_05 = "리더기 투껑열림";
        public const String BRE_06 = "리더기 센서오류";
        public const String BRE_07 = "지폐방출에러";
        public const String BRE_08 = "스택에러";
        public const String BRE_09 = "잘못된명령어";
        public const String BRE_88 = "정상작동";
        public const String BRE_98 = "알수없는 에러";
        public const String BRE_99 = "타임오버";

        public const String CRE_01 = "장비오류";
        public const String CRE_88 = "정상작동";

        public const String REP_01 = "용지부족";
        public const String REP_02 = "용지없음";
        public const String REP_88 = "정상작동";

        public const String DID_01 = "문 열림";
        public const String DID_02 = "호퍼 열림";
        public const String DID_03 = "지폐방출기 열림";
        public const String DID_04 = "폐권통 열림";
        public const String DID_05 = "동전통 열림";
        public const String DID_88 = "정상작동";
        public static String ParkCode { get; set; }
        public static String UserID { get; set; }
        public static String UserName { get; set; }
        public static String DevCode { get; set; }
        public static String DevName { get; set; }


        public static String OUTDefault_IN_Image { get; set; }
        public static String OUTDefault_OUT_Image { get; set; }

    }

    public class sound
    {
        [DllImport("WinMM.dll")]
        public static extern long PlaySound(String lpszName, int hModule, int dwFlags);

        public static string CurrentDirctory = "";
        public static void buttonSoundDingDong()
        {
            try
            {
                string soundfile = System.Windows.Forms.Application.StartupPath + @"\alarm.wav";
                PlaySound(soundfile, 0, 1);
            }
            catch (Exception ex)
            {

            }
        }
    }

    public static class ObjectExtensions
    {
        public static float StringToFloatWithNull(this object obj)
        {
            return String.IsNullOrEmpty((String)obj) ? (float)0.0 : float.Parse(obj.ToString());
        }

        public static float StringToIntWithNull(this object obj)
        {
            return String.IsNullOrEmpty((String)obj) ? (int)0.0 : int.Parse(obj.ToString());
        }

        public static String AddQuoteEmptyNULL(this String parmStr)
        {
            return String.IsNullOrEmpty(parmStr) || parmStr.Trim().Length < 1 ? "null" : String.Format("'{0}'", parmStr.Trim());
        }



        public static String EmptyToZero(this String parmStr)
        {
            return String.IsNullOrEmpty(parmStr) || parmStr.Trim().Length < 1 ? "0" : parmStr.Trim();
        }

        public static String EmptyToNULL(this String parmStr)
        {
            return String.IsNullOrEmpty(parmStr) || parmStr.Trim().Length < 1 ? "null" : parmStr.Trim();
        }

        public static String EmptyToValue(this String parmStr, String sValue)
        {
            return String.IsNullOrEmpty(parmStr) || parmStr.Trim().Length < 1 ? sValue : parmStr.Trim();
        }

        public static bool isEmpy(this string parmStr)
        {
            return string.IsNullOrEmpty(parmStr);
        }

        public static String MidH(this String parmStr, int nStart, int nLen)
        {
            byte[] bStr = Encoding.GetEncoding("korean").GetBytes(parmStr);

            if (bStr.Length < nLen)
                nLen = bStr.Length;

            return System.Text.Encoding.GetEncoding("korean").GetString(bStr, nStart, nLen == 0 ? bStr.Length - nStart : nLen);
        }

        public static int LenH(this String parmStr)
        {
            return Encoding.GetEncoding("korean").GetBytes(parmStr).Length;
        }

        public static String RightStr(this String parmStr, int nSize)
        {
            int nLen = parmStr.Length;
            if (nLen < 1) return String.Empty;

            return parmStr.Substring(nLen - 1, nLen < nSize ? nLen : nSize);
        }

        public static String StrReverse(this String parmStr)
        {
            Array arr = parmStr.ToCharArray();
            Array.Reverse(arr);
            Char[] c = (Char[])arr;

            return (new String(c));
        }
    }
}
