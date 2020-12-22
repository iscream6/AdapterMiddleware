using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace Common
{
    public class DiscountCodeInfo
    {
        public static List<DiscountCodeInfo> gLIST_DiscountCodeInfo = null;
        /// <summary>
        /// stringempty
        /// </summary>
        /// <param name="p_message"></param>
        /// <returns></returns>
        public static int GetStringEmptyOrNullConvertZero(string p_message)
        {

            return (string.IsNullOrEmpty(p_message.Trim()) ? 0 : Convert.ToInt32(p_message.Trim()));
        }
        /// <summary>
        /// 할인관련 모든값을 DB에서 가져온다.
        /// </summary>
        /// <returns></returns>
        public static bool GetDIscountInfo(ParkInfo pParkInfo, int pParkNo)
        {
            try
            {
                DataTable dt_DiscountInfo = DBActionDML.GetDiscountITable(pParkInfo, pParkNo);
                gLIST_DiscountCodeInfo = new List<DiscountCodeInfo>();
                foreach (DataRow item in dt_DiscountInfo.Rows)
                {

                    DiscountCodeInfo l_DiscountCode = new DiscountCodeInfo();
                    l_DiscountCode.DiscountCode = item["DiscountCode"].ToString();
                    l_DiscountCode.DiscountName = item["DiscountName"].ToString();
                    l_DiscountCode.DiscountType = GetStringEmptyOrNullConvertZero(item["DiscountType"].ToString());
                    l_DiscountCode.DiscountValue = GetStringEmptyOrNullConvertZero(item["DiscountValue"].ToString());
                    l_DiscountCode.Reserve1 = item["Reserve1"].ToString();
                    l_DiscountCode.Reserve2 = item["Reserve2"].ToString();
                    l_DiscountCode.Reserve3 = item["Reserve3"].ToString();
                    l_DiscountCode.Reserve4 = item["Reserve4"].ToString();
                    l_DiscountCode.Reserve5 = item["Reserve5"].ToString();
                    l_DiscountCode.CamUse = (item["CamUse"].ToString() == "1" ? true : false);
                    // 무인정산기 3.1.47 이후버젼만처리
                    l_DiscountCode.RemarkUse = (item["RemarkUse"].ToString() == "1" ? true : false);
                    // 무인정산기 3.1.47 이후버젼만처리완료
                    gLIST_DiscountCodeInfo.Add(l_DiscountCode);


                }
                if (gLIST_DiscountCodeInfo.Count > 0)
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
                return false;
            }
        }

        private string mDiscountCode = "";
        private string mDiscountName = "";
        private int mDiscountType = 0;
        private int mDiscountValue = 0;
        private string mReserve1 = string.Empty;
        private string mReserve2 = string.Empty;
        private string mReserve3 = string.Empty;
        private string mReserve4 = string.Empty;
        private string mReserve5 = string.Empty;
        private bool mCamUse = false;
        // 무인정산기 3.1.47 이후버젼만처리
        private bool mRemarkUse = false;
        // 무인정산기 3.1.47 이후버젼만처리완료

        /// <summary>
        /// 할인코드
        /// </summary>
        public string DiscountCode { set { mDiscountCode = value; } get { return mDiscountCode; } }
        /// <summary>
        /// 할인명
        /// </summary>
        public string DiscountName { set { mDiscountName = value; } get { return mDiscountName; } }
        /// <summary>
        /// 할인 형태. 0:시간할인, 1:비율할인, 2:금액할인, 3:고정금액(주차요금무시하고 할인값으로 처리), 4:완전무료, 5:24시간무료, 6:당일무료
        /// </summary>
        public int DiscountType { set { mDiscountType = value; } get { return mDiscountType; } }
        /// <summary>
        /// 할인값. DiscountType이 9:고정요금 프리패스(당일), 10:고정요금 프리패스(24시간) 인 경우 요금.
        /// </summary>
        public int DiscountValue { set { mDiscountValue = value; } get { return mDiscountValue; } }
        public string Reserve1 { set { mReserve1 = value; } get { return mReserve1; } }
        public string Reserve2 { set { mReserve2 = value; } get { return mReserve2; } }
        public string Reserve3 { set { mReserve3 = value; } get { return mReserve3; } }
        public string Reserve4 { set { mReserve4 = value; } get { return mReserve4; } }
        public string Reserve5 { set { mReserve5 = value; } get { return mReserve5; } }
        public bool CamUse { set { mCamUse = value; } get { return mCamUse; } }
        // 무인정산기 3.1.47 이후버젼만처리
        public bool RemarkUse { set { mRemarkUse = value; } get { return mRemarkUse; } }
        // 무인정산기 3.1.47 이후버젼만처리완료




    }
    /// <summary>
    /// 할인처리
    /// </summary>
    public class DIscountTicketOcs
    {
        public enum DiscountType
        {
            MoneyDiscount = 0,
            TimeDiscount = 1,
            PercentDiscount = 2,
            /// <summary>
            /// 1일별 고정금액 입차24시간기준 (첫날 요금이 고정금액보다 크면 첫날만 고정금액만큼만 주차요금받는다
            /// </summary>
            AllDayFIxDiscount = 3,
            /// <summary>
            /// 입차3시간까지만 고정요금받음 예를들어 3시간 주차시 1000원 주차요금 부과
            /// </summary>
            InHourDiscount = 4

        }
        /// <summary>
        /// 예외할인권 처리일때 시간이 들어오는 부분저장
        /// </summary>
        private int mWorkDayInsertCount = 0;
        public int WorkDayInsertCount
        {
            set { mWorkDayInsertCount = value; }
        }
        private int mFreeDayInsertCount = 0;
        public int FreeDayInsertCount
        {
            set { mFreeDayInsertCount = value; }
        }

        public enum TIcketReadingResult
        {
            /// <summary>
            /// 성공
            /// </summary>
            Success,
            /// <summary>
            /// 유효하지 않은 할인권
            /// </summary>
            NotTicket,
            /// <summary>
            /// 동일한 할인권을 사용하지 못할때
            /// </summary>
            DuplicatTIcket,
            /// <summary>
            /// 할인권 제한수량에 걸림
            /// </summary>
            NoAddTicket,

        }
        public enum DIscountTicketType
        {
            Ocs,
            Ticket,
            Web,
            Barcode,
            ButtonType
        }

        public TIcketReadingResult DiscountTIcket(DIscountTicketType pDIscountTicketType, string ticketInfo, NormalCarInfo _NormalCarInfo, Label pErrorTextbox)
        {

            try
            {

                string logDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                string ticketData = ticketInfo.Trim().Replace(" ", "");
                if (pDIscountTicketType == DIscountTicketType.Ticket)
                {
                    if (ticketData.Length < 20)
                    {
                        return TIcketReadingResult.NotTicket;
                    }
                }
                string TicketerrorMessage = string.Empty;

                string YYYYMMDD = "";
                int SaleGubun = 999;
                string FeeCode = "";
                string ParkZone = "";
                string gubun = "";
                string ApplovalTime = "";
                string DeviceNumber = "";
                string PrintSequnce = "";

                if (pDIscountTicketType == DIscountTicketType.Web)
                {
                    FeeCode = Convert.ToInt32(ticketInfo).ToString();   // 할인코드
                }

                else if (pDIscountTicketType == DIscountTicketType.ButtonType)
                {
                    FeeCode = Convert.ToInt32(ticketInfo).ToString();   // 할인코드
                }

                bool isTIcket = false;
                string discountName = "";
                string l_currentRealDIscountMoney = string.Empty; // 실제할인금액
                string l_currentDIscountMoney = string.Empty; // 액면할인금액


                foreach (DiscountCodeInfo _DiscountTableRow in DiscountCodeInfo.gLIST_DiscountCodeInfo)
                {
                    if (FeeCode == _DiscountTableRow.DiscountCode)
                    {
                        SaleGubun = _DiscountTableRow.DiscountType;
                    }
                    if (_DiscountTableRow.DiscountType == SaleGubun && SaleGubun == (int)DiscountType.TimeDiscount) // 시간 할인이면
                    {
                        if (_DiscountTableRow.DiscountCode == FeeCode)
                        {

                            DiscountFromTime(_NormalCarInfo, logDate, FeeCode, _DiscountTableRow.DiscountName, ref discountName, ref l_currentRealDIscountMoney, ref l_currentDIscountMoney, _DiscountTableRow, pDIscountTicketType);
                            isTIcket = true;

                        }
                    }
                    else if (_DiscountTableRow.DiscountType == SaleGubun && SaleGubun == (int)DiscountType.MoneyDiscount)  // 금액할인이면
                    {
                        if (_DiscountTableRow.DiscountCode == FeeCode)
                        {

                            DiscountFromMoney(_NormalCarInfo, logDate, FeeCode, _DiscountTableRow.DiscountName, ref discountName, ref l_currentRealDIscountMoney, ref l_currentDIscountMoney, _DiscountTableRow, pDIscountTicketType);
                            isTIcket = true;
                        }

                    }
                    else if (_DiscountTableRow.DiscountType == SaleGubun && SaleGubun == (int)DiscountType.PercentDiscount) // 비율활인이면
                    {

                        if (_DiscountTableRow.DiscountCode == FeeCode)
                        {


                            DiscountFromPercent(_NormalCarInfo, logDate, FeeCode, _DiscountTableRow.DiscountName, ref discountName, ref l_currentRealDIscountMoney, ref l_currentDIscountMoney, _DiscountTableRow, pDIscountTicketType);
                            isTIcket = true;
                        }

                    }


                    else if (_DiscountTableRow.DiscountType == SaleGubun && SaleGubun == (int)DiscountType.InHourDiscount) // 첫쨰날만 고정금액
                    {

                        if (_DiscountTableRow.DiscountCode == FeeCode)
                        {


                            DiscountInHour(_NormalCarInfo, logDate, FeeCode, _DiscountTableRow.DiscountName, ref discountName, ref l_currentRealDIscountMoney, ref l_currentDIscountMoney, _DiscountTableRow, pDIscountTicketType);
                            isTIcket = true;
                        }

                    }


                    if (discountName.Trim() != "")
                    {
                        DcDetail dcDetail = new DcDetail();
                        dcDetail.DcNo = _DiscountTableRow.DiscountCode;
                        dcDetail.DCType = _DiscountTableRow.DiscountType.ToString();
                        dcDetail.DcTkNO = ticketInfo;
                        dcDetail.DcAmt = Convert.ToInt32(l_currentDIscountMoney);
                        dcDetail.RealDcAmt = Convert.ToInt32(l_currentRealDIscountMoney);
                        dcDetail.DCTKIssueDate = string.Empty;
                        dcDetail.DCTKIssueTime = string.Empty;
                        dcDetail.Reserve4 = pDIscountTicketType.ToString();
                        dcDetail.Reserve5 = _DiscountTableRow.DiscountName;
                        dcDetail.ChkClosing = 0;
                        _NormalCarInfo.ListDcDetail.Add(dcDetail);
                        discountName = string.Empty;

                    }

                }


                if (!isTIcket)
                {
                    if (pErrorTextbox != null)
                    {
                        pErrorTextbox.Text = "비정상적인 할인티켓입니다.";
                    }
                    return TIcketReadingResult.NotTicket;
                }
                return TIcketReadingResult.Success;
            }
            catch (Exception ex)
            {
                if (pErrorTextbox != null)
                {
                    pErrorTextbox.Text = "DIscountTicketOcs :" + ex.ToString();
                }
                return TIcketReadingResult.NotTicket;
            }

        }


        /// <summary>
        /// 시간 할인
        /// </summary>
        /// <param name="logDate">현재날짜</param>
        /// <param name="FeeCode">할인코드</param>
        /// <param name="discountName">리턴받을 할인코드(현재 할인됬는지 체크하기 위해 존재) </param>
        /// <param name="p_currentRealDIscountMoney">지금 할인된 금액(기존 할인금액 제외)</param>
        /// <param name="_DiscountTableRow">할인코드리스트</param>
        /// <param name="discountType">할인종류(티켓,OCS)</param>/// 
        private void DiscountFromTime(NormalCarInfo _NormalCarInfo, string logDate, string FeeCode, string p_realDIscountName, ref string discountName, ref string p_currentRealDIscountMoney, ref string p_currentDIscountMoney, DiscountCodeInfo _DiscountTableRow, DIscountTicketType discountType)
        {
            // long currentDiscountTIme = sosujumDelete(_DiscountTableRow.DiscountValue.ToString());
            // TextCore.INFO(TextCore.INFOS.DISCOUNTTICEKT, "DIscountTicketOcs | DiscountFromTime", "시간할인권들어옴 [기존 남은주차시간]:" + (_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime).ToString() + " [기존 남은주차금액]:" + _NormalCarInfo.PaymentMoney.ToString() + " [할인권코드]" + FeeCode + " [할인권값]" + _DiscountTableRow.DiscountValue.ToString());
            // _NormalCarInfo.TotalDiscountTimeCalcureate(currentDiscountTIme); // 시간을 기존 할인시간에서 현재할인시간을 더한다
            // string indate = NPSYS.ConvetYears_Dash(_NormalCarInfo.InYMD) + " " + NPSYS.ConvetDay_Dash(_NormalCarInfo.InHMS);
            // string outDiscountdate = DateTime.ParseExact((_NormalCarInfo.OutYmd + _NormalCarInfo.OutHms).Substring(0, 12), "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture).AddMinutes(-_NormalCarInfo.TotalDiscountTime).ToString("yyyyMMddHHmmss");
            // int CurrenteRealDiscountMoney = 0; // 현재할인금액
            // CurrenteRealDiscountMoney = _NormalCarInfo.PaymentMoney - FeeAction.FeeCalcMoney(Convert.ToInt32(NPSYS.ParkCode), Convert.ToInt32(ParkInfoData.FeeInfo.CurrentFeeNo), _NormalCarInfo.InYMD, _NormalCarInfo.InHMS, outDiscountdate.Substring(0, 8), outDiscountdate.Substring(8));
            // if (_NormalCarInfo.PaymentMoney >= CurrenteRealDiscountMoney) // if (남은결제요금 >= 현재할인금액)
            // {
            //     p_currentRealDIscountMoney = CurrenteRealDiscountMoney.ToString();
            //     p_currentDIscountMoney = CurrenteRealDiscountMoney.ToString();

            // }
            // else
            // {
            //     p_currentDIscountMoney = CurrenteRealDiscountMoney.ToString();
            //     p_currentRealDIscountMoney = _NormalCarInfo.PaymentMoney.ToString();
            // }
            // if (_NormalCarInfo.ParkTime <= _NormalCarInfo.TotalDiscountTime)
            // {
            //     p_currentDIscountMoney = _NormalCarInfo.PaymentMoney.ToString();
            //     p_currentRealDIscountMoney = _NormalCarInfo.PaymentMoney.ToString();
            // }
            // _NormalCarInfo.TotalDiscountMoneyCalcureate(Convert.ToInt32(p_currentRealDIscountMoney.ToString()));


            // discountName = sosujumDelete(CurrenteRealDiscountMoney.ToString()).ToString();
            // TextCore.INFO(TextCore.INFOS.PAYINFO, "DIscountTicketOcs | DiscountFromTime", "[현재할인금액]:" + sosujumDelete(CurrenteRealDiscountMoney.ToString()).ToString() + " 할인권명:" + p_realDIscountName + " [현재받을금액]:" + _NormalCarInfo.PaymentMoney.ToString() + " [투입금액]:" + _NormalCarInfo.CurrentMoney.ToString() + " [총 주차시간]:" + _NormalCarInfo.ParkTime.ToString() + " [남은 결제 주차시간]:" + _NormalCarInfo.GetRemainderParktime().ToString());
            //// LPRDbSelect.DiscountINsert(logDate, _DiscountTableRow.DiscountName, _DiscountTableRow.DiscountName, FeeCode, Convert.ToInt32(p_currentRealDIscountMoney), Convert.ToInt32(0), _NormalCarInfo, discountType);

        }

        /// <summary>
        /// 퍼센트 할인
        /// </summary>
        /// <param name="logDate">현재날짜</param>
        /// <param name="FeeCode">할인코드</param>
        /// <param name="discountName">리턴받을 할인코드(현재 할인됬는지 체크하기 위해 존재) </param>
        /// <param name="l_currentDIscountMoney">지금 할인된 금액(기존 할인금액 제외)</param>
        /// <param name="_DiscountTableRow">할인코드리스트</param>
        /// <param name="discountType">할인종류(티켓,OCS)</param>/// 
        private void DiscountFromPercent(NormalCarInfo _NormalCarInfo, string logDate, string FeeCode, string p_realDIscountName, ref string discountName, ref string p_currentDIscountMoney, ref string p_currentRealDIscountMoney, DiscountCodeInfo _DiscountTableRow, DIscountTicketType discountType)
        {

            int currentDiscountMoney = (((_NormalCarInfo.PaymentMoney * _DiscountTableRow.DiscountValue) / 100) / 100) * 100;
            int currentRealDiscountMoney = 0;
            if (_NormalCarInfo.PaymentMoney >= currentDiscountMoney)
            {
                currentRealDiscountMoney = currentDiscountMoney;
                p_currentDIscountMoney = currentDiscountMoney.ToString();
                p_currentRealDIscountMoney = currentDiscountMoney.ToString();
            }
            else
            {
                currentRealDiscountMoney = _NormalCarInfo.PaymentMoney;
                p_currentDIscountMoney = currentDiscountMoney.ToString();
                p_currentRealDIscountMoney = currentRealDiscountMoney.ToString();

            }
            if (_DiscountTableRow.DiscountValue == 100)
            {
                currentRealDiscountMoney = _NormalCarInfo.PaymentMoney;
                p_currentDIscountMoney = currentDiscountMoney.ToString();
                p_currentRealDIscountMoney = currentRealDiscountMoney.ToString();

            }
            discountName = currentDiscountMoney.ToString();
            _NormalCarInfo.TotalDiscountMoneyCalcureate(currentRealDiscountMoney);
            // LPRDbSelect.DiscountINsert(logDate, _DiscountTableRow.DiscountName, _DiscountTableRow.DiscountName, FeeCode, Convert.ToInt32(p_currentRealDIscountMoney), Convert.ToInt32(0), _NormalCarInfo, discountType);


        }

        /// <summary>
        /// 금액 할인
        /// </summary>
        /// <param name="logDate">현재날짜</param>
        /// <param name="FeeCode">할인코드</param>
        /// <param name="discountName">리턴받을 할인코드(현재 할인됬는지 체크하기 위해 존재) </param>
        /// <param name="l_currentDIscountMoney">지금 할인된 금액(기존 할인금액 제외)</param>
        /// <param name="_DiscountTableRow">할인코드리스트</param>
        /// <param name="discountType">할인종류(티켓,OCS)</param>/// 
        private void DiscountFromMoney(NormalCarInfo _NormalCarInfo, string logDate, string FeeCode, string p_realDIscountName, ref string discountName, ref string p_currentDIscountMoney, ref string p_currentRealDIscountMoney, DiscountCodeInfo _DiscountTableRow, DIscountTicketType discountType)
        {
            int CurrenteDiscountMoney = _DiscountTableRow.DiscountValue; // 액면할인금액
            int CurrentRealDiscountMoney = 0; // 실제할인금액
            if (_NormalCarInfo.PaymentMoney >= CurrenteDiscountMoney) // 남은요금  > 할인금액
            {
                CurrentRealDiscountMoney = CurrenteDiscountMoney;
                p_currentDIscountMoney = CurrenteDiscountMoney.ToString();
                p_currentRealDIscountMoney = CurrenteDiscountMoney.ToString();
            }
            else
            {
                p_currentDIscountMoney = CurrenteDiscountMoney.ToString();
                CurrentRealDiscountMoney = _NormalCarInfo.PaymentMoney;
                p_currentRealDIscountMoney = CurrentRealDiscountMoney.ToString();
            }
            discountName = CurrentRealDiscountMoney.ToString();
            _NormalCarInfo.TotalDiscountMoneyCalcureate(CurrentRealDiscountMoney);
            //  LPRDbSelect.DiscountINsert(logDate, _DiscountTableRow.DiscountName, _DiscountTableRow.DiscountName, FeeCode, Convert.ToInt32(p_currentRealDIscountMoney), Convert.ToInt32(0), _NormalCarInfo, discountType);
        }


        private void DiscountInHour(NormalCarInfo _NormalCarInfo, string logDate, string FeeCode, string p_realDIscountName, ref string discountName, ref string p_currentRealDIscountMoney, ref string p_currentDIscountMoney, DiscountCodeInfo _DiscountTableRow, DIscountTicketType discountType)
        {
            //     long currentDiscountFixMoney = sosujumDelete(_DiscountTableRow.DiscountValue.ToString()); // 특정시간만 할인할 고정금액
            //     TextCore.INFO(TextCore.INFOS.DISCOUNTTICEKT, "DIscountTicketOcs | DiscountInHour", "일정시간만 할인처리 [기존 남은주차시간]:" + (_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime).ToString() + " [기존 남은주차금액]:" + _NormalCarInfo.PaymentMoney.ToString() + " [할인권코드]" + FeeCode + " [할인권값]" + _DiscountTableRow.DiscountValue.ToString());
            //     int minute = FeeAction.ElpaseType(FeeAction.ElapsTypes.Minute, _NormalCarInfo.InDate, _NormalCarInfo.OutYmd, _NormalCarInfo.OutHms); // 입차24시간기준 3시간까지 요금을 1000원으로 고정함
            //     int CurrenteRealDiscountMoney = 0; // 현재할인금액
            //     int fixTIme = Convert.ToInt32(_DiscountTableRow.Reserve1); // 고정시간 180이면 180분
            //     if (minute <= fixTIme)
            //     {
            //         if (_NormalCarInfo.ParkMoney < Convert.ToInt32(currentDiscountFixMoney))
            //         {
            //             CurrenteRealDiscountMoney = 0;
            //         }
            //         else
            //         {
            //             CurrenteRealDiscountMoney = _NormalCarInfo.ParkMoney - Convert.ToInt32(currentDiscountFixMoney);
            //         }
            //     }
            //     else
            //     {
            //         string nextDay = DateTime.ParseExact((_NormalCarInfo.InYMD + _NormalCarInfo.InHMS).Substring(0, 14), "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture).AddMinutes(fixTIme).ToString("yyyyMMddHHmmss");
            //         CurrenteRealDiscountMoney = FeeAction.FeeCalcMoney(Convert.ToInt32(NPSYS.ParkCode), Convert.ToInt32(ParkInfoData.FeeInfo.CurrentFeeNo), _NormalCarInfo.InYMD, _NormalCarInfo.InHMS, nextDay.Substring(0, 8), nextDay.Substring(8)) - Convert.ToInt32(currentDiscountFixMoney);


            //     }
            //     if (CurrenteRealDiscountMoney < 0)  // 주차요금이있다면
            //     {
            //         CurrenteRealDiscountMoney = 0;
            //     }
            //     p_currentRealDIscountMoney = CurrenteRealDiscountMoney.ToString();
            //     p_currentDIscountMoney = CurrenteRealDiscountMoney.ToString();
            //     _NormalCarInfo.TotalDiscountMoneyCalcureate(Convert.ToInt32(p_currentRealDIscountMoney.ToString()));
            //     discountName = sosujumDelete(CurrenteRealDiscountMoney.ToString()).ToString();
            //     TextCore.INFO(TextCore.INFOS.PAYINFO, "DIscountTicketOcs | DiscountOneDayFix", "[할인금액] " + sosujumDelete(CurrenteRealDiscountMoney.ToString()).ToString() + " [할인권명]" + p_realDIscountName + " [총 할인시간]" + _NormalCarInfo.TotalDiscountTime.ToString() + " [현재받을금액]" + _NormalCarInfo.PaymentMoney.ToString() + " [투입금액]" + _NormalCarInfo.CurrentMoney.ToString());
            ////     LPRDbSelect.DiscountINsert(logDate, _DiscountTableRow.DiscountName, _DiscountTableRow.DiscountName, FeeCode, Convert.ToInt32(p_currentRealDIscountMoney), Convert.ToInt32(0), _NormalCarInfo, discountType);
        }


        /// <summary>
        /// 당일무료(입차일기준)
        /// </summary>
        /// <param name="logDate">현재날짜</param>
        /// <param name="FeeCode">할인코드</param>
        /// <param name="discountName">리턴받을 할인코드 </param>
        /// <param name="l_currentDIscountMoney">지금 할인된 금액(기존 할인금액 제외)</param>
        /// <param name="_DiscountTableRow">할인코드리스트</param>
        /// <param name="discountType">할인종류(티켓,OCS)</param>/// 
        private void DiscountFromParkedCarFirstDay(NormalCarInfo _NormalCarInfo, string logDate, string FeeCode, string p_realDIscountName, ref string discountName, ref string l_currentDIscountMoney, DiscountCodeInfo _DiscountTableRow, DIscountTicketType discountType)
        {
            //long currentDiscountTIme = 0;
            //LogClass.INFO(LogClass.INFOS.DISCOUNTTICEKT, "PreDiscount|DiscountFromParkedCarFirstDay", "당일무료할인권들어옴 [기존 남은주차시간]:" + (_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime).ToString() + " [기존 남은주차금액]:" + _NormalCarInfo.PaymentMoney.ToString() + " [할인권코드]" + FeeCode+ " [할인권값]");
            //if (_NormalCarInfo.InYmd != _NormalCarInfo.OutYmd) // 입출차날이 다르면
            //{
            //    currentDiscountTIme = NPSYS.ElpaseMinute(_NormalCarInfo.InYmd + _NormalCarInfo.InHms, _NormalCarInfo.InYmd, "235959");
            //}
            //else
            //{
            //    currentDiscountTIme = NPSYS.ElpaseMinute(_NormalCarInfo.InYmd + _NormalCarInfo.InHms, _NormalCarInfo.OutYmd, _NormalCarInfo.OutHms);
            //}


            //_NormalCarInfo.TotalDiscountTimeCalcureate(currentDiscountTIme); // 시간을 기존 할인시간에서 현재할인시간을 더한다
            //long CurrenteDiscountMoney = 0;
            //if (_NormalCarInfo.TotalDiscountTime >= _NormalCarInfo.ParkTime) // 현재 총 할인시간이 주차시간보다 많거나 같으면 _
            //{
            //    CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney;
            //    _NormalCarInfo.DiscountMoney = _NormalCarInfo.ParkMoney;
            //    _NormalCarInfo.TotalDiscountTime = _NormalCarInfo.ParkTime;
            //}
            //else
            //{
            //    if ((_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime) > CarPayCalcurate.mTblFeeTable.BaseTime) // 현재 기본시간보다 남은시간이 크면
            //    {
            //        long mmmm = CarPayCalcurate.Feecode(_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime, false);
            //        CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney - CarPayCalcurate.Feecode(_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime, false);   // 기존에 남은 주차요금에서 현재 할인적용된 주차요금을 뺀 값이 금방 할인된 금액임 

            //    }
            //    else
            //    { // 현재 기본시간보다 남은시간이 적으면
            //        if (CarPayCalcurate.Bypass)
            //        {
            //            CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney - CarPayCalcurate.Feecode(_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime, true);
            //        }
            //        else
            //        {
            //            CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney - CarPayCalcurate.Feecode(_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime, false);
            //        }
            //    }
            //    if (_NormalCarInfo.PaymentMoney <= Convert.ToInt32(CurrenteDiscountMoney))  // 남은주차요금보다 할인금액이 크면 할인금액을 남은 주차요금에 맞춘다.
            //    {
            //        CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney;
            //    }

            //    _NormalCarInfo.TotalDiscountMoneyCalcureate(Convert.ToInt32(CurrenteDiscountMoney));

            //}


            //l_currentDIscountMoney = CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString();

            //discountName = CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString();

            //LogClass.INFO(LogClass.INFOS.PAYINFO, "PreDiscount|DiscountFromParkedCarFirstDay", "[현재할인금액]:" + CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString() + " [남은 주차요금]:" + _NormalCarInfo.PaymentMoney.ToString() + " [투입금액]:" + _NormalCarInfo.CurrentMoney.ToString() + " [총 주차시간]:" + _NormalCarInfo.ParkTime.ToString() + " [남은 결제 주차시간]:" + _NormalCarInfo.GetRemainderParktime().ToString() + " [현재 할인권이 실제 할인한 주차시간]:" + currentDiscountTIme.ToString());

        }
        /// <summary>
        /// 24시간 무료(입차일기준)
        /// </summary>
        /// <param name="logDate">현재날짜</param>
        /// <param name="FeeCode">할인코드</param>
        /// <param name="discountName">리턴받을 할인코드 </param>
        /// <param name="l_currentDIscountMoney">지금 할인된 금액(기존 할인금액 제외)</param>
        /// <param name="_DiscountTableRow">할인코드리스트</param>
        /// <param name="discountType">할인종류(티켓,OCS)</param>/// 
        private void DiscountFrom24Hour(NormalCarInfo _NormalCarInfo, string logDate, string FeeCode, string p_realDIscountName, ref string discountName, ref string l_currentDIscountMoney, DiscountCodeInfo _DiscountTableRow, DIscountTicketType discountType)
        {
            //long currentDiscountTIme = 24 * 60;
            //LogClass.INFO(LogClass.INFOS.DISCOUNTTICEKT, "PreDiscount|DiscountFrom24Hour", "24시간무료할인권들어옴 [기존 남은주차시간]:" + (_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime).ToString() + " [기존 남은주차금액]:" + _NormalCarInfo.PaymentMoney.ToString() + " [할인권코드]" + FeeCode + " [할인권값]");
            //if (_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime < currentDiscountTIme) // 현재 남은주차시간이 현재 할인하려는 시간보다 적으면
            //{
            //    currentDiscountTIme = _NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime;
            //}

            //_NormalCarInfo.TotalDiscountTimeCalcureate(currentDiscountTIme); // 시간을 기존 할인시간에서 현재할인시간을 더한다
            //long CurrenteDiscountMoney = 0;
            //if (_NormalCarInfo.TotalDiscountTime >= _NormalCarInfo.ParkTime) // 현재 총 할인시간이 주차시간보다 많거나 같으면 _
            //{
            //    CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney;
            //    _NormalCarInfo.DiscountMoney = _NormalCarInfo.ParkMoney;
            //    _NormalCarInfo.TotalDiscountTime = _NormalCarInfo.ParkTime;
            //}
            //else
            //{
            //    if ((_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime) > CarPayCalcurate.mTblFeeTable.BaseTime) // 현재 기본시간보다 남은시간이 크면
            //    {
            //        CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney - CarPayCalcurate.Feecode(_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime, false);   // 기존에 남은 주차요금에서 현재 할인적용된 주차요금을 뺀 값이 금방 할인된 금액임 
            //    }
            //    else
            //    { // 현재 기본시간보다 남은시간이 적으면
            //        if (CarPayCalcurate.Bypass)
            //        {
            //            CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney - CarPayCalcurate.Feecode(_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime, true);
            //        }
            //        else
            //        {
            //            CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney - CarPayCalcurate.Feecode(_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime, false);
            //        }
            //    }
            //    if (_NormalCarInfo.PaymentMoney <= Convert.ToInt32(CurrenteDiscountMoney))  // 남은주차요금보다 할인금액이 크면 할인금액을 남은 주차요금에 맞춘다.
            //    {
            //        CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney;
            //    }
            //    _NormalCarInfo.TotalDiscountMoneyCalcureate(Convert.ToInt32(CurrenteDiscountMoney));
            //}


            //l_currentDIscountMoney = CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString();

            //discountName = CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString();

            //TextCore.INFO(TextCore.INFOS.PAYINFO, "PreDiscount|DiscountFrom24Hour", "[현재할인금액]:" + CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString() + " [남은 주차요금]:" + _NormalCarInfo.PaymentMoney.ToString() + " [투입금액]:" + _NormalCarInfo.CurrentMoney.ToString() + " [총 주차시간]:" + _NormalCarInfo.ParkTime.ToString() + " [남은 결제 주차시간]:" + _NormalCarInfo.GetRemainderParktime().ToString() + " [현재 할인권이 실제 할인한 주차시간]:" + currentDiscountTIme.ToString());


        }

        /// <summary>
        /// 완전 무료(입차일기준)
        /// </summary>
        /// <param name="logDate">현재날짜</param>
        /// <param name="FeeCode">할인코드</param>
        /// <param name="discountName">리턴받을 할인코드(현재 할인됬는지 체크하기 위해 존재) </param>
        /// <param name="l_currentDIscountMoney">지금 할인된 금액(기존 할인금액 제외)</param>
        /// <param name="_DiscountTableRow">할인코드리스트</param>
        /// <param name="discountType">할인종류(티켓,OCS)</param>/// 
        private void DiscountFromAllFree(NormalCarInfo _NormalCarInfo, string logDate, string FeeCode, string p_realDIscountName, ref string discountName, ref string l_currentDIscountMoney, DiscountCodeInfo _DiscountTableRow, DIscountTicketType discountType)
        {
            //long currentDiscountTIme = _NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime;
            //TextCore.INFO(TextCore.INFOS.DISCOUNTTICEKT, "PreDiscount|DiscountFromAllFree", "완전무료할인권들어옴 [기존 남은주차시간]:" + (_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime).ToString() + " [기존 남은주차금액]:" + _NormalCarInfo.PaymentMoney.ToString() + " [할인권코드]" + FeeCode + " [할인권값]");
            //_NormalCarInfo.TotalDiscountTimeCalcureate(currentDiscountTIme);
            //long CurrenteDiscountMoney = Convert.ToInt64(_NormalCarInfo.PaymentMoney);
            //l_currentDIscountMoney = CurrenteDiscountMoney.ToString();
            //_NormalCarInfo.TotalDiscountMoneyCalcureate(_NormalCarInfo.PaymentMoney);
            //discountName = CarPayCalcurate.sosujumDelete(_DiscountTableRow._DiscountValue.ToString()).ToString();
            //TextCore.INFO(TextCore.INFOS.PAYINFO, "PreDiscount|DiscountFromAllFree", "[현재할인금액]:" + CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString() + " [남은 주차요금]:" + _NormalCarInfo.PaymentMoney.ToString() + " [투입금액]:" + _NormalCarInfo.CurrentMoney.ToString() + " [총 주차시간]:" + _NormalCarInfo.ParkTime.ToString() + " [남은 결제 주차시간]:" + _NormalCarInfo.GetRemainderParktime().ToString() + " [현재 할인권이 실제 할인한 주차시간]:" + currentDiscountTIme.ToString());
            //if (_NormalCarInfo.DiscountMoney > _NormalCarInfo.ParkMoney)
            //{
            //    _NormalCarInfo.DiscountMoney = _NormalCarInfo.ParkMoney;
            //}

        }




        /// <summary>
        /// 고정금액
        /// </summary>
        /// <param name="logDate">현재날짜</param>
        /// <param name="FeeCode">할인코드</param>
        /// <param name="discountName">리턴받을 할인코드(현재 할인됬는지 체크하기 위해 존재) </param>
        /// <param name="l_currentDIscountMoney">지금 할인된 금액(기존 할인금액 제외)</param>
        /// <param name="_DiscountTableRow">할인코드리스트</param>
        /// <param name="discountType">할인종류(티켓,OCS)</param>/// 
        private void DiscountFixedMoney(NormalCarInfo _NormalCarInfo, string logDate, string FeeCode, string p_realDIscountName, ref string discountName, ref string l_currentDIscountMoney, DiscountCodeInfo _DiscountTableRow, DIscountTicketType discountType)
        {
            //TextCore.INFO(TextCore.INFOS.DISCOUNTTICEKT, "PreDiscount|DiscountFixedMoney", "고정금액할인권들어옴 [기존 남은주차시간]:" + (_NormalCarInfo.ParkTime - _NormalCarInfo.TotalDiscountTime).ToString() + " [기존 남은주차금액]:" + _NormalCarInfo.PaymentMoney.ToString() + " [할인권코드]" + FeeCode + " [할인권값]" + _DiscountTableRow._DiscountValue.ToString());
            //long CurrenteDiscountMoney = CarPayCalcurate.sosujumDelete(_DiscountTableRow._DiscountValue.ToString());
            //if (_NormalCarInfo.PaymentMoney > Convert.ToInt32(CurrenteDiscountMoney)) // 남은주차요금보다 할인금액이 적으면 남은주차요금에서 현재 할인값을 뺀 금액이 실제 할인금액이된다.
            //{
            //    CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney - CurrenteDiscountMoney;
            //}
            //else
            //{
            //    CurrenteDiscountMoney = 0;
            //}
            //if (CurrenteDiscountMoney > 0)
            //{
            //    long CurrentDIscountTime = (Convert.ToInt64(CurrenteDiscountMoney) / CarPayCalcurate._CalcBaseFee._UnitMoney) * CarPayCalcurate._CalcBaseFee._UnitTime;
            //    _NormalCarInfo.TotalDiscountTimeCalcureate(Convert.ToInt64(Convert.ToInt32(CurrentDIscountTime)));
            //    if (_NormalCarInfo.PaymentMoney <= Convert.ToInt32(CurrenteDiscountMoney)) // 남은주차요금보다 할인금액이 크면 할인금액을 남은 주차요금에 맞춘다
            //    {
            //        CurrenteDiscountMoney = _NormalCarInfo.PaymentMoney;
            //    }
            //    _NormalCarInfo.TotalDiscountMoneyCalcureate(Convert.ToInt32(CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString())));
            //    discountName = CarPayCalcurate.sosujumDelete(_DiscountTableRow._DiscountValue.ToString()).ToString();

            //    l_currentDIscountMoney = CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString();
            //    TextCore.INFO(TextCore.INFOS.PAYINFO, "PreDiscount|DiscountFixedMoney", "[현재할인금액]:" + CarPayCalcurate.sosujumDelete(CurrenteDiscountMoney.ToString()).ToString() + " 할인권명:" + p_realDIscountName + " [남은 주차요금]:" + _NormalCarInfo.PaymentMoney.ToString() + " [투입금액]:" + _NormalCarInfo.CurrentMoney.ToString() + " [총 주차시간]:" + _NormalCarInfo.ParkTime.ToString() + " [남은 결제 주차시간]:" + _NormalCarInfo.GetRemainderParktime().ToString() + " [현재 할인권이 실제 할인한 주차시간]:" + CurrentDIscountTime.ToString());
            //    if (_NormalCarInfo.DiscountMoney > _NormalCarInfo.ParkMoney)
            //    {
            //        _NormalCarInfo.DiscountMoney = _NormalCarInfo.ParkMoney;
            //    }
            //}
            //else
            //{
            //    discountName = CarPayCalcurate.sosujumDelete(_DiscountTableRow.DiscountValue.ToString()).ToString();
            //    l_currentDIscountMoney = "0";
            //}

        }



        public static long sosujumDelete(string longDatatype)
        {
            string[] splitData = longDatatype.Split('.');
            if (splitData.Length > 1)
            {
                return Convert.ToInt64(splitData[0].ToString());
            }
            else
            {
                return Convert.ToInt64(splitData[0].ToString());

            }

        }


    }
    public class DcDetail
    {
        private string mDcNo = string.Empty;
        private int mDcAmt = 0;
        private int mRealDcAMt = 0;
        private string mDcTkNO = string.Empty;
        private string mDCTKIssueDate = string.Empty;
        private string mDCTKIssueTime = string.Empty;
        private string mDCType = string.Empty;
        private int mChkClosing = 0;
        private string mReserve1 = string.Empty;
        private string mReserve2 = string.Empty;
        private string mReserve3 = string.Empty;
        private string mReserve4 = string.Empty;
        private string mReserve5 = string.Empty;
        /// <summary>
        /// 할인권코드
        /// </summary>
        public string DcNo
        {
            set { mDcNo = value; }
            get { return mDcNo; }
        }
        /// <summary>
        /// 할인금액
        /// </summary>
        public int DcAmt
        {
            set { mDcAmt = value; }
            get { return mDcAmt; }
        }
        /// <summary>
        /// 실할인금액
        /// </summary>
        public int RealDcAmt
        {
            set { mRealDcAMt = value; }
            get { return mRealDcAMt; }
        }
        /// <summary>
        /// 할인권 전체데이터
        /// </summary>
        public string DcTkNO
        {
            set { mDcTkNO = value; }
            get { return mDcTkNO; }
        }
        /// <summary>
        /// 나도모름
        /// </summary>

        public string DCTKIssueDate
        {
            set { mDCTKIssueDate = value; }
            get { return mDCTKIssueDate; }
        }
        /// <summary>
        /// 나도모름
        /// </summary>
        public string DCTKIssueTime
        {
            set { mDCTKIssueTime = value; }
            get { return mDCTKIssueTime; }
        }
        /// <summary>
        /// 할인권타입 0 금액 1 시간 2 비율
        /// </summary>
        public string DCType
        {
            set { mDCType = value; }
            get { return mDCType; }
        }
        /// <summary>
        /// 마감횟수
        /// </summary>
        public int ChkClosing
        {
            set { mChkClosing = value; }
            get { return mChkClosing; }
        }


        /// <summary>
        /// 할인권명
        /// </summary>
        public string Reserve1
        {
            set { mReserve1 = value; }
            get { return mReserve1; }
        }
        /// <summary>
        /// 할인권명
        /// </summary>
        public string Reserve2
        {
            set { mReserve2 = value; }
            get { return mReserve2; }
        }
        /// <summary>
        /// 할인권명
        /// </summary>
        public string Reserve3
        {
            set { mReserve3 = value; }
            get { return mReserve3; }
        }

        /// <summary>
        /// 할인권명
        /// </summary>
        public string Reserve4
        {
            set { mReserve4 = value; }
            get { return mReserve4; }
        }
        /// <summary>
        /// 할인권명
        /// </summary>
        public string Reserve5
        {
            set { mReserve5 = value; }
            get { return mReserve5; }
        }
    }
}
