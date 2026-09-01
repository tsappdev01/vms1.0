using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for operations that work without a card connection:
    /// MRZ parsing, License Expiry, Config Cert Expiry, Validate Toolkit Response
    /// </summary>
    public class ToolkitUtilitiesViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // MRZ
        private string _mrzInput;
        private string _mrzDocumentType;
        private string _mrzIssuedCountry;
        private string _mrzCardNumber;
        private string _mrzIdNumber;
        private string _mrzDateOfBirth;
        private string _mrzGender;
        private string _mrzExpiryDate;
        private string _mrzNationality;
        private string _mrzFullName;

        // License Expiry
        private string _licenseExpiryDate;

        // Config Cert Expiry
        private string _configVGCertExpiry;
        private string _configLVCertExpiry;
        private string _serverTLSCertExpiry;
        private string _configAGCertExpiry;
        private string _configLicenseExpiry;

        // Validate Toolkit Response
        private string _validateXmlResponse;
        private string _validateCertFilePath;
        private string _validateChainFilePath;
        private string _validateService;
        private string _validateAction;
        private string _validateRequestId;
        private string _validateNonce;
        private string _validateCorrelationId;
        private string _validateCsn;
        private string _validateCardNumber;
        private string _validateIdNumber;
        private string _validateTimeStamp;
        private string _validateValidityInterval;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Commands
        public RelayCommand ParseMrzCommand { get; private set; }

        public RelayCommand GetLicenseExpiryCommand { get; private set; }

        public RelayCommand GetConfigCertExpiryCommand { get; private set; }

        public RelayCommand ValidateResponseCommand { get; private set; }

        public RelayCommand BrowseCertFileCommand { get; private set; }

        public RelayCommand BrowseChainFileCommand { get; private set; }

        // MRZ Properties
        public string MrzInput
        {
            get { return _mrzInput; }
            set { SetProperty(ref _mrzInput, value); }
        }

        public string MrzDocumentType
        {
            get { return _mrzDocumentType; }
            set { SetProperty(ref _mrzDocumentType, value); }
        }

        public string MrzIssuedCountry
        {
            get { return _mrzIssuedCountry; }
            set { SetProperty(ref _mrzIssuedCountry, value); }
        }

        public string MrzCardNumber
        {
            get { return _mrzCardNumber; }
            set { SetProperty(ref _mrzCardNumber, value); }
        }

        public string MrzIdNumber
        {
            get { return _mrzIdNumber; }
            set { SetProperty(ref _mrzIdNumber, value); }
        }

        public string MrzDateOfBirth
        {
            get { return _mrzDateOfBirth; }
            set { SetProperty(ref _mrzDateOfBirth, value); }
        }

        public string MrzGender
        {
            get { return _mrzGender; }
            set { SetProperty(ref _mrzGender, value); }
        }

        public string MrzExpiryDate
        {
            get { return _mrzExpiryDate; }
            set { SetProperty(ref _mrzExpiryDate, value); }
        }

        public string MrzNationality
        {
            get { return _mrzNationality; }
            set { SetProperty(ref _mrzNationality, value); }
        }

        public string MrzFullName
        {
            get { return _mrzFullName; }
            set { SetProperty(ref _mrzFullName, value); }
        }

        // License Expiry Properties
        public string LicenseExpiryDate
        {
            get { return _licenseExpiryDate; }
            set { SetProperty(ref _licenseExpiryDate, value); }
        }

        // Config Cert Expiry Properties
        public string ConfigVGCertExpiry
        {
            get { return _configVGCertExpiry; }
            set { SetProperty(ref _configVGCertExpiry, value); }
        }

        public string ConfigLVCertExpiry
        {
            get { return _configLVCertExpiry; }
            set { SetProperty(ref _configLVCertExpiry, value); }
        }

        public string ServerTLSCertExpiry
        {
            get { return _serverTLSCertExpiry; }
            set { SetProperty(ref _serverTLSCertExpiry, value); }
        }

        public string ConfigAGCertExpiry
        {
            get { return _configAGCertExpiry; }
            set { SetProperty(ref _configAGCertExpiry, value); }
        }

        public string ConfigLicenseExpiry
        {
            get { return _configLicenseExpiry; }
            set { SetProperty(ref _configLicenseExpiry, value); }
        }

        // Validate Toolkit Response Properties
        public string ValidateXmlResponse
        {
            get { return _validateXmlResponse; }
            set { SetProperty(ref _validateXmlResponse, value); }
        }

        public string ValidateCertFilePath
        {
            get { return _validateCertFilePath; }
            set { SetProperty(ref _validateCertFilePath, value); }
        }

        public string ValidateChainFilePath
        {
            get { return _validateChainFilePath; }
            set { SetProperty(ref _validateChainFilePath, value); }
        }

        public string ValidateService
        {
            get { return _validateService; }
            set { SetProperty(ref _validateService, value); }
        }

        public string ValidateAction
        {
            get { return _validateAction; }
            set { SetProperty(ref _validateAction, value); }
        }

        public string ValidateRequestId
        {
            get { return _validateRequestId; }
            set { SetProperty(ref _validateRequestId, value); }
        }

        public string ValidateNonce
        {
            get { return _validateNonce; }
            set { SetProperty(ref _validateNonce, value); }
        }

        public string ValidateCorrelationId
        {
            get { return _validateCorrelationId; }
            set { SetProperty(ref _validateCorrelationId, value); }
        }

        public string ValidateCsn
        {
            get { return _validateCsn; }
            set { SetProperty(ref _validateCsn, value); }
        }

        public string ValidateCardNumber
        {
            get { return _validateCardNumber; }
            set { SetProperty(ref _validateCardNumber, value); }
        }

        public string ValidateIdNumber
        {
            get { return _validateIdNumber; }
            set { SetProperty(ref _validateIdNumber, value); }
        }

        public string ValidateTimeStamp
        {
            get { return _validateTimeStamp; }
            set { SetProperty(ref _validateTimeStamp, value); }
        }

        public string ValidateValidityInterval
        {
            get { return _validateValidityInterval; }
            set { SetProperty(ref _validateValidityInterval, value); }
        }

        // Status
        public string OperationStatus
        {
            get { return _operationStatus; }
            set { SetProperty(ref _operationStatus, value); }
        }

        public SolidColorBrush OperationStatusColor
        {
            get { return _operationStatusColor; }
            set { SetProperty(ref _operationStatusColor, value); }
        }

        public ToolkitUtilitiesViewModel(MainViewModel main)
        {
            _main = main;

            ParseMrzCommand = new RelayCommand(WrapAsync(ParseMrzAsync));
            GetLicenseExpiryCommand = new RelayCommand(WrapAsync(GetLicenseExpiryAsync));
            GetConfigCertExpiryCommand = new RelayCommand(WrapAsync(GetConfigCertExpiryAsync));
            ValidateResponseCommand = new RelayCommand(WrapAsync(ValidateResponseAsync));
            BrowseCertFileCommand = new RelayCommand(() => BrowseCertFile());
            BrowseChainFileCommand = new RelayCommand(() => BrowseChainFile());
        }

        private void SetOpStatus(string message, bool isSuccess)
        {
            OperationStatus = message;
            OperationStatusColor = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
            _main.SetStatus(message, isSuccess);
        }

        private async Task ParseMrzAsync()
        {
            if (string.IsNullOrEmpty(MrzInput))
            {
                SetOpStatus("Please enter MRZ data", false);
                return;
            }

            _main.SetBusy(true, "Parsing MRZ data...");

            try
            {
                MRZData data = await Task.Run(() => _main.Toolkit.ParseMRZData(MrzInput));

                MrzDocumentType = data.DocumentType;
                MrzIssuedCountry = data.IssuedCountry;
                MrzCardNumber = data.CardNumber;
                MrzIdNumber = data.IdNumber;
                MrzDateOfBirth = data.DateOfBirth;
                MrzGender = data.Gender;
                MrzExpiryDate = data.CardExpiryDate;
                MrzNationality = data.Nationality;
                MrzFullName = data.FullName;

                SetOpStatus("MRZ data parsed successfully", true);
            }
            catch (ToolkitException tEx)
            {
                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private async Task GetLicenseExpiryAsync()
        {
            _main.SetBusy(true, "Getting license expiry date...");

            try
            {
                string expiryDate = await Task.Run(() => _main.Toolkit.GetLicenseExpiryDate());

                if (!string.IsNullOrEmpty(expiryDate))
                {
                    LicenseExpiryDate = expiryDate;
                    SetOpStatus("License expiry date: " + expiryDate, true);
                }
                else
                {
                    SetOpStatus("License expiry date is null or empty", false);
                }
            }
            catch (ToolkitException tEx)
            {
                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private async Task GetConfigCertExpiryAsync()
        {
            _main.SetBusy(true, "Getting config certificate expiry dates...");

            try
            {
                ConfigCertExpiryDate certExpiry = await Task.Run(
                    () => _main.Toolkit.GetConfigCertificateExpiryDate());

                ConfigVGCertExpiry = certExpiry.ConfigVGCertExpiry;
                ConfigLVCertExpiry = certExpiry.ConfigLVCertExpiry;
                ServerTLSCertExpiry = certExpiry.ServerTLSCertExpiry;
                ConfigAGCertExpiry = certExpiry.ConfigAGCertExpiry;
                ConfigLicenseExpiry = certExpiry.LicenseExpiry;

                SetOpStatus("Config certificate expiry dates retrieved", true);
            }
            catch (ToolkitException tEx)
            {
                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private void BrowseCertFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Certificate (*.cer)|*.cer|Certificate (*.cert)|*.cert|All Files (*.*)|*.*";
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == true)
            {
                ValidateCertFilePath = dlg.FileName;
            }
        }

        private void BrowseChainFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Certificate (*.cer)|*.cer|Certificate (*.cert)|*.cert|All Files (*.*)|*.*";
            dlg.Multiselect = false;

            if (dlg.ShowDialog() == true)
            {
                ValidateChainFilePath = dlg.FileName;
            }
        }

        private async Task ValidateResponseAsync()
        {
            if (string.IsNullOrEmpty(ValidateXmlResponse) ||
                string.IsNullOrEmpty(ValidateCertFilePath))
            {
                SetOpStatus("Please provide XML response and certificate file", false);
                return;
            }

            _main.SetBusy(true, "Validating toolkit response...");

            try
            {
                byte[] certData = File.ReadAllBytes(ValidateCertFilePath);
                byte[] chainData = null;

                if (!string.IsNullOrEmpty(ValidateChainFilePath))
                {
                    chainData = File.ReadAllBytes(ValidateChainFilePath);
                }

                ToolkitResponseData responseData = await Task.Run(
                    () => _main.Toolkit.ValidateToolkitResponse(
                        ValidateXmlResponse, certData, chainData));

                ValidateService = responseData.Service;
                ValidateAction = responseData.Action;
                ValidateRequestId = responseData.RequsetId;
                ValidateNonce = responseData.Nonce;
                ValidateCorrelationId = responseData.CorrelationId;
                ValidateCsn = responseData.CSN;
                ValidateCardNumber = responseData.CardNumber;
                ValidateIdNumber = responseData.IdNumber;
                ValidateTimeStamp = responseData.TimeStamp;
                ValidateValidityInterval = responseData.ValidityInterval.ToString();

                SetOpStatus("Toolkit response validated successfully", true);
            }
            catch (ToolkitException tEx)
            {
                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }
    }
}
