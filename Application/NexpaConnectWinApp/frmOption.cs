using NexpaAdapterStandardLib;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace NexpaConnectWinApp
{
    public partial class frmOption : Form
    {
        public frmOption()
        {
            InitializeComponent();
        }

        private void frmOption_Load(object sender, EventArgs e)
        {
            InitializeCtl();

            BindIniData();
        }

        private void InitializeCtl()
        {
            foreach (var field in typeof(NexpaAdapterType).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    ComboboxItem item = new ComboboxItem() { Text = field.GetValue(null).ToString(), Value = attribute.Description };
                    cmbNxpAdapter.Items.Add(item);
                }
            }

            foreach (var field in typeof(HomeNetAdapterType).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    ComboboxItem item = new ComboboxItem() { Text = field.GetValue(null).ToString(), Value = attribute.Description };
                    cmdHomAdpater.Items.Add(item);
                }
            }

            foreach (var field in typeof(EncoderType).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    ComboboxItem item = new ComboboxItem() { Text = field.GetValue(null).ToString(), Value = attribute.Description };
                    cmbNxpEncoder.Items.Add(item);
                    cmbHomEncoder.Items.Add(item);
                }
            }

            lsvNxpOption.Columns.Add("Key", 150);
            lsvNxpOption.Columns.Add("Value", 90);

            lblOptionKey.Text = "";
            txtOptionValue.Text = "";
        }

        private void BindIniData()
        {
            //Nexpa Adapter
            {
                var item = cmbNxpAdapter.Items.Cast<ComboboxItem>().Where(p => p.Value == SysConfig.Instance.Sys_NexpaAdapter);
                if (item != null && item.Count() > 0)
                    cmbNxpAdapter.SelectedItem = item.First();
            }

            //HomeNet Adapter
            {
                var item = cmdHomAdpater.Items.Cast<ComboboxItem>().Where(p => p.Value == SysConfig.Instance.Sys_HomeNetAdapter);
                if (item != null && item.Count() > 0)
                    cmdHomAdpater.SelectedItem = item.First();
            }

            //Nexpa
            {
                //Nexpa Tcp Port
                txtNxpTPort.Text = SysConfig.Instance.Nexpa_TcpPort;
                //Nexpa Web Port
                txtNxpWPort.Text = SysConfig.Instance.Nexpa_WebPort;
                //Nexpa 유도 ID
                txtNxpUWIP.Text = SysConfig.Instance.Nexpa_UWebIP;
                //Nexpa 유도 Port
                txtNxpUWPort.Text = SysConfig.Instance.Nexpa_UWebPort;

                string[] dataSourcies = SysConfig.Instance.DataSource.Split(',');
                if(dataSourcies != null && dataSourcies.Length >= 2)
                {
                    txtNxpDBIP.Text = dataSourcies[0];
                    txtNxpDBPort.Text = dataSourcies[1];
                }

                txtNxpDBName.Text = SysConfig.Instance.InitialCatalog;
                txtNxpDBID.Text = SysConfig.Instance.UserID;
                txtNxpDBPW.Text = SysConfig.Instance.Password;
                
                //Nexpa Encoder
                {
                    var item = cmbNxpEncoder.Items.Cast<ComboboxItem>().Where(p => p.Value == SysConfig.Instance.Nexpa_EncodCd);
                    if (item != null && item.Count() > 0)
                        cmbNxpEncoder.SelectedItem = item.First();
                }
                //Option..
                {
                    foreach (var option in SysConfig.Instance.Sys_Option)
                    {
                        ListViewItem item = new ListViewItem(option.Key);
                        item.SubItems.Add(option.Value);
                        lsvNxpOption.Items.Add(item);
                    }
                }
            }

            //HomeNet
            {
                txtHomBaudRate.Text = SysConfig.Instance.HS_BaudRate;
                txtHomPortName.Text = SysConfig.Instance.HS_PortName;
                txtHomParity.Text = SysConfig.Instance.HS_Parity;
                txtHomWPort.Text = SysConfig.Instance.HW_Port;
                txtHomSvrIP.Text = SysConfig.Instance.HT_IP;
                txtHomSvrPort.Text = SysConfig.Instance.HT_Port;
                txtHomTPort.Text = SysConfig.Instance.HT_MyPort;
                var item = cmbHomEncoder.Items.Cast<ComboboxItem>().Where(p => p.Value == SysConfig.Instance.HomeNet_EncodCd);
                if (item != null && item.Count() > 0)
                    cmbHomEncoder.SelectedItem = item.First();
                txtHomDomain.Text = SysConfig.Instance.HW_Domain;
                txtHomID.Text = SysConfig.Instance.HC_Id;
                txtHomPw.Text = SysConfig.Instance.HC_Pw;
                txtHomParkID.Text = SysConfig.Instance.ParkId;
                txtHomAuthToken.Text = SysConfig.Instance.AuthToken;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            bool isChange = false;
            {
                //=================Sys Config=================
                if (cmbNxpAdapter.SelectedItem != null)
                {
                    string key = (string)cmbNxpAdapter.Tag;
                    string value = ((ComboboxItem)cmbNxpAdapter.SelectedItem).Value;

                    if (value != SysConfig.Instance.Sys_NexpaAdapter)
                    {
                        SysConfig.Instance.WriteConfig(SysConfig.Sections.SysConfig, key, value);
                        isChange = true;
                    }
                }

                if (cmdHomAdpater.SelectedItem != null)
                {
                    string key = (string)cmdHomAdpater.Tag;
                    string value = ((ComboboxItem)cmdHomAdpater.SelectedItem).Value;

                    if (value != SysConfig.Instance.Sys_HomeNetAdapter)
                    {
                        SysConfig.Instance.WriteConfig(SysConfig.Sections.SysConfig, key, value);
                        isChange = true;
                    }
                }
                //=================Sys Config 완료=================

                //=================Nexpa Config=================
                if (txtNxpTPort.Text != SysConfig.Instance.Nexpa_TcpPort)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpTPort.Tag, txtNxpTPort.Text);
                    isChange = true;
                }

                if (txtNxpWPort.Text != SysConfig.Instance.Nexpa_WebPort)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpWPort.Tag, txtNxpWPort.Text);
                    isChange = true;
                }

                if (txtNxpUWIP.Text != SysConfig.Instance.Nexpa_UWebIP)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpUWIP.Tag, txtNxpUWIP.Text);
                    isChange = true;
                }

                if (txtNxpUWPort.Text != SysConfig.Instance.Nexpa_UWebPort)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpUWPort.Tag, txtNxpUWPort.Text);
                    isChange = true;
                }

                if (cmbNxpEncoder.SelectedItem != null)
                {
                    string key = (string)cmbNxpEncoder.Tag;
                    string value = ((ComboboxItem)cmbNxpEncoder.SelectedItem).Value;

                    if (value != SysConfig.Instance.Nexpa_EncodCd)
                    {
                        SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, key, value);
                        isChange = true;
                    }
                }

                string dataSource = $"{txtNxpDBIP.Text},{txtNxpDBPort.Text}";
                if(dataSource != SysConfig.Instance.DataSource)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpDBIP.Tag, txtNxpDBIP.Text);
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpDBPort.Tag, txtNxpDBPort.Text);
                    isChange = true;
                }

                if (txtNxpDBName.Text != SysConfig.Instance.InitialCatalog)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpDBName.Tag, txtNxpDBName.Text);
                    isChange = true;
                }
                if (txtNxpDBID.Text != SysConfig.Instance.UserID)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpDBID.Tag, txtNxpDBID.Text);
                    isChange = true;
                }
                if (txtNxpDBPW.Text != SysConfig.Instance.Password)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.NexpaConfig, (string)txtNxpDBPW.Tag, txtNxpDBPW.Text);
                    isChange = true;
                }

                StringBuilder strOption = new StringBuilder();
                foreach (ListViewItem item in lsvNxpOption.Items)
                {
                    
                }
                //=================Nexpa Config 완료=================

                //=================HomeNet Config=================
                if (txtHomBaudRate.Text != SysConfig.Instance.HS_BaudRate)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomBaudRate.Tag, txtHomBaudRate.Text);
                    isChange = true;
                }

                if (txtHomPortName.Text != SysConfig.Instance.HS_PortName)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomPortName.Tag, txtHomPortName.Text);
                    isChange = true;
                }

                if (txtHomParity.Text != SysConfig.Instance.HS_Parity)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomParity.Tag, txtHomParity.Text);
                    isChange = true;
                }

                if (txtHomWPort.Text != SysConfig.Instance.HW_Port)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomWPort.Tag, txtHomWPort.Text);
                    isChange = true;
                }

                if (txtHomSvrIP.Text != SysConfig.Instance.HT_IP)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomSvrIP.Tag, txtHomSvrIP.Text);
                    isChange = true;
                }

                if (txtHomSvrPort.Text != SysConfig.Instance.HT_Port)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomSvrPort.Tag, txtHomSvrPort.Text);
                    isChange = true;
                }

                if (txtHomTPort.Text != SysConfig.Instance.HT_MyPort)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomTPort.Tag, txtHomTPort.Text);
                    isChange = true;
                }

                if (txtHomDomain.Text != SysConfig.Instance.HW_Domain)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomDomain.Tag, txtHomDomain.Text);
                    isChange = true;
                }

                if (txtHomID.Text != SysConfig.Instance.HC_Id)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomID.Tag, txtHomID.Text);
                    isChange = true;
                }

                if (txtHomPw.Text != SysConfig.Instance.HC_Pw)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, (string)txtHomPw.Tag, txtHomPw.Text);
                    isChange = true;
                }

                if (cmbHomEncoder.SelectedItem != null)
                {
                    string key = (string)cmbHomEncoder.Tag;
                    string value = ((ComboboxItem)cmbHomEncoder.SelectedItem).Value;

                    if (value != SysConfig.Instance.HomeNet_EncodCd)
                    {
                        SysConfig.Instance.WriteConfig(SysConfig.Sections.HomeNetConfig, key, value);
                        isChange = true;
                    }
                }
                //=================HomeNet Config 완료=================

                //=================Etc Config =================
                if (txtHomParkID.Text != SysConfig.Instance.ParkId)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.EtcConfig, (string)txtHomParkID.Tag, txtHomParkID.Text);
                    isChange = true;
                }

                if (txtHomAuthToken.Text != SysConfig.Instance.AuthToken)
                {
                    SysConfig.Instance.WriteConfig(SysConfig.Sections.EtcConfig, (string)txtHomAuthToken.Tag, txtHomAuthToken.Text);
                    isChange = true;
                }

                //=================HomeNet Config 완료=================
            }

            if(isChange == true) this.DialogResult = DialogResult.OK;
            else this.DialogResult = DialogResult.Cancel;

        }

        private ListViewItem currentItem;

        private void lsvNxpOption_MouseDown(object sender, MouseEventArgs e)
        {
            currentItem = null;
            currentItem = lsvNxpOption.GetItemAt(e.X, e.Y);
            if(currentItem != null)
            {
                lblOptionKey.Text = currentItem.Text;
                txtOptionValue.Text = currentItem.SubItems[1].Text;
            }
            else
            {
                lblOptionKey.Text = "";
                txtOptionValue.Text = "";
            }
        }

        private void btnOptionApply_Click(object sender, EventArgs e)
        {
            if (currentItem == null || txtOptionValue.Text == currentItem.SubItems[0].Text) return;
            else
            {
                currentItem.SubItems[1].Text = txtOptionValue.Text;
            }
        }
    }

    internal class ComboboxItem
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }
}
