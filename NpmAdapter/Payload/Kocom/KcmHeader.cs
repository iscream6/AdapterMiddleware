using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Payload
{
    class KcmHeader
    {
        //Default : 0x12345678
        private const int HeaderKey = 305419896;
        /// <summary>
        /// 메시지 타입
        /// </summary>
        private int _msgType; 
        /// <summary>
        /// Body Size
        /// </summary>
        private int _msgLength;
        /// <summary>
        /// 단말기 단지 위치
        /// </summary>
        private int _town;
        /// <summary>
        /// 단말기 동 위치
        /// </summary>
        private int _dong;
        /// <summary>
        /// 단말기 호 위치
        /// </summary>
        private int _ho;
        /// <summary>
        /// 예비 변수
        /// </summary>
        private int _reserved;
    }
}
