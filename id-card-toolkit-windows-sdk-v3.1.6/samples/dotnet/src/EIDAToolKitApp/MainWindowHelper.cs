using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EIDAToolkitApp
{
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Clears all error messages 
        /// </summary>
        public void ClearErrorTextFields()
        {
            this.InitializeToolkitError.Text = "";
            this.ToolkitVersionError.Text = "";
            this.RegisterDeviceError.Text = "";
            this.DeviceIdError.Text = "";
            this.ListReadersError.Text = "";
            this.ReaderWithEmiratesIdCardError.Text = "";
            this.ConnectError.Text = "";
            this.InterfaceTypeError.Text = "";
            this.PraseMRZDataError.Text = "";
            this.ReadPublicDataError.Text = "";
            this.ReadPublicDataUsingNfcError.Text = "";
            this.ReadFamilyBookDataError.Text = "";
            this.CardStatusError.Text = "";
            this.FingerDataError.Text = "";
            this.AuthenticationBioOnServerError.Text = "";
            this.UnblockPinError.Text = "";
            this.ResetPinError.Text = "";
            this.GetPkiCertificatesError.Text = "";
            this.SignDataError.Text = "";
            this.PkiAuthenticationError.Text = "";
            this.PadesError.Text = "";
            this.XadesError.Text = "";
            this.CadesError.Text = "";
            this.DisconnectError.Text = "";
            this.CleanupToolkitError.Text = "";
            this.ValidateToolkitResponseError.Text = "";
            this.AuthCardAndBioError.Text = "";
            this.CardPublicDataEFError.Text = "";
            this.GetCSNError.Text = "";
            this.LicenseExpiryDateError.Text = "";
            this.ConfigCertExpiryDateError.Text = "";
            this.ReadDataError.Text = "";
            this.UpdateDataError.Text = "";
            this.ParseEFDataError.Text = "";
        }

        /// <summary>
        /// Clears All the fields in the UI
        /// </summary>
        public void ClearAllTextFields()
        {
            InitializeUsrCntrl.ProcessModeCheckBox.IsChecked = true;

            // Check Interface Type
            GetInterfaceTypeUsrCntrl.InterfaceTypeText.Text = "";

            // Get CSN
            GetCSNUsrCntrl.GetCSNText.Text = "";

            // Public Data
            PublicDataUsrCntrl.ClearPublicDataTextFields();

            // Public Data using NFC
            NFCAuthFieldsUsrCntrl.ClearNFCAuthTextFields();
            PublicDataUsingNFCUsrCntrl.ClearPublicUsingNFCDataTextFields();

            // Public Data EF
            CardPublicDataEFUsrCntrl.CardPublicDataEFTypeComboBox.SelectedIndex = 0;
            CardPublicDataEFUsrCntrl.ValidateSignatureCheckBox.IsChecked = false;
            CardPublicDataEFUsrCntrl.EFDataText.Text = "";

            // Parse EF Data
            ParseEFDataUsrCntrl.ParsedEFdataText.Text = "";
            ParseEFDataUsrCntrl.EFdataText.Text = "";

            // Family Book Data
            FamilyBookDataUsrCntrl.ClearFamilyBookDataTextFields();

            // Check Card Status
            CheckCardStatusUsrCntrl.CheckCardStatusText.Text = "";
            CheckCardStatusUsrCntrl.CardStatusXmlResponse.Text = "";

            // Finger Data
            GetFingerDataUsrCntrl.FirstFingerIdText.Text = "";
            GetFingerDataUsrCntrl.SecondFingerIdText.Text = "";
            GetFingerDataUsrCntrl.FirstFingerIndexText.Text = "";
            GetFingerDataUsrCntrl.SecondFingerIndexText.Text = "";

            // Biometric on server
            AuthBioOnServerUsrCntrl.AuthenticateBioOnServerTimeoutText.Text = "";
            AuthBioOnServerUsrCntrl.AuthenticateBioOnServerSelectFingerIndexComboBox.ItemsSource = null;
            AuthBioOnServerUsrCntrl.AuthenticateBioOnServerXmlResponse.Text = "";

            // Check Card Status and Authenticate Bio On Server
            AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioTimeoutText.Text = "";
            AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioSelectFingerIndexCB.ItemsSource = null;
            AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioXmlResponse.Text = "";

            // Unblock PIN
            UnblockPinUsrCntrl.UnblockPinText.Clear();
            UnblockPinUsrCntrl.UnblockConfirmPinText.Clear();
            UnblockPinUsrCntrl.UnblockPinSelectFingerIndexComboBox.ItemsSource = null;
            UnblockPinUsrCntrl.UnblockPinSensorTimeoutText.Text = "";
            UnblockPinUsrCntrl.UnblockPinXmlResponse.Text = "";

            // Reset PIN
            ResetPinUsrCntrl.ResetPinText.Clear();
            ResetPinUsrCntrl.ResetConfirmPinText.Clear();
            ResetPinUsrCntrl.ResetPinSelectFingerIndexComboBox.ItemsSource = null;
            ResetPinUsrCntrl.ResetPinSensorTimeoutText.Text = "";
            ResetPinUsrCntrl.ResetPinXmlResponse.Text = "";

            // Get PKI Certificates
            GetCertificatesUsrCntrl.AuthenticationCertificateText.Document.Blocks.Clear();
            GetCertificatesUsrCntrl.SigningCertificateText.Document.Blocks.Clear();
            GetCertificatesUsrCntrl.ExportAuthCertButton.IsEnabled = false;
            GetCertificatesUsrCntrl.ExportSignCertButton.IsEnabled = false;
            GetCertificatesUsrCntrl.GetPkiCertificatesXmlResponse.Text = "";

            // Sign/Verify
            SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
            SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
            SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = false;
            SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = false;
            SignOrVerifyUsrCntrl.SignDataXmlResponse.Text = "";

            // PKI Authentication
            AuthenticatePkiUsrCntrl.AuthenticatePkiXmlResponse.Text = "";

            // Read Data
            readDataUsrCntrl.ReadDataFileTypeComboBox.SelectedIndex = 0;
            readDataUsrCntrl.ReadDataXmlResponse.Text = "";

            // Update Data
            UpdateDataUsrCntrl.UpdateDataFileTypeComboBox.SelectedIndex = 0;
            UpdateDataUsrCntrl.UpdateDataXmlResponse.Text = "";
        }

        /// <summary>
        /// Clear Digital signature fields
        /// </summary>
        public void ClearDigitalSignatureTextFields()
        {
            // Pades
            PadesSignUsrCntrl.ClearPadesSignTextFields();
            PadesVerifyUsrCntrl.ClearPadesVerifyTextFields();

            // Xades
            XadesSignUsrCntrl.ClearXadesSignTextFields();
            XadesVerifyUsrCntrl.ClearXadesVerifyTextFields();

            // Cades
            CadesSignUserCntrl.ClearCadesSignTextFields();
            CadesVerifyUserCntrl.ClearCadesVerifyTextFields();
        }

        /// <summary>
        /// Determine if an application is running by name
        /// </summary>
        /// <param name="appName">Name of the application</param>
        /// <returns>
        /// True if an instance of this application
        /// is already running else false
        /// </returns>
        public static bool CheckForApplicationByName(string appName)
        {
            bool result = false;
            foreach (Process process in Process.GetProcesses("."))
            {
                try
                {
                    if (process.MainWindowTitle.ToString().ToLower() ==
                        appName.ToLower())
                    {
                        result = true;
                    }
                }
                catch
                {
                    result = false;
                }
            }
            return result;
        }

        /// <summary>
        /// Routine called when user changes selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListOfReadersCombobox_SelectionChanged(object sender, 
            SelectionChangedEventArgs e)
        {
            IsReaderSelected = false;
            if (ListReadersUsrCntrl.ReadersListComboBox.SelectedIndex > -1)
            {
                int index = ListReadersUsrCntrl.ReadersListComboBox.SelectedIndex;
                SelectedCardReader = CardReaders[index];
                IsReaderSelected = true;
            }
        }

        /// <summary>
        /// Routine called when Pades RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PadesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            PadesSignUsrCntrl.PadesSignSetToDefaultValues();
            XadesGrid.Visibility = Visibility.Hidden;
            CadesGrid.Visibility = Visibility.Hidden;
            PadesGrid.Visibility = Visibility.Visible;
            PadesSignRadioButton.IsChecked = true;
        }

        /// <summary>
        /// Routine called when Xades RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XadesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            XadesSignUsrCntrl.XadesSignSetToDefaultValues();
            PadesGrid.Visibility = Visibility.Hidden;
            CadesGrid.Visibility = Visibility.Hidden;
            XadesGrid.Visibility = Visibility.Visible;
            XadesSignRadioButton.IsChecked = true;
        }

        /// <summary>
        /// Routine called when Cades RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadesRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            CadesSignUserCntrl.CadesSignSetToDefaultValues();
            PadesGrid.Visibility = Visibility.Hidden;
            XadesGrid.Visibility = Visibility.Hidden;
            CadesGrid.Visibility = Visibility.Visible;
            CadesSignRadioButton.IsChecked = true;
        }

        /// <summary>
        /// Routine called when Pades Sign RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PadesSignRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            PadesSignUsrCntrl.PadesSignSignatureLevelComboBox.SelectedIndex = 0;
            PadesSignUsrCntrl.PadesSignPackageModeComboBox.SelectedIndex = 0;
            PadesSignUsrCntrl.PadesSignSignerNamePositionComboBoxText.SelectedIndex = 0;
            PadesVerifyUsrCntrl.Visibility = Visibility.Collapsed;
            PadesSignUsrCntrl.Visibility = Visibility.Visible;
            PadesVerifyButton.Visibility = Visibility.Hidden;
            PadesVerifyButton.IsDefault = false;
            PadesSignButton.Visibility = Visibility.Visible;
            PadesSignButton.IsDefault = true;
            PadesSignUsrCntrl.ScrollViewerPadesSign.ScrollToTop();
        }

        /// <summary>
        /// Routine called when Pades Verify RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PadesVerifyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            PadesVerifyUsrCntrl.PadesVerifyReportTypeComboBoxText.SelectedIndex = 0;
            PadesSignUsrCntrl.Visibility = Visibility.Collapsed;
            PadesVerifyUsrCntrl.Visibility = Visibility.Visible;
            PadesSignButton.Visibility = Visibility.Hidden;
            PadesSignButton.IsDefault = false;
            PadesVerifyButton.Visibility = Visibility.Visible;
            PadesVerifyButton.IsDefault = true;
            PadesVerifyUsrCntrl.ScrollViewerPadesVerify.ScrollToTop();
        }

        /// <summary>
        /// Routine called when Xades Sign RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XadesSignRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            XadesSignUsrCntrl.XadesSignSignatureLevelComboBox.SelectedIndex = 0;
            XadesSignUsrCntrl.XadesSignPackageModeComboBox.SelectedIndex = 0;
            XadesVerifyUsrCntrl.Visibility = Visibility.Collapsed;
            XadesSignUsrCntrl.Visibility = Visibility.Visible;
            XadesVerifyButton.Visibility = Visibility.Hidden;
            XadesVerifyButton.IsDefault = false;
            XadesSignButton.Visibility = Visibility.Visible;
            XadesSignButton.IsDefault = true;
            XadesSignUsrCntrl.ScrollViewerXadesSign.ScrollToTop();
        }

        /// <summary>
        /// Routine called when Xades Verify RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XadesVerifyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            XadesVerifyUsrCntrl.XadesVerifyReportTypeComboBoxText.SelectedIndex = 0;
            XadesSignUsrCntrl.Visibility = Visibility.Collapsed;
            XadesVerifyUsrCntrl.Visibility = Visibility.Visible;
            XadesSignButton.Visibility = Visibility.Hidden;
            XadesSignButton.IsDefault = false;
            XadesVerifyButton.Visibility = Visibility.Visible;
            XadesVerifyButton.IsDefault = true;
            XadesVerifyUsrCntrl.ScrollViewerXadesVerify.ScrollToTop();
        }

        /// <summary>
        /// Routine called when Cades Sign RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadesSignRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            CadesSignUserCntrl.CadesSignSignatureLevelCombobox.SelectedIndex = 0;
            CadesSignUserCntrl.CadesSignPackageModeComboBox.SelectedIndex = 0;
            CadesVerifyUserCntrl.Visibility = Visibility.Collapsed;
            CadesSignUserCntrl.Visibility = Visibility.Visible;
            CadesVerifyButton.Visibility = Visibility.Hidden;
            CadesVerifyButton.IsDefault = false;
            CadesSignButton.Visibility = Visibility.Visible;
            CadesSignButton.IsDefault = true;
            CadesSignUserCntrl.ScrollViewerCadesSign.ScrollToTop();
        }

        /// <summary>
        /// Routine called when Cades Verify RadioButton is checked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CadesVerifyRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            ClearErrorTextFields();
            CadesVerifyUserCntrl.CadesVerifyReportTypeComboBoxText.SelectedIndex = 0;
            CadesSignUserCntrl.Visibility = Visibility.Collapsed;
            CadesVerifyUserCntrl.Visibility = Visibility.Visible;
            CadesSignButton.Visibility = Visibility.Hidden;
            CadesSignButton.IsDefault = false;
            CadesVerifyButton.Visibility = Visibility.Visible;
            CadesVerifyButton.IsDefault = true;
            CadesVerifyUserCntrl.ScrollViewerCadesVerify.ScrollToTop();
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

        /// <summary>
        /// Drags window on mouse left button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_TITLEBAR_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        /// <summary>
        /// Exits the application window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_CLOSE_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Minimize the application window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PART_MINIMIZE_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}
