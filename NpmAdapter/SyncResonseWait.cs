using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NpmAdapter
{
    class SyncResonseWait
    {
        public bool bResponseSuccess = false;

        public async Task WaitTask(int second)
        {
            await Task.Run(() =>
            {
                int iSec = second * 100; //3초
                while (iSec > 0 && !bResponseSuccess)
                {
                    Thread.Sleep(10); //0.01초씩..쉰다...
                    iSec -= 1;
                }
            });
        }
    }
}
