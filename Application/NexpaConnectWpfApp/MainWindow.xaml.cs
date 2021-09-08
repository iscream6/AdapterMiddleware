using NexpaConnectWpfApp.Common;
using NpmAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.Themes;

namespace NexpaConnectWpfApp
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private NexPipe pipe;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.ApplyTheme("ExpressionDark");

            var tt = new AdpaterUC();

            this.AdapterBorder.Child = tt;
            tt.AdapterName = "TEST";

            pipe = new NexPipe();
            pipe.ShowTip += Pipe_ShowTip;
        }

        private void Pipe_ShowTip(int showSec, string title, string message)
        {
            throw new NotImplementedException();
        }
    }
}
