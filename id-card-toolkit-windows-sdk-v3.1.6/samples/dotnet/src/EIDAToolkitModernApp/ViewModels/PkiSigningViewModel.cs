using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for PKI Authentication, Sign Data (Auth/Sign cert), and Verify Signature
    /// </summary>
    public class PkiSigningViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // PKI Authentication
        private string _pkiPin;
        private string _pkiXmlResponse;
        private string _pkiAuthStatus;

        // Sign Data with Auth Cert
        private string _signAuthDataInput;
        private string _signAuthPin;
        private string _signAuthSignature;
        private string _signAuthXmlResponse;

        // Verify Signature with Auth Cert
        private string _verifyAuthResult;

        // Sign Data with Sign Cert
        private string _signDataInput;
        private string _signDataPin;
        private string _signDataSignature;
        private string _signDataXmlResponse;

        // Verify Signature with Sign Cert
        private string _verifySignResult;

        // Certificates (stored from PKI read)
        private byte[] _authCertificate;
        private byte[] _signCertificate;
        private byte[] _lastAuthSignature;
        private byte[] _lastSignSignature;
        private string _authCertPem;
        private string _signCertPem;
        private string _certPin;

        // Family Book Data
        private string _familyBookPin;
        private HeadOfFamily _familyBookHead;
        private ObservableCollection<Wife> _familyBookWives;
        private ObservableCollection<Child> _familyBookChildren;
        private string _familyBookXmlResponse;
        private bool _hasFamilyBookHead;
        private bool _hasFamilyBookWives;
        private bool _hasFamilyBookChildren;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Card PIN must be 4-16 digits on the EID card.
        private const int MinPinLength = 4;
        private const int MaxPinLength = 16;

        // Commands
        public RelayCommand AuthenticatePkiCommand { get; private set; }

        public RelayCommand SignDataAuthCertCommand { get; private set; }

        public RelayCommand VerifySignatureAuthCertCommand { get; private set; }

        public RelayCommand SignDataSignCertCommand { get; private set; }

        public RelayCommand VerifySignatureSignCertCommand { get; private set; }

        public RelayCommand ReadCertificatesCommand { get; private set; }

        public RelayCommand ReadFamilyBookDataCommand { get; private set; }

        // Certificate Properties
        public string CertPin
        {
            get { return _certPin; }
            set { SetProperty(ref _certPin, value); }
        }

        public string AuthCertPem
        {
            get { return _authCertPem; }
            set { SetProperty(ref _authCertPem, value); }
        }

        public string SignCertPem
        {
            get { return _signCertPem; }
            set { SetProperty(ref _signCertPem, value); }
        }

        // PKI Authentication Properties
        public string PkiPin
        {
            get { return _pkiPin; }
            set { SetProperty(ref _pkiPin, value); }
        }

        public string PkiXmlResponse
        {
            get { return _pkiXmlResponse; }
            set { SetProperty(ref _pkiXmlResponse, value); }
        }

        public string PkiAuthStatus
        {
            get { return _pkiAuthStatus; }
            set { SetProperty(ref _pkiAuthStatus, value); }
        }

        // Sign with Auth Cert Properties
        public string SignAuthDataInput
        {
            get { return _signAuthDataInput; }
            set { SetProperty(ref _signAuthDataInput, value); }
        }

        public string SignAuthPin
        {
            get { return _signAuthPin; }
            set { SetProperty(ref _signAuthPin, value); }
        }

        public string SignAuthSignature
        {
            get { return _signAuthSignature; }
            set { SetProperty(ref _signAuthSignature, value); }
        }

        public string SignAuthXmlResponse
        {
            get { return _signAuthXmlResponse; }
            set { SetProperty(ref _signAuthXmlResponse, value); }
        }

        // Verify Auth Cert Properties
        public string VerifyAuthResult
        {
            get { return _verifyAuthResult; }
            set { SetProperty(ref _verifyAuthResult, value); }
        }

        // Sign with Sign Cert Properties
        public string SignDataInput
        {
            get { return _signDataInput; }
            set { SetProperty(ref _signDataInput, value); }
        }

        public string SignDataPin
        {
            get { return _signDataPin; }
            set { SetProperty(ref _signDataPin, value); }
        }

        public string SignDataSignature
        {
            get { return _signDataSignature; }
            set { SetProperty(ref _signDataSignature, value); }
        }

        public string SignDataXmlResponse
        {
            get { return _signDataXmlResponse; }
            set { SetProperty(ref _signDataXmlResponse, value); }
        }

        // Verify Sign Cert Properties
        public string VerifySignResult
        {
            get { return _verifySignResult; }
            set { SetProperty(ref _verifySignResult, value); }
        }

        // Family Book Data Properties
        public string FamilyBookPin
        {
            get { return _familyBookPin; }
            set { SetProperty(ref _familyBookPin, value); }
        }

        public HeadOfFamily FamilyBookHead
        {
            get { return _familyBookHead; }
            set
            {
                if (SetProperty(ref _familyBookHead, value))
                {
                    HasFamilyBookHead = value != null;
                }
            }
        }

        public ObservableCollection<Wife> FamilyBookWives
        {
            get { return _familyBookWives; }
            set
            {
                if (SetProperty(ref _familyBookWives, value))
                {
                    HasFamilyBookWives = value != null && value.Count > 0;
                }
            }
        }

        public ObservableCollection<Child> FamilyBookChildren
        {
            get { return _familyBookChildren; }
            set
            {
                if (SetProperty(ref _familyBookChildren, value))
                {
                    HasFamilyBookChildren = value != null && value.Count > 0;
                }
            }
        }

        public string FamilyBookXmlResponse
        {
            get { return _familyBookXmlResponse; }
            set { SetProperty(ref _familyBookXmlResponse, value); }
        }

        public bool HasFamilyBookHead
        {
            get { return _hasFamilyBookHead; }
            set { SetProperty(ref _hasFamilyBookHead, value); }
        }

        public bool HasFamilyBookWives
        {
            get { return _hasFamilyBookWives; }
            set { SetProperty(ref _hasFamilyBookWives, value); }
        }

        public bool HasFamilyBookChildren
        {
            get { return _hasFamilyBookChildren; }
            set { SetProperty(ref _hasFamilyBookChildren, value); }
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

        public PkiSigningViewModel(MainViewModel main)
        {
            _main = main;

            AuthenticatePkiCommand = new RelayCommand(WrapAsync(AuthenticatePkiAsync));
            SignDataAuthCertCommand = new RelayCommand(WrapAsync(SignDataAuthCertAsync));
            VerifySignatureAuthCertCommand = new RelayCommand(WrapAsync(VerifySignatureAuthCertAsync));
            SignDataSignCertCommand = new RelayCommand(WrapAsync(SignDataSignCertAsync));
            VerifySignatureSignCertCommand = new RelayCommand(WrapAsync(VerifySignatureSignCertAsync));
            ReadCertificatesCommand = new RelayCommand(WrapAsync(ReadCertificatesAsync));
            ReadFamilyBookDataCommand = new RelayCommand(WrapAsync(ReadFamilyBookDataAsync));
        }

        private void SetOpStatus(string message, bool isSuccess)
        {
            OperationStatus = message;
            OperationStatusColor = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
            _main.SetStatus(message, isSuccess);
        }

        private bool ValidatePin(string pin)
        {
            if (string.IsNullOrEmpty(pin))
            {
                SetOpStatus("Please enter a PIN", false);
                return false;
            }

            if ((pin.Length < MinPinLength) || (pin.Length > MaxPinLength))
            {
                SetOpStatus("PIN must be between "
                    + MinPinLength + " and " + MaxPinLength + " digits", false);
                return false;
            }

            return true;
        }

        private async Task AuthenticatePkiAsync()
        {
            if (!ValidatePin(PkiPin))
            {
                return;
            }

            _main.SetBusy(true, "Performing PKI authentication...");

            try
            {
                ToolkitResponse response = await Task.Run(
                    () => _main.Toolkit.AuthenticatePki(PkiPin));

                PkiXmlResponse = response.XmlString;
                PkiAuthStatus = !string.IsNullOrEmpty(response.Status)
                    ? response.Status
                    : response.ResponseStatus.ToString();
                SetOpStatus("PKI Authentication: " + PkiAuthStatus, true);
                PkiPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    PkiXmlResponse = tEx.VGResponse;
                }

                if (ExceptionType.CardPinError == tEx.ExceptionType)
                {
                    SetOpStatus("Wrong PIN entered, " + tEx.AttemptsLeft + " attempts left", false);
                }
                else
                {
                    SetOpStatus(tEx.Message, false);
                }
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

        private static string FormatPem(byte[] certData, string label)
        {
            string base64 = Convert.ToBase64String(certData);
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("-----BEGIN " + label + "-----");
            for (int i = 0; i < base64.Length; i += 64)
            {
                sb.AppendLine(base64.Substring(i, Math.Min(64, base64.Length - i)));
            }

            sb.AppendLine("-----END " + label + "-----");
            return sb.ToString();
        }

        private async Task ReadCertificatesAsync()
        {
            _main.SetBusy(true, "Reading PKI certificates from card...");

            try
            {
                string pin = CertPin;

                if (!ValidatePin(pin))
                {
                    _main.SetBusy(false);
                    return;
                }

                CardCertificates certs = await Task.Run(
                    () => _main.Toolkit.GetPkiCertificates(pin));

                if (certs.AuthenticationCertificate != null)
                {
                    _authCertificate = certs.AuthenticationCertificate.ToArray();
                    AuthCertPem = FormatPem(_authCertificate, "CERTIFICATE");
                }

                if (certs.SigningCertificate != null)
                {
                    _signCertificate = certs.SigningCertificate.ToArray();
                    SignCertPem = FormatPem(_signCertificate, "CERTIFICATE");
                }

                SetOpStatus("PKI certificates read successfully", true);
                CertPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                if (ExceptionType.CardPinError == tEx.ExceptionType)
                {
                    SetOpStatus("Wrong PIN entered, " + tEx.AttemptsLeft + " attempts left", false);
                }
                else
                {
                    SetOpStatus(tEx.Message, false);
                }
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

        private void ClearFamilyBookData()
        {
            FamilyBookHead = null;
            FamilyBookWives = new ObservableCollection<Wife>();
            FamilyBookChildren = new ObservableCollection<Child>();
            FamilyBookXmlResponse = string.Empty;
        }

        private async Task ReadFamilyBookDataAsync()
        {
            if (!ValidatePin(FamilyBookPin))
            {
                return;
            }

            ClearFamilyBookData();
            _main.SetBusy(true, "Reading family book data from card...");

            try
            {
                CardFamilyBookData data = await Task.Run(
                    () => _main.Toolkit.ReadFamilyBookData(FamilyBookPin));

                FamilyBookHead = data.HeadOfFamily;

                if (data.Wives != null)
                {
                    FamilyBookWives = new ObservableCollection<Wife>(data.Wives);
                }

                if (data.Children != null)
                {
                    FamilyBookChildren = new ObservableCollection<Child>(data.Children);
                }

                FamilyBookXmlResponse = data.XmlString;
                SetOpStatus("Family book data read: " + data.ResponseStatus.ToString(), true);
                FamilyBookPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    FamilyBookXmlResponse = tEx.VGResponse;
                }

                if (ExceptionType.CardPinError == tEx.ExceptionType)
                {
                    SetOpStatus("Wrong PIN entered, " + tEx.AttemptsLeft + " attempts left", false);
                }
                else
                {
                    SetOpStatus(tEx.Message, false);
                }
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

        private async Task SignDataAuthCertAsync()
        {
            if (string.IsNullOrEmpty(SignAuthDataInput))
            {
                SetOpStatus("Please enter data to sign", false);
                return;
            }

            if (!ValidatePin(SignAuthPin))
            {
                return;
            }

            _main.SetBusy(true, "Signing data with authentication certificate...");

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(SignAuthDataInput);

                SignatureResponse response = await Task.Run(
                    () => _main.Toolkit.SignDataWithAuthCert(data, false, SignAuthPin));

                if (response.Signature != null)
                {
                    _lastAuthSignature = response.Signature.ToArray();
                    SignAuthSignature = Convert.ToBase64String(_lastAuthSignature);
                }

                SignAuthXmlResponse = response.XmlString;
                SetOpStatus("Data signed with auth cert: " + response.ResponseStatus.ToString(), true);
                SignAuthPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                if (ExceptionType.CardPinError == tEx.ExceptionType)
                {
                    SetOpStatus("Wrong PIN entered, " + tEx.AttemptsLeft + " attempts left", false);
                }
                else
                {
                    SetOpStatus(tEx.Message, false);
                }
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

        private async Task VerifySignatureAuthCertAsync()
        {
            if (string.IsNullOrEmpty(SignAuthDataInput))
            {
                SetOpStatus("No data available for verification", false);
                return;
            }

            if (_authCertificate == null || _authCertificate.Length == 0)
            {
                SetOpStatus("Authentication certificate not available. Please read certificates first.", false);
                return;
            }

            if (_lastAuthSignature == null || _lastAuthSignature.Length == 0)
            {
                SetOpStatus("No signature available. Please sign data first.", false);
                return;
            }

            _main.SetBusy(true, "Verifying signature with authentication certificate...");

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(SignAuthDataInput);

                await Task.Run(() =>
                    _main.Toolkit.VerifySignatureData(
                        data, false, _lastAuthSignature, _authCertificate));

                VerifyAuthResult = "Signature verification successful";
                SetOpStatus("Signature verified with auth cert", true);
            }
            catch (ToolkitException tEx)
            {
                VerifyAuthResult = "Verification failed: " + tEx.Message;
                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                VerifyAuthResult = "Verification failed: " + ex.Message;
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private async Task SignDataSignCertAsync()
        {
            if (string.IsNullOrEmpty(SignDataInput))
            {
                SetOpStatus("Please enter data to sign", false);
                return;
            }

            if (!ValidatePin(SignDataPin))
            {
                return;
            }

            _main.SetBusy(true, "Signing data with signing certificate...");

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(SignDataInput);

                SignatureResponse response = await Task.Run(
                    () => _main.Toolkit.SignDataWithSignCert(data, false, SignDataPin));

                if (response.Signature != null)
                {
                    _lastSignSignature = response.Signature.ToArray();
                    SignDataSignature = Convert.ToBase64String(_lastSignSignature);
                }

                SignDataXmlResponse = response.XmlString;
                SetOpStatus("Data signed with sign cert: " + response.ResponseStatus.ToString(), true);
                SignDataPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                if (ExceptionType.CardPinError == tEx.ExceptionType)
                {
                    SetOpStatus("Wrong PIN entered, " + tEx.AttemptsLeft + " attempts left", false);
                }
                else
                {
                    SetOpStatus(tEx.Message, false);
                }
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

        private async Task VerifySignatureSignCertAsync()
        {
            if (string.IsNullOrEmpty(SignDataInput))
            {
                SetOpStatus("No data available for verification", false);
                return;
            }

            if (_signCertificate == null || _signCertificate.Length == 0)
            {
                SetOpStatus("Signing certificate not available. Please read certificates first.", false);
                return;
            }

            if (_lastSignSignature == null || _lastSignSignature.Length == 0)
            {
                SetOpStatus("No signature available. Please sign data first.", false);
                return;
            }

            _main.SetBusy(true, "Verifying signature with signing certificate...");

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(SignDataInput);

                await Task.Run(() =>
                    _main.Toolkit.VerifySignatureData(
                        data, false, _lastSignSignature, _signCertificate));

                VerifySignResult = "Signature verification successful";
                SetOpStatus("Signature verified with sign cert", true);
            }
            catch (ToolkitException tEx)
            {
                VerifySignResult = "Verification failed: " + tEx.Message;
                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                VerifySignResult = "Verification failed: " + ex.Message;
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }
    }
}
