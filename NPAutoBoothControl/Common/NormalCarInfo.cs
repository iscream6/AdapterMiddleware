using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using FadeFox.Text;

namespace Common
{
    public class NormalCarInfo
    {
        #region 변수
        private int mParkNo = 0;
        private EnumCarTypes mCarTypes = EnumCarTypes.NOR_IO;
        private int mCarType = 0;
        private PaymentType mPaymentMethod = PaymentType.Cash;
        private ManualType mManualType = ManualType.None;
        public CarFreeType mCarFreeType = CarFreeType.FreeCar;
        private string mStartYMD = string.Empty;
        private string mCarKind = string.Empty;
        private int mPreCalc = 0;
        private string mNumType = string.Empty;
        private string mChannel = string.Empty;
        private string mInYMD = string.Empty;
        private string mInHMS = string.Empty;
        private string mPreProcYmd = string.Empty;
        private string mPreProcHms = string.Empty;
        private string mOutYmd = string.Empty;
        private string mOutHms = string.Empty;
        private long mParkTime = 0;
        private int mParkMoney = 0;
        private int mDiscountMoney = 0;
        private string mInCarNumber = string.Empty;


        private string mInUnitNo = string.Empty; // 입차기기
        private string mTkType = string.Empty;
        private string mTkNO = string.Empty;
        private string mCarLength = string.Empty;
        private int mOutRecog1 = 1;
        private int mOutChk = 0;
        public List<DcDetail> ListDcDetail = new List<DcDetail>();

        private PayType mCurrentPayType = PayType.MoneyOrFree;
        private OutChkType mCurrentOutType = OutChkType.SuccessOut;
        private string mCurrentOutName = string.Empty;
        private string mOutCarNumber = string.Empty;
        private string mOutCarNumber1 = string.Empty;
        private string mLastErrorMessage = string.Empty;

        private string mInCarPath = string.Empty;
        private string mOutCarPath = string.Empty;
        private string mInRCarNumber = string.Empty;
        private string mInRCarPath = string.Empty;
        private string mPreNorkey = string.Empty;
        private string mExpireYmd = string.Empty;
        private int mIntervelTime = 0;

        private long mPreParktime = 0;
        private string mPreOutYmd = "";
        private string mPreOutHms = "";
        private string mPreInYmd = "";
        private string mPreInHms = "";
        private int mGuestDiscountTime = 0;
        private int mNotDisChargeMoney = 0;
        private string mDiscountDetail = string.Empty;
        /// <summary>
        /// 사전 할인
        /// </summary>
        private string mPreDiscountContent = string.Empty;
        private int mTMoneyPay = 0;
        //////////  현금관련  ////////////////////////
        /// <summary>
        /// 현금 승인번호
        /// </summary>
        private string mCashReciptNo = string.Empty;
        /// <summary>
        /// 현금 승인일자
        /// </summary>
        private string mCashReciptAuthDate = string.Empty;
        /// <summary>
        /// 현금 승인응답코드
        /// </summary>
        private string mCashReciptRescode = string.Empty;
        /// <summary>
        /// 현금 응답메시지
        /// </summary>
        private string mCashReciptResMsg = string.Empty;
        private string mCashReciptApproveYmd = string.Empty;
        private string mCashReciptApproveHms = string.Empty;
        private string mCashReciptApprovalYmd = string.Empty;
        private string mCashReciptApprovalHms = string.Empty;
        private int mCashReciptRequestYesNo = 0;
        /////////////////////////////////////////////////////

        /////////////////////////카드관련 //////////////////
        private int mCardPay = 0;
        private string mCardAuthNumber = string.Empty;
        private string mCardAUthDate = string.Empty;
        private string mCardAUthTime = string.Empty;
        private string mCardRescode = string.Empty;
        private string mCardResMsg = string.Empty;
        private string mCardResMsg2 = string.Empty;
        private int mSupplyPay = 0;
        private int mTaxPay = 0;
        private string mTicketNum = string.Empty;
        private string mCardName = string.Empty;
        private string mCardNumber = string.Empty;
        private string mCardApproveYmd = string.Empty;
        private string mCardApproveHms = string.Empty;
        private string mCardApprovalYmd = string.Empty;
        private string mCardApprovalHms = string.Empty;
        private int mCardVanType = 1;
        private string mCardDDDNumber = string.Empty;
        private string mCardBankCode = string.Empty;
        private string mCardMagneMentCode = string.Empty;
        private string mCardMemberCode = string.Empty;
        private string mCardBankName = string.Empty;
        private string mCardApprovalType = string.Empty;
        private int mBeforeCardPay = 0;
        private string mCardAcquirerCode = string.Empty;
        private string mCardAcquirerName = string.Empty;
        ///////////////////////////////////////////////////

        private long mElapsedMinute = 0;
        private long mElapsedDay = 0;
        private int mInCome10Qty = 0;
        private int mInCome50Qty = 0;
        private int mInCome100Qty = 0;
        private int mInCome500Qty = 0;
        private int mInCome1000Qty = 0;
        private int mInCome5000Qty = 0;
        private int mInCome10000Qty = 0;
        private int mInCome50000Qty = 0;
        private int mOutCome10Qty = 0;
        private int mOutCome50Qty = 0;
        private int mOutCome100Qty = 0;
        private int mOutCome500Qty = 0;
        private int mOutCome1000Qty = 0;
        private int mOutCome5000Qty = 0;
        private int mOutCome10000Qty = 0;
        private int mOutCome50000Qty = 0;
        private int mCurrentMoney = 0;
        private int mCurrent5000Qty = 0;
        private int mCurrent1000Qty = 0;
        private int mCurrent500Qty = 0;
        private int mCurrent100Qty = 0;
        private int mCurrent50Qty = 0;

        /// <summary>
        /// 현재 금액 나마저 금액
        /// </summary>
        private int mCurrentEtcMoney = 0;

        private int mCharge5000Qty = 0;
        private int mCharge1000Qty = 0;
        private int mCharge500Qty = 0;
        private int mCharge100Qty = 0;
        private int mCharge50Qty = 0;
        private int mChargeMoney = 0;
        private int mCanclePayMoney = 0;
        private int mChargeEtcMoney = 0;
        private ParkInfo mCurrentParkInfo = null;
        private string mGroupName = "일반차량";
        private bool mIsBarOpen = false;
        private string mCurrentTotalDiscount = string.Empty;
        private string mIo = string.Empty;

        #endregion
        public NormalCarInfo()
        {

        }
        /// <summary>
        /// 출차 LPR에서 찍은 번호판정보가 정상일때(Success) 부분인식일때(PartSuccess) 완전미인식(Nosuccess)일때
        /// </summary>
        public enum CarSearchSuccess
        {
            Success,
            PartSuccess,
            /// <summary>
            /// 
            /// </summary>
            Nosuccess,
            /// <summary>
            /// 수동입차
            /// </summary>
            Manual,
            /// <summary>
            /// 시간대정기권
            /// </summary>
            RegTIme
        }
        public enum CarFreeType
        {
            /// <summary>
            /// 회차가아닌차량
            /// </summary>
            NONE,
            /// <summary>
            /// 회차차량
            /// </summary>
            FreeCar,
            /// <summary>
            /// 사전정산이후 인터벌이전에 나감
            /// </summary>
            PreFreeCar,
            /// <summary>
            /// 웹할인등으로 할인후 무료차량
            /// </summary>
            DiscountFreeCar

        }


        public enum PaymentType
        {
            None,
            Cash,
            CreditCard,
            TmoneyCard,
            CashTicket,
            Free,
            DiscountCard
        }



        public enum EnumCarTypes
        {
            /// <summary>
            /// 정기권
            /// </summary>
            REG_OK = 10,           // 정기권
            /// <summary>
            /// 정기권 입출차에 존재
            /// </summary>
            REG_IO = 11,          // 정기권 입출차에 존재
            /// <summary>
            /// 시간대 정기권
            /// </summary>
            REG_TERM = 12,         // 시간대 정기권
            /// <summary>
            /// 일반권
            /// </summary>
            NOR_OK = 20,        // 일반권
            /// <summary>
            /// 일반권 입출차에 존재
            /// </summary>
            NOR_IO = 21,       // 일반권 입출차에 존재
            /// <summary>
            /// 정기권 시간대 위반
            /// </summary>
            REG_TERM_OVER = 31,      // 정기권 시간대 위반
            /// <summary>
            /// 정기권 시간대 위반(시작시간전)
            /// </summary>
            REG_TERM_BEFORE = 32,     // 정기권 시간대 위반(시작시간전)
            /// <summary>
            /// 정기권 시간대 위반(시작시간후)
            /// </summary>
            REG_TERM_AFTER = 33,    // 정기권 시간대 위반(시작시간후)
            /// <summary>
            /// 정기권 중지
            /// </summary>
            REG_STOP = 41,   // 정기권 중지
            /// <summary>
            /// 정기권 만료
            /// </summary>
            REG_EXPIRE = 42,  // 정기권 만료
            /// <summary>
            /// 정기권 시작일 안됨
            /// </summary>
            REG_BEFORE = 43, // 정기권 시작일 안됨
            /// <summary>
            /// DB연결안됨
            /// </summary>
            DBNotAccess = 99,
            /// <summary>
            /// 수동요금
            /// </summary>
            MANUAL = 101,
            /// <summary>
            /// 시간대정기권
            /// </summary>
            REGTIME = 98


        }

        public EnumCarTypes CarTypes
        {
            set { mCarTypes = value; }
            get { return mCarTypes; }
        }
        /// <summary>
        /// 0:경차, 1:소형 ,2:중형 ,3:대형
        /// </summary>
        public int CarType
        {
            set { mCarType = value; }
            get { return mCarType; }
        }


        /// <summary>
        /// 결제종류
        /// </summary>
        public PaymentType PaymentMethod
        {
            get { return mPaymentMethod; }
            set { mPaymentMethod = value; }
        }


        public enum ManualType
        {
            None,
            /// <summary>
            /// 무인에서 주차권으로 처리할때
            /// </summary>
            inTimeParkingTicket,
            InTime,
            Paymoney,
            Time,

        }
        /// <summary>
        /// 수동정산 타입인지 아닌지 입차주차권 파킹티켓인지등
        /// </summary>
        public ManualType ManualTypes
        {
            set { mManualType = value; }
            get { return mManualType; }
        }






        // 정기권 종류. 1:일반적 정기권, 2:While-List정기권(VIP), 3:Black-List정기권, 4:시간대별정기권(주간권, 야간권 등), 5:요일별정기권(월-금 등)
        public enum CarRegType
        {
            /// <summary>
            /// 일반적 정기권
            /// </summary>
            Reg_Normal,
            /// <summary>
            /// While-List정기권(VIP)
            /// </summary>
            Reg_VIP,
            /// <summary>
            /// Black-List정기권
            /// </summary>
            Reg_BlackList,
            /// <summary>
            /// 시간대별정기권(주간권, 야간권 등) 
            /// </summary>
            Reg_Time,
            /// <summary>
            /// 요일별정기권(월-금 등)
            /// </summary>
            Reg_Day

        }

        /// <summary>
        /// 사전정산여부 0이면 사전정산안함 1이면 사전무인 2이면 센터사전무인
        /// </summary>
        public int PreCalc
        {
            set { mPreCalc = value; }
            get { return mPreCalc; }
        }



        /// <summary>
        /// 입차 일자
        /// </summary>
        public string InDate { get { return mInYMD + mInHMS; } }
        public void CanclePreCreditBooth()
        {

        }
        public void Clear()
        {
            mParkNo = 0;
            mCarTypes = EnumCarTypes.NOR_IO;
            mCarType = 0;
            mPaymentMethod = PaymentType.Cash;
            mManualType = ManualType.None;
            mStartYMD = string.Empty;
            mCarKind = "N";
            mPreCalc = 0;
            mNumType = string.Empty;
            mChannel = string.Empty;
            mInYMD = string.Empty;
            mInHMS = string.Empty;
            mPreProcYmd = string.Empty;
            mPreProcHms = string.Empty;
            mOutYmd = string.Empty;
            mOutHms = string.Empty;
            mParkTime = 0;
            mParkMoney = 0;
            mDiscountMoney = 0;
            mInCarNumber = string.Empty;
            mInUnitNo = string.Empty;
            mTkType = string.Empty;
            mTkNO = string.Empty;
            mCarLength = string.Empty;
            mOutRecog1 = 1;
            OutChk = 0;
            ListDcDetail = null;
            ListDcDetail = new List<DcDetail>();
            mCurrentPayType = PayType.MoneyOrFree;
            mCurrentOutType = OutChkType.SuccessOut;
            mCurrentOutName = string.Empty;
            mOutCarNumber = string.Empty;
            mOutCarNumber1 = string.Empty;
            mLastErrorMessage = string.Empty;
            mInCarPath = string.Empty;
            mOutCarPath = string.Empty;
            mInRCarNumber = string.Empty;
            mInRCarPath = string.Empty;
            mPreNorkey = string.Empty;
            mExpireYmd = string.Empty;
            mIntervelTime = 0;
            mPreParktime = 0;
            mPreOutYmd = "";
            mPreOutHms = "";
            mPreInYmd = "";
            mPreInHms = "";
            mGuestDiscountTime = 0;
            mNotDisChargeMoney = 0;
            mDiscountDetail = string.Empty;
            mPreDiscountContent = string.Empty;
            mTMoneyPay = 0;
            mCashReciptNo = string.Empty;
            mCashReciptAuthDate = string.Empty;
            mCashReciptRescode = string.Empty;
            mCashReciptResMsg = string.Empty;
            mCashReciptApproveYmd = string.Empty;
            mCashReciptApproveHms = string.Empty;
            mCashReciptApprovalYmd = string.Empty;
            mCashReciptApprovalHms = string.Empty;
            mCashReciptRequestYesNo = 0;
            mCardPay = 0;
            mCardAuthNumber = string.Empty;
            mCardAUthDate = string.Empty;
            mCardAUthTime = string.Empty;
            mCardRescode = string.Empty;
            mCardResMsg = string.Empty;
            mCardResMsg2 = string.Empty;
            mSupplyPay = 0;
            mTaxPay = 0;
            mTicketNum = string.Empty;
            mCardName = string.Empty;
            mCardNumber = string.Empty;
            mCardApproveYmd = string.Empty;
            mCardApproveHms = string.Empty;
            mCardApprovalYmd = string.Empty;
            mCardApprovalHms = string.Empty;
            mCardVanType = 1;
            mCardDDDNumber = string.Empty;
            mCardBankCode = string.Empty;
            mCardMagneMentCode = string.Empty;
            mCardMemberCode = string.Empty;
            mCardBankName = string.Empty;
            mCardApprovalType = string.Empty;
            mBeforeCardPay = 0;
            mCardAcquirerCode = string.Empty;
            mCardAcquirerName = string.Empty;
            mElapsedMinute = 0;
            mElapsedDay = 0;
            mInCome10Qty = 0;
            mInCome50Qty = 0;
            mInCome100Qty = 0;
            mInCome500Qty = 0;
            mInCome1000Qty = 0;
            mInCome5000Qty = 0;
            mInCome10000Qty = 0;
            mInCome50000Qty = 0;
            mOutCome10Qty = 0;
            mOutCome50Qty = 0;
            mOutCome100Qty = 0;
            mOutCome500Qty = 0;
            mOutCome1000Qty = 0;
            mOutCome5000Qty = 0;
            mOutCome10000Qty = 0;
            mOutCome50000Qty = 0;
            mCurrentMoney = 0;
            mCurrent5000Qty = 0;
            mCurrent1000Qty = 0;
            mCurrent500Qty = 0;
            mCurrent100Qty = 0;
            mCurrent50Qty = 0;
            mCurrentEtcMoney = 0;
            mCharge5000Qty = 0;
            mCharge1000Qty = 0;
            mCharge500Qty = 0;
            mCharge100Qty = 0;
            mCharge50Qty = 0;
            mChargeMoney = 0;
            mCanclePayMoney = 0;
            mChargeEtcMoney = 0;
            mGroupName = "일반차량";
            mIsBarOpen = false;
            mCurrentTotalDiscount = string.Empty;
            mIo = string.Empty;
        }


        public void Clone(NormalCarInfo pNormalCarInfo)
        {
            mParkNo = pNormalCarInfo.ParkNo;
            mCarTypes = pNormalCarInfo.CarTypes;
            mCarType = pNormalCarInfo.CarType;
            mPaymentMethod = pNormalCarInfo.PaymentMethod;
            mManualType = pNormalCarInfo.ManualTypes;
            mStartYMD = pNormalCarInfo.StartYmd;
            mCarKind = pNormalCarInfo.CarKind;
            mPreCalc = pNormalCarInfo.PreCalc;
            mNumType = pNormalCarInfo.NumType;
            mChannel = pNormalCarInfo.Channel;
            mInYMD = pNormalCarInfo.InYMD;
            mInHMS = pNormalCarInfo.InHMS;
            mPreProcYmd = pNormalCarInfo.PreProcYmd;
            mPreProcHms = pNormalCarInfo.PreProcHms;
            mOutYmd = pNormalCarInfo.OutYmd;
            mOutHms = pNormalCarInfo.OutHms;
            mParkTime = pNormalCarInfo.ParkTime;
            mParkMoney = pNormalCarInfo.ParkMoney;
            mDiscountMoney = pNormalCarInfo.DiscountMoney;
            mInCarNumber = pNormalCarInfo.InCarNumber;
            mInUnitNo = pNormalCarInfo.InUnitNo;
            mTkType = pNormalCarInfo.TkType;
            mTkNO = pNormalCarInfo.TkNO;
            mOutRecog1 = pNormalCarInfo.OutRecog1;
            mOutChk = pNormalCarInfo.OutChk;
            ListDcDetail = null;
            ListDcDetail = new List<DcDetail>();
            mCurrentPayType = pNormalCarInfo.CurrentPayType;
            mCurrentOutType = pNormalCarInfo.CurrentOutType;
            mCurrentOutName = pNormalCarInfo.CurrentOutName;
            mOutCarNumber = pNormalCarInfo.OutCarNumber;
            mLastErrorMessage = pNormalCarInfo.LastErrorMessage;
            mInCarPath = pNormalCarInfo.InCarPath;
            mOutCarPath = pNormalCarInfo.OutCarPath;
            mInRCarNumber = pNormalCarInfo.InRCarNumber; ;
            mInRCarPath = pNormalCarInfo.InRCarPath;
            mPreNorkey = pNormalCarInfo.PreNorkey;
            mExpireYmd = pNormalCarInfo.ExpireYmd;
            mIntervelTime = pNormalCarInfo.IntervelTime;
            mPreParktime = pNormalCarInfo.PreParktime;
            mPreOutYmd = pNormalCarInfo.PreOutYmd;
            mPreOutHms = pNormalCarInfo.PreOutHms;
            mPreInYmd = pNormalCarInfo.PreInYmd;
            mPreInHms = pNormalCarInfo.PreInHms;
            mGuestDiscountTime = pNormalCarInfo.GuestDiscountTime;
            mNotDisChargeMoney = pNormalCarInfo.NotDisChargeMoney;
            mDiscountDetail = pNormalCarInfo.DiscountDetail;
            mPreDiscountContent = pNormalCarInfo.PreDiscountContent;
            mTMoneyPay = pNormalCarInfo.TMoneyPay;
            mCashReciptNo = pNormalCarInfo.CashReciptNo;
            mCashReciptAuthDate = pNormalCarInfo.CashReciptAuthDate;
            mCashReciptRescode = pNormalCarInfo.CashReciptRescode;
            mCashReciptResMsg = pNormalCarInfo.CashReciptResMsg;
            mCashReciptApproveYmd = pNormalCarInfo.CashReciptApproveYmd;
            mCashReciptApproveHms = pNormalCarInfo.CashReciptApproveHms;
            mCashReciptApprovalYmd = pNormalCarInfo.CashReciptApprovalYmd;
            mCashReciptApprovalHms = pNormalCarInfo.CashReciptApprovalHms;
            mCashReciptRequestYesNo = pNormalCarInfo.CashReciptRequestYesNo;
            mCardPay = pNormalCarInfo.CardPay;
            mCardAuthNumber = pNormalCarInfo.CardAuthNumber;
            mCardAUthDate = pNormalCarInfo.CardAUthDate;
            mCardAUthTime = pNormalCarInfo.CardAUthTime;
            mCardRescode = pNormalCarInfo.CardRescode;
            mCardResMsg = pNormalCarInfo.CardResMsg;
            mCardResMsg2 = pNormalCarInfo.CardResMsg2;
            mSupplyPay = pNormalCarInfo.SupplyPay;
            mTaxPay = pNormalCarInfo.TaxPay;
            mTicketNum = pNormalCarInfo.TicketNum;
            mCardName = pNormalCarInfo.CardName;
            mCardNumber = pNormalCarInfo.CardNumber;
            mCardApproveYmd = pNormalCarInfo.CardApproveYmd;
            mCardApproveHms = pNormalCarInfo.CardApproveHms;
            mCardApprovalYmd = pNormalCarInfo.CardApprovalYmd;
            mCardApprovalHms = pNormalCarInfo.CardApprovalHms;
            mCardVanType = pNormalCarInfo.CardVanType;
            mCardDDDNumber = pNormalCarInfo.CardDDDNumber;
            mCardBankCode = pNormalCarInfo.CardBankCode;
            mCardMagneMentCode = pNormalCarInfo.CardMagneMentCode;
            mCardMemberCode = pNormalCarInfo.CardMemberCode;
            mCardBankName = pNormalCarInfo.CardBankName;
            mCardApprovalType = pNormalCarInfo.CardApprovalType;
            mBeforeCardPay = pNormalCarInfo.BeforeCardPay;
            mCardAcquirerCode = pNormalCarInfo.CardAcquirerCode;
            mCardAcquirerName = pNormalCarInfo.CardAcquirerName;
            mElapsedMinute = pNormalCarInfo.ElapsedMinute;
            mElapsedDay = pNormalCarInfo.ElapsedDay;
            mInCome10Qty = pNormalCarInfo.InCome10Qty;
            mInCome50Qty = pNormalCarInfo.InCome50Qty;
            mInCome100Qty = pNormalCarInfo.InCome100Qty;
            mInCome500Qty = pNormalCarInfo.InCome500Qty;
            mInCome1000Qty = pNormalCarInfo.InCome1000Qty;
            mInCome5000Qty = pNormalCarInfo.InCome5000Qty;
            mInCome10000Qty = pNormalCarInfo.InCome10000Qty;
            mInCome50000Qty = pNormalCarInfo.InCome50000Qty;
            mOutCome10Qty = pNormalCarInfo.OutCome10Qty;
            mOutCome50Qty = pNormalCarInfo.OutCome50Qty;
            mOutCome100Qty = pNormalCarInfo.OutCome100Qty;
            mOutCome500Qty = pNormalCarInfo.OutCome500Qty;
            mOutCome1000Qty = pNormalCarInfo.OutCome1000Qty;
            mOutCome5000Qty = pNormalCarInfo.OutCome5000Qty;
            mOutCome10000Qty = pNormalCarInfo.OutCome10000Qty;
            mOutCome50000Qty = pNormalCarInfo.OutCome50000Qty;
            mCurrentMoney = pNormalCarInfo.CurrentMoney;
            mCurrent5000Qty = pNormalCarInfo.Current5000Qty;
            mCurrent1000Qty = pNormalCarInfo.Current1000Qty;
            mCurrent500Qty = pNormalCarInfo.Current500Qty;
            mCurrent100Qty = pNormalCarInfo.Current100Qty;
            mCurrent50Qty = pNormalCarInfo.Current50Qty;
            mCurrentEtcMoney = pNormalCarInfo.CurrentEtcMoney;
            mCharge5000Qty = pNormalCarInfo.Charge5000Qty;
            mCharge1000Qty = pNormalCarInfo.Charge1000Qty;
            mCharge500Qty = pNormalCarInfo.Charge500Qty;
            mCharge100Qty = pNormalCarInfo.Charge100Qty;
            mCharge50Qty = pNormalCarInfo.Charge50Qty;
            mChargeMoney = pNormalCarInfo.ChargeMoney;
            mCanclePayMoney = pNormalCarInfo.CanclePayMoney;
            mChargeEtcMoney = pNormalCarInfo.ChargeEtcMoney;
            mGroupName = pNormalCarInfo.GroupName;
            mIsBarOpen = pNormalCarInfo.IsBarOpen;
            mCurrentTotalDiscount = pNormalCarInfo.CurrentTotalDiscount;

        }

        public void CloneParkInfo(ParkInfo pParkInfo)
        {
            mCurrentParkInfo.Clone(pParkInfo);
        }

        public void CanCleClear()
        {
            this.mPaymentMethod = PaymentType.Cash;
            CurrentMoney = 0;
            mCurrent50Qty = 0;
            mCurrent100Qty = 0;
            mCurrent500Qty = 0;
            mCurrent1000Qty = 0;
            mCurrent5000Qty = 0;
            mCurrentEtcMoney = 0;

            mChargeMoney = 0;
            mCharge50Qty = 0;
            mCharge100Qty = 0;
            mCharge500Qty = 0;
            mCharge1000Qty = 0;
            mCharge5000Qty = 0;
            mChargeEtcMoney = 0;


        }
        public int ParkNo
        {
            set { mParkNo = value; }
            get { return mParkNo; }
        }
        public string CarKind
        {
            set { mCarKind = value; }
            get { return mCarKind; }

        }
        public string PreProcYmd
        {
            set { mPreProcYmd = value; }
            get { return mPreProcYmd; }
        }

        public string PreProcHms
        {
            set { mPreProcHms = value; }
            get { return mPreProcHms; }
        }
        /// <summary>
        /// 입차장비
        /// </summary>
        public string InUnitNo
        {
            set { mInUnitNo = value; }
            get { return mInUnitNo; }
        }
        /// <summary>
        /// 1:일반차량,2:정기차량,4:면제차량,9:수동차량
        /// </summary>
        public string TkType
        {
            set { mTkType = value; }
            get { return mTkType; }
        }



        public string TkNO
        {
            set { mTkNO = value; }
            get { return mTkNO; }
        }

        /// <summary>
        /// 인식여부 1이면 정상인식 2이면 부분인식 3이면 미인식
        /// </summary>
        public int OutRecog1
        {
            set { mOutRecog1 = value; }
            get { return mOutRecog1; }
        }

        public int OutChk
        {
            set { mOutChk = value; }
            get { return mOutChk; }
        }

        /// <summary>
        /// 요금정산타입
        /// </summary>
        public enum PayType
        {
            /// <summary>
            /// 무료 아님 현금
            /// </summary>
            MoneyOrFree = 1,
            /// <summary>
            /// 신용카드
            /// </summary>
            Credit = 2,

        }

        /// <summary>
        /// 출차시 출차타입...민원출차,...할인출차등등
        /// </summary>
        public PayType CurrentPayType
        {
            set { mCurrentPayType = value; }
            get { return mCurrentPayType; }
        }

        /// <summary>
        /// 출차타입
        /// </summary>
        public enum OutChkType
        {
            /// <summary>
            /// 미출차
            /// </summary>
            NotOut = 0,
            /// <summary>
            /// 정상출차
            /// </summary>
            SuccessOut = 1,
            /// <summary>
            /// 할인출차
            /// </summary>
            DiscountedOut = 2,
            /// <summary>
            /// 전체할인처리후무료출차
            /// </summary>
            DiscountFreeOut = 3,
            /// <summary>
            /// 회차
            /// </summary>
            HwaCha = 4,
            /// <summary>
            /// 무료시간대출차
            /// </summary>
            FreeTimeOut = 5,
            /// <summary>
            /// 민원출차
            /// </summary>
            MinontOut = 6,
            /// <summary>
            /// 미출차처리
            /// </summary>
            MiChylchaProcess = 7,
            /// <summary>
            /// 사전무인정산
            /// </summary>
            PreAutoBooth = 8,
            /// <summary>
            /// 선불정산출차
            /// </summary>
            PrePayOut

        }

        public EnumCarTypes GetCarType(DataTable p_dt)
        {
            try
            {
                if (p_dt == null || p_dt.Rows.Count == 0)
                {
                    return EnumCarTypes.NOR_IO;
                }
                else
                {
                    string l_Expired = p_dt.Rows[0]["ExpDateT"].ToString().Replace("-", "");
                    string l_Start = p_dt.Rows[0]["ExpDateF"].ToString().Replace("-", "");
                    if ((Convert.ToInt32(l_Expired) >= Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"))) &&
                         (Convert.ToInt32(l_Start) <= Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"))))
                    {
                        return EnumCarTypes.REG_OK;

                    }
                    else
                    {
                        return EnumCarTypes.REG_EXPIRE;
                    }
                }
            }
            catch (Exception ex)
            {
                return EnumCarTypes.NOR_IO;
            }
        }

        public EnumCarTypes GetCarType(string p_ExpireYmd)
        {
            try
            {
                string l_Expired = p_ExpireYmd.Replace("-", "");
                if (Convert.ToInt32(l_Expired) >= Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd")))
                {
                    return EnumCarTypes.REG_OK;
                }
                else
                {
                    return EnumCarTypes.REG_EXPIRE;
                }
            }
            catch (Exception ex)
            {
                return EnumCarTypes.NOR_IO;

            }

        }

        //public CarTypes GetCarType(string p_StartYmd, string p_ExpireYmd)
        //{
        //    try
        //    {
        //        string l_Start = p_StartYmd.Replace("-", "");
        //        string l_Expired = p_ExpireYmd.Replace("-", "");
        //        if ((Convert.ToInt32(l_Expired) >= Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"))) &&
        //             (Convert.ToInt32(l_Start) <= Convert.ToInt32(DateTime.Now.ToString("yyyyMMdd"))))
        //        {
        //            return CarTypes.REG_OK;
        //        }
        //        else
        //        {
        //            return CarTypes.REG_EXPIRE;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "CarInfo|GetCarType", ex.ToString());
        //        return CarTypes.NOR_IO;

        //    }

        //}

        /// <summary>
        /// 출차시 출차타입...민원출차,...할인출차등등
        /// </summary>
        public OutChkType CurrentOutType
        {
            set { mCurrentOutType = value; }
            get { return mCurrentOutType; }
        }


        public string CurrentOutName
        {
            set { mCurrentOutName = value; }
            get { return mCurrentOutName; }
        }
        /// <summary>
        /// 출차타입을 설정한다 할인받고 나간차량인지 순수요금을 낸 차량인지 등등...
        /// </summary>
        public void SetOucChkType(bool pIsAutoBooth)
        {
            mCurrentOutName = string.Empty;
            if (!pIsAutoBooth)
            {
                mCurrentOutType = OutChkType.PreAutoBooth;
                mCurrentOutName = "사전정산";
                return;
            }
            if (this.ParkMoney == 0) // 요금이 없다면 
            {
                mCurrentOutType = OutChkType.HwaCha;
                mCurrentOutName = "회차";
            }
            else // 주차요금이 있다면
            {
                if (DiscountMoney > 0) // 할인이있다면
                {
                    if (ParkMoney <= DiscountMoney) // 주차요금 전체를 할인했다면
                    {
                        mCurrentOutType = OutChkType.DiscountFreeOut;
                    }
                    else
                    {
                        mCurrentOutType = OutChkType.DiscountedOut;
                    }
                }
                else
                {
                    mCurrentOutType = OutChkType.SuccessOut;
                }
            }
            if (CardPay > 0)
            {
                CurrentPayType = PayType.Credit;
            }
            else
            {
                CurrentPayType = PayType.MoneyOrFree;
            }
        }


        public string InCarNumber
        {
            set { mInCarNumber = value; }
            get { return mInCarNumber; }
        }

        public string OutCarNumber
        {
            set { mOutCarNumber = value; }
            get { return mOutCarNumber; }
        }



        public string OutCarNumber1
        {
            set { mOutCarNumber1 = value; }
            get { return mOutCarNumber1; }
        }




        public string InCarPath
        {

            set { mInCarPath = value; }
            get { return mInCarPath; }
        }

        public string OutCarPath
        {

            set { mOutCarPath = value; }
            get { return mOutCarPath; }
        }

        public string InRCarNumber
        {
            set { mInRCarNumber = value; }
            get { return mInRCarNumber; }
        }

        public string InRCarPath
        {
            set { mInRCarPath = value; }
            get { return mInRCarPath; }
        }


        public string LastErrorMessage
        {
            set { mLastErrorMessage = value; }
            get { return mLastErrorMessage; }
        }






        public string PreNorkey
        {
            set { mPreNorkey = value; }
            get { return mPreNorkey; }
        }


        public string NumType
        {
            set { mNumType = value; }
            get { return mNumType; }
        }
        public string Channel
        {
            set { mChannel = value; }
            get { return mChannel; }
        }





        public string InYMD
        {
            set { mInYMD = value; }
            get { return mInYMD; }
        }

        public string InHMS
        {
            set { mInHMS = value; }
            get { return mInHMS; }
        }

        public string StartYmd
        {
            set { mStartYMD = value; }
            get { return mStartYMD; }
        }


        public string ExpireYmd
        {
            set { mExpireYmd = value; }
            get { return mExpireYmd; }
        }



        public long ParkTime
        {
            set { mParkTime = value; }
            get { return mParkTime; }
        }


        /// <summary>
        /// 사전정산차량의 출차시 사전정산시간에서 interval을 뺀나머지가 주차요금임 
        /// </summary>
        public int IntervelTime
        {
            set { mIntervelTime = value; }
            get { return mIntervelTime; }
        }

        public int ParkMoney
        {
            set { mParkMoney = value; }
            get { return mParkMoney; }
        }



        /// <summary>
        /// 이전 입차  년월일
        /// </summary>
        public string PreInYmd
        {
            set { mPreInYmd = value; }
            get { return mPreInYmd; }
        }
        /// <summary>
        /// 이전 입차 시간
        /// </summary>
        public string PreInHms
        {
            set { mPreInHms = value; }
            get { return mPreInHms; }
        }

        /// <summary>
        /// 이전 출차  년월일
        /// </summary>
        public string PreOutYmd
        {
            set { mPreOutYmd = value; }
            get { return mPreOutYmd; }
        }
        /// <summary>
        /// 이전 출차 시간
        /// </summary>
        public string PreOutHms
        {
            set { mPreOutHms = value; }
            get { return mPreOutHms; }
        }
        /// <summary>
        /// 바로 이전 주차시간
        /// </summary>
        public long PreParktime
        {
            set { mPreParktime = value; }
            get { return mPreParktime; }
        }



        /// <summary>
        /// 방문자 할인시간
        /// </summary>
        public int GuestDiscountTime
        {
            set { mGuestDiscountTime = value; }
            get { return mGuestDiscountTime; }
        }
        /// <summary>
        /// 방출할려는 금액중에 방출안된 금액(보관증 발행시 사용 변수)
        /// </summary>
        public int NotDisChargeMoney
        {
            set { mNotDisChargeMoney = value; }
            get { return mNotDisChargeMoney; }
        }

        /// <summary>
        /// 자동으로 할인금액에 따라 자동으로 받을금액이 생성된다.
        /// </summary>
        public int ReceiveMoney
        {
            get
            {
                int receveMoney = ParkMoney - DiscountMoney;
                if (receveMoney < 0)
                {
                    receveMoney = 0;
                }

                return receveMoney;
            }
        }

        /// <summary>
        /// 할인권이 입수됨에 따라 "할인권:수량" 포맷을 만들어 주는 함수 ex) "22:2시간:1,23:3시간:3"   "코드:이름:수량" 으로 들어감 
        /// </summary>
        /// <param name="p_CodeName"></param>
        public string DiscountDetail
        {
            get { return mDiscountDetail; }
            set { mDiscountDetail = value; }
        }


        /// <summary>
        /// 사전 할인 IONDATA의 RESERVE6을 사용
        /// </summary>
        public string PreDiscountContent
        {
            get { return mPreDiscountContent; }
            set { mPreDiscountContent = value; }
        }


        public int DiscountMoney
        {
            set { mDiscountMoney = value; }
            get { return mDiscountMoney; }
        }
        /// <summary>
        /// 교통카드 금액
        /// </summary>
        public int TMoneyPay
        {
            set { mTMoneyPay = value; }
            get { return mTMoneyPay; }

        }

        /// <summary>
        /// 현금 승인번호
        /// </summary>
        public string CashReciptNo
        {
            set { mCashReciptNo = value; }
            get { return mCashReciptNo; }

        }


        /// <summary>
        /// 현금 승인일자
        /// </summary>
        public string CashReciptAuthDate
        {
            set { mCashReciptAuthDate = value; }
            get { return mCashReciptAuthDate; }

        }

        /// <summary>
        /// 현금 승인응답코드
        /// </summary>
        public string CashReciptRescode
        {
            set { mCashReciptRescode = value; }
            get { return mCashReciptRescode; }

        }

        /// <summary>
        /// 현금 응답메시지
        /// </summary>
        public string CashReciptResMsg
        {
            set { mCashReciptResMsg = value; }
            get { return mCashReciptResMsg; }

        }




        public string CashReciptApproveYmd
        {
            set { mCashReciptApproveYmd = value; }
            get { return mCashReciptApproveYmd; }

        }



        public string CashReciptApproveHms
        {
            set { mCashReciptApproveHms = value; }
            get { return mCashReciptApproveHms; }

        }



        public string CashReciptApprovalYmd
        {
            set { mCashReciptApprovalYmd = value; }
            get { return mCashReciptApprovalYmd; }

        }



        public string CashReciptApprovalHms
        {
            set { mCashReciptApprovalHms = value; }
            get { return mCashReciptApprovalHms; }

        }


        /// <summary>
        /// 청구여부.  0:안함,  1:청구함
        /// </summary>
        public int CashReciptRequestYesNo
        {
            set { mCashReciptRequestYesNo = value; }
            get { return mCashReciptRequestYesNo; }

        }


        /// <summary>
        /// 신용카드 금액
        /// </summary>
        public int CardPay
        {
            set { mCardPay = value; }
            get { return mCardPay; }

        }


        /// <summary>
        /// 승인번호
        /// </summary>
        public string CardAuthNumber
        {
            set { mCardAuthNumber = value; }
            get { return mCardAuthNumber; }

        }


        /// <summary>
        /// 승인일자
        /// </summary>
        public string CardAUthDate
        {
            set { mCardAUthDate = value; }
            get { return mCardAUthDate; }

        }

        /// <summary>
        /// 승인일자
        /// </summary>
        public string CardAUthTime
        {
            set { mCardAUthTime = value; }
            get { return mCardAUthTime; }

        }

        /// <summary>
        /// 카드 승인응답코드
        /// </summary>
        public string CardRescode
        {
            set { mCardRescode = value; }
            get { return mCardRescode; }

        }


        /// <summary>
        /// 카드 응답메시지
        /// </summary>
        public string CardResMsg
        {
            set { mCardResMsg = value; }
            get { return mCardResMsg; }

        }


        /// <summary>
        /// 카드 응답메시지
        /// </summary>
        public string CardResMsg2
        {
            set { mCardResMsg2 = value; }
            get { return mCardResMsg2; }

        }


        /// <summary>
        /// 공급가
        /// </summary>
        public int SupplyPay
        {
            set { mSupplyPay = value; }
            get { return mSupplyPay; }
        }



        public int TaxPay
        {
            set { mTaxPay = value; }
            get { return mTaxPay; }

        }

        public enum TicketType
        {
            /// <summary>
            /// 주차권
            /// </summary>
            T,
            /// <summary>
            /// 할인권
            /// </summary>
            D,
            /// <summary>
            /// OCS
            /// </summary>
            O
        }



        public string TicketNum
        {
            set { mTicketNum = value; }
            get { return mTicketNum; }
        }







        public string CardName
        {
            set { mCardName = value; }
            get { return mCardName; }

        }



        public string CardNumber
        {
            set { mCardNumber = value; }
            get { return mCardNumber; }

        }




        public string CardApproveYmd
        {
            set { mCardApproveYmd = value; }
            get { return mCardApproveYmd; }

        }



        public string CardApproveHms
        {
            set { mCardApproveHms = value; }
            get { return mCardApproveHms; }

        }



        public string CardApprovalYmd
        {
            set { mCardApprovalYmd = value; }
            get { return mCardApprovalYmd; }

        }



        public string CardApprovalHms
        {
            set { mCardApprovalHms = value; }
            get { return mCardApprovalHms; }

        }


        /// <summary>
        /// Van사 형태.  1:DDC, 2:EDI, 3:EDC
        /// </summary>
        public int CardVanType
        {
            set { mCardVanType = value; }
            get { return mCardVanType; }

        }



        public string CardDDDNumber
        {
            set { mCardDDDNumber = value; }
            get { return mCardDDDNumber; }

        }



        public string CardBankCode
        {
            set { mCardBankCode = value; }
            get { return mCardBankCode; }

        }

        public string CardMagneMentCode
        {
            set { mCardMagneMentCode = value; }
            get { return mCardMagneMentCode; }
        }


        /// <summary>
        /// 카드 가맹정코드
        /// </summary>
        public string CardMemberCode
        {
            set { mCardMemberCode = value; }
            get { return mCardMemberCode; }
        }


        public string CardBankName
        {
            set { mCardBankName = value; }
            get { return mCardBankName; }

        }



        public string CardApprovalType
        {
            set { mCardApprovalType = value; }
            get { return mCardApprovalType; }

        }




        public int BeforeCardPay
        {
            set { mBeforeCardPay = value; }
            get { return mBeforeCardPay; }

        }

        /// <summary>
        /// 매입사코드
        /// </summary>
        public string CardAcquirerCode
        {
            set { mCardAcquirerCode = value; }
            get { return mCardAcquirerCode; }

        }


        /// <summary>
        /// 매입사명
        /// </summary>
        public string CardAcquirerName
        {
            set { mCardAcquirerName = value; }
            get { return mCardAcquirerName; }

        }


        /// <summary>
        /// 할인금액을 넣으면 총합을 구해서 DiscountMoney에 자동으로 집어넣고 자동으로 PaymentMoney 도 계산된다
        /// </summary>
        /// <param name="discountmoney"></param>
        public void TotalDiscountMoneyCalcureate(int discount_money)
        {

            DiscountMoney += discount_money;
        }
        private long mTotalDiscountTIme = 0;
        /// <summary>
        /// 할인된 시간을 누적한다
        /// </summary>
        /// <param name="discountTime"></param>
        public void TotalDiscountTimeCalcureate(long discountTime)
        {
            mTotalDiscountTIme += discountTime;

        }
        /// <summary>
        /// 총할인시간
        /// </summary>
        public long TotalDiscountTime
        {
            set { mTotalDiscountTIme = value; }
            get { return mTotalDiscountTIme; }
        }


        /// <summary>
        /// 출차하려는 날짜 YYYYMMDD
        /// </summary>
        public string OutYmd
        {
            set { mOutYmd = value; }
            get { return mOutYmd; }
        }
        /// <summary>
        /// 출차하려는시간 HHMMSS
        /// </summary>
        public string OutHms
        {
            set { mOutHms = value; }
            get { return mOutHms; }
        }



        /// <summary>
        /// 경과 분(화면 표시용)
        /// </summary>
        public long ElapsedMinute
        {
            set { ElapsedMinute = value; }
            get { return mElapsedMinute; }
        }

        private long mElapsedHour = 0;
        /// <summary>
        /// 경과 시간(화면 표시용)
        /// </summary>
        public long ElapsedHour
        {
            set { mElapsedHour = value; }
            get { return mElapsedHour; }
        }


        /// <summary>
        /// 경과 일(화면 표시용)
        /// </summary>
        public long ElapsedDay
        {
            set { mElapsedDay = value; }
            get { return mElapsedDay; }
        }

        public string GroupName
        {
            set { mGroupName = value; }
            get { return mGroupName; }
        }

        public bool IsBarOpen
        {
            set { mIsBarOpen = value; }
            get { return mIsBarOpen; }
        }

        public void ElpaseMinute(string startDate, string YYMMDD, string HHMMSS)
        {
            string _startDate = startDate.Replace("-", "").Replace(":", "").Replace(" ", "").Trim();
            int Years = Convert.ToInt32(_startDate.Substring(0, 4));
            int month = Convert.ToInt32(_startDate.Substring(4, 2));
            int day = Convert.ToInt32(_startDate.Substring(6, 2));
            int hour = Convert.ToInt32(_startDate.Substring(8, 2));
            int minute = Convert.ToInt32(_startDate.Substring(10, 2));
            int second = Convert.ToInt32(_startDate.Substring(12, 2));
            string _enddata = (YYMMDD + HHMMSS).Replace(":", "").Replace("-", "").Replace(" ", "").Trim();

            int EndYears = Convert.ToInt32(_enddata.Substring(0, 4));
            int Endmonth = Convert.ToInt32(_enddata.Substring(4, 2));
            int Endday = Convert.ToInt32(_enddata.Substring(6, 2));
            int Endhour = Convert.ToInt32(_enddata.Substring(8, 2));
            int Endminute = Convert.ToInt32(_enddata.Substring(10, 2));
            int Endsecond = Convert.ToInt32(_enddata.Substring(12, 2));

            DateTime oldDate = DateTime.ParseExact(_startDate.Substring(0, 12), "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture);
            DateTime newDate = DateTime.ParseExact(_enddata.Substring(0, 12), "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture);
            TimeSpan ts = newDate - oldDate;
            int dirrednceInMinute = Convert.ToInt32(ts.TotalMinutes);
            if (dirrednceInMinute <= 0)
            {
                dirrednceInMinute = 1;
            }
            mParkTime = Convert.ToInt64(dirrednceInMinute);

            CalculateElapsedTime(Convert.ToInt64(mParkTime.ToString("######")));
        }

        /// <summary>
        /// 분 총 경과시간을 일 시간 분으로 분리 시킴
        /// </summary>
        private void CalculateElapsedTime(long TotalMinute)
        {
            if (TotalMinute <= 1)
            {
                TotalMinute = 1;
                mElapsedMinute = 1;
                mElapsedHour = 0;
                mElapsedDay = 0;
                return;
            }
            try
            {
                long temp = TotalMinute;

                mElapsedMinute = temp % 60;

                temp -= mElapsedMinute;

                temp /= 60;

                mElapsedHour = temp % 24;

                temp -= mElapsedHour;

                temp /= 24;

                mElapsedDay = temp;

            }
            catch (Exception ex)
            {
            }
        }



        public int InCome10Qty
        {
            get { return mInCome10Qty; }
            set { mInCome10Qty = value; }
        }

        public int InCome50Qty
        {
            get { return mInCome50Qty; }
            set { mInCome50Qty = value; }
        }

        public int InCome100Qty
        {
            get { return mInCome100Qty; }
            set { mInCome100Qty = value; }
        }

        public int InCome500Qty
        {
            get { return mInCome500Qty; }
            set { mInCome500Qty = value; }

        }

        public int InCome1000Qty
        {
            get { return mInCome1000Qty; }
            set { mInCome1000Qty = value; }
        }

        public int InCome5000Qty
        {
            get { return mInCome5000Qty; }
            set { mInCome5000Qty = value; }
        }
        public int InCome10000Qty
        {
            get { return mInCome10000Qty; }
            set { mInCome10000Qty = value; }
        }

        public int InCome50000Qty
        {
            get { return mInCome50000Qty; }
            set { mInCome50000Qty = value; }
        }

        public int OutCome10Qty
        {
            get { return mOutCome10Qty; }
            set { mOutCome10Qty = value; }
        }

        public int OutCome50Qty
        {
            get { return mOutCome50Qty; }
            set { mOutCome50Qty = value; }
        }

        public int OutCome100Qty
        {
            get { return mOutCome100Qty; }
            set { mOutCome100Qty = value; }
        }

        public int OutCome500Qty
        {
            get { return mOutCome500Qty; }
            set { mOutCome500Qty = value; }
        }

        public int OutCome1000Qty
        {
            get { return mOutCome1000Qty; }
            set { mOutCome1000Qty = value; }
        }

        public int OutCome5000Qty
        {
            get { return mOutCome5000Qty; }
            set { mOutCome5000Qty = value; }
        }
        public int OutCome10000Qty
        {
            get { return mOutCome10000Qty; }
            set { mOutCome10000Qty = value; }
        }

        public int OutCome50000Qty
        {
            get { return mOutCome50000Qty; }
            set { mOutCome50000Qty = value; }
        }


        /// <summary>
        /// 현재 일자
        /// </summary>
        public string CurrentDate { get; set; }




        /// <summary>
        /// 현금결제시 현재 투입 금액, 해당 금액이 설정되면 현재 금액에 대한 반환 지폐수량,
        /// 거스름돈에 대한 지폐 수량이 자동으로 설정됨
        /// </summary>
        public int CurrentMoney
        {
            get { return mCurrentMoney; }
            set
            {
                mCurrentMoney = value;
                CalculateCurrent();
                CalculateCharge();
            }
        }


        /// <summary>
        /// 현재 투입 금액 5000원권 수량
        /// </summary>
        public int Current5000Qty
        {
            get { return mCurrent5000Qty; }
            set { mCurrent5000Qty = value; }
        }


        /// <summary>
        /// 현재 투입 금액 1000원권 수량
        /// </summary>
        public int Current1000Qty
        {
            get { return mCurrent1000Qty; }
            set { mCurrent1000Qty = value; }
        }


        /// <summary>
        /// 현재 투입 금액 500원 수량
        /// </summary>
        public int Current500Qty
        {
            get { return mCurrent500Qty; }
            set { mCurrent500Qty = value; }
        }


        /// <summary>
        /// 현재 투입 금액 100원 수량
        /// </summary>
        public int Current100Qty
        {
            get { return mCurrent100Qty; }
            set { mCurrent100Qty = value; }
        }


        /// <summary>
        /// 현재 투입 금액 50원 수량
        /// </summary>
        public int Current50Qty
        {
            get { return mCurrent50Qty; }
            set { mCurrent50Qty = value; }
        }





        /// <summary>
        /// 현재 투입 금액 5000금액
        /// </summary>
        public int Current5000Money
        {
            get { return mCurrent5000Qty * 5000; }
        }

        /// <summary>
        /// 현재 투입 금액 1000금액
        /// </summary>
        public int Current1000Money
        {
            get { return mCurrent1000Qty * 1000; }
        }

        /// <summary>
        /// 현재 투입 금액 500원 금액
        /// </summary>
        public int Current500Money
        {
            get { return mCurrent500Qty * 500; }
        }

        /// <summary>
        /// 현재 투입 금액 100원 금액
        /// </summary>
        public int Current100Money
        {
            get { return mCurrent100Qty * 100; }
        }

        /// <summary>
        /// 현재 투입 금액 50원 금액
        /// </summary>
        public int Current50Money
        {
            get { return mCurrent50Qty * 50; }
        }

        /// <summary>
        /// 현재 투입 금액 동전 금액
        /// </summary>
        public int CurrentCoinMoney
        {
            get { return Current500Money + Current100Money + Current50Money; }
        }


        public int CurrentEtcMoney
        {
            get { return mCurrentEtcMoney; }
        }

        /// <summary>
        /// 현재 금액 계산
        /// </summary>
        /// <returns></returns>
        public bool CalculateCurrent()
        {
            int temp = mCurrentMoney;

            mCurrent5000Qty = temp / 5000;

            temp = temp % 5000;

            mCurrent1000Qty = temp / 1000;

            temp = temp % 1000;

            mCurrent500Qty = temp / 500;

            temp = temp % 500;

            mCurrent100Qty = temp / 100;

            temp = temp % 100;

            mCurrent50Qty = temp / 50;

            mCurrentEtcMoney = temp % 50;

            return true;
        }

        /// <summary>
        /// 거스름 돈 계산
        /// </summary>
        /// <returns></returns>
        public bool CalculateCharge()
        {
            mChargeMoney = 0;

            mCharge50Qty = 0;
            mCharge100Qty = 0;
            mCharge500Qty = 0;
            mCharge1000Qty = 0;
            mCharge5000Qty = 0;
            if (mCurrentMoney + DiscountMoney > ParkMoney) //현재 투입금액+할인금액이이 주차요금보다 많을때
                mChargeMoney = mCurrentMoney + DiscountMoney - ParkMoney;

            if (mChargeMoney < 0)
            {
                mChargeMoney = 0;
            }

            int temp = mChargeMoney;

            mCharge5000Qty = temp / 5000;

            temp = temp % 5000;

            mCharge1000Qty = temp / 1000;

            temp = temp % 1000;

            mCharge500Qty = temp / 500;

            temp = temp % 500;

            mCharge100Qty = temp / 100;

            temp = temp % 100;

            mCharge50Qty = temp / 50;

            mChargeEtcMoney = temp % 50;

            return true;
        }


        /// <summary>
        /// 거스름 돈 5000원권 수량
        /// </summary>
        public int Charge5000Qty
        {
            get { return mCharge5000Qty; }
            set { mCharge5000Qty = value; }
        }


        /// <summary>
        /// 거스름 돈 1000원권 수량
        /// </summary>
        public int Charge1000Qty
        {
            get { return mCharge1000Qty; }
            set { mCharge1000Qty = value; }
        }


        /// <summary>
        /// 거스름 돈 500원 수량
        /// </summary>
        public int Charge500Qty
        {
            get { return mCharge500Qty; }
            set { mCharge500Qty = value; }
        }


        /// <summary>
        /// 거스름 돈 100원 수량
        /// </summary>
        public int Charge100Qty
        {
            get { return mCharge100Qty; }
            set { mCharge100Qty = value; }
        }


        /// <summary>
        /// 거스름 돈 50원 수량
        /// </summary>
        public int Charge50Qty
        {
            get { return mCharge50Qty; }
            set { mCharge50Qty = value; }
        }


        /// <summary>
        /// 거스름 돈 총 금액
        /// </summary>
        public int ChargeMoney
        {
            get { return mChargeMoney; }

        }

        /// <summary>
        /// 거스름 돈 5000원권 금액
        /// </summary>
        public int Charge5000Money
        {
            get { return mCharge5000Qty * 5000; }
        }

        /// <summary>
        /// 거스름 돈 1000원권 금액
        /// </summary>
        public int Charge1000Money
        {
            get { return mCharge1000Qty * 1000; }
        }

        /// <summary>
        /// 거스름 돈 500원 금액
        /// </summary>
        public int Charge500Money
        {
            get { return mCharge500Qty * 500; }
        }

        /// <summary>
        /// 거스름 돈 100원 금액
        /// </summary>
        public int Charge100Money
        {
            get { return mCharge100Qty * 100; }
        }

        /// <summary>
        /// 거스름 돈 50원 금액
        /// </summary>
        public int Charge50Money
        {
            get { return mCharge50Qty * 50; }
        }

        /// <summary>
        /// 거스름돈 동전 금액
        /// </summary>
        public int ChargeCoinMoney
        {
            get { return Charge500Money + Charge100Money + Charge50Money; }
        }

        /// <summary>
        /// 거스름돈을 5000,1000,500,100,50원 수량값을 더해 다시세팅한다.
        /// </summary>
        public void SetChargeMoney()
        {
            mChargeMoney = Charge5000Money + Charge1000Money + Charge500Money + Charge100Money + Charge50Money;
        }

        public void SetCurrentMoney()
        {
            mCurrentMoney = Current5000Money + Current1000Money + Current500Money + Current100Money + Current50Money;
        }



        /// <summary>
        /// 현금 투입후 취소시 방출할금액(방출이 비정상일때는 보관증 금액으로 사용됨)
        /// </summary>
        public int CanclePayMoney
        {
            set { mCanclePayMoney = value; }
            get { return mCanclePayMoney; }
        }
        /// <summary>
        /// 거스름돈을 수량을 모두 0으로 변경한다.
        /// </summary>
        public void SetResetChargeMoney()
        {
            mCharge5000Qty = 0;
            mCharge1000Qty = 0;
            mCharge500Qty = 0;
            mCharge100Qty = 0;
            mCharge50Qty = 0;

        }
        public string CurrentTotalDiscount
        {
            set { mCurrentTotalDiscount = value; }
            get { return mCurrentTotalDiscount; }

        }

        public string IO
        {
            set { mIo = value; }
            get { return mIo; }

        }

        /// <summary>
        /// 5000, 1000, 500, 100 단위로 계산하고 남은 나머지 금액
        /// </summary>
        public int ChargeEtcMoney
        {
            get { return mChargeEtcMoney; }
        }

        /// <summary>
        /// 입력받은 영수증 번호
        /// </summary>
        public string ScanReceiptNo { get; set; }

        /// <summary>
        /// 입력받은 영수증에 대한 기타 정보들
        /// </summary>
        public string ScanReceiptNoExtra { get; set; }

        /// <summary>
        /// 정산기에서 출력한 영수증 번호
        /// </summary>
        public string PrintReceiptNo { get; set; }

        /// <summary>
        /// 정산기에서 출력한 보관증 번호
        /// </summary>
        public string PrintCashTicketNo { get; set; }



        #region 교통카드

        /// <summary>
        /// 카드 일련번호: BCD 5byte
        /// </summary>
        public string TMoneySaveCardSerial
        {
            get;
            set;
        }


        /// <summary>
        /// 거래순버
        /// </summary>
        public string TMoneySaveLogIndex
        {
            set;
            get;
        }

        /// <summary>
        /// 교통카드 저장 종류
        /// </summary>
        public string TMoneySaveCardtype
        {
            set;
            get;
        }

        /// <summary>
        /// 처리구분
        /// </summary>
        public string TMoneySaveStep
        {
            set;
            get;
        }

        /// <summary>
        /// 지불구분
        /// </summary>
        public int TMoneySavePurchaseType
        {
            set;
            get;
        }
        /// <summary>
        /// 지불구분
        /// </summary>
        public string TMoneySaveUserType
        {
            set;
            get;
        }
        /// <summary>
        /// 할인율
        /// </summary>
        public int TMoneySaveDiscountRation
        {
            set;
            get;
        }
        /// <summary>
        /// 전자화폐 거래 일련번호(Trcount)
        /// </summary>
        public int TMoneySaveNTep
        {
            set;
            get;
        }
        /// <summary>
        /// 전자화폐 잔액(TrAfterBal)
        /// </summary>
        public int TMoneySaveBaLep
        {
            set;
            get;
        }
        /// <summary>
        /// 전자화폐 잔액(TrAmount)
        /// </summary>
        public int TMoneySaveMpda
        {
            set;
            get;
        }

        /// <summary>
        /// 발급자(5byte)
        /// </summary>
        public string TMoneySaveRID
        {
            set;
            get;
        }

        /// <summary>
        /// 인증자
        /// </summary>
        public string TMoneySaveMAC
        {
            set;
            get;
        }
        /// <summary>
        /// RFTSAM        ///
        /// </summary>
        public string TMoneySaveRFTSAM
        {
            set;
            get;
        }
        /// <summary>
        /// 단말기 Serial 번호
        /// </summary>
        public string TMoneySaveTermSerial
        {
            set;
            get;
        }
        /// <summary>
        /// 예비
        /// </summary>
        public string TMoneySaveRFU
        {
            set;
            get;
        }


        public void ClearTmoneyData()
        {
            TMoneyPay = 0;
            TMoneySaveBaLep = 0;
            TMoneySaveCardSerial = "";
            TMoneySaveCardtype = "";
            TMoneySaveDiscountRation = 0;
            TMoneySaveLogIndex = "";
            TMoneySaveMAC = "";
            TMoneySaveMpda = 0;
            TMoneySaveNTep = 0;
            TMoneySavePurchaseType = 0;
            TMoneySaveRFTSAM = "";
            TMoneySaveRFU = "";
            TMoneySaveRID = "";
            TMoneySaveStep = "";
            TMoneySaveTermSerial = "";
            TMoneySaveUserType = "";
        }
        public void SaveTmoney(byte[] data)
        {

            List<byte> mbyte = new List<byte>();
            foreach (byte item in data)
            {
                mbyte.Add(item);
            }
            int i = 0;
            i += 7;
            TMoneySaveLogIndex = TextCore.StringToByte(mbyte.GetRange(i, 2).ToArray());  // 저장


            i += 2;
            TMoneySaveCardtype = TextCore.StringToByte(mbyte.GetRange(i, 1).ToArray());  // CardKind

            i += 1;
            TMoneySaveStep = TextCore.StringToByte(mbyte.GetRange(i, 1).ToArray());  // Step
            i += 1;
            TMoneySaveCardSerial = TextCore.HexaToDecimal(mbyte.GetRange(i, 4).ToArray()).ToString(); //CardSerial 인트로
            i += 8;
            TMoneySavePurchaseType = Convert.ToInt32(TextCore.HexaToDecimal(mbyte.GetRange(i, 1).ToArray()));  // PuchaseType
            i += 1;
            TMoneySaveUserType = TextCore.StringToByte(mbyte.GetRange(i, 2).ToArray());
            i += 2;
            TMoneySaveDiscountRation = Convert.ToInt32(TextCore.HexaToDecimal(mbyte.GetRange(i, 2).ToArray())); // DiscountRatio 인트로
            i += 2;
            TMoneySaveNTep = Convert.ToInt32(TextCore.HexaToDecimal(mbyte.GetRange(i, 4).ToArray())); // TrCount 인트로
            i += 4;
            TMoneySaveBaLep = Convert.ToInt32(TextCore.HexaToDecimal(mbyte.GetRange(i, 4).ToArray()));// TrAfterBal 인트로
            i += 4;
            TMoneySaveMpda = Convert.ToInt32(TextCore.HexaToDecimal(mbyte.GetRange(i, 4).ToArray())); // TrAmount) 인트로
            i += 4;
            TMoneySaveRID = TextCore.StringToByte(mbyte.GetRange(i, 8).ToArray());  // RID
            i += 8;
            TMoneySaveMAC = TextCore.StringToByte(mbyte.GetRange(i, 4).ToArray()); //Mac
            i += 4;
            TMoneySaveRFTSAM = TextCore.StringToByte(mbyte.GetRange(i, 4).ToArray());  //RFTCSN
            i += 4;

            TMoneySaveTermSerial = TextCore.StringToByte(mbyte.GetRange(i, 5).ToArray());  // TermNumber
            i += 5;
            TMoneySaveRFU = TextCore.StringToByte(mbyte.GetRange(i, 5).ToArray());

        }
        #endregion
        /// <summary>
        /// 주차시간에서 할인시간을 뺀 나머지
        /// </summary>
        public long GetRemainderParktime()
        {
            return (ParkTime - mTotalDiscountTIme);
        }

        /// <summary>
        /// 요금에서 할인 금액등을 제외한 실제 결제할 금액
        /// </summary>
        public int PaymentMoney
        {
            get
            {
                int paymentMoney = ParkMoney - DiscountMoney - CurrentMoney - CardPay - TMoneyPay;

                if (paymentMoney < 0)
                    return 0;
                else
                    return paymentMoney;
            }
        }

        private string getImagePath(string pinCarImagePath)
        {
            if (pinCarImagePath.Trim() == "")
            {
                return "";

            }
            try
            {
                pinCarImagePath = pinCarImagePath.ToUpper().Replace(":9080", "").Replace("HTTP:", "").Replace((char)0x2F, (char)0x5c);
                return pinCarImagePath;

            }
            catch (Exception ex)
            {
                return "";
            }
        }
        /// <summary>

        //public void SetParkingCarTable(DataTable l_InRegCarTable)
        //{
        //    try
        //    {
        //        foreach (DataRow item in l_InRegCarTable.Rows)
        //        {


        //            InYMD = item["ProcDate"].ToString().Replace("-", "").Replace(":", "");

        //            InHMS = item["ProcTime"].ToString().Replace("-", "").Replace(":", "");
        //            InUnitNo = item["UnitNo"].ToString().Replace("-", "").Replace(":", "");

        //            InCarNumber = item["InCarNo1"].ToString();

        //            InCarPath = getImagePath(item["InImage1"].ToString());

        //            InRCarNumber = item["InCarNo2"].ToString();


        //            InRCarPath = getImagePath(item["InImage2"].ToString());

        //            ExpireYmd = item["ExpDateT"].ToString().Replace("-", "");

        //            CarLength = item["CarType"].ToString();

        //            TkType = item["TKTYPE"].ToString();

        //            TkNO = item["TkNo"].ToString();
        //            PreDiscountContent = item["DiscountContent"].ToString();
        //            OutChk = Convert.ToInt32(item["OutChk"].ToString());
        //            if (OutChk == 8) // 사전무인에서 계산한거라면
        //            {
        //                PreCalc = 1;
        //                PreProcYmd = InYMD;
        //                PreProcHms = InHMS;
        //                InYMD = item["OutDate"].ToString().Replace("-", "").Replace(":", "");
        //                InHMS = item["OutTime"].ToString().Replace("-", "").Replace(":", "");
        //            }
        //            if (NPSYS.g_IsAutoBooth == false)
        //            {
        //                OutYmd = DateTime.Now.ToString("yyyyMMdd");
        //                OutHms = DateTime.Now.ToString("HHmmss");

        //            }
        //            if (InCarNumber.Contains("X") && (!InRCarNumber.Contains("X") && !InRCarNumber.Contains("0000")))
        //            {
        //                OutCarNumber = InRCarNumber;
        //            }
        //            else
        //            {
        //                OutCarNumber = InCarNumber;
        //            }


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        TextCore.INFO(TextCore.INFOS.PROGRAM_ERROR, "CarInfo|SetParkingCarTable", "[차량변수저장오류]" + ex.ToString());
        //    }
        //}




        public void SetNotParkingCarTable(DataTable l_RegCarTable)
        {
            foreach (DataRow item in l_RegCarTable.Rows)
            {

                CarKind = "R";
                ExpireYmd = item["ExpDateT"].ToString().Replace("-", "");

            }
        }

        /// <summary>
        /// 선할인차량이 있다면 할인및 사전정산한차량 할인규칙갱신
        /// </summary>
        public void CheckPreDiscount()
        {
            //// 할인권번호:값:명칭:수량:업체명,할인권번호:값:명칭:수량:업체명
            ////1:120:2시간(무료):1:메종두라라뺑
            //// 1:120:2시간(무료):1:948키친,1:120:2시간(무료):1:주커피
            //if (PreDiscountContent.Trim() != string.Empty) // 웹할인유무
            //{
            //    DataTable resultWebNexpa = LPRDbSelect.GetWebNexpa(NPSYS.m_MSSQL, this.TkNO);

            //    if (resultWebNexpa != null && resultWebNexpa.Rows.Count > 0)
            //    {
            //        string webdiscountdetail = string.Empty;
            //        DIscountTicketOcs m_DIscountTicketOcs = new DIscountTicketOcs();
            //        foreach (DataRow resultitem in resultWebNexpa.Rows)
            //        {
            //            int discountCount = Convert.ToInt32(resultitem["discountCount"]);
            //            string discountCode = resultitem["DiscountCode"].ToString();
            //            for (int i = 0; i < discountCount; i++)
            //            {
            //                m_DIscountTicketOcs.DiscountTIcket(DIscountTicketOcs.DIscountTicketType.Web, discountCode, this, null);
            //            }
            //        }
            //    }

            //}
            //if (PreCalc == 1 && PreDiscountContent.Trim() == string.Empty) // 사전정산했던차량이 웹할인도 없었다면
            //{
            //    DataTable dt= LPRDbSelect.GetPrePaymentDiscountInfo(NPSYS.m_MSSQL, this);
            //    if (dt != null && dt.Rows.Count > 0)
            //    {
            //        foreach (DataRow item in dt.Rows)
            //        {
            //            SaleTicketUsePermissions.TicketAdd(item["DCNO"].ToString());
            //            TextCore.INFO(TextCore.INFOS.PROGRAM_INFO, "NormalCarInfo | CheckPreDiscount", "[기존사전정산한차량 할인사용내역] " + item["DCNO"].ToString() + " 할인사용");
            //        }

            //    }
            //}

        }


        /// <summary>
        /// 차량번호 6자리로 정기권 차량번호를 가져온다
        /// </summary>
        /// <param name="p_InCarInfo"></param>
        public string GetRegCarNumberSixNumber(ParkInfo pParkInfo, int pParkNo, string pCarnumber)
        {
            string lSixCarnumber = "";
            //if (NPSYS.g_UseSixNumberJungi == false)
            //{
            //    return string.Empty;
            //}
            for (int i = 0; i < pCarnumber.Length; i++)
            {
                if (TextCore.IsInt(pCarnumber.Substring(i, 1)))
                {
                    lSixCarnumber += pCarnumber.Substring(i, 1);
                }
            }
            if (lSixCarnumber.Length != 6)
            {
                return "";
            }
            try
            {
                string lCarnumber = DBActionDML.GetRegCarnumberSearchSixNumber(pParkInfo, pParkNo, lSixCarnumber);
                return lCarnumber;


            }
            catch (Exception ex)
            {
                return "";
            }

        }
        /// <summary>
        /// 차량종류식별로 리턴값 CarType.NOR_OK , CarType.REG_OK , CarType.REG_EXPIRE
        /// </summary>
        /// <param name="p_carNum"></param>
        /// <returns></returns>
        public NormalCarInfo.EnumCarTypes getCarTypeIndenty(ParkInfo pParkInfo, int pParkNo, string p_carNum)
        {
            try
            {
                DataTable _dt = DBActionDML.GetRegMasterCar_Expire(pParkInfo, pParkNo, p_carNum);
                if (_dt == null || _dt.Rows.Count == 0)
                {
                    string l_Carnumber = GetRegCarNumberSixNumber(pParkInfo, pParkNo, p_carNum);
                    if (l_Carnumber != string.Empty)
                    {
                        OutCarNumber = l_Carnumber;
                        _dt = DBActionDML.GetRegMasterCar_Expire(pParkInfo, pParkNo, l_Carnumber);
                    }
                }
                NormalCarInfo.EnumCarTypes l_Cartype = GetCarType(_dt);
                if (pParkInfo.CurrentDbServer.IsDbConnected == true)
                {
                    return l_Cartype;
                }
                else
                {
                    return l_Cartype = NormalCarInfo.EnumCarTypes.DBNotAccess;
                }
            }
            catch (Exception ex)
            {
                return NormalCarInfo.EnumCarTypes.DBNotAccess;
            }
        }


    }
}