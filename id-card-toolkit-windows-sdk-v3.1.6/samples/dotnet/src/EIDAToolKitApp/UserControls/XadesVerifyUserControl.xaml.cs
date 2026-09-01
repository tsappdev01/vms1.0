using System.Windows.Controls;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for XadesVerifyUserControl.xaml
    /// </summary>
    public partial class XadesVerifyUserControl : UserControl
    {
        public XadesVerifyUserControl()
        {
            InitializeComponent();
        }

        public void ClearXadesVerifyTextFields()
        {
            XadesVerifyFilePathText.Text = "";
            XadesVerifyOcspUrlText.Text = "";
            XadesVerifyCertPathText.Text = "";
            XadesVerifyReportTypeComboBoxText.SelectedIndex = 0;
            XadesVerifyDocDetachedMode.IsChecked = false;
            XadesVerifyVerificationReport.Text = "";
        }
    }
}
