using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace NpmCommon
{
    public class NpmThread : IDisposable
    {
        private Thread ProcessThread;
        private TimeSpan ProcessDelay;
        private ManualResetEventSlim ProcessEvent = new ManualResetEventSlim(false);
        ManualResetEvent _pauseFailProcessEvent = new ManualResetEvent(false);
        private delegate void ProcessSafeCallDelegate();

        public Action ThreadAction { get; set; }

        public NpmThread(string threadName, TimeSpan timeSpan)
        {
            ProcessEvent = new ManualResetEventSlim(false);
            _pauseFailProcessEvent = new ManualResetEvent(false);

            ProcessThread = new Thread(new ThreadStart(ProcessAction));
            ProcessThread.Name = threadName;
            ProcessDelay = timeSpan;
        }

        public bool Start()
        {
            try
            {
                if (ProcessThread.IsAlive)
                {
                    _pauseFailProcessEvent.Set();
                }
                else
                {
                    ProcessThread.Start();
                    _pauseFailProcessEvent.Set();
                }

                Log.WriteLog(LogType.Info, $"NpmThread | Start", $"Thread 시작", LogAdpType.Biz);

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"NpmThread | Start", $"{ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
            
        }

        public bool Stop()
        {
            try
            {
                _pauseFailProcessEvent.Reset();

                Log.WriteLog(LogType.Info, $"NpmThread | Start", $"Thread 멈춤", LogAdpType.Biz);

                return true;
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"NpmThread | Stop", $"{ex.Message}\r\n{ex.StackTrace}");
                return false;
            }
            
        }

        private void ProcessAction()
        {
            do
            {
                if (ProcessEvent.IsSet) return;

                try
                {
                    if (ThreadAction != null) //새 Access Token 을 발급 받는다.
                    {
                        ThreadAction();
                    }
                    else
                    {
                        Log.WriteLog(LogType.Error, $"NpmThread | ProcessAction", $"Thread Action 값이 Null 입니다.");
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, $"NpmThread | ProcessAction", $"{ex.Message}\r\n{ex.StackTrace}");
                }

                ProcessEvent.Wait(ProcessDelay);
            }
            while (_pauseFailProcessEvent.WaitOne());
        }

        public void Dispose()
        {
            ProcessEvent.Set();
            _pauseFailProcessEvent.Reset();
            
            ProcessThread.Abort();
            ProcessThread = null;
            ThreadAction = null;
        }
    }
}
