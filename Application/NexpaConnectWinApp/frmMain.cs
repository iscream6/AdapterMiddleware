using NexpaAdapterStandardLib;
using NpmAdapter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NexpaConnectWinApp
{
    public partial class frmMain : Form
    {
        #region Private Fields

        private NexPipe pipe;
        private bool isRunNexpa = false;
        private bool isRunHomeNet = false;
        private bool autoClear = false;
        private bool isShutdown = false;
        private static DateTime startDate;

        #endregion

        #region Constructor

        public frmMain()
        {
            //Form이 시작한 시간을 설정한다.
            startDate = DateTime.Now;

            InitializeComponent();
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;

            pipe = new NexPipe();
            pipe.ShowTip += Pipe_ShowTip;
            SystemStatus.Instance.StatusChanged += FrmMain_StatusChanged; ;
        }

        #endregion

        #region Main Controls Events

        private void frmMain_Load(object sender, EventArgs e)
        {
            //과거 로그를 지우자..
            if (SysConfig.Instance.LogLimit != null && SysConfig.Instance.LogLimit != "")
            {
                StdHelper.DeleteLogFiles(Application.StartupPath + "\\Log", int.Parse(SysConfig.Instance.LogLimit));
            }
            Initialize();
            isShutdown = false;
        }

        /// <summary>
        /// 초기화 버튼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInit_Click(object sender, EventArgs e)
        {
            bool status = pipe.GeneratePipe();
            if (status)
            {
                btnInit.Enabled = false;
                btnStartHomeNet.Enabled = true;
                btnStartNexpa.Enabled = true;
                lblSttInit.BackColor = Color.Lime;
                lblSttInit.ForeColor = Color.FromArgb(64, 64, 64);
                lblSttInit.Text = "성공";
                lblSttNexpa.Text = "대기";
                lblSttHomeNet.Text = "대기";
                btnInit.BackColor = Color.FromArgb(224, 224, 224, 224);
            }
            else
            {
                btnInit.Enabled = true;
                btnStartHomeNet.Enabled = false;
                btnStartNexpa.Enabled = false;

                lblSttNexpa.Text = "대기";
                lblSttHomeNet.Text = "대기";
            }
        }

        private void Pipe_ShowTip(int showSec, string title, string message)
        {
            notifyTray.ShowBalloonTip(showSec, title, message, ToolTipIcon.Info);
        }

        /// <summary>
        /// 넥스파 가동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartNexpa_Click(object sender, EventArgs e)
        {

            bool status = false;

            if (isRunNexpa) //가동중
            {
                isRunNexpa = false;
                status = pipe.StopAdapter(AdapterType.nexpa);
                btnStartNexpa.Text = "넥스파 가동";
            }
            else //정지중
            {
                isRunNexpa = true;
                status = pipe.StartAdapter(AdapterType.nexpa);
                btnStartNexpa.Text = "넥스파 정지";
            }

            if (status)
            {
                if (isRunNexpa) //가동중
                {
                    btnStartNexpa.Text = "넥스파 정지";
                    lblSttNexpa.BackColor = Color.Lime;
                    lblSttNexpa.Text = "성공";
                    lblSttNexpa.ForeColor = Color.FromArgb(64, 64, 64);
                }
                else //정지중
                {
                    btnStartNexpa.Text = "넥스파 가동";
                    lblSttNexpa.BackColor = Color.FromArgb(224, 224, 224);
                    lblSttNexpa.ForeColor = Color.FromArgb(64, 64, 64);
                    lblSttNexpa.Text = "대기";
                }
            }
            else
            {
                if (isRunNexpa)
                {
                    isRunHomeNet = false;
                    btnStartNexpa.Text = "넥스파 가동";
                    lblSttNexpa.BackColor = Color.Red;
                    lblSttNexpa.ForeColor = Color.WhiteSmoke;
                    lblSttNexpa.Text = "실패";
                }
            }
        }

        /// <summary>
        /// 홈넷 가동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartHomeNet_Click(object sender, EventArgs e)
        {
            bool status = false;
            try
            {
                if (isRunHomeNet) //가동중
                {
                    isRunHomeNet = false;
                    status = pipe.StopAdapter(AdapterType.homenet);
                }
                else //정지중
                {
                    isRunHomeNet = true;
                    status = pipe.StartAdapter(AdapterType.homenet);
                }

                FrmMain_StatusChanged(LogAdpType.HomeNet, $"홈넷 클릭 : {status}");

                if (status)
                {
                    if (isRunHomeNet) //가동중
                    {
                        btnStartHomeNet.Text = "홈넷 정지";
                        lblSttHomeNet.BackColor = Color.Lime;
                        lblSttHomeNet.Text = "성공";
                        lblSttHomeNet.ForeColor = Color.FromArgb(64, 64, 64);
                    }
                    else //정지중
                    {
                        btnStartHomeNet.Text = "홈넷 가동";
                        lblSttHomeNet.BackColor = Color.FromArgb(224, 224, 224);
                        lblSttHomeNet.ForeColor = Color.FromArgb(64, 64, 64);
                        lblSttHomeNet.Text = "대기";
                    }
                }
                else
                {
                    if (isRunHomeNet)
                    {
                        isRunHomeNet = false;
                        btnStartHomeNet.Text = "홈넷 가동";
                        lblSttHomeNet.BackColor = Color.Red;
                        lblSttHomeNet.ForeColor = Color.WhiteSmoke;
                        lblSttHomeNet.Text = "실패";
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        private void btnLogAutoClear_Click(object sender, EventArgs e)
        {
            if (autoClear)
            {
                autoClear = false;
                btnLogAutoClear.Text = "시작";
                timerLogClear.Stop();
            }
            else
            {
                autoClear = true;
                btnLogAutoClear.Text = "중지";

                int sec = 30; //30초;
                int.TryParse(txtClearSec.Text, out sec);

                timerLogClear.Interval = sec * 1000;
                timerLogClear.Start();
            }
        }

        private void btnLogClear_Click(object sender, EventArgs e)
        {
            try
            {
                txtSysLog?.Clear();
            }
            catch (Exception)
            {
            }
        }

        private void timerLogClear_Tick(object sender, EventArgs e)
        {
            try
            {
                txtSysLog?.Clear();
            }
            catch (Exception)
            {
            }
        }

        #endregion

        #region 초기화

        private void Initialize()
        {
            HomeNetAdapterType homAdapter = StdHelper.GetValueFromDescription<HomeNetAdapterType>(SysConfig.Instance.Sys_HomeNetAdapter);

            btnInit.Enabled = true;
            btnStartHomeNet.Enabled = false;
            btnStartNexpa.Enabled = false;
            lblSttInit.BackColor = Color.FromArgb(224, 224, 224);
            lblSttInit.ForeColor = Color.FromArgb(64, 64, 64);
            lblSttInit.Text = "대기";
            lblSttHomeNet.BackColor = Color.FromArgb(224, 224, 224);
            lblSttHomeNet.ForeColor = Color.FromArgb(64, 64, 64);
            lblSttHomeNet.Text = "";
            lblSttNexpa.BackColor = Color.FromArgb(224, 224, 224);
            lblSttNexpa.ForeColor = Color.FromArgb(64, 64, 64);
            lblSttNexpa.Text = "";

            if (homAdapter == HomeNetAdapterType.None)
            {
                lblSttHomeNet.Visible = false;
                btnStartHomeNet.Visible = false;
                label2.Visible = false; // ▶ 표시도 제거
            }

            //테스트 모드 여부
            if (SysConfig.Instance.Sys_Option.GetValueToUpper("TestMode") == "Y")
            {
                grbTest.Visible = true;
                this.Height = 650;
            }
            else
            {
                grbTest.Visible = false;
                this.Height = 650 - 177;
            }

            //자동시작 여부
            if (SysConfig.Instance.Sys_Option.GetValueToUpper("AutoStart") == "Y")
            {
                btnInit.PerformClick();
                Thread.Sleep(1000);
                btnStartNexpa.PerformClick();
                Thread.Sleep(1000);

                if (homAdapter != HomeNetAdapterType.None)
                {
                    btnStartHomeNet.PerformClick();
                }

                txtClearSec.Text = "90";
                btnLogAutoClear.PerformClick();
            }

            if (SysConfig.Instance.Sys_Option.GetValueToUpper("AutoDaeth") == "Y")
            {
                timerDeath.Start();
            }
        }

        #endregion

        #region Log Display 처리

        delegate void AppendLogText(Control ctl, string log);

        private void SafeAppendLogText(Control ctl, string log)
        {
            if (ctl.InvokeRequired)
            {
                ctl.Invoke(new AppendLogText(SafeAppendLogText), ctl, log);
            }
            else
            {
                ctl.Text += log;
            }
        }
        delegate void CrossThreadSafetySetText(Control ctl, String text);


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
                SafeAppendLogText(txtSysLog, builder.ToString() + "\r");
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate () { txtSysLog.SelectionStart = txtSysLog.Text.Length; }));
                }
                else
                {
                    txtSysLog.SelectionStart = txtSysLog.Text.Length;
                }
            }
            catch (Exception)
            {

            }
        }

        #endregion

        #region 창닫기, 프로그램 종료

        private void frmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isShutdown) pipe.Dispose();
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isShutdown)
            {
                var process = Process.GetProcessesByName("NexpaConnectWinApp");
                timerDeath.Stop();
                timerDeath.Dispose();
                e.Cancel = false;
                foreach (var item in process)
                {
                    item.Kill();
                }
            }
            else
            {
                e.Cancel = true;
                this.Visible = false; //화면을 닫지 않고 숨긴다.
            }
        }


        private void timerDeath_Tick(object sender, EventArgs e)
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
                this.Dispose();
            }
        }

        #endregion

        #region Form 마우스로 이동
        //===================== Form 마우스로 이동 =====================

        private bool tagMove;
        private int valX, valY;

        private void pnlTop_MouseDown(object sender, MouseEventArgs e)
        {
            tagMove = true;
            valX = e.X;
            valY = e.Y;
        }

        private void pnlTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (tagMove)
            {
                this.SetDesktopLocation(MousePosition.X - valX, MousePosition.Y - valY);
            }
        }

        private void pnlTop_MouseUp(object sender, MouseEventArgs e)
        {
            tagMove = false;
        }

        //===================== Form 마우스로 이동 완료 =====================
        #endregion

        #region Form 작업표시줄 이동, 닫기

        //===================== Form 작업표시줄 이동, 닫기 =====================
        private enum ImageList
        {
            UnderLine,
            UnderLine_Over,
            X,
            X_Over
        }

        private void picBtnHide_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void picBtnHide_MouseEnter(object sender, EventArgs e)
        {
            picBtnHide.Image = imgLstButton.Images[(int)ImageList.UnderLine_Over];
        }

        private void picBtnHide_MouseLeave(object sender, EventArgs e)
        {
            picBtnHide.Image = imgLstButton.Images[(int)ImageList.UnderLine];
        }

        private void picBtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void picBtnClose_MouseEnter(object sender, EventArgs e)
        {
            picBtnClose.Image = imgLstButton.Images[(int)ImageList.X_Over];
        }

        private void picBtnClose_MouseLeave(object sender, EventArgs e)
        {
            picBtnClose.Image = imgLstButton.Images[(int)ImageList.X];
        }

        //===================== Form 작업표시줄 이동, 닫기 완료 =====================

        #endregion

        #region ContextMenu Events

        private void 설정ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (frmOption option = new frmOption())
                {
                    if (option.ShowDialog() == DialogResult.OK)
                    {
                        if (MessageBox.Show("You need to restart the program to apply the settings. " +
                            "Do you want to restart?", "You neet restart!", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(Application.ExecutablePath); // to start new instance of application
                            isShutdown = true;
                            this.Close();
                            this.Dispose();
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private void mnuOpenLogFolder_Click(object sender, EventArgs e)
        {
            string sLogPath = Application.StartupPath + "\\Log";
            if (Directory.Exists(sLogPath))
            {
                Process.Start(sLogPath);
            }
        }

        private void 프로그램종료XToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                isShutdown = true;
                this.Close();
                this.Dispose();
            }
            catch (Exception)
            {

            }
        }

        private void mnuShutdown_Click(object sender, EventArgs e)
        {
            isShutdown = true;
            this.Close();
            this.Dispose();
        }

        private void mnuActive_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
        }

        #endregion

        #region Test

        private void btnSendTest_Click(object sender, EventArgs e)
        {
            //{"command":"alert_incar","data":[{"dong":"811","ho":"101","car_number":"18��2876","date_time":"20200831153703"}]}
            //{\"command\":\"status_ack\",\"data\":[{\"code\":\"testcode\",\"message\":\"testmessage\"}]}
            //{"command":"alert_incar","data":[{"dong":"108","ho":"1203","car_number":"09서4666","date_time":"20200907150105"}]}
            //string dd = "{\"command\":\"alert_incar\",\"data\":[{\"dong\":\"108\",\"ho\":\"1203\",\"car_number\":\"09서4666\",\"date_time\":\"20200907150105\"}]}";
            string dd = "{\"command\":\"location_map\",\"data\":{\"dong\":\"108\",\"ho\":\"1203\",\"car_number\":\"97소1607\",\"date_time\":\"20200914095540\"}}";
            AdapterType type = AdapterType.none;
            Encoding encoding = null;
            if (rdoNexpa.Checked)
            {
                type = AdapterType.nexpa;
                encoding = SysConfig.Instance.HomeNet_Encoding;
            }
            else if (rdoHomeNet.Checked)
            {
                type = AdapterType.homenet;
                encoding = SysConfig.Instance.Nexpa_Encoding;
            }
            else
            {
                type = AdapterType.none;
            }
            pipe.TestSendMessage(type, encoding.GetBytes(txtMessage.Text));
        }

        private void btnReceiveTest_Click(object sender, EventArgs e)
        {
            List<byte> lst = new List<byte>();
            ////=======Status(Alive)======
            //lst.Add(0xE8);
            //lst.Add(0xE8);
            //lst.Add(0xA5);
            //lst.Add(0x71);
            //lst.Add(CalCheckSum(lst));
            //pipe.TestReceiveMessage(AdapterType.homenet, lst.ToArray());
            //=======Status(Alive)======

            //=======Status(Fail)======
            lst.Add(0xE8);
            lst.Add(0xE8);
            lst.Add(0xAA);
            lst.Add(0x52);
            lst.Add(0x00);
            lst.Add(0x09);
            lst.Add(0x08);
            lst.Add(0x04);
            lst.Add(0x01);
            lst.Add(0xFC);
            pipe.TestReceiveMessage(AdapterType.homenet, lst.ToArray());
            //=======Status(Alive)======

            //string test = "{\"command\":\"alert_incar\",\"data\":[{\"dong\":\"811\",\"ho\":\"101\",\"car_number\":\"11가1111\",\"date_time\":\"20200831153703\"}]}";
            //pipe.TestReceiveMessage(AdapterType.nexpa, Encoding.UTF8.GetBytes(test));

            //======= Status(Alive) ======
            //lst.Add(0xE9);
            //lst.Add(0xE9);
            //lst.Add(0xAD);
            //lst.Add(0x33);
            //lst.Add(0x00);
            //lst.Add(0x00);
            //lst.Add(0x00);
            //lst.Add(0x33);
            //lst.Add(0x33);
            //lst.Add(0x00);
            //lst.Add(0x00);
            //lst.Add(0x00);
            //lst.Add(0x33);
            //lst.Add(0xC1);
            //lst.Add(0xC1);
            //lst.Add(0xA5);
            //lst.Add(0x33);
            //lst.Add(0x96);
            //lst.Add(0xBB);
            //lst.Add(0xA4);
            //lst.Add(0x33);
            //lst.Add(0x2C);
            //lst.Add(0xD5);
            //lst.Add(0xA6);
            //lst.Add(0x71);
            //lst.Add(0x81);
            //lst.Add(0x56);
            //lst.Add(0xE8);
            //lst.Add(0xE8);
            //lst.Add(0xA5);
            //lst.Add(0x71);
            //lst.Add(0xD4);
            ////lst.Add(CalCheckSum(lst));
            //pipe.TestReceiveMessage(AdapterType.nexpa, lst.ToArray());
            //======= Status(Alive) ======
        }

        private byte CalCheckSum(List<byte> _PacketData)
        {
            Byte _CheckSumByte = 0x00;
            byte[] b = _PacketData.ToArray();

            for (int i = 0; i < b.Length; i++)
                _CheckSumByte ^= b[i];
            return _CheckSumByte;
        }

        private void btnCommaxTest_Click(object sender, EventArgs e)
        {
            if (TestTimer.Enabled)
            {
                TestTimer.Stop();
                btnCommaxTest.Text = "시작";
            }
            else
            {
                int sec = 30; //30초;
                int.TryParse(txtCommaxSec.Text, out sec);
                TestTimer.Interval = sec * 1000;
                TestTimer.Start();
                btnCommaxTest.Text = "정지";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string dong = txtDong.Text;
            string ho = txtHo.Text;
            string carNum = txtCarNum.Text;
            string type = cmbType.Text == "입차" ? "alert_incar" : "alert_outcar";
            string dd = "{\"command\":\"" + type + "\",\"data\":{\"dong\":\"" + dong + "\",\"ho\":\"" + ho + "\",\"car_number\":\"" + carNum + "\",\"date_time\":\"" + DateTime.Now.ToString("yyyyMMddHHmmss") + "\"}}";
            pipe.TestSendMessage(AdapterType.homenet, Encoding.UTF8.GetBytes(dd));
        }

        private void TestTimer_Tick(object sender, EventArgs e)
        {
            string dong = txtDong.Text;
            string ho = txtHo.Text;
            string carNum = txtCarNum.Text;
            string type = cmbType.Text == "입차" ? "alert_incar" : "alert_outcar";
            string dd = "{\"command\":\"" + type + "\",\"data\":{\"dong\":\"" + dong + "\",\"ho\":\"" + ho + "\",\"car_number\":\"" + carNum + "\",\"date_time\":\"" + DateTime.Now.ToString("yyyyMMddHHmmss") + "\"}}";
            pipe.TestSendMessage(AdapterType.homenet, Encoding.UTF8.GetBytes(dd));
        }

        private void btnInCarAck_Click(object sender, EventArgs e)
        {
            //BB AE 51 00 0B 01 01 0B 0B 08 1F 0F 25 72
            List<byte> lst = new List<byte>();
            //=======Status(Alive)======
            lst.Add(0xE8);
            lst.Add(0xE8);
            lst.Add(0xA9);
            lst.Add(0x51);
            lst.Add(0x00);
            lst.Add(0x0B);
            lst.Add(0x01);
            lst.Add(0x01);
            lst.Add(CalCheckSum(lst));
            pipe.TestReceiveMessage(AdapterType.homenet, lst.ToArray());
        }

        private void btnInCarSend_Click(object sender, EventArgs e)
        {
            string test = "{\"command\":\"alert_incar\",\"data\":[{\"dong\":\"811\",\"ho\":\"101\",\"car_number\":\"11가1111\",\"date_time\":\"20200831153703\"}]}";
            pipe.TestReceiveMessage(AdapterType.nexpa, Encoding.UTF8.GetBytes(test));
        }

        private void btnSttAlive_Click(object sender, EventArgs e)
        {
            List<byte> lst = new List<byte>();
            //=======Status(Alive)======
            lst.Add(0xE8);
            lst.Add(0xE8);
            lst.Add(0xA5);
            lst.Add(0x33);
            lst.Add(CalCheckSum(lst));
            pipe.TestReceiveMessage(AdapterType.homenet, lst.ToArray());
        }

        #endregion

    }
}
