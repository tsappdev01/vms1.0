using System.Windows;
using System.Windows.Controls;
using AE.EmiratesId.IdCard;

namespace EIDAToolkitModernApp.Views
{
    public partial class ToolkitUtilitiesView : UserControl
    {
        public ToolkitUtilitiesView()
        {
            InitializeComponent();
            LogStatusTextBlock.Text = Toolkit.IsLogSupported()
                ? "Toolkit log: supported by loaded DLL"
                : "Toolkit log: not supported (old DLL -- writes will no-op)";
        }

        private void WriteLogInfo_Click(object sender, RoutedEventArgs e)
        {
            Toolkit.Log(Toolkit.LOG_INFO,
                LogModuleTextBox.Text,
                LogMessageTextBox.Text);
        }

        private void WriteLogWarn_Click(object sender, RoutedEventArgs e)
        {
            Toolkit.Log(Toolkit.LOG_WARN,
                LogModuleTextBox.Text,
                LogMessageTextBox.Text);
        }

        private void WriteLogError_Click(object sender, RoutedEventArgs e)
        {
            Toolkit.Log(Toolkit.LOG_ERROR,
                LogModuleTextBox.Text,
                LogMessageTextBox.Text);
        }
    }
}
