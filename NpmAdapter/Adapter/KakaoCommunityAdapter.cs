using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter.Adapter
{
    class KakaoCommunityAdapter : IAdapter
    {
        private bool isRun = false;
        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => isRun;

        public event IAdapter.ShowBallonTip ShowTip;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool Initialize()
        {
            throw new NotImplementedException();
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            throw new NotImplementedException();
        }

        public bool StartAdapter()
        {
            throw new NotImplementedException();
        }

        public bool StopAdapter()
        {
            throw new NotImplementedException();
        }

        public void TestReceive(byte[] buffer)
        {
            throw new NotImplementedException();
        }
    }
}
