using System.Windows.Controls;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for PadesVerifyUserControl.xaml
    /// </summary>
    public partial class PadesVerifyUserControl : UserControl
    {
        public PadesVerifyUserControl()
        {
            InitializeComponent();
        }

        public void ClearPadesVerifyTextFields()
        {
            PadesVerifyFilePathText.Text = "";
            PadesVerifyOcspUrlText.Text = "";
            PadesVerifyCertPathText.Text = "";
            PadesVerifyReportTypeComboBoxText.SelectedIndex = 0;
            PadesVerifyDocDetachedMode.IsChecked = false;
            PadesVerifyVerificationReport.Text = "";
        }
    }
}
