using System.ComponentModel;
using System.Text;
using System.Windows.Controls;

namespace NexpaConnectWpfApp.UC
{
    /// <summary>
    /// NpmMessageBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class NpmMessageBox : UserControl, INotifyPropertyChanged
    {
        private StringBuilder strMsg = new StringBuilder();

        public event PropertyChangedEventHandler PropertyChanged;

        public NpmMessageBox()
        {
            InitializeComponent();
        }

        public string strMessage
        {
            get
            {
                return strMsg?.ToString();
            }
            set
            {
                strMsg.Append(value);
                OnPropertyChanged("strMessage");
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));

            srlLog.ScrollToBottom();
        }

        public void WriteMessage(string Message)
        {
            if (strMsg.Length == 0)
            {
                strMsg.Append(Message);
            }
            else
            {
                strMsg.Append("\r" + Message);
            }

            OnPropertyChanged("strMessage");
        }

        public void Clear()
        {
            strMsg.Clear();

            OnPropertyChanged("strMessage");
        }
    }
}
