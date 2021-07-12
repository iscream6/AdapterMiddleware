using NexpaAdapterStandardLib;
using NpmAdapter.kakao;
using System;
using System.Collections.Generic;
using System.Text;

namespace NpmAdapter
{
    class KakaoPipe : AbstractPipe , IDisposable
    {
        public event IAdapter.ShowBallonTip ShowTip;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public override bool GeneratePipe()
        {
            if(NexpaConfig.GetInstance().ReadSuccess == false)
            {
                Log.WriteLog(LogType.Error, "KakaoPipe | GeneratePipe", "Failed to read nexpa.config file.");
                return false;
            }

            return true;
        }

        public override bool StartAdapter(AdapterType type)
        {
            throw new NotImplementedException();
        }

        public override bool StopAdapter(AdapterType type)
        {
            throw new NotImplementedException();
        }
    }
}
