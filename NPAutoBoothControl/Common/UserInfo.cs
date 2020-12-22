using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class UserInfo
    {
        private string mUserId = string.Empty;
        public string UserId
        {
            set { mUserId = value; }
            get { return mUserId; }
        }

        private string mUserName = string.Empty;
        public string UserName
        {
            set { mUserName = value; }
            get { return mUserName; }
        }
        private string mUserPwd = string.Empty;
        public string UserPwd
        {
            set { mUserPwd = value; }
            get { return mUserPwd; }
        }
        private UserLevel mCurrentUserLevel = UserLevel.Worker;
        public UserLevel CurrentUserLevel
        {
            set { mCurrentUserLevel = value; }
            get { return mCurrentUserLevel; }
        }
        public enum UserLevel
        {
            /// <summary>
            /// 최대레벨 유저
            /// </summary>
            Manager = 0,
            /// <summary>
            /// 중간레벨 유저
            /// </summary>
            SubManager = 1,
            /// <summary>
            /// 근무자
            /// </summary>
            Worker = 2
        }
    }
}
