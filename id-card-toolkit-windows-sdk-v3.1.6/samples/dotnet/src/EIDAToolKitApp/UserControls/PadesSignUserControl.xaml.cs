using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for PadesSignUserControl.xaml
    /// </summary>
    public partial class PadesSignUserControl : UserControl
    {
        public PadesSignUserControl()
        {
            InitializeComponent();
        }

        public void PadesSignSetToDefaultValues()
        {
            PadesSignCountryCodeText.Text = "uae";
            PadesSignStateOrProvinceText.Text = "AbuDhabi";
            PadesSignPostalCodeText.Text = "1234";
            PadesSignLocalityText.Text = "uae";
            PadesSignStreetText.Text = "KhalifaCity";
            PadesSignSignReasonText.Text = "testing pades signature";
            PadesSignSignerLocationText.Text = "uae";
            PadesSignSignerContactInfoText.Text = "mazyad mall abudhabi";
            PadesSignSignatureXAxisText.Text = "300";
            PadesSignSignatureYAxisText.Text = "500";
            PadesSignFontNameText.Text = "sans";
            PadesSignBackgroundColorText.Text = "#FFEEFF";
            PadesSignFontColorText.Text = "#000000";
            PadesSignFontSizeText.Text = "20";
            PadesSignSignatureTextText.Text = "testing pdf signature";
            PadesSignSignVisible.IsChecked = true;
            PadesSignSignerNamePositionComboBoxText.SelectedIndex = 0;
            PadesSignPageNumberText.Text = "1";
        }

        public void ClearPadesSignTextFields()
        {
            PadesSignFilePathText.Text = "";
            PadesSignSignSignatureImagePathText.Text = "";
            PadesSignPinText.Clear();
            PadesSignTsaUrlText.Text = "";
            PadesSignOcspUrlText.Text = "";
            PadesSignCertPathText.Text = "";
            PadesSignSignatureLevelComboBox.SelectedIndex = 0;
            PadesSignPackageModeComboBox.SelectedIndex = 0;
            padesSignOtherOptionsCheckBox.IsChecked = false;
            PadesSignSetToDefaultValues();
            PadesSignXmlResponse.Text = "";

        }

        /// <summary>
        /// Set to default values of pades sign params fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PadesSignSetToDefaultValuesButton_Click(object sender, RoutedEventArgs e)
        {
            PadesSignSetToDefaultValues();
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
