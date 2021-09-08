using System;

namespace NpmAdapter.Adapter
{
    /// <summary>
    /// 아파트너 + 현대통신
    /// </summary>
    class AptHdnMultier : IAdapter
    {
        public IAdapter TargetAdapter { get; set; }

        public bool IsRuning => throw new NotImplementedException();

        public event IAdapter.ShowBallonTip ShowTip;

        private IAdapter Apartner { get; set; }
        private IAdapter Hundai { get; set; }
        public string reqPid { get; set; }

        public void Dispose()
        {
            Apartner.Dispose();
            Hundai.Dispose();
        }

        public bool Initialize()
        {
            Apartner = new AptNrAdapter();
            Hundai = new HdnAdapter();

            bool bResult = Apartner.Initialize();
            bResult &= Hundai.Initialize();

            Apartner.TargetAdapter = this.TargetAdapter;
            Hundai.TargetAdapter = this.TargetAdapter;

            return bResult;
        }

        public void SendMessage(byte[] buffer, long offset, long size, string pid = null)
        {
            Apartner.SendMessage(buffer, offset, size, pid);
            Hundai.SendMessage(buffer, offset, size, pid);
        }

        public bool StartAdapter()
        {
            bool bResult = Apartner.StartAdapter();
            bResult &= Hundai.StartAdapter();

            return bResult;
        }

        public bool StopAdapter()
        {
            bool bResult = Apartner.StopAdapter();
            bResult &= Hundai.StopAdapter();

            return bResult;
        }

        public void TestReceive(byte[] buffer)
        {
            
        }
    }
}
