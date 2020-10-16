﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter
{
    interface IAdapter : IDisposable
    {
        IAdapter TargetAdapter { get; set; }

        bool IsRuning { get; }

        /// <summary>
        /// 초기화.... 
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Adapter 가동
        /// </summary>
        /// <returns></returns>
        bool StartAdapter();

        /// <summary>
        /// Adapter 중지
        /// </summary>
        /// <returns></returns>
        bool StopAdapter();

        /// <summary>
        /// Message 처리
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        void SendMessage(byte[] buffer, long offset, long size);

        /// <summary>
        /// Receive Test 용...
        /// </summary>
        /// <param name="buffer"></param>
        void TestReceive(byte[] buffer);
    }
}
