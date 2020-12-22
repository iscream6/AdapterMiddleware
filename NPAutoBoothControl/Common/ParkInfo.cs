using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class ParkInfo
    {
        private int mParkNo = 0;
        private string mParkName = string.Empty;
        private DBServer mCurrentDBServer = null;
        private ParkInfoSetting mCurrentParkInfoSetting = ParkInfoSetting.NotSettingParkNo;
        private Parking mCurrentParking = Parking.ParkingZone;
        private UserInfo mCurrentUserInfo = null;

        /// <summary>
        /// 주차장번호
        /// </summary>
        public int ParkNo
        {
            set { mParkNo = value; }
            get { return mParkNo; }
        }
        /// <summary>
        /// 주차장명
        /// </summary>
        public string ParkName
        {
            set { mParkName = value; }
            get { return mParkName; }
        }
        /// <summary>
        /// 주차장레벨 SettingParkNo이면 현재 설정파일상 등록되어있는 주차장
        /// </summary>
        public enum ParkInfoSetting
        {
            /// <summary>
            ///  현재 설정파일에 등록되어있는 주차장
            /// </summary>
            SettingParkNo,
            /// <summary>
            /// DB로 읽어온 다른주차장
            /// </summary>
            NotSettingParkNo
        }
        public enum Parking
        {
            /// <summary>
            /// 통합센터
            /// </summary>
            TotalCenter,
            /// <summary>
            /// 일반주차장
            /// </summary>
            ParkingZone
        }
        /// <summary>
        /// 현재 주차장의 User정보
        /// </summary>
        public UserInfo CurrentUserInfo
        {
            set { mCurrentUserInfo = value; }
            get { return mCurrentUserInfo; }
        }
        /// <summary>
        /// 현재 주차장의 DB서버 연결자원
        /// </summary>
        public DBServer CurrentDbServer
        {
            set { mCurrentDBServer = value; }
            get { return mCurrentDBServer; }

        }
        /// <summary>
        /// 현재 주차장정보 SettingParkNo이면 설정에 등록된 주차장
        /// </summary>
        public ParkInfoSetting CurrentParkInfoSetting
        {
            set { mCurrentParkInfoSetting = value; }
            get { return mCurrentParkInfoSetting; }

        }
        /// <summary>
        /// TotalCenter(통합센터)인지 일반주차장인지구별
        /// </summary>
        public Parking CurrentParking
        {
            set { mCurrentParking = value; }
            get { return mCurrentParking; }
        }

        public void Clone(ParkInfo pParkInfo)
        {
            ParkNo = pParkInfo.ParkNo;
            ParkName = pParkInfo.ParkName;
            mCurrentDBServer = pParkInfo.mCurrentDBServer;
            mCurrentParkInfoSetting = pParkInfo.mCurrentParkInfoSetting;
            mCurrentUserInfo = pParkInfo.mCurrentUserInfo;
            mCurrentParking = pParkInfo.mCurrentParking;

        }

    }
}
