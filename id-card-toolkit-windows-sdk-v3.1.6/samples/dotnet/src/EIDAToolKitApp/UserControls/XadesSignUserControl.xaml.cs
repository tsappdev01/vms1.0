using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for XadesSignUserControl.xaml
    /// </summary>
    public partial class XadesSignUserControl : UserControl
    {
        public XadesSignUserControl()
        {
            InitializeComponent();
        }

        public void XadesSignSetToDefaultValues()
        {
            XadesSignCountryCodeText.Text = "uae";
            XadesSignStateOrProvinceText.Text = "AbuDhabi";
            XadesSignPostalCodeText.Text = "1234";
            XadesSignLoacalityText.Text = "uae";
            XadesSignStreetText.Text = "KhalifaCity";
        }

        public void ClearXadesSignTextFields()
        {
            XadesSignFilePathText.Text = "";
            XadesSignPinText.Clear();
            XadesSignTsaUrlText.Text = "";
            XadesSignOcspUrlText.Text = "";
            XadesSignCertPathText.Text = "";
            XadesSignSignatureLevelComboBox.SelectedIndex = 0;
            XadesSignPackageModeComboBox.SelectedIndex = 0;
            XadesSignOtherOptionsChkBx.IsChecked = false;
            XadesSignSignatureText.Text = "";
            XadesSignXmlResponse.Text = "";
            XadesSignSetToDefaultValues();
        }

        /// <summary>
        /// Set to default values of xades sign params fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XadesSignSetToDefaultValuesButton_Click(object sender, RoutedEventArgs e)
        {
            XadesSignSetToDefaultValues();
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
