using NexpaConnectWpfApp.UC;
using NpmAdapter;
using NpmCommon;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace NexpaConnectWpfApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Tray 기능 전역변수

        private NotifyIcon notifyTray;
        private ContextMenuStrip contextMenuTray;
        private System.Windows.Forms.ToolStripMenuItem mnuShutdown;
        private System.Windows.Forms.ToolStripMenuItem mnuActive;
        private ToolStripSeparator toolStripSeparator1;
        private bool isShutdown = false;

        #endregion

        private NexPipe pipe;
        private static DateTime startDate;

        private NpmMessageBox LogMessageUC;
        private NpmMessageBox AdapterMessageUC;

        private Brush RunBrush;
        private Brush StopBrush;
        private Brush StatusBrush;

        private string StatusMsg;

        #region 자동 종료 전역 변수

        private Thread AuthoDeathThread;
        private TimeSpan waitForAuthoDeathProcess;
        private ManualResetEventSlim shutdownAuthoDeathEvent = new ManualResetEventSlim(false);
        private ManualResetEvent _pauseFailAutoDeathEvent = new ManualResetEvent(false);
        private delegate void AuthoDeathSafeCallDelegate();

        #endregion

        #region 자동 Log Clear 변수

        private Thread LogClearThread;
        private TimeSpan waitForLogClearProcess;
        private ManualResetEventSlim LogClearEvent = new ManualResetEventSlim(false);
        private ManualResetEvent _pauseFailLogClearEvent = new ManualResetEvent(false);
        private delegate void LogClearSafeCallDelegate();

        #endregion

        #region Binding Properties & Events

        public event PropertyChangedEventHandler PropertyChanged;

        public Brush StatusColor
        {
            get { return StatusBrush; }
            set
            {
                StatusBrush = value;
                OnPropertyChanged("StatusColor");
            }
        }

        public string StatusMessage
        {
            get { return StatusMsg; }
            set
            {
                StatusMsg = value;
                OnPropertyChanged("StatusMessage");
            }
        }

        public string StartDateTime
        {
            get { return startDate.ToString("yyyy-MM-dd HH:mm:ss"); }
            set
            {
                startDate = DateTime.ParseExact(value, "yyyyMMddHHmmss", null); ;
                OnPropertyChanged("StartDateTime");
            }
        }

        protected void OnPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            #region Tray 기능

            notifyTray = new NotifyIcon();
            contextMenuTray = new ContextMenuStrip();
            mnuActive = new ToolStripMenuItem();
            mnuShutdown = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();

            // 
            // contextMenuTray
            // 
            contextMenuTray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuActive,
            mnuShutdown,
            toolStripSeparator1});
            contextMenuTray.Name = "contextMenuTray";
            contextMenuTray.Size = new System.Drawing.Size(151, 76);
            // 
            // mnuActive
            // 
            this.mnuActive.Name = "mnuActive";
            this.mnuActive.Size = new System.Drawing.Size(150, 22);
            this.mnuActive.Text = "창 활성화";
            this.mnuActive.Click += new System.EventHandler(this.mnuActive_Click);
            // 
            // mnuShutdown
            // 
            this.mnuShutdown.Name = "mnuShutdown";
            this.mnuShutdown.Size = new System.Drawing.Size(150, 22);
            this.mnuShutdown.Text = "프로그램 종료";
            this.mnuShutdown.Click += new System.EventHandler(this.mnuShutdown_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(147, 6);
            // 
            // notifyTray
            // 
            this.notifyTray.ContextMenuStrip = this.contextMenuTray;
            System.Drawing.Bitmap b = NexpaConnectWpfApp.Properties.Resources.NPMB;
            IntPtr pIcon = b.GetHicon();
            System.Drawing.Icon ico = System.Drawing.Icon.FromHandle(pIcon);
            this.notifyTray.Icon = ico;
            ico.Dispose();
            this.notifyTray.Text = "NexPipe";
            this.notifyTray.Visible = true;

            #endregion

            StartDateTime = DateTime.Now.ToString("yyyyMMddHHmmss");

            pipe = new NexPipe();
            pipe.ShowTip += Pipe_ShowTip;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SysConfig.Instance.LogLimit != null && SysConfig.Instance.LogLimit != "")
            {
                StdHelper.DeleteLogFiles(AppDomain.CurrentDomain.BaseDirectory + "\\Log", int.Parse(SysConfig.Instance.LogLimit));
            }

            SystemStatus.Instance.StatusChanged += FrmMain_StatusChanged;

            InitializeControl();
            InitializeThread();
            LaunchNexPipe();

            isShutdown = false;
        }

        private void FrmMain_StatusChanged(LogAdpType adapterType, string statusMessage)
        {
            try
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(DateTime.Now.ToString("yy-MM-dd hh:mm:ss "));
                if (adapterType == LogAdpType.none)
                {
                    builder.Append($"[Error] ");
                }
                else
                {
                    builder.Append($"[{adapterType.ToString()}] ");
                }
                builder.Append(statusMessage);
                LogMessageUC.WriteMessage(builder.ToString());
            }
            catch (Exception)
            {

            }
        }

        private bool LaunchNexPipe()
        {
            bool status = pipe.GeneratePipe();
            pipe.StartAdapter(AdapterType.nexpa);
            pipe.StartAdapter(AdapterType.homenet);

            if(status)
            {
                StatusColor = RunBrush;
                StatusMessage = "NexPipe System succeeded.";
            }
            else
            {
                StatusColor = StopBrush;
                StatusMessage = "Failed to run NexPipe System.";
            }

            return status;
        }

        private void InitializeThread()
        {
            AuthoDeathThread = new Thread(new ThreadStart(ShutDownAction));
            AuthoDeathThread.Name = "Auto Shutdonw";
            waitForAuthoDeathProcess = TimeSpan.FromSeconds(1); //1초

            if (SysConfig.Instance.Sys_Option.GetValueToUpper("AutoDaeth") == "Y")
            {
                try
                {
                    if (AuthoDeathThread.IsAlive)
                    {
                        _pauseFailAutoDeathEvent.Set();
                    }
                    else
                    {
                        AuthoDeathThread.Start();
                        _pauseFailAutoDeathEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    Log.WriteLog(LogType.Error, $"WPFWindow | InitializeThread", $"{ex.StackTrace}");
                }
            }

            LogClearThread = new Thread(new ThreadStart(LogClearAction));
            LogClearThread.Name = "Log Clear";
            waitForLogClearProcess = TimeSpan.FromMinutes(5); //5분

            try
            {
                if (LogClearThread.IsAlive)
                {
                    _pauseFailLogClearEvent.Set();
                }
                else
                {
                    LogClearThread.Start();
                    _pauseFailLogClearEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(LogType.Error, $"WPFWindow | InitializeThread", $"{ex.StackTrace}");
            }
        }

        private void LogClearAction()
        {
            do
            {
                if (LogClearEvent.IsSet) return;
                {
                    try
                    {
                        LogMessageUC.Clear();
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogType.Error, $"WPFWindow | LogClearAction", $"{ex.StackTrace}");
                    }
                }

                LogClearEvent.Wait(waitForLogClearProcess);
            } 
            while (_pauseFailLogClearEvent.WaitOne());
            
        }

        private void ShutDownAction()
        {
            do
            {
                if (shutdownAuthoDeathEvent.IsSet) return;
                {
                    try
                    {
                        //새벽 4시가 되면...스스로 죽는다...
                        string time = DateTime.Now.ToString("yyyyMMddHHmmss");
                        string compareTime = startDate.ToString("yyyyMMddHHmmss");

                        if (time.Substring(0, 8) == compareTime.Substring(0, 8)) return;

                        if (time.Substring(8, 4) == "0401")
                        {
                            Log.WriteLog(LogType.Info, $"frmMain | timerDeath_Tick", $"프로그램 자동 종료 : {time}", LogAdpType.Nexpa);

                            //죽자
                            isShutdown = true;
                            this.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLog(LogType.Error, $"WPFWindow | ShutDownAction", $"{ex.StackTrace}");
                    }
                }

                shutdownAuthoDeathEvent.Wait(waitForAuthoDeathProcess);
            }
            while (_pauseFailAutoDeathEvent.WaitOne());
        }

        /// <summary>
        /// User Control 초기화
        /// </summary>
        /// <returns></returns>
        private bool InitializeControl()
        {
            try
            {
                LogMessageUC = new NpmMessageBox();
                AdapterMessageUC = new NpmMessageBox();
                LogBorder.Child = LogMessageUC;
                AdapterBorder.Child = AdapterMessageUC;

                RunBrush = Brushes.LimeGreen;
                StopBrush = Brushes.OrangeRed;

                StatusColor = StopBrush;
                StatusMessage = "Run";
                //startDate
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private void Pipe_ShowTip(int showSec, string title, string message)
        {
            notifyTray.ShowBalloonTip(showSec, title, message, ToolTipIcon.Info);
        }

        #region Tray 기능 Events

        private void mnuShutdown_Click(object sender, EventArgs e)
        {
            isShutdown = true;
            this.Close();
        }

        private void mnuActive_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Show();
            this.WindowState = WindowState.Normal;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (isShutdown) pipe?.Dispose();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (isShutdown)
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
                this.Hide();
            }
        }

        #endregion

    }
}
