using System.Windows.Controls;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for CadesVerifyUserControl.xaml
    /// </summary>
    public partial class CadesVerifyUserControl : UserControl
    {
        public CadesVerifyUserControl()
        {
            InitializeComponent();
        }

        public void ClearCadesVerifyTextFields()
        {
            CadesVerifyFilePathText.Text = "";
            CadesVerifyOcspUrlText.Text = "";
            CadesVerifyCertPathText.Text = "";
            CadesVerifyReportTypeComboBoxText.SelectedIndex = 0;
            CadesVerifyVerificationReport.Text = "";
        }
    }
}
