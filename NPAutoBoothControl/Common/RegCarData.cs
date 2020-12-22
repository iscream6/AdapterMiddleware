using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// 정기권정보클래스
    /// </summary>
    public class RegCarData
    {
        private int mParkNo = 0;
        public int ParkNo
        {
            set { mParkNo = value; }
            get { return mParkNo; }
        }
        private string mParkName = string.Empty;
        public string ParkName
        {
            set { mParkName = value; }
            get { return mParkName; }

        }
        private int mTKType = 2;
        public int TkType
        {
            set { mTKType = value; }
            get { return mTKType; }
        }
        private int mGroupNo = 0;
        public int GroupNo
        {
            set { mGroupNo = value; }
            get { return mGroupNo; }

        }
        private string mCarNo = string.Empty;
        public string CarNo
        {
            set { mCarNo = value; }
            get { return mCarNo; }

        }
        private string mTkNo = string.Empty;
        public string TkNo
        {
            set { mTkNo = value; }
            get { return mTkNo; }
        }
        private string mName = string.Empty;
        public string Name
        {
            set { mName = value; }
            get { return mName; }
        }
        private string mTellNo = string.Empty;
        public string TellNo
        {
            set { mTellNo = value; }
            get { return mTellNo; }
        }
        private string mCompName = string.Empty;
        public string CompName
        {
            set { mCompName = value; }
            get { return mCompName; }
        }
        private string mDeptName = string.Empty;
        public string DeptName
        {
            set { mDeptName = value; }
            get { return mDeptName; }
        }
        private string mIssueDate = string.Empty;
        public string IssueDate
        {
            set { mIssueDate = value; }
            get { return mIssueDate; }
        }

        private int mIssueAmt = 0;
        public int IssueAmt
        {
            set { mIssueAmt = value; }
            get { return mIssueAmt; }
        }

        private int mStatus = 0;
        public int Status
        {
            set { mStatus = value; }
            get { return mStatus; }
        }

        private string mExpDateF = string.Empty;
        public string ExpDateF
        {
            set { mExpDateF = value; }
            get { return mExpDateF; }
        }

        private string mExpDateT = string.Empty;
        public string ExpDateT
        {
            set { mExpDateT = value; }
            get { return mExpDateT; }
        }

        private int mWPNo = 0;
        public int WPNo
        {
            set { mWPNo = value; }
            get { return mWPNo; }
        }

        private int mLastParkNo = 0;
        public int LastParkNo
        {
            set { mLastParkNo = value; }
            get { return mLastParkNo; }
        }

        private int mLastUnitNo = 0;
        public int LastUnitNo
        {
            set { mLastUnitNo = value; }
            get { return mLastUnitNo; }
        }

        private int mIOStatusNo = 2;
        public int IOStatusNo
        {
            set { mIOStatusNo = value; }
            get { return mIOStatusNo; }
        }

        private int mCurrAmt = 0;
        public int CurrAmt
        {
            set { mCurrAmt = value; }
            get { return mCurrAmt; }
        }

        private int mAPB = 3;
        public int APB
        {
            set { mAPB = value; }
            get { return mAPB; }
        }

        private int mCarType = 0;
        public int CarType
        {
            set { mCarType = value; }
            get { return mCarType; }
        }

        private string mReserve1 = string.Empty;
        public string Reserve1
        {
            set { mReserve1 = value; }
            get { return mReserve1; }
        }


        private string mReserve2 = string.Empty;
        public string Reserve2
        {
            set { mReserve2 = value; }
            get { return mReserve2; }
        }

        private string mReserve3 = string.Empty;
        public string Reserve3
        {
            set { mReserve3 = value; }
            get { return mReserve3; }
        }

        private string mReserve4 = string.Empty;
        public string Reserve4
        {
            set { mReserve4 = value; }
            get { return mReserve4; }
        }

        private string mReserve5 = string.Empty;
        public string Reserve5
        {
            set { mReserve5 = value; }
            get { return mReserve5; }
        }


        private string mReserve6 = string.Empty;
        public string Reserve6
        {
            set { mReserve6 = value; }
            get { return mReserve6; }
        }


        private string mReserve7 = string.Empty;
        public string Reserve7
        {
            set { mReserve7 = value; }
            get { return mReserve7; }
        }

        private string mReserve8 = string.Empty;
        public string Reserve8
        {
            set { mReserve8 = value; }
            get { return mReserve8; }
        }

        private string mReserve9 = string.Empty;
        public string Reserve9
        {
            set { mReserve9 = value; }
            get { return mReserve9; }
        }

        private string mReserve10 = string.Empty;
        public string Reserve10
        {
            set { mReserve10 = value; }
            get { return mReserve5; }
        }

        private int mUseYes = 0;
        public int UseYes
        {
            set { mUseYes = value; }
            get { return mUseYes; }
        }
        private string mAddress = string.Empty;
        public string Address
        {
            set { mAddress = value; }
            get { return mAddress; }
        }
        private string mLastUseDate = string.Empty;
        public string LastUseDate
        {
            set { mLastUseDate = value; }
            get { return mLastUseDate; }
        }
        private string mLastUseTime = string.Empty;
        public string LastUseTime
        {
            set { mLastUseTime = value; }
            get { return mLastUseTime; }
        }
        private int mIssueType = 0;
        public int IssueType
        {
            set { mIssueType = value; }
            get { return mIssueType; }
        }
    }
}
