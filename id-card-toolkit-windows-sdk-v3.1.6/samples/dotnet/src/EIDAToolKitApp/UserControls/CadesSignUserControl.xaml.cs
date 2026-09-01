using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for CadesSignUserControl.xaml
    /// </summary>
    public partial class CadesSignUserControl : UserControl
    {
        public CadesSignUserControl()
        {
            InitializeComponent();
        }

        public void CadesSignSetToDefaultValues()
        {
            CadesSignCountryCodeText.Text = "uae";
            CadesSignStateOrProvinceText.Text = "AbuDhabi";
            CadesSignPostalCodeText.Text = "1234";
            CadesSignLoacalityText.Text = "uae";
            CadesSignStreetText.Text = "KhalifaCity";
        }

        public void ClearCadesSignTextFields()
        {
            CadesSignFilePathText.Text = "";
            CadesSignPinText.Clear();
            CadesSignTsaUrlText.Text = "";
            CadesSignOcspUrlText.Text = "";
            CadesSignCertPathText.Text = "";
            CadesSignSignatureLevelCombobox.SelectedIndex = 0;
            CadesSignPackageModeComboBox.SelectedIndex = 0;
            cadesSignOtherOptionsChkBx.IsChecked = false;
            CadesSignSignatureText.Text = "";
            CadesSignXmlResponse.Text = "";
            CadesSignSetToDefaultValues();
        }

        /// <summary>
        /// Set to default values of cades sign params fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadesSignSetToDefaultValuesButton_Click(object sender, RoutedEventArgs e)
        {
            CadesSignSetToDefaultValues();
        }

        /// <summary>
        /// Validate the input control without entering non-numeric values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                e.Handled = true;
                return;
            }

            Regex regex = new Regex("[^0-9]+");
            if (e.Handled = regex.IsMatch(e.Text))
            {
                MessageBox.Show("Must enter numueric values", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
