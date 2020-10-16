namespace NexpaConnectWinApp
{
    partial class frmMain
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnInit = new System.Windows.Forms.Button();
            this.btnSendTest = new System.Windows.Forms.Button();
            this.btnReceiveTest = new System.Windows.Forms.Button();
            this.btnStartNexpa = new System.Windows.Forms.Button();
            this.btnStartHomeNet = new System.Windows.Forms.Button();
            this.grbSystem = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtClearSec = new System.Windows.Forms.TextBox();
            this.btnLogClear = new System.Windows.Forms.Button();
            this.btnLogAutoClear = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.txtSysLog = new System.Windows.Forms.RichTextBox();
            this.lblSttHomeNet = new System.Windows.Forms.Label();
            this.lblSttNexpa = new System.Windows.Forms.Label();
            this.lblSttInit = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.timerLogClear = new System.Windows.Forms.Timer(this.components);
            this.TestTimer = new System.Windows.Forms.Timer(this.components);
            this.btnCommaxTest = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtCommaxSec = new System.Windows.Forms.TextBox();
            this.txtDong = new System.Windows.Forms.TextBox();
            this.txtHo = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtCarNum = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.cmbType = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnSttAlive = new System.Windows.Forms.Button();
            this.btnInCarSend = new System.Windows.Forms.Button();
            this.btnInCarAck = new System.Windows.Forms.Button();
            this.pnlCmdTest = new System.Windows.Forms.Panel();
            this.rdoNexpa = new System.Windows.Forms.RadioButton();
            this.pnlSendMessageTest = new System.Windows.Forms.Panel();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.rdoHomeNet = new System.Windows.Forms.RadioButton();
            this.grbTest = new System.Windows.Forms.GroupBox();
            this.grbSystem.SuspendLayout();
            this.pnlCmdTest.SuspendLayout();
            this.pnlSendMessageTest.SuspendLayout();
            this.grbTest.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnInit
            // 
            this.btnInit.BackColor = System.Drawing.Color.White;
            this.btnInit.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnInit.Location = new System.Drawing.Point(6, 20);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(85, 23);
            this.btnInit.TabIndex = 0;
            this.btnInit.Text = "초기화";
            this.btnInit.UseVisualStyleBackColor = false;
            this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
            // 
            // btnSendTest
            // 
            this.btnSendTest.Location = new System.Drawing.Point(214, 3);
            this.btnSendTest.Name = "btnSendTest";
            this.btnSendTest.Size = new System.Drawing.Size(168, 23);
            this.btnSendTest.TabIndex = 1;
            this.btnSendTest.Text = "SendMessgae";
            this.btnSendTest.UseVisualStyleBackColor = true;
            this.btnSendTest.Click += new System.EventHandler(this.btnSendTest_Click);
            // 
            // btnReceiveTest
            // 
            this.btnReceiveTest.Location = new System.Drawing.Point(3, 119);
            this.btnReceiveTest.Name = "btnReceiveTest";
            this.btnReceiveTest.Size = new System.Drawing.Size(168, 23);
            this.btnReceiveTest.TabIndex = 2;
            this.btnReceiveTest.Text = "ReceiveMessge";
            this.btnReceiveTest.UseVisualStyleBackColor = true;
            this.btnReceiveTest.Click += new System.EventHandler(this.btnReceiveTest_Click);
            // 
            // btnStartNexpa
            // 
            this.btnStartNexpa.BackColor = System.Drawing.Color.White;
            this.btnStartNexpa.Enabled = false;
            this.btnStartNexpa.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnStartNexpa.Location = new System.Drawing.Point(125, 20);
            this.btnStartNexpa.Name = "btnStartNexpa";
            this.btnStartNexpa.Size = new System.Drawing.Size(85, 23);
            this.btnStartNexpa.TabIndex = 4;
            this.btnStartNexpa.Text = "넥스파 가동";
            this.btnStartNexpa.UseVisualStyleBackColor = false;
            this.btnStartNexpa.Click += new System.EventHandler(this.btnStartNexpa_Click);
            // 
            // btnStartHomeNet
            // 
            this.btnStartHomeNet.BackColor = System.Drawing.Color.White;
            this.btnStartHomeNet.Enabled = false;
            this.btnStartHomeNet.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnStartHomeNet.Location = new System.Drawing.Point(244, 20);
            this.btnStartHomeNet.Name = "btnStartHomeNet";
            this.btnStartHomeNet.Size = new System.Drawing.Size(85, 23);
            this.btnStartHomeNet.TabIndex = 5;
            this.btnStartHomeNet.Text = "홈넷 가동";
            this.btnStartHomeNet.UseVisualStyleBackColor = false;
            this.btnStartHomeNet.Click += new System.EventHandler(this.btnStartHomeNet_Click);
            // 
            // grbSystem
            // 
            this.grbSystem.Controls.Add(this.label4);
            this.grbSystem.Controls.Add(this.label3);
            this.grbSystem.Controls.Add(this.txtClearSec);
            this.grbSystem.Controls.Add(this.btnLogClear);
            this.grbSystem.Controls.Add(this.btnLogAutoClear);
            this.grbSystem.Controls.Add(this.label6);
            this.grbSystem.Controls.Add(this.txtSysLog);
            this.grbSystem.Controls.Add(this.lblSttHomeNet);
            this.grbSystem.Controls.Add(this.lblSttNexpa);
            this.grbSystem.Controls.Add(this.lblSttInit);
            this.grbSystem.Controls.Add(this.label2);
            this.grbSystem.Controls.Add(this.label1);
            this.grbSystem.Controls.Add(this.btnInit);
            this.grbSystem.Controls.Add(this.btnStartHomeNet);
            this.grbSystem.Controls.Add(this.btnStartNexpa);
            this.grbSystem.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.grbSystem.Location = new System.Drawing.Point(12, 12);
            this.grbSystem.Name = "grbSystem";
            this.grbSystem.Size = new System.Drawing.Size(824, 426);
            this.grbSystem.TabIndex = 6;
            this.grbSystem.TabStop = false;
            this.grbSystem.Text = "시스템";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(644, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 12);
            this.label4.TabIndex = 19;
            this.label4.Text = "Auto Clear :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(797, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(18, 12);
            this.label3.TabIndex = 18;
            this.label3.Text = "초";
            // 
            // txtClearSec
            // 
            this.txtClearSec.Location = new System.Drawing.Point(734, 103);
            this.txtClearSec.Name = "txtClearSec";
            this.txtClearSec.Size = new System.Drawing.Size(56, 21);
            this.txtClearSec.TabIndex = 17;
            // 
            // btnLogClear
            // 
            this.btnLogClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogClear.BackColor = System.Drawing.Color.White;
            this.btnLogClear.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnLogClear.Location = new System.Drawing.Point(646, 159);
            this.btnLogClear.Name = "btnLogClear";
            this.btnLogClear.Size = new System.Drawing.Size(174, 23);
            this.btnLogClear.TabIndex = 16;
            this.btnLogClear.Text = "수동 Clear";
            this.btnLogClear.UseVisualStyleBackColor = false;
            this.btnLogClear.Click += new System.EventHandler(this.btnLogClear_Click);
            // 
            // btnLogAutoClear
            // 
            this.btnLogAutoClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogAutoClear.BackColor = System.Drawing.Color.White;
            this.btnLogAutoClear.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.btnLogAutoClear.Location = new System.Drawing.Point(646, 130);
            this.btnLogAutoClear.Name = "btnLogAutoClear";
            this.btnLogAutoClear.Size = new System.Drawing.Size(174, 23);
            this.btnLogAutoClear.TabIndex = 15;
            this.btnLogAutoClear.Text = "시작";
            this.btnLogAutoClear.UseVisualStyleBackColor = false;
            this.btnLogAutoClear.Click += new System.EventHandler(this.btnLogAutoClear_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 12);
            this.label6.TabIndex = 14;
            this.label6.Text = "System Log";
            // 
            // txtSysLog
            // 
            this.txtSysLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSysLog.Font = new System.Drawing.Font("굴림", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.txtSysLog.Location = new System.Drawing.Point(6, 103);
            this.txtSysLog.Name = "txtSysLog";
            this.txtSysLog.ReadOnly = true;
            this.txtSysLog.Size = new System.Drawing.Size(632, 317);
            this.txtSysLog.TabIndex = 13;
            this.txtSysLog.Text = "";
            this.txtSysLog.WordWrap = false;
            // 
            // lblSttHomeNet
            // 
            this.lblSttHomeNet.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lblSttHomeNet.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSttHomeNet.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblSttHomeNet.Location = new System.Drawing.Point(244, 49);
            this.lblSttHomeNet.Name = "lblSttHomeNet";
            this.lblSttHomeNet.Size = new System.Drawing.Size(85, 23);
            this.lblSttHomeNet.TabIndex = 12;
            this.lblSttHomeNet.Text = "상태";
            this.lblSttHomeNet.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSttNexpa
            // 
            this.lblSttNexpa.BackColor = System.Drawing.Color.Red;
            this.lblSttNexpa.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSttNexpa.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.lblSttNexpa.Location = new System.Drawing.Point(125, 49);
            this.lblSttNexpa.Name = "lblSttNexpa";
            this.lblSttNexpa.Size = new System.Drawing.Size(85, 23);
            this.lblSttNexpa.TabIndex = 11;
            this.lblSttNexpa.Text = "상태";
            this.lblSttNexpa.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblSttInit
            // 
            this.lblSttInit.BackColor = System.Drawing.Color.Lime;
            this.lblSttInit.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblSttInit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.lblSttInit.Location = new System.Drawing.Point(6, 49);
            this.lblSttInit.Name = "lblSttInit";
            this.lblSttInit.Size = new System.Drawing.Size(85, 23);
            this.lblSttInit.TabIndex = 10;
            this.lblSttInit.Text = "상태";
            this.lblSttInit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label2.ForeColor = System.Drawing.Color.Blue;
            this.label2.Location = new System.Drawing.Point(213, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 23);
            this.label2.TabIndex = 9;
            this.label2.Text = "▶";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.ForeColor = System.Drawing.Color.Blue;
            this.label1.Location = new System.Drawing.Point(94, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(28, 23);
            this.label1.TabIndex = 8;
            this.label1.Text = "▶";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // timerLogClear
            // 
            this.timerLogClear.Interval = 10000;
            this.timerLogClear.Tick += new System.EventHandler(this.timerLogClear_Tick);
            // 
            // TestTimer
            // 
            this.TestTimer.Interval = 10000;
            this.TestTimer.Tick += new System.EventHandler(this.TestTimer_Tick);
            // 
            // btnCommaxTest
            // 
            this.btnCommaxTest.Location = new System.Drawing.Point(3, 3);
            this.btnCommaxTest.Name = "btnCommaxTest";
            this.btnCommaxTest.Size = new System.Drawing.Size(122, 23);
            this.btnCommaxTest.TabIndex = 20;
            this.btnCommaxTest.Text = "시작";
            this.btnCommaxTest.UseVisualStyleBackColor = true;
            this.btnCommaxTest.Click += new System.EventHandler(this.btnCommaxTest_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(194, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(18, 12);
            this.label5.TabIndex = 22;
            this.label5.Text = "초";
            // 
            // txtCommaxSec
            // 
            this.txtCommaxSec.Location = new System.Drawing.Point(131, 5);
            this.txtCommaxSec.Name = "txtCommaxSec";
            this.txtCommaxSec.Size = new System.Drawing.Size(56, 21);
            this.txtCommaxSec.TabIndex = 21;
            // 
            // txtDong
            // 
            this.txtDong.Location = new System.Drawing.Point(54, 62);
            this.txtDong.Name = "txtDong";
            this.txtDong.Size = new System.Drawing.Size(82, 21);
            this.txtDong.TabIndex = 23;
            // 
            // txtHo
            // 
            this.txtHo.Location = new System.Drawing.Point(54, 89);
            this.txtHo.Name = "txtHo";
            this.txtHo.Size = new System.Drawing.Size(82, 21);
            this.txtHo.TabIndex = 24;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 66);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(18, 12);
            this.label7.TabIndex = 25;
            this.label7.Text = "동";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(5, 93);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(18, 12);
            this.label8.TabIndex = 26;
            this.label8.Text = "호";
            // 
            // txtCarNum
            // 
            this.txtCarNum.Location = new System.Drawing.Point(54, 116);
            this.txtCarNum.Name = "txtCarNum";
            this.txtCarNum.Size = new System.Drawing.Size(82, 21);
            this.txtCarNum.TabIndex = 27;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(3, 120);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(31, 12);
            this.label9.TabIndex = 28;
            this.label9.Text = "차번";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(5, 39);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(31, 12);
            this.label10.TabIndex = 29;
            this.label10.Text = "타입";
            // 
            // cmbType
            // 
            this.cmbType.FormattingEnabled = true;
            this.cmbType.Items.AddRange(new object[] {
            "입차",
            "출차"});
            this.cmbType.Location = new System.Drawing.Point(54, 34);
            this.cmbType.Name = "cmbType";
            this.cmbType.Size = new System.Drawing.Size(82, 20);
            this.cmbType.TabIndex = 30;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(227, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(168, 23);
            this.button1.TabIndex = 20;
            this.button1.Text = "1번만 보내기";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnSttAlive
            // 
            this.btnSttAlive.Location = new System.Drawing.Point(284, 39);
            this.btnSttAlive.Name = "btnSttAlive";
            this.btnSttAlive.Size = new System.Drawing.Size(89, 23);
            this.btnSttAlive.TabIndex = 31;
            this.btnSttAlive.Text = "StatusAlive";
            this.btnSttAlive.UseVisualStyleBackColor = true;
            this.btnSttAlive.Click += new System.EventHandler(this.btnSttAlive_Click);
            // 
            // btnInCarSend
            // 
            this.btnInCarSend.Location = new System.Drawing.Point(284, 68);
            this.btnInCarSend.Name = "btnInCarSend";
            this.btnInCarSend.Size = new System.Drawing.Size(89, 23);
            this.btnInCarSend.TabIndex = 32;
            this.btnInCarSend.Text = "InCarSend";
            this.btnInCarSend.UseVisualStyleBackColor = true;
            this.btnInCarSend.Click += new System.EventHandler(this.btnInCarSend_Click);
            // 
            // btnInCarAck
            // 
            this.btnInCarAck.Location = new System.Drawing.Point(284, 97);
            this.btnInCarAck.Name = "btnInCarAck";
            this.btnInCarAck.Size = new System.Drawing.Size(89, 23);
            this.btnInCarAck.TabIndex = 33;
            this.btnInCarAck.Text = "IncarAck";
            this.btnInCarAck.UseVisualStyleBackColor = true;
            this.btnInCarAck.Click += new System.EventHandler(this.btnInCarAck_Click);
            // 
            // pnlCmdTest
            // 
            this.pnlCmdTest.Controls.Add(this.btnCommaxTest);
            this.pnlCmdTest.Controls.Add(this.btnInCarAck);
            this.pnlCmdTest.Controls.Add(this.txtCommaxSec);
            this.pnlCmdTest.Controls.Add(this.btnInCarSend);
            this.pnlCmdTest.Controls.Add(this.label5);
            this.pnlCmdTest.Controls.Add(this.btnSttAlive);
            this.pnlCmdTest.Controls.Add(this.txtDong);
            this.pnlCmdTest.Controls.Add(this.button1);
            this.pnlCmdTest.Controls.Add(this.txtHo);
            this.pnlCmdTest.Controls.Add(this.cmbType);
            this.pnlCmdTest.Controls.Add(this.label7);
            this.pnlCmdTest.Controls.Add(this.label10);
            this.pnlCmdTest.Controls.Add(this.label8);
            this.pnlCmdTest.Controls.Add(this.label9);
            this.pnlCmdTest.Controls.Add(this.txtCarNum);
            this.pnlCmdTest.Location = new System.Drawing.Point(6, 20);
            this.pnlCmdTest.Name = "pnlCmdTest";
            this.pnlCmdTest.Size = new System.Drawing.Size(407, 145);
            this.pnlCmdTest.TabIndex = 34;
            // 
            // rdoNexpa
            // 
            this.rdoNexpa.AutoSize = true;
            this.rdoNexpa.Location = new System.Drawing.Point(3, 6);
            this.rdoNexpa.Name = "rdoNexpa";
            this.rdoNexpa.Size = new System.Drawing.Size(65, 16);
            this.rdoNexpa.TabIndex = 20;
            this.rdoNexpa.TabStop = true;
            this.rdoNexpa.Text = "Nexpa";
            this.rdoNexpa.UseVisualStyleBackColor = true;
            // 
            // pnlSendMessageTest
            // 
            this.pnlSendMessageTest.Controls.Add(this.txtMessage);
            this.pnlSendMessageTest.Controls.Add(this.rdoHomeNet);
            this.pnlSendMessageTest.Controls.Add(this.rdoNexpa);
            this.pnlSendMessageTest.Controls.Add(this.btnSendTest);
            this.pnlSendMessageTest.Controls.Add(this.btnReceiveTest);
            this.pnlSendMessageTest.Location = new System.Drawing.Point(427, 20);
            this.pnlSendMessageTest.Name = "pnlSendMessageTest";
            this.pnlSendMessageTest.Size = new System.Drawing.Size(388, 145);
            this.pnlSendMessageTest.TabIndex = 35;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(3, 28);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(380, 55);
            this.txtMessage.TabIndex = 22;
            // 
            // rdoHomeNet
            // 
            this.rdoHomeNet.AutoSize = true;
            this.rdoHomeNet.Location = new System.Drawing.Point(69, 7);
            this.rdoHomeNet.Name = "rdoHomeNet";
            this.rdoHomeNet.Size = new System.Drawing.Size(82, 16);
            this.rdoHomeNet.TabIndex = 21;
            this.rdoHomeNet.TabStop = true;
            this.rdoHomeNet.Text = "HomeNet";
            this.rdoHomeNet.UseVisualStyleBackColor = true;
            // 
            // grbTest
            // 
            this.grbTest.Controls.Add(this.pnlCmdTest);
            this.grbTest.Controls.Add(this.pnlSendMessageTest);
            this.grbTest.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.grbTest.Location = new System.Drawing.Point(12, 444);
            this.grbTest.Name = "grbTest";
            this.grbTest.Size = new System.Drawing.Size(824, 177);
            this.grbTest.TabIndex = 36;
            this.grbTest.TabStop = false;
            this.grbTest.Text = "Test";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(842, 626);
            this.Controls.Add(this.grbTest);
            this.Controls.Add(this.grbSystem);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "frmMain";
            this.Text = "NexPipe";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.grbSystem.ResumeLayout(false);
            this.grbSystem.PerformLayout();
            this.pnlCmdTest.ResumeLayout(false);
            this.pnlCmdTest.PerformLayout();
            this.pnlSendMessageTest.ResumeLayout(false);
            this.pnlSendMessageTest.PerformLayout();
            this.grbTest.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnInit;
        private System.Windows.Forms.Button btnSendTest;
        private System.Windows.Forms.Button btnReceiveTest;
        private System.Windows.Forms.Button btnStartNexpa;
        private System.Windows.Forms.Button btnStartHomeNet;
        private System.Windows.Forms.GroupBox grbSystem;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RichTextBox txtSysLog;
        private System.Windows.Forms.Label lblSttHomeNet;
        private System.Windows.Forms.Label lblSttNexpa;
        private System.Windows.Forms.Label lblSttInit;
        private System.Windows.Forms.Button btnLogClear;
        private System.Windows.Forms.Button btnLogAutoClear;
        private System.Windows.Forms.Timer timerLogClear;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtClearSec;
        private System.Windows.Forms.Button btnCommaxTest;
        private System.Windows.Forms.Timer TestTimer;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtCommaxSec;
        private System.Windows.Forms.TextBox txtDong;
        private System.Windows.Forms.TextBox txtHo;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtCarNum;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cmbType;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnSttAlive;
        private System.Windows.Forms.Button btnInCarSend;
        private System.Windows.Forms.Button btnInCarAck;
        private System.Windows.Forms.Panel pnlCmdTest;
        private System.Windows.Forms.RadioButton rdoNexpa;
        private System.Windows.Forms.Panel pnlSendMessageTest;
        private System.Windows.Forms.TextBox txtMessage;
        private System.Windows.Forms.RadioButton rdoHomeNet;
        private System.Windows.Forms.GroupBox grbTest;
    }
}

