using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class NPCOMMON
    {
        /// <summary>
        /// 여러 주차장 정보를 가진 LIST
        /// </summary>
        public static List<ParkInfo> mListParkInfo = new List<ParkInfo>();
        /// <summary>
        /// 주차장정보를 주차장번호로 가져온다
        /// </summary>
        /// <param name="pParkNo"></param>
        /// <returns></returns>
        public static ParkInfo GetParkInfo(int pParkNo)
        {
            return mListParkInfo.Find(x => x.ParkNo == pParkNo);
        }
        ///// <summary>
        ///// 주차장정보를 유저명으로 가져온다
        ///// </summary>
        ///// <param name="pUserId"></param>
        ///// <returns></returns>
        //public static ParkInfo GetParkInfo(string pUserId)
        //{
        //    return mListParkInfo.Find(x => x.CurrentUserInfo.UserId == pUserId);
        //}
        /// <summary>
        /// 주차장정보를 가져온다
        /// </summary>
        /// <param name="pParkLevel"></param>
        /// <returns></returns>
        public static ParkInfo GetParkInfo(ParkInfo.ParkInfoSetting pParkingInfoSetting)
        {
            try
            {
                return mListParkInfo.Find(x => x.CurrentParkInfoSetting == pParkingInfoSetting);
            }
            catch
            {
                return null;
            }
        }
    }
}
