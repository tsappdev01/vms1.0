using System;
using System.Collections;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.Generic;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Toolkit ToolkitObj = null;
        public CardReader[] CardReaders = new CardReader[0];
        public CardReader SelectedCardReader = null;
        public FingerData[] FingerDataArray = null;
        public bool IsToolkitInitialized = false;
        public bool IsReaderSelected = false;
        public bool IsCardConnected = false;

        public byte[] AuthenticateCertificate = null;
        public byte[] SigningCertificate = null;
        public byte[] Signature = null;

        // DSS related global variables
        public byte[] XadesSignature = null;
        public byte[] CadesSignature = null;
        public string PadesSignFileDirectoryPath = "";
        public string PadesSignFileName = "";
        public string PadesSignFileExt = "";
        public string PadesVerifyFileDirectoryPath = "";
        public string PadesVerifyFileName = "";
        public string PadesVerifyFileExt = "";
        public string XadesSignFileDirectoryPath = "";
        public string XadesSignFileName = "";
        public string XadesSignFileExt = "";
        public string XadesVerifyFileDirectoryPath = "";
        public string XadesVerifyFileName = "";
        public string XadesVerifyFileExt = "";
        public string CadesSignFileDirectoryPath = "";
        public string CadesSignFileName = "";
        public string CadesSignFileExt = "";

        /// <summary>
        /// Constructor initializes the mainwindow components
        /// </summary>
        public MainWindow()
        {
            const string appName = "EIDAToolkit";

            // Check if an instance of application is already running
            if (CheckForApplicationByName(appName))
            {
                MessageBox.Show
                    ("One Instance is already running", "EIDA Toolkit");
                this.Close();
            }
            else
            {
                InitializeComponent();
            }
        }

        public FingerData[] ReadFingerDataFromCard()
        {
            FingerDataArray = null;

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        MessageBox.Show("Toolkit is not initialized, please initialize the toolkit");
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        MessageBox.Show("Please list and select a reader");
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        MessageBox.Show("Please connect to the card");
                        break;
                    }

                    // Get stored finger indexes from the Emirates ID Card
                    FingerDataArray = SelectedCardReader.GetFingerData();
                }
                catch (ToolkitException tEx)
                {
                    MessageBox.Show(tEx.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                // final break                
                break;
            }
            return FingerDataArray;
        }

        /// <summary>
        /// Routine called when tabControl selection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.Source is TabControl)
            {
                ClearErrorTextFields();

                if (CardDataTabItem.IsSelected)
                {
                    PublicDataUsrCntrl.ReadNonModifiableDataCheckbox.IsChecked = true;
                    PublicDataUsrCntrl.ReadModifiableDataCheckbox.IsChecked = true;
                    PublicDataUsrCntrl.ReadPhotographyCheckbox.IsChecked = true;
                    PublicDataUsrCntrl.ReadSignatureCheckbox.IsChecked = true;
                    PublicDataUsrCntrl.ReadAddressCheckbox.IsChecked = true;
                    PublicDataUsrCntrl.ScrollViewerPublicData.ScrollToTop();
                }
                if (CardDataUsingNfcTabItem.IsSelected)
                {
                    PublicDataUsingNFCUsrCntrl.Nfc_ReadNonModifiableDataCheckbox.IsChecked = true;
                    PublicDataUsingNFCUsrCntrl.Nfc_ReadModifiableDataCheckbox.IsChecked = true;
                    PublicDataUsingNFCUsrCntrl.Nfc_ReadPhotographyCheckbox.IsChecked = true;
                    PublicDataUsingNFCUsrCntrl.Nfc_ReadSignatureCheckbox.IsChecked = true;
                    PublicDataUsingNFCUsrCntrl.Nfc_ReadAddressCheckbox.IsChecked = true;
                    ReadCardPublicDataUsingNfcButton.IsEnabled = false;
                    PublicDataUsingNFCUsrCntrl.ScrollViewerNfcPublicData.ScrollToTop();
                }
                if (FamilyBookDataTabItem.IsSelected)
                {
                    FamilyBookDataUsrCntrl.ScrollViewerFamilyBookData.ScrollToTop();
                }
                if (AuthenticateBioOnServerTabItem.IsSelected)
                {
                    FingerData[] FingerDataArray = null;
                    List<FingerIndex> fingerIndexList = new List<FingerIndex>();
                    AuthBioOnServerUsrCntrl.AuthenticateBioOnServerSelectFingerIndexComboBox.ItemsSource = null;

                    FingerDataArray = ReadFingerDataFromCard();

                    if (FingerDataArray != null)
                    {
                        fingerIndexList.Add(FingerDataArray[0].FingerIndex);
                        fingerIndexList.Add(FingerDataArray[1].FingerIndex);

                        AuthBioOnServerUsrCntrl.AuthenticateBioOnServerSelectFingerIndexComboBox.ItemsSource = fingerIndexList;
                        AuthBioOnServerUsrCntrl.AuthenticateBioOnServerSelectFingerIndexComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Failed to read Finger data values");
                    }
                }
                if (AuthCardAndBioTabItem.IsSelected)
                {
                    FingerData[] FingerDataArray = null;
                    List<FingerIndex> fingerIndexList = new List<FingerIndex>();
                    AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioSelectFingerIndexCB.ItemsSource = null;

                    FingerDataArray = ReadFingerDataFromCard();

                    if (FingerDataArray != null)
                    {
                        fingerIndexList.Add(FingerDataArray[0].FingerIndex);
                        fingerIndexList.Add(FingerDataArray[1].FingerIndex);

                        AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioSelectFingerIndexCB.ItemsSource = fingerIndexList;
                        AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioSelectFingerIndexCB.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Failed to read Finger data values");
                    }
                }
                if (UnblockPinTabItem.IsSelected)
                {
                    FingerData[] FingerDataArray = null;
                    List<FingerIndex> fingerIndexList = new List<FingerIndex>();
                    UnblockPinUsrCntrl.UnblockPinSelectFingerIndexComboBox.ItemsSource = null;

                    FingerDataArray = ReadFingerDataFromCard();

                    if (FingerDataArray != null)
                    {
                        fingerIndexList.Add(FingerDataArray[0].FingerIndex);
                        fingerIndexList.Add(FingerDataArray[1].FingerIndex);

                        UnblockPinUsrCntrl.UnblockPinSelectFingerIndexComboBox.ItemsSource = fingerIndexList;
                        UnblockPinUsrCntrl.UnblockPinSelectFingerIndexComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Failed to read Finger data values");
                    }
                }
                if (ResetPinTabItem.IsSelected)
                {
                    FingerData[] FingerDataArray = null;
                    List<FingerIndex> fingerIndexList = new List<FingerIndex>();
                    ResetPinUsrCntrl.ResetPinSelectFingerIndexComboBox.ItemsSource = null;

                    FingerDataArray = ReadFingerDataFromCard();

                    if (FingerDataArray != null)
                    {
                        fingerIndexList.Add(FingerDataArray[0].FingerIndex);
                        fingerIndexList.Add(FingerDataArray[1].FingerIndex);

                        ResetPinUsrCntrl.ResetPinSelectFingerIndexComboBox.ItemsSource = fingerIndexList;
                        ResetPinUsrCntrl.ResetPinSelectFingerIndexComboBox.SelectedIndex = 0;
                    }
                    else
                    {
                        MessageBox.Show("Failed to read Finger data values");
                    }
                }
                if (GetCertificatesTabItem.IsSelected)
                {
                    this.GetCertificatesUsrCntrl.ScrollViewerGetCertificates.ScrollToTop();
                }
                if (SignOrVerifyTabItem.IsSelected)
                {
                    this.SignOrVerifyUsrCntrl.ScrollViewerSignOrVerify.ScrollToTop();
                }
                if (DigitalSignatureTabItem.IsSelected)
                {
                    PadesRadioButton.IsChecked = true;
                }
                if (ResponseValidationTabItem.IsSelected)
                {
                    ResponseValidationUsrCntrl.ScrollViewerToolkitResponseValidation.ScrollToTop();
                }
                if (CardPublicDataEFTabItem.IsSelected)
                {
                }
                if (ReadDataTabItem.IsSelected)
                {
                    readDataUsrCntrl.ReadDataFileTypeComboBox.SelectedIndex = 0;
                }
                if (UpdateDataTabItem.IsSelected)
                {
                    UpdateDataUsrCntrl.UpdateDataFileTypeComboBox.SelectedIndex = 0;
                }
            }
        }

        /// <summary>
        /// Initialize Emirates ID Card Toolkit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void InitializeToolkitButton_Click
            (object sender, RoutedEventArgs e)
        {
            this.InitializeToolkitButton.IsEnabled = false;
            IsToolkitInitialized = false;
            IsReaderSelected = false;
            IsCardConnected = false;
            ClearErrorTextFields();

            // local variables
            string configParams = null;
            bool processMode = false;

            this.InitializeToolkitError.Text = "Please wait...";
            this.InitializeToolkitError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    string configFilePath = "config_ap";
                    if (File.Exists(configFilePath))
                    {
                        // Read config_ap file content into a string
                        configParams = File.ReadAllText(configFilePath);
                    }

                    processMode = Convert.ToBoolean(InitializeUsrCntrl.ProcessModeCheckBox.IsChecked);

                    // Initialize Toolkit
                    ToolkitObj = new Toolkit(processMode, configParams);

                    IsToolkitInitialized = true;
                    CleanupToolkitButton.IsEnabled = true;
                    this.InitializeToolkitError.Text = "Success";
                    this.InitializeToolkitError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.InitializeToolkitButton.IsEnabled = true;
                    this.InitializeToolkitError.Text = tEx.Message;
                    this.InitializeToolkitError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.InitializeToolkitButton.IsEnabled = true;
                    this.InitializeToolkitError.Text = ex.Message;
                    this.InitializeToolkitError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
        }

        /// <summary>
        /// Get the version of Toolkit being used
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetToolkitVersionButton_Click(object sender, RoutedEventArgs e)
        {
            this.GetToolkitVersionButton.IsEnabled = false;
            this.ToolkitVersionUsrCntrl.ToolkitVersionText.Text = "";
            ClearErrorTextFields();

            // local variables
            string toolkitVersion = null;

            this.ToolkitVersionError.Text = "Please wait...";
            this.ToolkitVersionError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ToolkitVersionError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ToolkitVersionError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Toolkit version
                    toolkitVersion = ToolkitObj.GetToolkitVersion();
                    if (!String.IsNullOrEmpty(toolkitVersion))
                    {
                        this.ToolkitVersionUsrCntrl.ToolkitVersionText.Text = toolkitVersion;
                        this.ToolkitVersionError.Text = "Success";
                        this.ToolkitVersionError.Foreground = Brushes.RoyalBlue;
                    }
                    else
                    {
                        this.ToolkitVersionError.Text = "Toolkit version is null or empty";
                        this.ToolkitVersionError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (ToolkitException tEx)
                {
                    this.ToolkitVersionError.Text = tEx.Message;
                    this.ToolkitVersionError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ToolkitVersionError.Text = ex.Message;
                    this.ToolkitVersionError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.GetToolkitVersionButton.IsEnabled = true;
        }

        /// <summary>
        /// Register the device with Validation Gateway(VG) 
        /// against the Service Provider(SP) license
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RegisterDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterDeviceButton.IsEnabled = false;
            RegisterDeviceUsrCntrl.RegisterDeviceRegistrationIdText.Text = "";
            RegisterDeviceUsrCntrl.RegisterDeviceXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            string requestId = null;
            string userId = null;
            string password = null;
            string deviceReferenceId = null;

            this.RegisterDeviceError.Text = "Please wait...";
            this.RegisterDeviceError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    if (String.IsNullOrEmpty(RegisterDeviceUsrCntrl.RegisterDeviceUserNameText.Text) ||
                        String.IsNullOrEmpty(RegisterDeviceUsrCntrl.RegisterDevicePasswordText.Password) ||
                        String.IsNullOrEmpty(RegisterDeviceUsrCntrl.RegisterDeviceReferenceIdText.Text))
                    {
                        this.RegisterDeviceError.Text =
                            "Please enter the required above fields";
                        this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    userId = RegisterDeviceUsrCntrl.RegisterDeviceUserNameText.Text;
                    password = RegisterDeviceUsrCntrl.RegisterDevicePasswordText.Password;
                    deviceReferenceId = RegisterDeviceUsrCntrl.RegisterDeviceReferenceIdText.Text;

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.RegisterDeviceError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = ToolkitObj.PrepareRequest(requestId);

                    // Encrypt and base64 encoding of userid and password
                    string encodedUserId = Helper.Base64Encryption(requestHandle,
                        userId, ToolkitObj);
                    string encodedPassword = Helper.Base64Encryption(requestHandle,
                        password, ToolkitObj);

                    // Registers the device
                    RegisterDeviceResponse registerDeviceResponse =
                        ToolkitObj.RegisterDevice(encodedUserId, encodedPassword,
                        deviceReferenceId);

                    // Compare Request ID of response
                    if (!Helper.CompareRequestId(requestId,
                        registerDeviceResponse.XmlString))
                    {
                        this.RegisterDeviceError.Text =
                            "Request ID verification failed, response is tampered";
                        this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature of response is valid or not
                    if (!Helper.VerifySignature(registerDeviceResponse.XmlString))
                    {
                        this.RegisterDeviceError.Text =
                            "Response signature verification failed, response is tampered";
                        this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.RegisterDeviceUsrCntrl.RegisterDeviceRegistrationIdText.Text =
                        registerDeviceResponse.DeviceRegistrationId;
                    this.RegisterDeviceUsrCntrl.RegisterDeviceXmlResponse.Text = registerDeviceResponse.XmlString;
                    this.RegisterDeviceError.Text =
                        registerDeviceResponse.ResponseStatus.ToString();
                    this.RegisterDeviceError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.RegisterDeviceUsrCntrl.RegisterDeviceXmlResponse.Text = tEx.VGResponse;
                                this.RegisterDeviceError.Text = tEx.Message;
                                this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                            }
                            else
                            {
                                this.RegisterDeviceError.Text = "Signature verification failed";
                                this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                            }
                        }
                        else
                        {
                            this.RegisterDeviceError.Text = "Request ID verification failed";
                            this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        this.RegisterDeviceError.Text = tEx.Message;
                        this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (Exception ex)
                {
                    this.RegisterDeviceError.Text = ex.Message;
                    this.RegisterDeviceError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.RegisterDeviceButton.IsEnabled = true;
        }

        /// <summary>
        /// Get Device ID
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetDeviceIDButton_Click(object sender, RoutedEventArgs e)
        {
            GetDeviceIdButton.IsEnabled = false;
            DeviceIdUsrCntrl.DeviceIdText.Text = "";
            ClearErrorTextFields();

            // local variables
            string deviceId = null;

            this.DeviceIdError.Text = "Please wait...";
            this.DeviceIdError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.DeviceIdError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.DeviceIdError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the device ID
                    deviceId = ToolkitObj.GetDeviceId();
                    if (!String.IsNullOrEmpty(deviceId))
                    {
                        this.DeviceIdUsrCntrl.DeviceIdText.Text = deviceId;
                        this.DeviceIdError.Text = "Success";
                        this.DeviceIdError.Foreground = Brushes.RoyalBlue;
                    }
                    else
                    {
                        this.DeviceIdError.Text = "Device id is null or empty";
                        this.DeviceIdError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (ToolkitException tEx)
                {
                    this.DeviceIdError.Text = tEx.Message;
                    this.DeviceIdError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.DeviceIdError.Text = ex.Message;
                    this.DeviceIdError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.GetDeviceIdButton.IsEnabled = true;
        }

        /// <summary>
        /// Get the Expiry Date of the Toolkit SDK License issued to the Service Provider
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetLicenseExpiryDateButton_Click(object sender, RoutedEventArgs e)
        {
            GetLicenseExpiryDateButton.IsEnabled = false;
            LicenseExpiryDateUsrCntrl.LicenseExpiryDateText.Text = "";
            ClearErrorTextFields();

            // local variables
            string expiryDate = null;

            this.LicenseExpiryDateError.Text = "Please wait...";
            this.LicenseExpiryDateError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.LicenseExpiryDateError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.LicenseExpiryDateError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get License Expiry Date
                    expiryDate = ToolkitObj.GetLicenseExpiryDate();
                    if (!String.IsNullOrEmpty(expiryDate))
                    {
                        this.LicenseExpiryDateUsrCntrl.LicenseExpiryDateText.Text = expiryDate;
                        this.LicenseExpiryDateError.Text = "Success";
                        this.LicenseExpiryDateError.Foreground = Brushes.RoyalBlue;
                    }
                    else
                    {
                        this.LicenseExpiryDateError.Text = "License expiry date is null or empty";
                        this.LicenseExpiryDateError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (ToolkitException tEx)
                {
                    this.LicenseExpiryDateError.Text = tEx.Message;
                    this.LicenseExpiryDateError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.LicenseExpiryDateError.Text = ex.Message;
                    this.LicenseExpiryDateError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.GetLicenseExpiryDateButton.IsEnabled = true;
        }

        /// <summary>
        /// Parse the provided MRZ string
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ParseMRZDataButton_Click(object sender, RoutedEventArgs e)
        {
            this.ParseMRZDataButton.IsEnabled = false;
            ClearErrorTextFields();
            ParseMRZDataUsrCntrl.ClearParseMRZDataTextFields();

            // local variables
            string mrzData = "";

            this.PraseMRZDataError.Text = "Please wait...";
            this.PraseMRZDataError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if MRZ Data provided is empty
                    if (String.IsNullOrEmpty(ParseMRZDataUsrCntrl.MRZdataText.Text))
                    {
                        this.PraseMRZDataError.Text = "Please enter MRZ Data.";
                        this.PraseMRZDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.PraseMRZDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.PraseMRZDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get MRZ Data string
                    mrzData = ParseMRZDataUsrCntrl.MRZdataText.Text;

                    // Parse the data
                    MRZData mrzDataAttributes = ToolkitObj.ParseMRZData(mrzData);

                    // Fill Data
                    ParseMRZDataUsrCntrl.FillParseMRZDataTextFields(mrzDataAttributes);

                    this.PraseMRZDataError.Text = "Success";
                    this.PraseMRZDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.PraseMRZDataError.Text = tEx.Message;
                    this.PraseMRZDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.PraseMRZDataError.Text = ex.Message;
                    this.PraseMRZDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.ParseMRZDataButton.IsEnabled = true;
        }

        /// <summary>
        /// Get Config Certificate Expiry Date
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetConfigCertExpiryDateButton_Click(object sender, RoutedEventArgs e)
        {
            GetConfigCertExpiryDateButton.IsEnabled = false;
            ConfiCertExpiryDateUsrCntrl.ConfigVGCertExpiryDateText.Text = "";
            ConfiCertExpiryDateUsrCntrl.ConfigLVCertExpiryDateText.Text = "";
            ConfiCertExpiryDateUsrCntrl.ServerTLSCertExpiryDateText.Text = "";
            ConfiCertExpiryDateUsrCntrl.ConfigAGCertExpiryDateText.Text = "";
            ConfiCertExpiryDateUsrCntrl.ConfigLicenseExpiryDateText.Text = "";
            ClearErrorTextFields();

            // local variables
            ConfigCertExpiryDate configCertExpiryDate;

            this.ConfigCertExpiryDateError.Text = "Please wait...";
            this.ConfigCertExpiryDateError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ConfigCertExpiryDateError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ConfigCertExpiryDateError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get License Expiry Date
                    configCertExpiryDate = ToolkitObj.GetConfigCertificateExpiryDate();

                    ConfiCertExpiryDateUsrCntrl.ConfigVGCertExpiryDateText.Text = configCertExpiryDate.ConfigVGCertExpiry;
                    ConfiCertExpiryDateUsrCntrl.ConfigLVCertExpiryDateText.Text = configCertExpiryDate.ConfigLVCertExpiry;
                    ConfiCertExpiryDateUsrCntrl.ServerTLSCertExpiryDateText.Text = configCertExpiryDate.ServerTLSCertExpiry;
                    ConfiCertExpiryDateUsrCntrl.ConfigAGCertExpiryDateText.Text = configCertExpiryDate.ConfigAGCertExpiry;
                    ConfiCertExpiryDateUsrCntrl.ConfigLicenseExpiryDateText.Text = configCertExpiryDate.LicenseExpiry;

                    //this.LicenseExpiryDateText.Text = expiryDate;
                    this.ConfigCertExpiryDateError.Text = "Success";
                    this.ConfigCertExpiryDateError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.ConfigCertExpiryDateError.Text = tEx.Message;
                    this.ConfigCertExpiryDateError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ConfigCertExpiryDateError.Text = ex.Message;
                    this.ConfigCertExpiryDateError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.GetConfigCertExpiryDateButton.IsEnabled = true;
        }

        /// <summary>
        ///  Gets the list of readers connected to the system.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ListReadersButton_Click(object sender, RoutedEventArgs e)
        {
            this.ListReadersButton.IsEnabled = false;
            ListReadersUsrCntrl.ReadersListComboBox.ItemsSource = null;
            GetReaderWithEmiratesIdUsrCntrl.ReaderWithEmiratesIdCardText.Text = "";
            SelectedCardReader = null;
            IsReaderSelected = false;
            IsCardConnected = false;
            ClearErrorTextFields();

            // local variables
            ArrayList availableReaders = new ArrayList();

            this.ListReadersError.Text = "Please wait...";
            this.ListReadersError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ListReadersError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ListReadersError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // List the readers
                    CardReaders = ToolkitObj.ListReaders();
                    if (0 == CardReaders.Length)
                    {
                        // No reader found
                        SelectedCardReader = null;
                        IsReaderSelected = false;
                        IsCardConnected = false;
                        this.ListReadersError.Text = "No smartcard reader is found";
                        this.ListReadersError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    for (int counter = 0; counter < CardReaders.Length; counter++)
                        availableReaders.Add(CardReaders[counter].Name);

                    ListReadersUsrCntrl.ReadersListComboBox.ItemsSource = availableReaders;
                    ListReadersUsrCntrl.ReadersListComboBox.SelectedIndex = 0;
                    this.ListReadersError.Text = "Success";
                    this.ListReadersError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    ListReadersUsrCntrl.ReadersListComboBox.ItemsSource = null;
                    this.ListReadersError.Text = tEx.Message;
                    this.ListReadersError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    ListReadersUsrCntrl.ReadersListComboBox.ItemsSource = null;
                    this.ListReadersError.Text = ex.Message;
                    this.ListReadersError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.ListReadersButton.IsEnabled = true;
        }

        /// <summary>
        /// Get first reader with Emirates ID Card inserted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetReaderWithEmiratesIdCardButton_Click(object sender, RoutedEventArgs e)
        {
            this.GetReaderWithEmiratesIdCardButton.IsEnabled = false;
            GetReaderWithEmiratesIdUsrCntrl.ReaderWithEmiratesIdCardText.Text = "";
            ListReadersUsrCntrl.ReadersListComboBox.ItemsSource = null;
            SelectedCardReader = null;
            IsReaderSelected = false;
            IsCardConnected = false;
            ClearErrorTextFields();

            this.ReaderWithEmiratesIdCardError.Text = "Please wait...";
            this.ReaderWithEmiratesIdCardError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ReaderWithEmiratesIdCardError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ReaderWithEmiratesIdCardError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Reader with Emirated ID Card
                    SelectedCardReader = ToolkitObj.GetReaderWithEmiratesId();

                    IsReaderSelected = true;
                    this.GetReaderWithEmiratesIdUsrCntrl.ReaderWithEmiratesIdCardText.Text = SelectedCardReader.Name;
                    this.ReaderWithEmiratesIdCardError.Text = "Success";
                    this.ReaderWithEmiratesIdCardError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.ReaderWithEmiratesIdCardError.Text = tEx.Message;
                    this.ReaderWithEmiratesIdCardError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ReaderWithEmiratesIdCardError.Text = ex.Message;
                    this.ReaderWithEmiratesIdCardError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.GetReaderWithEmiratesIdCardButton.IsEnabled = true;
        }

        /// <summary>
        /// Connect to the selected smart card reader
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            this.ConnectButton.IsEnabled = false;
            IsCardConnected = false;
            ClearErrorTextFields();

            this.ConnectError.Text = "Please wait...";
            this.ConnectError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ConnectError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ConnectError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected
                    if (!IsReaderSelected)
                    {
                        this.ConnectError.Text = "Please list and select a reader";
                        this.ConnectError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Connect to the Emirates Id card
                    SelectedCardReader.Connect();

                    IsCardConnected = true;
                    ClearAllTextFields();
                    this.ConnectError.Text = "Success";
                    this.ConnectError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.ConnectError.Text = tEx.Message;
                    this.ConnectError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ConnectError.Text = ex.Message;
                    this.ConnectError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.ConnectButton.IsEnabled = true;
        }

        /// <summary>
        /// Get the Card communication interface
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetInterfaceTypeButton_Click(object sender, RoutedEventArgs e)
        {
            GetInterfaceTypeButton.IsEnabled = false;
            GetInterfaceTypeUsrCntrl.InterfaceTypeText.Text = "";
            ClearErrorTextFields();

            // local variables
            int interfaceType = 0;

            this.InterfaceTypeError.Text = "Please wait...";
            this.InterfaceTypeError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.InterfaceTypeError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.InterfaceTypeError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected
                    if (!IsReaderSelected)
                    {
                        this.InterfaceTypeError.Text = "Please list and select a reader";
                        this.InterfaceTypeError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.InterfaceTypeError.Text = "Please connect to the card";
                        this.InterfaceTypeError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Interface type
                    interfaceType = SelectedCardReader.GetInterfaceType();
                    this.GetInterfaceTypeUsrCntrl.InterfaceTypeText.Text =
                        Convert.ToString((InterfaceType)interfaceType);

                    this.InterfaceTypeError.Text = "Success";
                    this.InterfaceTypeError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.InterfaceTypeError.Text = tEx.Message;
                    this.InterfaceTypeError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.InterfaceTypeError.Text = ex.Message;
                    this.InterfaceTypeError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.GetInterfaceTypeButton.IsEnabled = true;
        }

        /// <summary>
        /// Retrieve the serial number of the Emirates ID Card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetCSN_Click(object sender, RoutedEventArgs e)
        {
            GetCSNButton.IsEnabled = false;
            GetCSNUsrCntrl.GetCSNText.Text = "";
            ClearErrorTextFields();

            // Local Variables
            string csn = null;

            this.GetCSNError.Text = "Please wait...";
            this.GetCSNError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.GetCSNError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.GetCSNError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.GetCSNError.Text = "Please list and select a reader";
                        this.GetCSNError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.GetCSNError.Text = "Please connect to the card";
                        this.GetCSNError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Interface type
                    csn = SelectedCardReader.GetCSN();
                    if (!String.IsNullOrEmpty(csn))
                    {
                        this.GetCSNUsrCntrl.GetCSNText.Text = csn;
                        this.GetCSNError.Text = "Success";
                        this.GetCSNError.Foreground = Brushes.RoyalBlue;
                    }
                    else
                    {
                        this.GetCSNError.Text = "CSN value is null or empty";
                        this.GetCSNError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (ToolkitException tEx)
                {
                    this.GetCSNError.Text = tEx.Message;
                    this.GetCSNError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.GetCSNError.Text = ex.Message;
                    this.GetCSNError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.GetCSNButton.IsEnabled = true;
        }

        /// <summary>
        /// Read public data from the card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReadCardPublicDataButton_Click(object sender, RoutedEventArgs e)
        {
            ReadCardPublicDataButton.IsEnabled = false;
            PublicDataUsrCntrl.ClearPublicDataTextFields();
            ClearErrorTextFields();

            // local variables
            string requestId = null;

            this.ReadPublicDataError.Text = "Please wait...";
            this.ReadPublicDataError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ReadPublicDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ReadPublicDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.ReadPublicDataError.Text = "Please list and select a reader";
                        this.ReadPublicDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.ReadPublicDataError.Text = "Please connect to the card";
                        this.ReadPublicDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Generate Request Id
                    requestId = Helper.GenerateRequestId();

                    // Read public data from card
                    bool readNonModifiableData =
                        Convert.ToBoolean(PublicDataUsrCntrl.ReadNonModifiableDataCheckbox.IsChecked);
                    bool readModifiableData =
                        Convert.ToBoolean(PublicDataUsrCntrl.ReadModifiableDataCheckbox.IsChecked);
                    bool readPhotography =
                        Convert.ToBoolean(PublicDataUsrCntrl.ReadPhotographyCheckbox.IsChecked);
                    bool readSignatueImage =
                        Convert.ToBoolean(PublicDataUsrCntrl.ReadSignatureCheckbox.IsChecked);
                    bool readAddress = Convert.ToBoolean(PublicDataUsrCntrl.ReadAddressCheckbox.IsChecked);

                    CardPublicData cardPublicData = SelectedCardReader.ReadPublicData(
                        requestId,
                        readNonModifiableData,
                        readModifiableData,
                        readPhotography,
                        readSignatueImage,
                        readAddress);

                    // Fill data       
                    PublicDataUsrCntrl.FillPublicDataTextFields(cardPublicData);
                    this.ReadPublicDataError.Text = cardPublicData.ResponseStatus.ToString();
                    this.ReadPublicDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    PublicDataUsrCntrl.ClearPublicDataTextFields();
                    this.ReadPublicDataError.Text = tEx.Message;
                    this.ReadPublicDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    PublicDataUsrCntrl.ClearPublicDataTextFields();
                    this.ReadPublicDataError.Text = ex.Message;
                    this.ReadPublicDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            ReadCardPublicDataButton.IsEnabled = true;
        }

        /// <summary>
        /// Retrieve Elementary File data stored on the public areas of the ID card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReadPublicDataEF_Click(object sender, RoutedEventArgs e)
        {
            this.ReadCardPublicDataEFButton.IsEnabled = false;
            this.CardPublicDataEFUsrCntrl.EFDataText.Text = "";
            ClearErrorTextFields();

            // local variables
            bool validateSignature = false;
            PublicDataEFType publicDataEFType;

            this.CardPublicDataEFError.Text = "Please wait...";
            this.CardPublicDataEFError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    if (CardPublicDataEFUsrCntrl.CardPublicDataEFTypeComboBox.Items.Count != 0)
                    {
                        publicDataEFType = (PublicDataEFType)(CardPublicDataEFUsrCntrl.CardPublicDataEFTypeComboBox.SelectedValue);
                    }
                    else
                    {
                        this.CardPublicDataEFError.Text = "Please select plublic data EF type";
                        this.CardPublicDataEFError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    validateSignature = Convert.ToBoolean(CardPublicDataEFUsrCntrl.ValidateSignatureCheckBox.IsChecked);

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.CardPublicDataEFError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.CardPublicDataEFError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.CardPublicDataEFError.Text = "Please list and select a reader";
                        this.CardPublicDataEFError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.CardPublicDataEFError.Text = "Please connect to the card";
                        this.CardPublicDataEFError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Read Public Data EF
                    byte[] efData = SelectedCardReader.ReadPublicDataEF(
                        publicDataEFType,
                        validateSignature);

                    if (efData == null || efData.Length == 0)
                    {
                        this.CardPublicDataEFError.Text = "Public data EF is null or empty";
                        this.CardPublicDataEFError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.CardPublicDataEFUsrCntrl.EFDataText.Text = Convert.ToBase64String(efData);
                    
                    this.CardPublicDataEFError.Text = "Success";
                    this.CardPublicDataEFError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.CardPublicDataEFError.Text = tEx.Message;
                    this.CardPublicDataEFError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.CardPublicDataEFError.Text = ex.Message;
                    this.CardPublicDataEFError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.ReadCardPublicDataEFButton.IsEnabled = true;
        }
               
        /// <summary>
        /// Set NFC authentication parameters
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SetNfcAuthenticationParametersButton_Click(object sender, RoutedEventArgs e)
        {
            this.SetNfcAuthenticationParametersButton.IsEnabled = false;
            this.ReadCardPublicDataUsingNfcButton.IsEnabled = false;
            PublicDataUsingNFCUsrCntrl.ClearPublicUsingNFCDataTextFields();
            ClearErrorTextFields();

            // local variables
            string cardNumber = null;
            string dateOfBirth = null;
            string expiryDate = null;

            this.ReadPublicDataUsingNfcError.Text = "Please wait...";
            this.ReadPublicDataUsingNfcError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if NFC authentication params fields entered by user are not empty
                    if (String.IsNullOrEmpty(NFCAuthFieldsUsrCntrl.SetCardNumberText.Text) || String.IsNullOrEmpty(NFCAuthFieldsUsrCntrl.SetDateOfBirthDayText.Text) ||
                        String.IsNullOrEmpty(NFCAuthFieldsUsrCntrl.SetDateOfBirthMonthText.Text) || String.IsNullOrEmpty(NFCAuthFieldsUsrCntrl.SetDateOfBirthYearText.Text) ||
                        String.IsNullOrEmpty(NFCAuthFieldsUsrCntrl.SetExpiryDateDayText.Text) || String.IsNullOrEmpty(NFCAuthFieldsUsrCntrl.SetExpiryDateMonthText.Text) ||
                        String.IsNullOrEmpty(NFCAuthFieldsUsrCntrl.SetExpiryDateYearText.Text))
                    {
                        this.ReadPublicDataUsingNfcError.Text = "Fill all mandatory fields";
                        this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the Nfc authentication params fields
                    cardNumber = NFCAuthFieldsUsrCntrl.SetCardNumberText.Text;
                    dateOfBirth = NFCAuthFieldsUsrCntrl.SetDateOfBirthYearText.Text + NFCAuthFieldsUsrCntrl.SetDateOfBirthMonthText.Text + NFCAuthFieldsUsrCntrl.SetDateOfBirthDayText.Text;
                    expiryDate = NFCAuthFieldsUsrCntrl.SetExpiryDateYearText.Text + NFCAuthFieldsUsrCntrl.SetExpiryDateMonthText.Text + NFCAuthFieldsUsrCntrl.SetExpiryDateDayText.Text;

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ReadPublicDataUsingNfcError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.ReadPublicDataUsingNfcError.Text = "Please list and select a reader";
                        this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.ReadPublicDataUsingNfcError.Text = "Please connect to the card";
                        this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Set NFC authentication parameters of the card
                    SelectedCardReader.SetNfcAuthenticationParameters(
                        cardNumber,
                        dateOfBirth,
                        expiryDate);

                    this.ReadPublicDataUsingNfcError.Text = "Success";
                    this.ReadPublicDataUsingNfcError.Foreground = Brushes.RoyalBlue;
                    this.ReadCardPublicDataUsingNfcButton.IsEnabled = true;
                }
                catch (ToolkitException tEx)
                {
                    this.ReadCardPublicDataUsingNfcButton.IsEnabled = false;
                    this.ReadPublicDataUsingNfcError.Text = tEx.Message;
                    this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ReadCardPublicDataUsingNfcButton.IsEnabled = false;
                    this.ReadPublicDataUsingNfcError.Text = ex.Message;
                    this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            SetNfcAuthenticationParametersButton.IsEnabled = true;
        }

        /// <summary>
        /// Clear NFC authentication text Fields
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearNfcAuthenticationParametersButton_Click(object sender, RoutedEventArgs e)
        {
            NFCAuthFieldsUsrCntrl.ClearNFCAuthTextFields();
            ReadPublicDataUsingNfcError.Text = "";
            ReadCardPublicDataUsingNfcButton.IsEnabled = false;
        }

        /// <summary>
        /// Read public data from the card using NFC Device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReadCardPublicaDataUsingNfcButton_Click(object sender, RoutedEventArgs e)
        {
            ReadCardPublicDataUsingNfcButton.IsEnabled = false;
            PublicDataUsingNFCUsrCntrl.ClearPublicUsingNFCDataTextFields();
            ClearErrorTextFields();

            // local variables
            string requestId = null;

            this.ReadPublicDataUsingNfcError.Text = "Please wait...";
            this.ReadPublicDataUsingNfcError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ReadPublicDataUsingNfcError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.ReadPublicDataUsingNfcError.Text = "Please list and select a reader";
                        this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.ReadPublicDataUsingNfcError.Text = "Please connect to the card";
                        this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    bool readNonModifiableData = Convert.ToBoolean(PublicDataUsingNFCUsrCntrl.Nfc_ReadNonModifiableDataCheckbox.IsChecked);
                    bool readModifiableData = Convert.ToBoolean(PublicDataUsingNFCUsrCntrl.Nfc_ReadModifiableDataCheckbox.IsChecked);
                    bool readPhotography = Convert.ToBoolean(PublicDataUsingNFCUsrCntrl.Nfc_ReadPhotographyCheckbox.IsChecked);
                    bool readSignatueImage = Convert.ToBoolean(PublicDataUsingNFCUsrCntrl.Nfc_ReadSignatureCheckbox.IsChecked);
                    bool readAddress = Convert.ToBoolean(PublicDataUsingNFCUsrCntrl.Nfc_ReadAddressCheckbox.IsChecked);

                    // Reads Public Data From Card
                    CardPublicData cardPublicData = SelectedCardReader.ReadPublicData(
                                        requestId,
                                        readNonModifiableData,
                                        readModifiableData,
                                        readPhotography,
                                        readSignatueImage,
                                        readAddress);

                    // Fill data
                    PublicDataUsingNFCUsrCntrl.FillPublicUsingNFCDataTextFields(cardPublicData);

                    this.ReadPublicDataUsingNfcError.Text = cardPublicData.ResponseStatus.ToString();
                    this.ReadPublicDataUsingNfcError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.ReadPublicDataUsingNfcError.Text = tEx.Message;
                    this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ReadPublicDataUsingNfcError.Text = ex.Message;
                    this.ReadPublicDataUsingNfcError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            ReadCardPublicDataUsingNfcButton.IsEnabled = true;
        }

        /// <summary>
        /// Read the family book data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReadFamilyBookDataButton_Click(object sender, RoutedEventArgs e)
        {
            ReadFamilyBookDataButton.IsEnabled = false;
            FamilyBookDataUsrCntrl.ClearFamilyBookDataTextFields();
            ClearErrorTextFields();

            // local variables
            string requestId = null;

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ReadFamilyBookDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.ReadFamilyBookDataError.Text = "Please list and select a reader";
                        this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.ReadFamilyBookDataError.Text = "Please connect to the card";
                        this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Pin verification dialog
                    var result = VerifyPinDialogBox.ShowDialog();
                    if (false == result)
                    {
                        // If cancelled by user
                        ErrorDialogBox.ShowDialog("Error", "Cancelled by user");
                        await Task.Delay(2500);
                        ErrorDialogBox.HideDialog();

                        this.ReadFamilyBookDataError.Text = "Cancelled by user";
                        this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the pin from pin verification dialog
                    string pin = VerifyPinDialogBox.PasswordText.Password;
                    int pinLength = pin.Length;
                    if (pinLength < 4 || pinLength > 16)
                    {
                        this.ReadFamilyBookDataError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.ReadFamilyBookDataError.Text = "Please wait...";
                    this.ReadFamilyBookDataError.Foreground = Brushes.RoyalBlue;
                    await Task.Delay(100);

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    // Read Family Book Data from card
                    CardFamilyBookData cardFamilyBookData = SelectedCardReader.ReadFamilyBookData(
                        encodedPin);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, cardFamilyBookData.XmlString))
                    {
                        this.ReadFamilyBookDataError.Text = "Request ID verification failed, response is tampered";
                        this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(cardFamilyBookData.XmlString))
                    {
                        this.ReadFamilyBookDataError.Text = "Response signature verification failed, response is tampered";
                        this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Fill FamilyBook Data
                    FamilyBookDataUsrCntrl.FillFamilyBookDataTextFields(cardFamilyBookData);
                    this.ReadFamilyBookDataError.Text = cardFamilyBookData.ResponseStatus.ToString();
                    this.ReadFamilyBookDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    FamilyBookDataUsrCntrl.ClearFamilyBookDataTextFields();

                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                FamilyBookDataUsrCntrl.FamilyBookXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.ReadFamilyBookDataError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                                }
                                else
                                {
                                    this.ReadFamilyBookDataError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.ReadFamilyBookDataError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.ReadFamilyBookDataError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.ReadFamilyBookDataError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                        }
                        else
                        {
                            this.ReadFamilyBookDataError.Text = tEx.Message;
                        }
                    }
                    this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    FamilyBookDataUsrCntrl.ClearFamilyBookDataTextFields();
                    this.ReadFamilyBookDataError.Text = ex.Message;
                    this.ReadFamilyBookDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            VerifyPinDialogBox.PasswordText.Password = "";
            ReadFamilyBookDataButton.IsEnabled = true;
        }

        /// <summary>
        /// Checks the card status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CheckCardStatusButton_Click(object sender, RoutedEventArgs e)
        {
            this.CheckCardStatusButton.IsEnabled = false;
            this.CheckCardStatusUsrCntrl.CheckCardStatusText.Text = "";
            this.CheckCardStatusUsrCntrl.CardStatusXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            string requestId = null;

            this.CardStatusError.Text = "Please wait...";
            this.CardStatusError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.CardStatusError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.CardStatusError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.CardStatusError.Text = "Please list and select a reader";
                        this.CardStatusError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.CardStatusError.Text = "Please connect to the card";
                        this.CardStatusError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // checks the card status 
                    ToolkitResponse toolkitResponse = SelectedCardReader.CheckCardStatus(
                        requestId);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                    {
                        this.CardStatusError.Text = "Request ID verification failed, response is tampered";
                        this.CardStatusError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(toolkitResponse.XmlString))
                    {
                        this.CardStatusError.Text = "Response signature verification failed, response is tampered";
                        this.CardStatusError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.CheckCardStatusUsrCntrl.CardStatusXmlResponse.Text = toolkitResponse.XmlString;
                    this.CheckCardStatusUsrCntrl.CheckCardStatusText.Text = toolkitResponse.Status;
                    this.CardStatusError.Text = toolkitResponse.ResponseStatus.ToString();
                    this.CardStatusError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.CheckCardStatusUsrCntrl.CardStatusXmlResponse.Text = tEx.VGResponse;
                                this.CardStatusError.Text = tEx.Message;
                                this.CardStatusError.Foreground = Brushes.IndianRed;
                            }
                            else
                            {
                                this.CardStatusError.Text = "Signature verification failed";
                                this.CardStatusError.Foreground = Brushes.IndianRed;
                            }
                        }
                        else
                        {
                            this.CardStatusError.Text = "Request ID verification failed";
                            this.CardStatusError.Foreground = Brushes.IndianRed;
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        this.CardStatusError.Text = tEx.Message;
                        this.CardStatusError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (Exception ex)
                {
                    this.CardStatusError.Text = ex.Message;
                    this.CardStatusError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.CheckCardStatusButton.IsEnabled = true;
        }

        /// <summary>
        /// Get Finger Data from the Emirates ID Card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetFingerDataButton_Click(object sender, RoutedEventArgs e)
        {
            this.GetFingerDataButton.IsEnabled = false;
            FingerDataArray = null;
            GetFingerDataUsrCntrl.FirstFingerIdText.Text = "";
            GetFingerDataUsrCntrl.SecondFingerIdText.Text = "";
            GetFingerDataUsrCntrl.FirstFingerIndexText.Text = "";
            GetFingerDataUsrCntrl.SecondFingerIndexText.Text = "";
            ClearErrorTextFields();

            this.FingerDataError.Text = "Please wait...";
            this.FingerDataError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.FingerDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.FingerDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.FingerDataError.Text = "Please list and select a reader";
                        this.FingerDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.FingerDataError.Text = "Please connect to the card";
                        this.FingerDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get stored finger indexes from the Emirates ID Card
                    FingerDataArray = SelectedCardReader.GetFingerData();

                    this.GetFingerDataUsrCntrl.FirstFingerIdText.Text = Convert.ToString(FingerDataArray[0].FingerId);
                    this.GetFingerDataUsrCntrl.SecondFingerIdText.Text = Convert.ToString(FingerDataArray[1].FingerId);
                    this.GetFingerDataUsrCntrl.FirstFingerIndexText.Text = Convert.ToString(FingerDataArray[0].FingerIndex);
                    this.GetFingerDataUsrCntrl.SecondFingerIndexText.Text = Convert.ToString(FingerDataArray[1].FingerIndex);

                    this.FingerDataError.Text = "Success";
                    this.FingerDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.FingerDataError.Text = tEx.Message;
                    this.FingerDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.FingerDataError.Text = ex.Message;
                    this.FingerDataError.Foreground = Brushes.IndianRed;
                }

                // final break                
                break;
            }

            this.GetFingerDataButton.IsEnabled = true;
        }

        /// <summary>
        /// Verify Biometric Authentication on server 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AuthenticationBioOnServerButton_Click(object sender, RoutedEventArgs e)
        {
            this.AuthenticationBioOnServerButton.IsEnabled = false;
            this.AuthBioOnServerUsrCntrl.AuthenticateBioOnServerXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            string requestId = null;
            int sensorTimeout = 0;
            FingerData SelectedFingerData = null;

            this.AuthenticationBioOnServerError.Text = "Please wait...";
            this.AuthenticationBioOnServerError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    if (String.IsNullOrEmpty(AuthBioOnServerUsrCntrl.AuthenticateBioOnServerTimeoutText.Text))
                    {
                        this.AuthenticationBioOnServerError.Text = "Please enter sensor timeout";
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get sensor timeout
                    sensorTimeout = Convert.ToInt32(AuthBioOnServerUsrCntrl.AuthenticateBioOnServerTimeoutText.Text);
                    if (sensorTimeout <= 0)
                    {
                        this.AuthenticationBioOnServerError.Text = "Sensor timeout must be greater than zero";
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (AuthBioOnServerUsrCntrl.AuthenticateBioOnServerSelectFingerIndexComboBox.Items.Count != 0)
                    {
                        int index = AuthBioOnServerUsrCntrl.AuthenticateBioOnServerSelectFingerIndexComboBox.SelectedIndex;
                        SelectedFingerData = FingerDataArray[index];
                    }
                    else
                    {
                        this.AuthenticationBioOnServerError.Text = "Selected Finger index should not be empty";
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.AuthenticationBioOnServerError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.AuthenticationBioOnServerError.Text = "Please list and select a reader";
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.AuthenticationBioOnServerError.Text = "Please connect to the card";
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Authenticate Biometric On Server
                    ToolkitResponse toolkitResponse = SelectedCardReader.AuthenticateBiometricOnServer(
                        requestId,
                        SelectedFingerData.FingerIndex,
                        sensorTimeout);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                    {
                        this.AuthenticationBioOnServerError.Text = "Request ID verification failed, response is tampered";
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(toolkitResponse.XmlString))
                    {
                        this.AuthenticationBioOnServerError.Text = "Response signature verification failed, response is tampered";
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.AuthBioOnServerUsrCntrl.AuthenticateBioOnServerXmlResponse.Text = toolkitResponse.XmlString;
                    this.AuthenticationBioOnServerError.Text = toolkitResponse.Status;
                    this.AuthenticationBioOnServerError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.AuthBioOnServerUsrCntrl.AuthenticateBioOnServerXmlResponse.Text = tEx.VGResponse;
                                this.AuthenticationBioOnServerError.Text = tEx.Message;
                                this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                            }
                            else
                            {
                                this.AuthenticationBioOnServerError.Text = "Signature verification failed";
                                this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                            }
                        }
                        else
                        {
                            this.AuthenticationBioOnServerError.Text = "Request ID verification failed";
                            this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        this.AuthenticationBioOnServerError.Text = tEx.Message;
                        this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (Exception ex)
                {
                    this.AuthenticationBioOnServerError.Text = ex.Message;
                    this.AuthenticationBioOnServerError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.AuthenticationBioOnServerButton.IsEnabled = true;
        }

        /// <summary>
        /// Validates Emirates ID card and performs biometric authentication with 
        /// Validation Gateway (VG) service against the captured finger image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AuthCardAndBioButton_Click(object sender, RoutedEventArgs e)
        {
            this.AuthCardAndBioButton.IsEnabled = false;
            this.AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            string requestId = null;
            int sensorTimeout = 0;
            FingerData SelectedFingerData = null;

            this.AuthCardAndBioError.Text = "Please wait...";
            this.AuthCardAndBioError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    if (String.IsNullOrEmpty(AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioTimeoutText.Text))
                    {
                        this.AuthCardAndBioError.Text = "Please enter sensor timeout";
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get sensor timeout
                    sensorTimeout = Convert.ToInt32(AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioTimeoutText.Text);
                    if (sensorTimeout <= 0)
                    {
                        this.AuthCardAndBioError.Text = "Sensor timeout must be greater than zero";
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioSelectFingerIndexCB.Items.Count != 0)
                    {
                        int index = AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioSelectFingerIndexCB.SelectedIndex;
                        SelectedFingerData = FingerDataArray[index];
                    }
                    else
                    {
                        this.AuthCardAndBioError.Text = "Selected Finger index should not be empty";
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.AuthCardAndBioError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.AuthCardAndBioError.Text = "Please list and select a reader";
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.AuthCardAndBioError.Text = "Please connect to the card";
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Authenticate Biometric On Server
                    ToolkitResponse toolkitResponse = SelectedCardReader.AuthenticateCardAndBiometric(
                        requestId,
                        SelectedFingerData.FingerIndex,
                        sensorTimeout);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                    {
                        this.AuthCardAndBioError.Text = "Request ID verification failed, response is tampered";
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(toolkitResponse.XmlString))
                    {
                        this.AuthCardAndBioError.Text = "Response signature verification failed, response is tampered";
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioXmlResponse.Text = toolkitResponse.XmlString;
                    this.AuthCardAndBioError.Text = toolkitResponse.Status;
                    this.AuthCardAndBioError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.AuthCardAndBioOnServerUsrCntrl.AuthCardAndBioXmlResponse.Text = tEx.VGResponse;
                                this.AuthCardAndBioError.Text = tEx.Message;
                                this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                            }
                            else
                            {
                                this.AuthCardAndBioError.Text = "Signature verification failed";
                                this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                            }
                        }
                        else
                        {
                            this.AuthCardAndBioError.Text = "Request ID verification failed";
                            this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        this.AuthCardAndBioError.Text = tEx.Message;
                        this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (Exception ex)
                {
                    this.AuthCardAndBioError.Text = ex.Message;
                    this.AuthCardAndBioError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.AuthCardAndBioButton.IsEnabled = true;
        }

        /// <summary>
        /// Unblock ID card Pki pin
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UnblockPinButton_Click(object sender, RoutedEventArgs e)
        {
            this.UnblockPinButton.IsEnabled = false;
            this.UnblockPinUsrCntrl.UnblockPinXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            string pin = null;
            string requestId = null;
            int sensorTimeout = 0;
            FingerData SelectedFingerData = null;

            this.UnblockPinError.Text = "Please wait...";
            this.UnblockPinError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if fields are not empty
                    if (String.IsNullOrEmpty(UnblockPinUsrCntrl.UnblockPinText.Password))
                    {
                        this.UnblockPinError.Text = "Please enter the pin";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (String.IsNullOrEmpty(UnblockPinUsrCntrl.UnblockConfirmPinText.Password))
                    {
                        this.UnblockPinError.Text = "Please enter the confirm pin";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (String.IsNullOrEmpty(UnblockPinUsrCntrl.UnblockPinSensorTimeoutText.Text))
                    {
                        this.UnblockPinError.Text = "Please enter sensor timeout";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (!UnblockPinUsrCntrl.UnblockPinText.Password.Equals(UnblockPinUsrCntrl.UnblockConfirmPinText.Password))
                    {
                        this.UnblockPinError.Text = "Pin does not match";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (UnblockPinUsrCntrl.UnblockPinSelectFingerIndexComboBox.Items.Count != 0)
                    {
                        int index = UnblockPinUsrCntrl.UnblockPinSelectFingerIndexComboBox.SelectedIndex;
                        SelectedFingerData = FingerDataArray[index];
                    }
                    else
                    {
                        this.UnblockPinError.Text = "Selected Finger index should not be empty";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the pin
                    pin = UnblockPinUsrCntrl.UnblockPinText.Password;
                    if (pin.Length < 4 || pin.Length > 16)
                    {
                        this.UnblockPinError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get sensor timeout
                    sensorTimeout = Convert.ToInt32(UnblockPinUsrCntrl.UnblockPinSensorTimeoutText.Text);
                    if (sensorTimeout <= 0)
                    {
                        this.UnblockPinError.Text = "Sensor timeout must be greater than zero";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.UnblockPinError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.UnblockPinError.Text = "Please list and select a reader";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.UnblockPinError.Text = "Please connect to the card";
                        this.UnblockPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    // Encrypt and base64 encode of pin
                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    // Unblock ID card PKI pin
                    ToolkitResponse toolkitResponse = SelectedCardReader.UnblockPin(
                        encodedPin,
                        SelectedFingerData,
                        sensorTimeout);

                    // Check if XML string is available in response
                    if (!String.IsNullOrEmpty(toolkitResponse.XmlString))
                    {
                        // Compare request id
                        if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                        {
                            this.UnblockPinError.Text = "Request ID verification failed, response is tampered";
                            this.UnblockPinError.Foreground = Brushes.IndianRed;
                            break;
                        }

                        // Check signature is valid or not
                        if (!Helper.VerifySignature(toolkitResponse.XmlString))
                        {
                            this.UnblockPinError.Text = "Response signature verification failed, response is tampered";
                            this.UnblockPinError.Foreground = Brushes.IndianRed;
                            break;
                        }
                        this.UnblockPinUsrCntrl.UnblockPinXmlResponse.Text = toolkitResponse.XmlString;
                    }

                    this.UnblockPinError.Text = toolkitResponse.ResponseStatus.ToString();
                    this.UnblockPinError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.UnblockPinUsrCntrl.UnblockPinXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.UnblockPinError.Text = tEx.AttemptsLeft + " card block attempts left to retry this operation";
                                }
                                else
                                {
                                    this.UnblockPinError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.UnblockPinError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.UnblockPinError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.UnblockPinError.Text = tEx.AttemptsLeft + " card block attempts left to retry this operation";
                        }
                        else
                        {
                            this.UnblockPinError.Text = tEx.Message;
                        }
                    }
                    this.UnblockPinError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.UnblockPinError.Text = ex.Message;
                    this.UnblockPinError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.UnblockPinButton.IsEnabled = true;
        }

        /// <summary>
        /// Perform emirates ID card pin reset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ResetPinButton_Click(object sender, RoutedEventArgs e)
        {
            this.ResetPinButton.IsEnabled = false;
            this.ResetPinUsrCntrl.ResetPinXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            string pin = null;
            string requestId = null;
            int sensorTimeout = 0;
            FingerData SelectedFingerData = null;

            this.ResetPinError.Text = "Please wait...";
            this.ResetPinError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // check if pins are not empty
                    if (String.IsNullOrEmpty(ResetPinUsrCntrl.ResetPinText.Password))
                    {
                        this.ResetPinError.Text = "Please enter the pin";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (String.IsNullOrEmpty(ResetPinUsrCntrl.ResetConfirmPinText.Password))
                    {
                        this.ResetPinError.Text = "Please enter the confirm pin";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (String.IsNullOrEmpty(ResetPinUsrCntrl.ResetPinSensorTimeoutText.Text))
                    {
                        this.ResetPinError.Text = "Please enter the sensor timeout";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (!ResetPinUsrCntrl.ResetPinText.Password.Equals(ResetPinUsrCntrl.ResetConfirmPinText.Password))
                    {
                        this.ResetPinError.Text = "Pin does not match";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (ResetPinUsrCntrl.ResetPinSelectFingerIndexComboBox.Items.Count != 0)
                    {
                        int index = ResetPinUsrCntrl.ResetPinSelectFingerIndexComboBox.SelectedIndex;
                        SelectedFingerData = FingerDataArray[index];
                    }
                    else
                    {
                        this.ResetPinError.Text = "Selected Finger index should not be empty";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the pin
                    pin = ResetPinUsrCntrl.ResetPinText.Password;
                    if (pin.Length < 4 || pin.Length > 16)
                    {
                        this.ResetPinError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    sensorTimeout = Convert.ToInt32(ResetPinUsrCntrl.ResetPinSensorTimeoutText.Text);
                    if (sensorTimeout <= 0)
                    {
                        this.ResetPinError.Text = "Sensor timeout must be greater than zero";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ResetPinError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.ResetPinError.Text = "Please list and select a reader";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.ResetPinError.Text = "Please connect to the card";
                        this.ResetPinError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    // Encrypt and base64 encode of pin
                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    // Reset Pin
                    ToolkitResponse toolkitResponse = SelectedCardReader.ResetPin(
                        encodedPin,
                        SelectedFingerData,
                        sensorTimeout);

                    // Check if XML string is available in response
                    if (!String.IsNullOrEmpty(toolkitResponse.XmlString))
                    {
                        // Compare request id
                        if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                        {
                            this.ResetPinError.Text = "Request ID verification failed, response is tampered";
                            this.ResetPinError.Foreground = Brushes.IndianRed;
                            break;
                        }

                        // Check signature is valid or not
                        if (!Helper.VerifySignature(toolkitResponse.XmlString))
                        {
                            this.ResetPinError.Text = "Response signature verification failed, response is tampered";
                            this.ResetPinError.Foreground = Brushes.IndianRed;
                            break;
                        }
                        this.ResetPinUsrCntrl.ResetPinXmlResponse.Text = toolkitResponse.XmlString;
                    }

                    this.ResetPinError.Text = toolkitResponse.ResponseStatus.ToString();
                    this.ResetPinError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.ResetPinUsrCntrl.ResetPinXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.ResetPinError.Text = tEx.AttemptsLeft + " card block attempts left to retry this operation";
                                }
                                else
                                {
                                    this.ResetPinError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.ResetPinError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.ResetPinError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.ResetPinError.Text = tEx.AttemptsLeft + "card block attempts left to retry this operation";
                        }
                        else
                        {
                            this.ResetPinError.Text = tEx.Message;
                        }
                    }
                    this.ResetPinError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ResetPinError.Text = ex.Message;
                    this.ResetPinError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.ResetPinButton.IsEnabled = true;
        }

        /// <summary>
        /// Read PKI Certificates from the card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void GetPkiCertificatesButton_Click(object sender, RoutedEventArgs e)
        {
            this.GetPkiCertificatesButton.IsEnabled = false;
            this.GetCertificatesUsrCntrl.ExportAuthCertButton.IsEnabled = false;
            this.GetCertificatesUsrCntrl.ExportSignCertButton.IsEnabled = false;
            this.GetCertificatesUsrCntrl.GetPkiCertificatesXmlResponse.Text = "";
            GetCertificatesUsrCntrl.AuthenticationCertificateText.Document.Blocks.Clear();
            GetCertificatesUsrCntrl.SigningCertificateText.Document.Blocks.Clear();
            SigningCertificate = null;
            AuthenticateCertificate = null;
            ClearErrorTextFields();

            // local variables
            string requestId = null;
            string pin = null;
            int pinLength = 0;

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.GetPkiCertificatesError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.GetPkiCertificatesError.Text = "Please list and select a reader";
                        this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.GetPkiCertificatesError.Text = "Please connect to the card";
                        this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Pin verification dialog
                    var res = VerifyPinDialogBox.ShowDialog();
                    if (false == res)
                    {
                        // Cancelled by user
                        ErrorDialogBox.ShowDialog("Error", "Cancelled by user");
                        await Task.Delay(2500);
                        ErrorDialogBox.HideDialog();
                        this.GetPkiCertificatesError.Text = "Cancelled by user";
                        this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the pin from verification dialog
                    pin = VerifyPinDialogBox.PasswordText.Password;
                    pinLength = pin.Length;
                    if (pinLength < 4 || pinLength > 16)
                    {
                        this.GetPkiCertificatesError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.GetPkiCertificatesError.Text = "Please wait...";
                    this.GetPkiCertificatesError.Foreground = Brushes.RoyalBlue;
                    await Task.Delay(100);

                    // Generate Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    // Get the Emirates ID card certificates
                    CardCertificates cardCertificates = SelectedCardReader.GetPkiCertificates(encodedPin);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, cardCertificates.XmlString))
                    {
                        this.GetPkiCertificatesError.Text = "Request ID verification failed, response is tampered";
                        this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(cardCertificates.XmlString))
                    {
                        this.GetPkiCertificatesError.Text = "Response signature verification failed, response is tampered";
                        this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (cardCertificates.AuthenticationCertificate != null)
                    {
                        AuthenticateCertificate = cardCertificates.AuthenticationCertificate.ToArray();
                        string authenticateCertificateString = Convert.ToBase64String(AuthenticateCertificate);
                        GetCertificatesUsrCntrl.AuthenticationCertificateText.Document.Blocks.Add(new Paragraph(new Run(authenticateCertificateString)));
                    }

                    if (cardCertificates.SigningCertificate != null)
                    {
                        SigningCertificate = cardCertificates.SigningCertificate.ToArray();
                        string signingCertificateString = Convert.ToBase64String(SigningCertificate);
                        GetCertificatesUsrCntrl.SigningCertificateText.Document.Blocks.Add(new Paragraph(new Run(signingCertificateString)));
                    }

                    this.GetCertificatesUsrCntrl.GetPkiCertificatesXmlResponse.Text = cardCertificates.XmlString;
                    this.GetCertificatesUsrCntrl.ExportAuthCertButton.IsEnabled = true;
                    this.GetCertificatesUsrCntrl.ExportSignCertButton.IsEnabled = true;
                    this.GetPkiCertificatesError.Text = cardCertificates.ResponseStatus.ToString();
                    this.GetPkiCertificatesError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    GetCertificatesUsrCntrl.AuthenticationCertificateText.Document.Blocks.Clear();
                    GetCertificatesUsrCntrl.SigningCertificateText.Document.Blocks.Clear();
                    SigningCertificate = null;
                    AuthenticateCertificate = null;

                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.GetCertificatesUsrCntrl.GetPkiCertificatesXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.GetPkiCertificatesError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                                }
                                else
                                {
                                    this.GetPkiCertificatesError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.GetPkiCertificatesError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.GetPkiCertificatesError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.GetPkiCertificatesError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                        }
                        else
                        {
                            this.GetPkiCertificatesError.Text = tEx.Message;
                        }
                    }
                    this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    GetCertificatesUsrCntrl.AuthenticationCertificateText.Document.Blocks.Clear();
                    GetCertificatesUsrCntrl.SigningCertificateText.Document.Blocks.Clear();
                    SigningCertificate = null;
                    AuthenticateCertificate = null;
                    this.GetPkiCertificatesError.Text = ex.Message;
                    this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            VerifyPinDialogBox.PasswordText.Password = "";
            this.GetPkiCertificatesButton.IsEnabled = true;
        }

        /// <summary>
        /// Exports Authentication certificate's byte array data to a file 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportAuthCertButton_Click(object sender, RoutedEventArgs e)
        {
            this.GetCertificatesUsrCntrl.ExportAuthCertButton.IsEnabled = false;
            ClearErrorTextFields();

            if (null == AuthenticateCertificate || 0 == AuthenticateCertificate.Length)
            {
                this.GetPkiCertificatesError.Text = "Please read the certificates before exporting";
                this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                this.GetCertificatesUsrCntrl.ExportAuthCertButton.IsEnabled = true;
                return;
            }

            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Authentication Certificate";
                dlg.Filter = "Certificate file (*.cer)|*.cer";

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    // Save document
                    string base64String = Convert.ToBase64String(AuthenticateCertificate);
                    File.WriteAllText(dlg.FileName, base64String);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

            this.GetCertificatesUsrCntrl.ExportAuthCertButton.IsEnabled = true;
        }

        /// <summary>
        /// Exports Signing certificate's byte array data to a file 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportSignCertButton_Click(object sender, RoutedEventArgs e)
        {
            this.GetCertificatesUsrCntrl.ExportSignCertButton.IsEnabled = false;
            ClearErrorTextFields();

            if (null == SigningCertificate || 0 == SigningCertificate.Length)
            {
                this.GetPkiCertificatesError.Text = "Please read the certificates before exporting";
                this.GetPkiCertificatesError.Foreground = Brushes.IndianRed;
                this.GetCertificatesUsrCntrl.ExportSignCertButton.IsEnabled = true;
                return;
            }

            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.FileName = "Signing Certificate";
                dlg.Filter = "Certificate file (*.cer)|*.cer";

                // Show save file dialog box
                Nullable<bool> result = dlg.ShowDialog();

                if (result == true)
                {
                    // Save document
                    string base64String = Convert.ToBase64String(SigningCertificate);
                    File.WriteAllText(dlg.FileName, base64String);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }

            this.GetCertificatesUsrCntrl.ExportSignCertButton.IsEnabled = true;
        }

        /// <summary>
        /// Digitally sign data or hash with Authentication Certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SignDataWithAuthCertButton_Click(object sender, RoutedEventArgs e)
        {
            this.SignOrVerifyUsrCntrl.SignDataWithAuthCertButton.IsEnabled = false;
            this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = false;
            this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = false;
            this.SignOrVerifyUsrCntrl.SignDataXmlResponse.Text = "";
            SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
            Signature = null;
            ClearErrorTextFields();

            // local variables
            byte[] input = null;
            int inputLength = 0;
            int pinLength = 0;
            string pin = null;
            string requestId = null;
            bool isInputHash;
            TextRange plainTextRange = null;

            for (; ; )
            {
                // Get the data provided for Signing
                int inull = -1;
                int iGarbage = -1;

                plainTextRange = new TextRange(this.SignOrVerifyUsrCntrl.DataToSignText.Document.ContentStart,
                                                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.ContentEnd);
                inull = plainTextRange.Text.CompareTo("");
                if (0 == inull)
                {
                    this.SignDataError.Text = "Please enter data for signing";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                    break;
                }

                iGarbage = plainTextRange.Text.CompareTo("\r\n");
                if (0 == iGarbage)
                {
                    this.SignDataError.Text = "Please enter data for signing";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                    break;
                }

                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.SignDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.SignDataError.Text = "Please list and select a reader";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.SignDataError.Text = "Please connect to the card";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Converting the provided data into bytes
                    inputLength = plainTextRange.Text.Length - 2;
                    char[] PlainData = new char[inputLength];
                    plainTextRange.Text.CopyTo(0, PlainData, 0, inputLength);
                    input = Encoding.UTF8.GetBytes(PlainData);

                    // Pin verification dialog dialog
                    var result = VerifyPinDialogBox.ShowDialog();
                    if (false == result)
                    {
                        // Cancelled by user
                        this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                        ErrorDialogBox.ShowDialog("Error", "Cancelled by user");
                        await Task.Delay(2500);
                        ErrorDialogBox.HideDialog();
                        this.SignDataError.Text = "Cancelled by user";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the pin from pin verification dialog
                    pin = VerifyPinDialogBox.PasswordText.Password;
                    pinLength = pin.Length;
                    if (pinLength < 4 || pinLength > 16)
                    {
                        this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                        this.SignDataError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.SignDataError.Text = "Please wait...";
                    this.SignDataError.Foreground = Brushes.RoyalBlue;
                    await Task.Delay(100);

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    // Encrypt and base64 encode of pin
                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    isInputHash = false;

                    // Digitally sign data or hash provided with auth key
                    SignatureResponse signatureResponse = SelectedCardReader.SignChallenge(
                            input,
                            isInputHash,
                            encodedPin);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, signatureResponse.XmlString))
                    {
                        this.SignDataError.Text = "Request ID verification failed, response is tampered";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(signatureResponse.XmlString))
                    {
                        this.SignDataError.Text = "Response signature verification failed, response is tampered";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (signatureResponse.Signature != null)
                    {
                        Signature = signatureResponse.Signature.ToArray();
                        string signatureString = Convert.ToBase64String(Signature);
                        SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Add(new Paragraph(new Run(signatureString)));
                    }

                    this.SignOrVerifyUsrCntrl.SignDataXmlResponse.Text = signatureResponse.XmlString;
                    this.SignDataError.Text = signatureResponse.ResponseStatus.ToString();
                    this.SignDataError.Foreground = Brushes.RoyalBlue;
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = true;
                }
                catch (ToolkitException tEx)
                {
                    this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = false;
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = false;

                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.SignOrVerifyUsrCntrl.SignDataXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.SignDataError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                                }
                                else
                                {
                                    this.SignDataError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.SignDataError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.SignDataError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.SignDataError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                        }
                        else
                        {
                            this.SignDataError.Text = tEx.Message;
                        }
                    }
                    this.SignDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = false;
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = false;
                    this.SignDataError.Text = ex.Message;
                    this.SignDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            VerifyPinDialogBox.PasswordText.Password = "";
            this.SignOrVerifyUsrCntrl.SignDataWithAuthCertButton.IsEnabled = true;
        }

        /// <summary>
        /// Verify digital signature of data or hash with Authentication Certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void VerifySignatureWithAuthCertButton_Click(object sender, RoutedEventArgs e)
        {
            this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = false;
            ClearErrorTextFields();

            // local variables
            byte[] input = null;
            int inputLength = 0;
            int inull = -1;
            int iGarbage = -1;
            bool isInputHash = false;
            TextRange dataTextRange = null;

            for (; ; )
            {
                // Get the data provided for verifying
                dataTextRange = new TextRange(this.SignOrVerifyUsrCntrl.DataToSignText.Document.ContentStart,
                                                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.ContentEnd);
                inull = dataTextRange.Text.CompareTo("");
                if (0 == inull)
                {
                    this.SignDataError.Text = "No data is available for verification";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
                    break;
                }

                iGarbage = dataTextRange.Text.CompareTo("\r\n");
                if (0 == iGarbage)
                {
                    this.SignDataError.Text = "No data is available for verification";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
                    break;
                }

                if (null == AuthenticateCertificate || 0 == AuthenticateCertificate.Length)
                {
                    this.SignDataError.Text = "Authentication certificate is empty or null. Please read certificates before verifying";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
                    break;
                }

                if (null == Signature || 0 == Signature.Length)
                {
                    this.SignDataError.Text = "Please sign the data before verifying";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
                    break;
                }

                try
                {
                    this.SignDataError.Text = "Please wait...";
                    this.SignDataError.Foreground = Brushes.RoyalBlue;
                    await Task.Delay(100);

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.SignDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Convert the data provided into bytes
                    inputLength = dataTextRange.Text.Length - 2;
                    char[] PlainData = new char[inputLength];
                    dataTextRange.Text.CopyTo(0, PlainData, 0, inputLength);
                    input = Encoding.UTF8.GetBytes(PlainData);

                    // verify data signed using authenticate certificate
                    SelectedCardReader.VerifySignature(
                        input,
                        isInputHash,
                        Signature,
                        AuthenticateCertificate);

                    this.SignDataError.Text = "Success";
                    this.SignDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.SignDataError.Text = tEx.Message;
                    this.SignDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.SignDataError.Text = ex.Message;
                    this.SignDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = true;
        }

        /// <summary>
        /// Digitally sign data or hash with Signing Certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SignDataWithSignCertButton_Click(object sender, RoutedEventArgs e)
        {
            this.SignOrVerifyUsrCntrl.SignDataWithSignCertButton.IsEnabled = false;
            this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = false;
            this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = false;
            this.SignOrVerifyUsrCntrl.SignDataXmlResponse.Text = "";
            SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
            Signature = null;
            ClearErrorTextFields();

            // local variables
            byte[] input = null;
            int inputLength = 0;
            int pinLength = 0;
            string requestId = null;
            string pin = null;
            bool isInputHash;
            TextRange plainTextRange = null;

            for (; ; )
            {
                // Get the data provided for Signing
                int inull = -1;
                int iGarbage = -1;

                plainTextRange = new TextRange(this.SignOrVerifyUsrCntrl.DataToSignText.Document.ContentStart,
                                                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.ContentEnd);
                inull = plainTextRange.Text.CompareTo("");
                if (0 == inull)
                {
                    this.SignDataError.Text = "Please enter data for signing";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                    break;
                }

                iGarbage = plainTextRange.Text.CompareTo("\r\n");
                if (0 == iGarbage)
                {
                    this.SignDataError.Text = "Please enter data for signing";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                    break;
                }

                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.SignDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.SignDataError.Text = "Please list and select a reader";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.SignDataError.Text = "Please connect to the card";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Converting the provided data into bytes
                    inputLength = plainTextRange.Text.Length - 2;
                    char[] PlainData = new char[inputLength];
                    plainTextRange.Text.CopyTo(0, PlainData, 0, inputLength);
                    input = Encoding.UTF8.GetBytes(PlainData);

                    // Pin verification dialog 
                    var result = VerifyPinDialogBox.ShowDialog();
                    if (false == result)
                    {
                        // Cancelled by user
                        this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                        ErrorDialogBox.ShowDialog("Error", "Cancelled by user");
                        await Task.Delay(2500);
                        ErrorDialogBox.HideDialog();
                        this.SignDataError.Text = "Cancelled by user";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the pin from pin verification dialog
                    pin = VerifyPinDialogBox.PasswordText.Password;
                    pinLength = pin.Length;
                    if (pinLength < 4 || pinLength > 16)
                    {
                        this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                        this.SignDataError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.SignDataError.Text = "Please wait...";
                    this.SignDataError.Foreground = Brushes.RoyalBlue;
                    await Task.Delay(100);

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    // Encrypt and base64 encode of pin
                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    isInputHash = false;

                    // Digitally sign data or hash provided with sign key
                    SignatureResponse signatureResponse = SelectedCardReader.SignData(
                            input,
                            isInputHash,
                            encodedPin);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, signatureResponse.XmlString))
                    {
                        this.SignDataError.Text = "Request ID verification failed, response is tampered";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(signatureResponse.XmlString))
                    {
                        this.SignDataError.Text = "Response signature verification failed, response is tampered";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    if (signatureResponse.Signature != null)
                    {
                        Signature = signatureResponse.Signature.ToArray();
                        string signatureString = Convert.ToBase64String(Signature);
                        SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Add(new Paragraph(new Run(signatureString)));
                    }

                    this.SignOrVerifyUsrCntrl.SignDataXmlResponse.Text = signatureResponse.XmlString;
                    this.SignDataError.Text = signatureResponse.ResponseStatus.ToString();
                    this.SignDataError.Foreground = Brushes.RoyalBlue;
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = true;
                }
                catch (ToolkitException tEx)
                {
                    this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = false;
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = false;

                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.SignDataError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                                }
                                else
                                {
                                    this.SignDataError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.SignDataError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.SignDataError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.SignDataError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                        }
                        else
                        {
                            this.SignDataError.Text = tEx.Message;
                        }
                    }
                    this.SignDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.SignOrVerifyUsrCntrl.SignatureText.Document.Blocks.Clear();
                    this.SignDataError.Text = ex.Message;
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.IsEnabled = false;
                    this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = false;
                }

                // final break
                break;
            }

            VerifyPinDialogBox.PasswordText.Password = "";
            this.SignOrVerifyUsrCntrl.SignDataWithSignCertButton.IsEnabled = true;
        }

        /// <summary>
        /// Verify digital signature of data or hash with Signing Certificate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void VerifySignatureWithSignKeyButton_Click(object sender, RoutedEventArgs e)
        {
            this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = false;
            ClearErrorTextFields();

            // local variables
            byte[] input = null;
            int inputLength = 0;
            int inull = -1;
            int iGarbage = -1;
            bool isInputHash = false;
            TextRange dataTextRange = null;

            for (; ; )
            {
                // Get the data provided for verifying
                dataTextRange = new TextRange(this.SignOrVerifyUsrCntrl.DataToSignText.Document.ContentStart,
                                                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.ContentEnd);
                inull = dataTextRange.Text.CompareTo("");
                if (0 == inull)
                {
                    this.SignDataError.Text = "No plain data is available for verification";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
                    break;
                }

                iGarbage = dataTextRange.Text.CompareTo("\r\n");
                if (0 == iGarbage)
                {
                    this.SignDataError.Text = "No Data is available for verification";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
                    break;
                }

                if (null == SigningCertificate || 0 == SigningCertificate.Length)
                {
                    this.SignDataError.Text = "Signing certificate is empty or null. Please read certificates before verifying";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
                    break;
                }

                if (null == Signature || 0 == Signature.Length)
                {
                    this.SignDataError.Text = "Please sign the data before verifying";
                    this.SignDataError.Foreground = Brushes.IndianRed;
                    this.SignOrVerifyUsrCntrl.DataToSignText.Document.Blocks.Clear();
                    break;
                }

                try
                {
                    this.SignDataError.Text = "Please wait...";
                    this.SignDataError.Foreground = Brushes.RoyalBlue;
                    await Task.Delay(100);

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.SignDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.SignDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Convert the data provided into bytes
                    inputLength = dataTextRange.Text.Length - 2;
                    char[] PlainData = new char[inputLength];
                    dataTextRange.Text.CopyTo(0, PlainData, 0, inputLength);
                    input = Encoding.UTF8.GetBytes(PlainData);

                    // Verify data signed using signing certificate
                    SelectedCardReader.VerifySignature(
                        input,
                        isInputHash,
                        Signature,
                        SigningCertificate);

                    this.SignDataError.Text = "Success";
                    this.SignDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.SignDataError.Text = tEx.Message;
                    this.SignDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.SignDataError.Text = ex.Message;
                    this.SignDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.IsEnabled = true;
        }

        /// <summary>
        /// Perform emirates ID card PKI Authentication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AutheticatePkiButton_Click(object sender, RoutedEventArgs e)
        {
            this.PkiAuthenticationButton.IsEnabled = false;
            this.AuthenticatePkiUsrCntrl.AuthenticatePkiXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            string pin = null;
            string requestId = null;
            int pinLength = 0;

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.PkiAuthenticationError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.PkiAuthenticationError.Text = "Please list and select a reader";
                        this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.PkiAuthenticationError.Text = "Please connect to the card";
                        this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // pin verification dialog
                    var result = VerifyPinDialogBox.ShowDialog();
                    if (false == result)
                    {
                        // Cancelled by user
                        ErrorDialogBox.ShowDialog("Error", "Cancelled by user");
                        await Task.Delay(2500);
                        ErrorDialogBox.HideDialog();
                        this.PkiAuthenticationError.Text = "Cancelled by user";
                        this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the pin from verification dialog
                    pin = VerifyPinDialogBox.PasswordText.Password;
                    pinLength = pin.Length;
                    if (pinLength < 4 || pinLength > 16)
                    {
                        this.PkiAuthenticationError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.PkiAuthenticationError.Text = "Please wait...";
                    this.PkiAuthenticationError.Foreground = Brushes.RoyalBlue;
                    await Task.Delay(100);

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    // Encrypt and base64 encode of pin
                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    // PKI Authentication
                    ToolkitResponse toolkitResponse = SelectedCardReader.AuthenticatePki(encodedPin);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                    {
                        this.PkiAuthenticationError.Text = "Request ID verification failed, response is tampered";
                        this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(toolkitResponse.XmlString))
                    {
                        this.PkiAuthenticationError.Text = "Response signature verification failed, response is tampered";
                        this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.AuthenticatePkiUsrCntrl.AuthenticatePkiXmlResponse.Text = toolkitResponse.XmlString;
                    this.PkiAuthenticationError.Text = toolkitResponse.Status;
                    this.PkiAuthenticationError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.AuthenticatePkiUsrCntrl.AuthenticatePkiXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.PkiAuthenticationError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                                }
                                else
                                {
                                    this.PkiAuthenticationError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.PkiAuthenticationError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.PkiAuthenticationError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.PkiAuthenticationError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                        }
                        else
                        {
                            this.PkiAuthenticationError.Text = tEx.Message;
                        }
                    }
                    this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.PkiAuthenticationError.Text = ex.Message;
                    this.PkiAuthenticationError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            VerifyPinDialogBox.PasswordText.Password = "";
            this.PkiAuthenticationButton.IsEnabled = true;
        }

        /// <summary>
        /// Read Container Data from the card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ReadDataButton_Click(object sender, RoutedEventArgs e)
        {
            this.ReadDataButton.IsEnabled = false;
            readDataUsrCntrl.ClearDataContainerTextFields();
            ClearErrorTextFields();

            // local variables
            FileType fileType;
            string requestId = null;

            this.ReadDataError.Text = "Please wait...";
            this.ReadDataError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    if (readDataUsrCntrl.ReadDataFileTypeComboBox.Items.Count != 0)
                    {
                        fileType = (FileType)readDataUsrCntrl.ReadDataFileTypeComboBox.SelectedValue;
                    }
                    else
                    {
                        this.ReadDataError.Text = "Please select file type";
                        this.ReadDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.ReadDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.ReadDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.ReadDataError.Text = "Please list and select a reader";
                        this.ReadDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.ReadDataError.Text = "Please connect to the card";
                        this.ReadDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Read Container Data
                    DataContainer dataContainer = SelectedCardReader.ReadData(fileType, requestId);

                    readDataUsrCntrl.FillDataContainerTextFields(dataContainer);
                    this.ReadDataError.Text = dataContainer.ResponseStatus.ToString();
                    this.ReadDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.ReadDataError.Text = tEx.Message;
                    this.ReadDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ReadDataError.Text = ex.Message;
                    this.ReadDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.ReadDataButton.IsEnabled = true;
        }

        /// <summary>
        /// Update Container Data from the card
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void UpdateDataButton_Click(object sender, RoutedEventArgs e)
        {
            this.UpdateDataButton.IsEnabled = false;
            this.UpdateDataUsrCntrl.UpdateDataXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            ContainerType fileType;
            string requestId = null;

            this.UpdateDataError.Text = "Please wait...";
            this.UpdateDataError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    if (UpdateDataUsrCntrl.UpdateDataFileTypeComboBox.Items.Count != 0)
                    {
                        fileType = (ContainerType)UpdateDataUsrCntrl.UpdateDataFileTypeComboBox.SelectedValue;
                    }
                    else
                    {
                        this.UpdateDataError.Text = "Please select file type";
                        this.UpdateDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.UpdateDataError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.UpdateDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.UpdateDataError.Text = "Please list and select a reader";
                        this.UpdateDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.UpdateDataError.Text = "Please connect to the card";
                        this.UpdateDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    ToolkitResponse toolkitResponse = SelectedCardReader.UpdateData(fileType, requestId);

                    // Compare request id
                    if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                    {
                        this.UpdateDataError.Text = "Request ID verification failed, response is tampered";
                        this.UpdateDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check signature is valid or not
                    if (!Helper.VerifySignature(toolkitResponse.XmlString))
                    {
                        this.UpdateDataError.Text = "Response signature verification failed, response is tampered";
                        this.UpdateDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.UpdateDataUsrCntrl.UpdateDataXmlResponse.Text = toolkitResponse.XmlString;
                    this.UpdateDataError.Text = toolkitResponse.ResponseStatus.ToString();
                    this.UpdateDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                this.UpdateDataUsrCntrl.UpdateDataXmlResponse.Text = tEx.VGResponse;
                                this.UpdateDataError.Text = tEx.Message;
                                this.UpdateDataError.Foreground = Brushes.IndianRed;
                            }
                            else
                            {
                                this.UpdateDataError.Text = "Signature verification failed";
                                this.UpdateDataError.Foreground = Brushes.IndianRed;
                            }
                        }
                        else
                        {
                            this.UpdateDataError.Text = "Request ID verification failed";
                            this.UpdateDataError.Foreground = Brushes.IndianRed;
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        this.UpdateDataError.Text = tEx.Message;
                        this.UpdateDataError.Foreground = Brushes.IndianRed;
                    }
                }
                catch (Exception ex)
                {
                    this.UpdateDataError.Text = ex.Message;
                    this.UpdateDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.UpdateDataButton.IsEnabled = true;
        }

        #region DSS Functions

        #region Pades

        /// <summary>
        /// Browse pdf file for signing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowsePadesSignFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter for .pdf file extension
                fileDialog.Multiselect = false;
                fileDialog.Filter = "pdf Documents (*.pdf)|*.pdf";

                //Open file dialog by calling ShowDialog method 
                Nullable<bool> value = fileDialog.ShowDialog();

                // Get the selected file name
                PadesSignUsrCntrl.PadesSignFilePathText.Text = "";
                PadesSignFileDirectoryPath = "";
                PadesSignFileName = "";
                PadesSignFileExt = "";
                if (value == true)
                {
                    string file = fileDialog.FileName;
                    if (file.Length > 256)
                    {
                        MessageBox.Show("File path length must not be greater than 256 characters.", "Error");
                        return;
                    }
                    PadesSignUsrCntrl.PadesSignFilePathText.Text = file;
                    PadesSignFileDirectoryPath = Path.GetDirectoryName(file);
                    PadesSignFileName = Path.GetFileNameWithoutExtension(file);
                    PadesSignFileExt = Path.GetExtension(file);
                }
            }
            catch (Exception ex)
            {
                PadesSignUsrCntrl.PadesSignFilePathText.Text = "";
                PadesSignFileDirectoryPath = "";
                PadesSignFileName = "";
                PadesSignFileExt = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Browse image file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowsePadesSignatureImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension
                fileDialog.Multiselect = false;
                fileDialog.Filter = "JPG Files (*.jpg)|*.jpg|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|All Files (*.jpeg;*.jpg;*.png)|*.jpeg;*.jpg;*.png";

                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> value = fileDialog.ShowDialog();

                // Get the selected file name
                PadesSignUsrCntrl.PadesSignSignSignatureImagePathText.Text = "";
                if (value == true)
                {
                    string file = fileDialog.FileName;
                    if (file.Length > 256)
                    {
                        MessageBox.Show("File Path length must not be greater than 256 characters.", "Error");
                        return;
                    }
                    PadesSignUsrCntrl.PadesSignSignSignatureImagePathText.Text = file;
                }
            }
            catch (Exception ex)
            {
                PadesSignUsrCntrl.PadesSignSignSignatureImagePathText.Text = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Perform digital signing of PDF document following PAdES standard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PadesSignButton_Click(object sender, RoutedEventArgs e)
        {
            this.PadesSignButton.IsEnabled = false;
            PadesSignUsrCntrl.PadesSignXmlResponse.Text = "";
            ClearErrorTextFields();

            // local variables
            string requestId = null;
            string pin = null;
            int pinLength = 0;

            this.PadesError.Text = "Please wait...";
            this.PadesError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                if (String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignFilePathText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignPinText.Password) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignTsaUrlText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignOcspUrlText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignCertPathText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignCountryCodeText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignStateOrProvinceText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignPostalCodeText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignLocalityText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignStreetText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignReasonText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignerLocationText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignerContactInfoText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignatureXAxisText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignatureYAxisText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignSignatureImagePathText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignFontNameText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignBackgroundColorText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignFontColorText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignFontSizeText.Text) ||
                    String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignatureTextText.Text) || String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignPageNumberText.Text))
                {
                    PadesError.Text = "Please fill all the above fields";
                    PadesError.Foreground = Brushes.IndianRed;
                    break;
                }

                try
                {
                    // Get the pin from verification dialog
                    pin = PadesSignUsrCntrl.PadesSignPinText.Password;
                    pinLength = pin.Length;
                    if (pinLength < 4 || pinLength > 16)
                    {
                        this.PadesError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.PadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.PadesError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.PadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.PadesError.Text = "Please list and select a reader";
                        this.PadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.PadesError.Text = "Please connect to the card";
                        this.PadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    // Encrypt and base64 encode of pin
                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    // Fill digital signer location structure
                    SignerLocation signerLocation = new SignerLocation();
                    signerLocation.CountryCode = PadesSignUsrCntrl.PadesSignCountryCodeText.Text;
                    signerLocation.StateOrProvince = PadesSignUsrCntrl.PadesSignStateOrProvinceText.Text;
                    signerLocation.PostalCode = PadesSignUsrCntrl.PadesSignPostalCodeText.Text;
                    signerLocation.Locality = PadesSignUsrCntrl.PadesSignLocalityText.Text;
                    signerLocation.Street = PadesSignUsrCntrl.PadesSignStreetText.Text;

                    // Fill digital signature context structure
                    SigningContext signingContext = new SigningContext();
                    signingContext.SignatureLevel = (SignatureLevel)(PadesSignUsrCntrl.PadesSignSignatureLevelComboBox.SelectedValue);
                    signingContext.PackagingMode = (PackagingMode)(PadesSignUsrCntrl.PadesSignPackageModeComboBox.SelectedValue);
                    signingContext.Location = signerLocation;
                    signingContext.TsaUrl = PadesSignUsrCntrl.PadesSignTsaUrlText.Text;
                    signingContext.OcspUrl = PadesSignUsrCntrl.PadesSignOcspUrlText.Text;
                    signingContext.CertPath = PadesSignUsrCntrl.PadesSignCertPathText.Text;
                    signingContext.EncodedPin = encodedPin;

                    // Additional PADES specific parameters structure
                    PadesSignParams padesSignParams = new PadesSignParams();
                    padesSignParams.SignReason = PadesSignUsrCntrl.PadesSignSignReasonText.Text;
                    padesSignParams.SignerLocation = PadesSignUsrCntrl.PadesSignSignerLocationText.Text;
                    padesSignParams.SignerContactInfo = PadesSignUsrCntrl.PadesSignSignerContactInfoText.Text;
                    padesSignParams.SignatureXAxis = !String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignatureXAxisText.Text) ? Convert.ToInt32(PadesSignUsrCntrl.PadesSignSignatureXAxisText.Text) : 300;
                    padesSignParams.SignatureYAxis = !String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignSignatureYAxisText.Text) ? Convert.ToInt32(PadesSignUsrCntrl.PadesSignSignatureYAxisText.Text) : 500;
                    padesSignParams.SignatureImage = PadesSignUsrCntrl.PadesSignSignSignatureImagePathText.Text;
                    padesSignParams.FontName = PadesSignUsrCntrl.PadesSignFontNameText.Text;
                    padesSignParams.BackgroundColor = PadesSignUsrCntrl.PadesSignBackgroundColorText.Text;
                    padesSignParams.FontColor = PadesSignUsrCntrl.PadesSignFontColorText.Text;
                    padesSignParams.FontSize = !String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignFontSizeText.Text) ? Convert.ToInt32(PadesSignUsrCntrl.PadesSignFontSizeText.Text) : 20;
                    padesSignParams.SignatureText = PadesSignUsrCntrl.PadesSignSignatureTextText.Text;

                    if (true == PadesSignUsrCntrl.PadesSignSignVisible.IsChecked)
                        padesSignParams.SignVisible = 1;
                    else
                        padesSignParams.SignVisible = 0;

                    padesSignParams.NamePosition = (SignerNamePosition)(PadesSignUsrCntrl.PadesSignSignerNamePositionComboBoxText.SelectedValue);
                    padesSignParams.PageNumber = !String.IsNullOrEmpty(PadesSignUsrCntrl.PadesSignPageNumberText.Text) ? Convert.ToInt32(PadesSignUsrCntrl.PadesSignPageNumberText.Text) : 1;

                    // Get the path of the file to be signed
                    string pdfFilePath = PadesSignUsrCntrl.PadesSignFilePathText.Text;

                    // Set the output path for signed document  
                    string signedPdfFilePath = PadesSignFileDirectoryPath + "\\" + PadesSignFileName + "_signed" + PadesSignFileExt;

                    // Digital signing of PDF document
                    ToolkitResponse toolkitResponse = SelectedCardReader.PadesSign(signingContext, pdfFilePath, signedPdfFilePath, padesSignParams);

                    // Check if XML string is available in response
                    if (!String.IsNullOrEmpty(toolkitResponse.XmlString))
                    {
                        // Compare request id
                        if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                        {
                            this.PadesError.Text = "Request ID verification failed, response is tampered";
                            this.PadesError.Foreground = Brushes.IndianRed;
                            break;
                        }

                        // Check signature is valid or not
                        if (!Helper.VerifySignature(toolkitResponse.XmlString))
                        {
                            this.PadesError.Text = "Response signature verification failed, response is tampered";
                            this.PadesError.Foreground = Brushes.IndianRed;
                            break;
                        }
                        PadesSignUsrCntrl.PadesSignXmlResponse.Text = toolkitResponse.XmlString;
                    }

                    this.PadesError.Text = toolkitResponse.ResponseStatus.ToString();
                    this.PadesError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    PadesSignUsrCntrl.PadesSignPinText.Clear();

                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                PadesSignUsrCntrl.PadesSignXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.PadesError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                                }
                                else
                                {
                                    this.PadesError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.PadesError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.PadesError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.PadesError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                        }
                        else
                        {
                            this.PadesError.Text = tEx.Message;
                        }
                    }
                    this.PadesError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.PadesError.Text = ex.Message;
                    this.PadesError.Foreground = Brushes.IndianRed;
                }

                // Final break
                break;
            }

            this.PadesSignButton.IsEnabled = true;
        }

        /// <summary>
        /// Browse pdf file for verifying
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowsePadesVerifyFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter for .pdf file extension
                fileDialog.Multiselect = false;
                fileDialog.Filter = "pdf Documents (*.pdf)|*.pdf";

                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> retVal = fileDialog.ShowDialog();

                // Get the selected file name
                PadesVerifyUsrCntrl.PadesVerifyFilePathText.Text = "";
                PadesVerifyFileDirectoryPath = "";
                PadesVerifyFileName = "";
                PadesVerifyFileExt = "";
                if (retVal == true)
                {
                    string file = fileDialog.FileName;
                    if (file.Length > 256)
                    {
                        MessageBox.Show("File Path length must not be greater than 256 characters.", "Error");
                        return;
                    }
                    PadesVerifyUsrCntrl.PadesVerifyFilePathText.Text = file;
                    PadesVerifyFileDirectoryPath = Path.GetDirectoryName(file);
                    PadesVerifyFileName = Path.GetFileNameWithoutExtension(file);
                    PadesVerifyFileExt = Path.GetExtension(file);
                }
            }
            catch (Exception ex)
            {
                PadesVerifyUsrCntrl.PadesVerifyFilePathText.Text = "";
                PadesVerifyFileDirectoryPath = "";
                PadesVerifyFileName = "";
                PadesVerifyFileExt = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Perform digital verification of PDF document following PAdES standard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void PadesVerifyButton_Click(object sender, RoutedEventArgs e)
        {
            this.PadesVerifyButton.IsEnabled = false;
            PadesVerifyUsrCntrl.PadesVerifyVerificationReport.Text = "";
            ClearErrorTextFields();

            // local variables
            string verificationReport = null;

            this.PadesError.Text = "Please wait...";
            this.PadesError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {

                if (String.IsNullOrEmpty(PadesVerifyUsrCntrl.PadesVerifyFilePathText.Text) || String.IsNullOrEmpty(PadesVerifyUsrCntrl.PadesVerifyOcspUrlText.Text) ||
                    String.IsNullOrEmpty(PadesVerifyUsrCntrl.PadesVerifyCertPathText.Text))
                {
                    this.PadesError.Text = "Please fill required above fields";
                    this.PadesError.Foreground = Brushes.IndianRed;
                    break;
                }

                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.PadesError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.PadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.PadesError.Text = "Please list and select a reader";
                        this.PadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.PadesError.Text = "Please connect to the card";
                        this.PadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Fill verification context structure
                    VerificationContext verificationContext = new VerificationContext();
                    verificationContext.OcspUrl = PadesVerifyUsrCntrl.PadesVerifyOcspUrlText.Text;
                    verificationContext.CertPath = PadesVerifyUsrCntrl.PadesVerifyCertPathText.Text;
                    verificationContext.ReportType = (ReportType)(PadesVerifyUsrCntrl.PadesVerifyReportTypeComboBoxText.SelectedValue);
                    if (true == PadesVerifyUsrCntrl.PadesVerifyDocDetachedMode.IsChecked)
                    {
                        verificationContext.IsDeattached = true;
                    }
                    else
                    {
                        verificationContext.IsDeattached = false;
                    }

                    // Get the path of the file to verify signature
                    string signedPdfFilePath = PadesVerifyUsrCntrl.PadesVerifyFilePathText.Text;

                    // verification of PDF document
                    verificationReport = SelectedCardReader.PadesVerify(verificationContext, signedPdfFilePath);

                    PadesVerifyUsrCntrl.PadesVerifyVerificationReport.Text = verificationReport;
                    this.PadesError.Text = "Success";
                    this.PadesError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.PadesError.Text = tEx.Message;
                    this.PadesError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.PadesError.Text = ex.Message;
                    this.PadesError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.PadesVerifyButton.IsEnabled = true;
        }

        #endregion

        #region Xades

        /// <summary>
        /// Browse xml file for signing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseXadesSignFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter for .xml file extension
                fileDialog.Multiselect = false;
                fileDialog.Filter = "xml Documents (*.xml)|*.xml";

                //Open File Dialog by calling ShowDialog method 
                Nullable<bool> value = fileDialog.ShowDialog();

                // Get the selected file name
                XadesSignUsrCntrl.XadesSignFilePathText.Text = "";
                XadesSignFileDirectoryPath = "";
                XadesSignFileName = "";
                XadesSignFileExt = "";
                if (value == true)
                {
                    string file = fileDialog.FileName;
                    if (file.Length > 256)
                    {
                        MessageBox.Show("File Path length must not be greater than 256 characters.", "Error");
                        return;
                    }
                    XadesSignUsrCntrl.XadesSignFilePathText.Text = file;
                    XadesSignFileDirectoryPath = Path.GetDirectoryName(file);
                    XadesSignFileName = Path.GetFileNameWithoutExtension(file);
                    XadesSignFileExt = Path.GetExtension(file);
                }
            }
            catch (Exception ex)
            {
                XadesSignUsrCntrl.XadesSignFilePathText.Text = "";
                XadesSignFileDirectoryPath = "";
                XadesSignFileName = "";
                XadesSignFileExt = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Perform Digital signing of XML document following XAdES standard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void XadesSignButton_Click(object sender, RoutedEventArgs e)
        {
            this.XadesSignButton.IsEnabled = false;
            this.XadesError.Foreground = Brushes.RoyalBlue;
            XadesSignUsrCntrl.XadesSignSignatureText.Text = "";
            XadesSignUsrCntrl.XadesSignXmlResponse.Text = "";
            XadesSignature = null;
            ClearErrorTextFields();

            // local variables
            string requestId = null;
            string pin = null;
            int pinLength = 0;

            XadesError.Text = "Please wait...";
            XadesError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                if (String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignFilePathText.Text) || String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignPinText.Password) ||
                    String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignTsaUrlText.Text) || String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignOcspUrlText.Text) ||
                    String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignCertPathText.Text) || String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignCountryCodeText.Text) ||
                    String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignStateOrProvinceText.Text) || String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignPostalCodeText.Text) ||
                    String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignLoacalityText.Text) || String.IsNullOrEmpty(XadesSignUsrCntrl.XadesSignStreetText.Text))
                {
                    XadesError.Text = "Please fill all the above fields";
                    this.XadesError.Foreground = Brushes.IndianRed;
                    break;
                }

                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.XadesError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.XadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.XadesError.Text = "Please list and select a reader";
                        this.XadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.XadesError.Text = "Please connect to the card";
                        this.XadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get the pin from verification dialog
                    pin = XadesSignUsrCntrl.XadesSignPinText.Password;
                    pinLength = pin.Length;
                    if (pinLength < 4 || pinLength > 16)
                    {
                        this.XadesError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.XadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    // Encrypt and base64 encode of pin
                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    // Fill digital signer location structure
                    SignerLocation signerLocation = new SignerLocation();
                    signerLocation.CountryCode = XadesSignUsrCntrl.XadesSignCountryCodeText.Text;
                    signerLocation.StateOrProvince = XadesSignUsrCntrl.XadesSignStateOrProvinceText.Text;
                    signerLocation.PostalCode = XadesSignUsrCntrl.XadesSignPostalCodeText.Text;
                    signerLocation.Locality = XadesSignUsrCntrl.XadesSignLoacalityText.Text;
                    signerLocation.Street = XadesSignUsrCntrl.XadesSignStreetText.Text;

                    // Fill digital signature context structure
                    SigningContext signingContext = new SigningContext();
                    signingContext.SignatureLevel = (SignatureLevel)(XadesSignUsrCntrl.XadesSignSignatureLevelComboBox.SelectedValue);
                    signingContext.PackagingMode = (PackagingMode)(XadesSignUsrCntrl.XadesSignPackageModeComboBox.SelectedValue);
                    signingContext.Location = signerLocation;
                    signingContext.TsaUrl = XadesSignUsrCntrl.XadesSignTsaUrlText.Text;
                    signingContext.OcspUrl = XadesSignUsrCntrl.XadesSignOcspUrlText.Text;
                    signingContext.CertPath = XadesSignUsrCntrl.XadesSignCertPathText.Text;
                    signingContext.EncodedPin = encodedPin;

                    // Get the path of the file to be signed
                    string xmlFilePath = XadesSignUsrCntrl.XadesSignFilePathText.Text;

                    string signedXmlFilePath = null;
                    if (PackagingMode.Detached != signingContext.PackagingMode)
                    {
                        // Set the output path for signed document  
                        signedXmlFilePath = XadesSignFileDirectoryPath + "\\" + XadesSignFileName + "_signed" + XadesSignFileExt;
                    }

                    // Signing of XML document
                    ToolkitResponse toolkitResponse = SelectedCardReader.XadesSign(signingContext, xmlFilePath, signedXmlFilePath);

                    // Check if XML string is available in response
                    if (!String.IsNullOrEmpty(toolkitResponse.XmlString))
                    {
                        // Compare request id
                        if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                        {
                            this.XadesError.Text = "Request ID verification failed, response is tampered";
                            this.XadesError.Foreground = Brushes.IndianRed;
                            break;
                        }

                        // Check signature is valid or not
                        if (!Helper.VerifySignature(toolkitResponse.XmlString))
                        {
                            this.XadesError.Text = "Response signature verification failed, response is tampered";
                            this.XadesError.Foreground = Brushes.IndianRed;
                            break;
                        }
                        XadesSignUsrCntrl.XadesSignXmlResponse.Text = toolkitResponse.XmlString;
                    }

                    if (PackagingMode.Detached == signingContext.PackagingMode)
                    {
                        // If detached mode, then signature is available
                        XadesSignature = toolkitResponse.DetachedSignature.ToArray();
                        if (XadesSignature != null && XadesSignature.Length != 0)
                            XadesSignUsrCntrl.XadesSignSignatureText.Text = Convert.ToBase64String(XadesSignature);
                    }

                    this.XadesError.Text = toolkitResponse.ResponseStatus.ToString();
                    this.XadesError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    XadesSignUsrCntrl.XadesSignPinText.Clear();

                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                XadesSignUsrCntrl.XadesSignXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.XadesError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                                }
                                else
                                {
                                    this.XadesError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.XadesError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.XadesError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.XadesError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                        }
                        else
                        {
                            this.XadesError.Text = tEx.Message;
                        }
                    }
                    this.XadesError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.XadesError.Text = ex.Message;
                    this.XadesError.Foreground = Brushes.IndianRed;
                }

                // Final break
                break;
            }

            this.XadesSignButton.IsEnabled = true;
        }

        /// <summary>
        /// Browse XML file for verification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseXadesVerifyFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter for .xml file extension
                fileDialog.Multiselect = false;
                fileDialog.Filter = "xml Documents (*.xml)|*.xml";

                // OpenFileDialog by calling ShowDialog method 
                Nullable<bool> retVal = fileDialog.ShowDialog();

                // Get the selected file
                XadesVerifyUsrCntrl.XadesVerifyFilePathText.Text = "";
                XadesVerifyFileDirectoryPath = "";
                XadesVerifyFileName = "";
                XadesVerifyFileExt = "";
                if (retVal == true)
                {
                    string file = fileDialog.FileName;
                    if (file.Length > 256)
                    {
                        MessageBox.Show("File Path length must not be greater than 256 characters.", "Error");
                        return;
                    }
                    XadesVerifyUsrCntrl.XadesVerifyFilePathText.Text = file;
                    XadesVerifyFileDirectoryPath = Path.GetDirectoryName(file);
                    XadesVerifyFileName = Path.GetFileNameWithoutExtension(file);
                    XadesVerifyFileExt = Path.GetExtension(file);
                }
            }
            catch (Exception ex)
            {
                XadesVerifyUsrCntrl.XadesVerifyFilePathText.Text = "";
                XadesVerifyFileDirectoryPath = "";
                XadesVerifyFileName = "";
                XadesVerifyFileExt = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Perform Digital verification of XML document following XAdES standard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void XadesVerifyButton_Click(object sender, RoutedEventArgs e)
        {
            this.XadesVerifyButton.IsEnabled = false;
            XadesVerifyUsrCntrl.XadesVerifyVerificationReport.Text = "";
            ClearErrorTextFields();

            // local variables
            string verificationReport = null;

            this.XadesError.Text = "Please wait...";
            this.XadesError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {

                if (String.IsNullOrEmpty(XadesVerifyUsrCntrl.XadesVerifyFilePathText.Text) || String.IsNullOrEmpty(XadesVerifyUsrCntrl.XadesVerifyOcspUrlText.Text) ||
                    String.IsNullOrEmpty(XadesVerifyUsrCntrl.XadesVerifyCertPathText.Text))
                {
                    this.XadesError.Text = "Please fill required above fields";
                    this.XadesError.Foreground = Brushes.IndianRed;
                    break;
                }

                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.XadesError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.XadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.XadesError.Text = "Please list and select a reader";
                        this.XadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.XadesError.Text = "Please connect to the card";
                        this.XadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Fill verification context structure
                    VerificationContext verificationContext = new VerificationContext();
                    verificationContext.OcspUrl = XadesVerifyUsrCntrl.XadesVerifyOcspUrlText.Text;
                    verificationContext.CertPath = XadesVerifyUsrCntrl.XadesVerifyCertPathText.Text;
                    verificationContext.ReportType = (ReportType)(XadesVerifyUsrCntrl.XadesVerifyReportTypeComboBoxText.SelectedValue);
                    if (true == XadesVerifyUsrCntrl.XadesVerifyDocDetachedMode.IsChecked)
                    {
                        verificationContext.IsDeattached = true;
                    }
                    else
                    {
                        verificationContext.IsDeattached = false;
                    }

                    // Get the path of the file signed
                    string signedXmlFilePath = XadesVerifyUsrCntrl.XadesVerifyFilePathText.Text;

                    // Signature verification of XML document
                    verificationReport = SelectedCardReader.XadesVerify(verificationContext, signedXmlFilePath, XadesSignature);

                    XadesVerifyUsrCntrl.XadesVerifyVerificationReport.Text = verificationReport;
                    this.XadesError.Text = "Success";
                    this.XadesError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.XadesError.Text = tEx.Message;
                    this.XadesError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.XadesError.Text = ex.Message;
                    this.XadesError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }

            this.XadesVerifyButton.IsEnabled = true;
        }

        #endregion

        #region Cades

        /// <summary>
        /// Browse file for signing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseCadesSignFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter
                fileDialog.Multiselect = false;
                fileDialog.Filter = "All files (*.*)|*.*";

                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> retVal = fileDialog.ShowDialog();

                // Get the selected file name
                CadesSignUserCntrl.CadesSignFilePathText.Text = "";
                CadesSignFileDirectoryPath = "";
                CadesSignFileName = "";
                CadesSignFileExt = "";
                if (retVal == true)
                {
                    string file = fileDialog.FileName;
                    if (file.Length > 256)
                    {
                        MessageBox.Show("File Path length must not be greater than 256 characters.", "Error");
                        return;
                    }
                    CadesSignUserCntrl.CadesSignFilePathText.Text = file;
                    CadesSignFileDirectoryPath = Path.GetDirectoryName(file);
                    CadesSignFileName = Path.GetFileNameWithoutExtension(file);
                    CadesSignFileExt = Path.GetExtension(file);
                }
            }
            catch (Exception ex)
            {
                CadesSignUserCntrl.CadesSignFilePathText.Text = "";
                CadesSignFileDirectoryPath = "";
                CadesSignFileName = "";
                CadesSignFileExt = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Perform Digital signing of a document following CAdES standard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CadesSignButton_Click(object sender, RoutedEventArgs e)
        {
            this.CadesSignButton.IsEnabled = false;
            CadesSignUserCntrl.CadesSignSignatureText.Text = "";
            CadesSignUserCntrl.CadesSignXmlResponse.Text = "";
            CadesSignature = null;
            ClearErrorTextFields();

            // local variables
            string requestId = null;
            string pin = null;
            int pinLength = 0;

            this.CadesError.Text = "Please wait...";
            this.CadesError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                if (String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignFilePathText.Text) || String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignPinText.Password) ||
                    String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignTsaUrlText.Text) || String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignOcspUrlText.Text) ||
                    String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignCertPathText.Text) || String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignCountryCodeText.Text) ||
                    String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignStateOrProvinceText.Text) || String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignPostalCodeText.Text) ||
                    String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignLoacalityText.Text) || String.IsNullOrEmpty(CadesSignUserCntrl.CadesSignStreetText.Text))
                {
                    this.CadesError.Text = "Please fill all the above fields";
                    this.CadesError.Foreground = Brushes.IndianRed;
                    break;
                }

                try
                {
                    // Get the pin from verification dialog
                    pin = CadesSignUserCntrl.CadesSignPinText.Password;
                    pinLength = pin.Length;
                    if (pinLength < 4 || pinLength > 16)
                    {
                        this.CadesError.Text = "Pin cannot be less than 4 digits and greater than 16 digits";
                        this.CadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.CadesError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.CadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.CadesError.Text = "Please list and select a reader";
                        this.CadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.CadesError.Text = "Please connect to the card";
                        this.CadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Get Request ID
                    requestId = Helper.GenerateRequestId();

                    // Prepare request
                    string requestHandle = SelectedCardReader.PrepareRequest(requestId);

                    // Encrypt and base64 encode of pin
                    string encodedPin = Helper.Base64Encryption(requestHandle, pin, ToolkitObj);

                    // Fill digital signer location structure
                    SignerLocation signerLocation = new SignerLocation();
                    signerLocation.CountryCode = CadesSignUserCntrl.CadesSignCountryCodeText.Text;
                    signerLocation.StateOrProvince = CadesSignUserCntrl.CadesSignStateOrProvinceText.Text;
                    signerLocation.PostalCode = CadesSignUserCntrl.CadesSignPostalCodeText.Text;
                    signerLocation.Locality = CadesSignUserCntrl.CadesSignLoacalityText.Text;
                    signerLocation.Street = CadesSignUserCntrl.CadesSignStreetText.Text;

                    // Fill digital signature context structure
                    SigningContext signingContext = new SigningContext();
                    signingContext.SignatureLevel = (SignatureLevel)(CadesSignUserCntrl.CadesSignSignatureLevelCombobox.SelectedValue);
                    signingContext.PackagingMode = (PackagingMode)(CadesSignUserCntrl.CadesSignPackageModeComboBox.SelectedValue);
                    signingContext.Location = signerLocation;
                    signingContext.TsaUrl = CadesSignUserCntrl.CadesSignTsaUrlText.Text;
                    signingContext.OcspUrl = CadesSignUserCntrl.CadesSignOcspUrlText.Text;
                    signingContext.CertPath = CadesSignUserCntrl.CadesSignCertPathText.Text;
                    signingContext.EncodedPin = encodedPin;

                    // Get the path of the file to be signed
                    string inputFilePath = CadesSignUserCntrl.CadesSignFilePathText.Text;

                    // Signing of document
                    ToolkitResponse toolkitResponse = SelectedCardReader.CadesSign(
                        signingContext,
                        inputFilePath);

                    // Check if XML string is available in response
                    if (!String.IsNullOrEmpty(toolkitResponse.XmlString))
                    {
                        // Compare request id
                        if (!Helper.CompareRequestId(requestId, toolkitResponse.XmlString))
                        {
                            this.CadesError.Text = "Request ID verification failed, response is tampered";
                            this.CadesError.Foreground = Brushes.IndianRed;
                            break;
                        }

                        // Check signature is valid or not
                        if (!Helper.VerifySignature(toolkitResponse.XmlString))
                        {
                            this.CadesError.Text = "Response signature verification failed, response is tampered";
                            this.CadesError.Foreground = Brushes.IndianRed;
                            break;
                        }
                        CadesSignUserCntrl.CadesSignXmlResponse.Text = toolkitResponse.XmlString;
                    }

                    CadesSignature = toolkitResponse.DetachedSignature.ToArray();
                    CadesSignUserCntrl.CadesSignSignatureText.Text = Convert.ToBase64String(CadesSignature);

                    this.CadesError.Text = toolkitResponse.ResponseStatus.ToString();
                    this.CadesError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    CadesSignUserCntrl.CadesSignPinText.Clear();

                    // Check if response is available 
                    if (!String.IsNullOrEmpty(tEx.VGResponse))
                    {
                        // Compare Request ID of response
                        if (Helper.CompareRequestId(requestId, tEx.VGResponse))
                        {
                            // Check signature of response is valid or not
                            if (Helper.VerifySignature(tEx.VGResponse))
                            {
                                CadesSignUserCntrl.CadesSignXmlResponse.Text = tEx.VGResponse;
                                if (ExceptionType.CardPinError == tEx.ExceptionType)
                                {
                                    this.CadesError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                                }
                                else
                                {
                                    this.CadesError.Text = tEx.Message;
                                }
                            }
                            else
                            {
                                this.CadesError.Text = "Signature verification failed";
                            }
                        }
                        else
                        {
                            this.CadesError.Text = "Request ID verification failed";
                        }
                    }
                    else
                    {
                        // If response is not available then print the error message
                        if (ExceptionType.CardPinError == tEx.ExceptionType)
                        {
                            this.CadesError.Text = "Wrong pin entered, " + tEx.AttemptsLeft + " attempts left to retry this operation";
                        }
                        else
                        {
                            this.CadesError.Text = tEx.Message;
                        }
                    }
                    this.CadesError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.CadesError.Text = ex.Message;
                    this.CadesError.Foreground = Brushes.IndianRed;
                }

                // Final break
                break;
            }
            this.CadesSignButton.IsEnabled = true;
        }

        /// <summary>
        /// Browse file for verification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseCadesVerifyFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter
                fileDialog.Multiselect = false;
                fileDialog.Filter = "All files (*.*)|*.*";

                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> retVal = fileDialog.ShowDialog();

                // Get the selected file name
                CadesVerifyUserCntrl.CadesVerifyFilePathText.Text = "";
                if (retVal == true)
                {
                    string file = fileDialog.FileName;
                    if (file.Length > 256)
                    {
                        MessageBox.Show("File Path length must not be greater than 256 characters.", "Error");
                        return;
                    }
                    CadesVerifyUserCntrl.CadesVerifyFilePathText.Text = file;
                }
            }
            catch (Exception ex)
            {
                CadesVerifyUserCntrl.CadesVerifyFilePathText.Text = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        /// <summary>
        /// Perform Digital verification of a document following CAdES standard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CadesVerifyButton_Click(object sender, RoutedEventArgs e)
        {
            this.CadesVerifyButton.IsEnabled = false;
            CadesVerifyUserCntrl.CadesVerifyVerificationReport.Text = "";
            ClearErrorTextFields();

            // local variables
            string verificationReport = null;

            CadesError.Text = "Please wait...";
            this.CadesError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                if (String.IsNullOrEmpty(CadesVerifyUserCntrl.CadesVerifyOcspUrlText.Text) || String.IsNullOrEmpty(CadesVerifyUserCntrl.CadesVerifyCertPathText.Text) ||
                    String.IsNullOrEmpty(CadesVerifyUserCntrl.CadesVerifyFilePathText.Text))
                {
                    this.CadesError.Text = "Please fill required above fields";
                    this.CadesError.Foreground = Brushes.IndianRed;
                    break;
                }

                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.CadesError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.CadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if a reader is selected or not
                    if (!IsReaderSelected)
                    {
                        this.CadesError.Text = "Please list and select a reader";
                        this.CadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.CadesError.Text = "Please connect to the card";
                        this.CadesError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Fill verification context structure
                    VerificationContext verificationContext = new VerificationContext();
                    verificationContext.OcspUrl = CadesVerifyUserCntrl.CadesVerifyOcspUrlText.Text;
                    verificationContext.CertPath = CadesVerifyUserCntrl.CadesVerifyCertPathText.Text; ;
                    verificationContext.ReportType = (ReportType)(CadesVerifyUserCntrl.CadesVerifyReportTypeComboBoxText.SelectedValue);
                    if (true == CadesVerifyUserCntrl.CadesVerifyDocDetachedMode.IsChecked)
                    {
                        verificationContext.IsDeattached = true;
                    }
                    else
                    {
                        verificationContext.IsDeattached = false;
                    }

                    // Get the path of the file to verify signature
                    string signedFilePath = CadesVerifyUserCntrl.CadesVerifyFilePathText.Text;

                    // Signature verification of file
                    verificationReport = SelectedCardReader.CadesVerify(
                        verificationContext,
                        signedFilePath,
                        CadesSignature);

                    CadesVerifyUserCntrl.CadesVerifyVerificationReport.Text = verificationReport;
                    this.CadesError.Text = "Success";
                    this.CadesError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.CadesError.Text = tEx.Message;
                    this.CadesError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.CadesError.Text = ex.Message;
                    this.CadesError.Foreground = Brushes.IndianRed;
                }

                // Final break
                break;
            }
            this.CadesVerifyButton.IsEnabled = true;
        }

        #endregion

        #endregion

        /// <summary>
        /// Disconnect the connected reader
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            this.DisconnectButton.IsEnabled = false;
            ClearErrorTextFields();

            this.DisconnectError.Text = "Please wait...";
            this.DisconnectError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.DisconnectError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.DisconnectError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Check if card is connected or not
                    if (!IsCardConnected)
                    {
                        this.DisconnectError.Text = "Card is not connected";
                        this.DisconnectError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Disconnect the connected reader
                    SelectedCardReader.Disconnect();

                    IsCardConnected = false;
                    ClearAllTextFields();
                    this.DisconnectError.Text = "Success";
                    this.DisconnectError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.DisconnectError.Text = tEx.Message;
                    this.DisconnectError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.DisconnectError.Text = ex.Message;
                    this.DisconnectError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.DisconnectButton.IsEnabled = true;
        }

        /// <summary>
        /// Cleanup Emirates ID Card Toolkit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CleanupToolkitButton_Click(object sender, RoutedEventArgs e)
        {
            this.CleanupToolkitButton.IsEnabled = false;
            ClearErrorTextFields();

            this.CleanupToolkitError.Text = "Please wait...";
            this.CleanupToolkitError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if Toolkit is Initialized or not
                    if (!IsToolkitInitialized)
                    {
                        this.CleanupToolkitError.Text =
                            "Toolkit is not initialized, please initialize the toolkit";
                        this.CleanupToolkitError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    // Toolkit cleanup
                    ToolkitObj.Cleanup();

                    // Toolkit Version
                    ToolkitVersionUsrCntrl.ToolkitVersionText.Text = "";

                    // Register Device
                    RegisterDeviceUsrCntrl.RegisterDeviceUserNameText.Text = "";
                    RegisterDeviceUsrCntrl.RegisterDevicePasswordText.Clear();
                    RegisterDeviceUsrCntrl.RegisterDeviceReferenceIdText.Text = "";
                    RegisterDeviceUsrCntrl.RegisterDeviceRegistrationIdText.Text = "";
                    RegisterDeviceUsrCntrl.RegisterDeviceXmlResponse.Text = "";

                    // List Readers
                    ListReadersUsrCntrl.ReadersListComboBox.ItemsSource = null;
                    GetReaderWithEmiratesIdUsrCntrl.ReaderWithEmiratesIdCardText.Text = "";

                    // Device ID
                    DeviceIdUsrCntrl.DeviceIdText.Text = "";

                    // License Expiry
                    LicenseExpiryDateUsrCntrl.LicenseExpiryDateText.Text = "";

                    // Config Cert Expiry Date
                    ConfiCertExpiryDateUsrCntrl.ConfigVGCertExpiryDateText.Text = "";
                    ConfiCertExpiryDateUsrCntrl.ConfigLVCertExpiryDateText.Text = "";
                    ConfiCertExpiryDateUsrCntrl.ServerTLSCertExpiryDateText.Text = "";
                    ConfiCertExpiryDateUsrCntrl.ConfigAGCertExpiryDateText.Text = "";
                    ConfiCertExpiryDateUsrCntrl.ConfigLicenseExpiryDateText.Text = "";

                    //Mrz Data
                    ParseMRZDataUsrCntrl.ClearParseMRZDataTextFields();

                    // DSS
                    ClearDigitalSignatureTextFields();

                    InitializeToolkitButton.IsEnabled = true;
                    this.CleanupToolkitError.Text = "Success";
                    this.CleanupToolkitError.Foreground = Brushes.RoyalBlue;

                    IsToolkitInitialized = false;
                    IsReaderSelected = false;
                    IsCardConnected = false;
                    ToolkitObj = null;
                    SelectedCardReader = null;
                    ClearAllTextFields();
                }
                catch (ToolkitException tEx)
                {
                    this.CleanupToolkitButton.IsEnabled = true;
                    this.CleanupToolkitError.Text = tEx.Message;
                    this.CleanupToolkitError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.CleanupToolkitButton.IsEnabled = true;
                    this.CleanupToolkitError.Text = ex.Message;
                    this.CleanupToolkitError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
        }

        /// <summary>
        /// Parse EF Data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ParseEFDataButton_Click(object sender, RoutedEventArgs e)
        {
            this.ParseEFDataButton.IsEnabled = false;
            this.ParseEFDataUsrCntrl.ParsedEFdataText.Text = "";
            ClearErrorTextFields();

            // local variables
            byte[] efData;

            this.ParseEFDataError.Text = "Please wait...";
            this.ParseEFDataError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                try
                {
                    // Check if EF Data provided is empty
                    if (String.IsNullOrEmpty(ParseEFDataUsrCntrl.EFdataText.Text))
                    {
                        this.ParseEFDataError.Text = "Please enter EF Data.";
                        this.ParseEFDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    efData = Convert.FromBase64String(ParseEFDataUsrCntrl.EFdataText.Text);

                    // Parse EF Data
                    string parsedEFData = CardReader.ParseEFData(efData);

                    if (String.IsNullOrEmpty(parsedEFData))
                    {
                        this.ParseEFDataError.Text = "Parsed EF data is null or empty";
                        this.ParseEFDataError.Foreground = Brushes.IndianRed;
                        break;
                    }

                    this.ParseEFDataUsrCntrl.ParsedEFdataText.Text = parsedEFData;

                    this.ParseEFDataError.Text = "Success";
                    this.ParseEFDataError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.ParseEFDataError.Text = tEx.Message;
                    this.ParseEFDataError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ParseEFDataError.Text = ex.Message;
                    this.ParseEFDataError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.ParseEFDataButton.IsEnabled = true;
        }

        /// <summary>
        /// Validate the Toolkit Response
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ValidateToolkitResponseButton_Click(object sender, RoutedEventArgs e)
        {
            this.ValidateToolkitResponseButton.IsEnabled = false;
            this.ValidateToolkitResponseError.Foreground = Brushes.RoyalBlue;
            ResponseValidationUsrCntrl.ClearResponseValidationTextFields();
            ClearErrorTextFields();

            // local variables
            byte[] certificateData = null;
            byte[] certificateChain = null;
            string toolkitResponse = null;

            ValidateToolkitResponseError.Text = "Please wait...";
            ValidateToolkitResponseError.Foreground = Brushes.RoyalBlue;
            await Task.Delay(100);

            for (; ; )
            {
                if (String.IsNullOrEmpty(ResponseValidationUsrCntrl.ToolkitResponseText.Text) || String.IsNullOrEmpty(ResponseValidationUsrCntrl.CertificateDataFilePathText.Text) ||
                    String.IsNullOrEmpty(ResponseValidationUsrCntrl.CertificateChainFilePathText.Text))
                {
                    ValidateToolkitResponseError.Text = "Please fill all the above fields";
                    ValidateToolkitResponseError.Foreground = Brushes.IndianRed;
                    break;
                }
                try
                {
                    certificateData = File.ReadAllBytes(ResponseValidationUsrCntrl.CertificateDataFilePathText.Text);
                    certificateChain = File.ReadAllBytes(ResponseValidationUsrCntrl.CertificateChainFilePathText.Text);
                    toolkitResponse = ResponseValidationUsrCntrl.ToolkitResponseText.Text;

                    SignatureValidator signatureValidator = new SignatureValidator(certificateData, certificateChain);
                    ToolkitResponseData toolkitResponseData = signatureValidator.ValidateToolkitResponse(toolkitResponse);

                    // Fill the fields
                    ResponseValidationUsrCntrl.FillResponseValidationTextFields(toolkitResponseData);

                    this.ValidateToolkitResponseError.Text = "Success";
                    this.ValidateToolkitResponseError.Foreground = Brushes.RoyalBlue;
                }
                catch (ToolkitException tEx)
                {
                    this.ValidateToolkitResponseError.Text = tEx.Message;
                    this.ValidateToolkitResponseError.Foreground = Brushes.IndianRed;
                }
                catch (Exception ex)
                {
                    this.ValidateToolkitResponseError.Text = ex.Message;
                    this.ValidateToolkitResponseError.Foreground = Brushes.IndianRed;
                }

                // final break
                break;
            }
            this.ValidateToolkitResponseButton.IsEnabled = true;
        }

        /// <summary>
        /// This event will be executed when Main Window is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ListReadersUsrCntrl.ReadersListComboBox.SelectionChanged += ListOfReadersCombobox_SelectionChanged;
            CardPublicDataEFUsrCntrl.CardPublicDataEFTypeComboBox.ItemsSource = Enum.GetValues(typeof(PublicDataEFType)).Cast<PublicDataEFType>();
            GetCertificatesUsrCntrl.ExportAuthCertButton.Click += ExportAuthCertButton_Click;
            GetCertificatesUsrCntrl.ExportSignCertButton.Click += ExportSignCertButton_Click;
            SignOrVerifyUsrCntrl.SignDataWithAuthCertButton.Click += SignDataWithAuthCertButton_Click;
            SignOrVerifyUsrCntrl.SignDataWithSignCertButton.Click += SignDataWithSignCertButton_Click;
            SignOrVerifyUsrCntrl.VerifySignatureWithAuthCertButton.Click += VerifySignatureWithAuthCertButton_Click;
            SignOrVerifyUsrCntrl.VerifySignatureWithSignCertButton.Click += VerifySignatureWithSignKeyButton_Click;

            var signatureLevels = Enum.GetValues(typeof(SignatureLevel)).Cast<SignatureLevel>();
            PadesSignUsrCntrl.PadesSignSignatureLevelComboBox.ItemsSource = signatureLevels;
            XadesSignUsrCntrl.XadesSignSignatureLevelComboBox.ItemsSource = signatureLevels;
            CadesSignUserCntrl.CadesSignSignatureLevelCombobox.ItemsSource = signatureLevels;

            var packagingModes = Enum.GetValues(typeof(PackagingMode)).Cast<PackagingMode>();
            PadesSignUsrCntrl.PadesSignPackageModeComboBox.ItemsSource = packagingModes;
            XadesSignUsrCntrl.XadesSignPackageModeComboBox.ItemsSource = packagingModes;
            CadesSignUserCntrl.CadesSignPackageModeComboBox.ItemsSource = packagingModes.Where(i => i.ToString() == "Detached");

            PadesSignUsrCntrl.PadesSignSignerNamePositionComboBoxText.ItemsSource =
                Enum.GetValues(typeof(SignerNamePosition)).Cast<SignerNamePosition>();

            var reportTypes = Enum.GetValues(typeof(ReportType)).Cast<ReportType>();
            PadesVerifyUsrCntrl.PadesVerifyReportTypeComboBoxText.ItemsSource = reportTypes;
            XadesVerifyUsrCntrl.XadesVerifyReportTypeComboBoxText.ItemsSource = reportTypes;
            CadesVerifyUserCntrl.CadesVerifyReportTypeComboBoxText.ItemsSource = reportTypes;

            var fileTypes = Enum.GetValues(typeof(FileType)).Cast<FileType>();
            readDataUsrCntrl.ReadDataFileTypeComboBox.ItemsSource = fileTypes.Take(2);
            UpdateDataUsrCntrl.UpdateDataFileTypeComboBox.ItemsSource = fileTypes;

            PadesSignUsrCntrl.BrowsePadesSignFile.Click += BrowsePadesSignFile_Click;
            PadesSignUsrCntrl.BrowsePadesSignatureImage.Click += BrowsePadesSignatureImage_Click;
            PadesVerifyUsrCntrl.BrowsePadesVerifyFile.Click += BrowsePadesVerifyFile_Click;
            XadesSignUsrCntrl.BrowseXadesSignFile.Click += BrowseXadesSignFile_Click;
            XadesVerifyUsrCntrl.BrowseXadesVerifyFile.Click += BrowseXadesVerifyFile_Click;
            CadesSignUserCntrl.BrowseCadesSignFile.Click += BrowseCadesSignFile_Click;
            CadesVerifyUserCntrl.BrowseCadesVerifyFile.Click += BrowseCadesVerifyFile_Click;

            InitializeToolkitButton.IsEnabled = true;
            CleanupToolkitButton.IsEnabled = false;
        }
    }
}


