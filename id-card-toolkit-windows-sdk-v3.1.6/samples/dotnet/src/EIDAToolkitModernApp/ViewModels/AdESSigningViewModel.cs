using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

using EIDAToolkitModernApp.Services;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for AdES digital signature operations (CAdES-B, XAdES-B, PAdES-B)
    /// </summary>
    public class AdESSigningViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // CAdES
        private string _cadesInputFile;
        private string _cadesPin;
        private string _cadesSignature;
        private string _cadesVerifyResult;
        private byte[] _cadesSignatureBytes;

        // XAdES
        private string _xadesInputFile;
        private string _xadesOutputFile;
        private string _xadesPin;
        private string _xadesResult;

        // PAdES
        private string _padesInputFile;
        private string _padesOutputFile;
        private string _padesPin;
        private string _padesSignReason;
        private string _padesSignLocation;
        private string _padesSignatureImage;
        private string _padesResult;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Card PIN must be 4-16 digits on the EID card.
        private const int MinPinLength = 4;
        private const int MaxPinLength = 16;

        // Commands
        public RelayCommand SignCadesCommand { get; private set; }

        public RelayCommand VerifyCadesCommand { get; private set; }

        public RelayCommand SignXadesCommand { get; private set; }

        public RelayCommand SignPadesCommand { get; private set; }

        // CAdES Properties
        public string CadesInputFile
        {
            get { return _cadesInputFile; }
            set { SetProperty(ref _cadesInputFile, value); }
        }

        public string CadesPin
        {
            get { return _cadesPin; }
            set { SetProperty(ref _cadesPin, value); }
        }

        public string CadesSignature
        {
            get { return _cadesSignature; }
            set { SetProperty(ref _cadesSignature, value); }
        }

        public string CadesVerifyResult
        {
            get { return _cadesVerifyResult; }
            set { SetProperty(ref _cadesVerifyResult, value); }
        }

        // XAdES Properties
        public string XadesInputFile
        {
            get { return _xadesInputFile; }
            set { SetProperty(ref _xadesInputFile, value); }
        }

        public string XadesOutputFile
        {
            get { return _xadesOutputFile; }
            set { SetProperty(ref _xadesOutputFile, value); }
        }

        public string XadesPin
        {
            get { return _xadesPin; }
            set { SetProperty(ref _xadesPin, value); }
        }

        public string XadesResult
        {
            get { return _xadesResult; }
            set { SetProperty(ref _xadesResult, value); }
        }

        // PAdES Properties
        public string PadesInputFile
        {
            get { return _padesInputFile; }
            set { SetProperty(ref _padesInputFile, value); }
        }

        public string PadesOutputFile
        {
            get { return _padesOutputFile; }
            set { SetProperty(ref _padesOutputFile, value); }
        }

        public string PadesPin
        {
            get { return _padesPin; }
            set { SetProperty(ref _padesPin, value); }
        }

        public string PadesSignReason
        {
            get { return _padesSignReason; }
            set { SetProperty(ref _padesSignReason, value); }
        }

        public string PadesSignLocation
        {
            get { return _padesSignLocation; }
            set { SetProperty(ref _padesSignLocation, value); }
        }

        public string PadesSignatureImage
        {
            get { return _padesSignatureImage; }
            set { SetProperty(ref _padesSignatureImage, value); }
        }

        public string PadesResult
        {
            get { return _padesResult; }
            set { SetProperty(ref _padesResult, value); }
        }

        // Status Properties
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

        public AdESSigningViewModel(MainViewModel main)
        {
            _main = main;

            SignCadesCommand = new RelayCommand(WrapAsync(SignCadesAsync));
            VerifyCadesCommand = new RelayCommand(WrapAsync(VerifyCadesAsync));
            SignXadesCommand = new RelayCommand(WrapAsync(SignXadesAsync));
            SignPadesCommand = new RelayCommand(WrapAsync(SignPadesAsync));
        }

        private void SetLocalStatus(string message, bool isSuccess)
        {
            OperationStatus = message;
            OperationStatusColor = isSuccess
                ? new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#D13438"));
        }

        private bool ValidatePin(string pin)
        {
            if (string.IsNullOrEmpty(pin))
            {
                SetLocalStatus("Please enter the card PIN", false);
                return false;
            }

            if ((pin.Length < MinPinLength) || (pin.Length > MaxPinLength))
            {
                SetLocalStatus("PIN must be between "
                    + MinPinLength + " and " + MaxPinLength + " digits", false);
                return false;
            }

            return true;
        }

        // Surface ToolkitException details consistently across all AdES ops.
        // CardPinError needs AttemptsLeft; any VGResponse payload is useful for diagnostics.
        private void HandleToolkitException(string operation, ToolkitException tEx)
        {
            if (ExceptionType.CardPinError == tEx.ExceptionType)
            {
                SetLocalStatus(operation + " failed: wrong PIN, "
                    + tEx.AttemptsLeft + " attempts left", false);
            }
            else
            {
                SetLocalStatus(operation + " failed: " + tEx.Message, false);
            }
        }

        private async Task SignCadesAsync()
        {
            if (string.IsNullOrEmpty(CadesInputFile))
            {
                SetLocalStatus("Please select an input file", false);
                return;
            }

            if (!ValidatePin(CadesPin))
            {
                return;
            }

            _main.SetBusy(true, "Signing with CAdES-B...");

            try
            {
                // Run the sign on a background thread -- CAdES sign goes to VG and
                // can take seconds. Keeping it on the UI thread would freeze WPF.
                ToolkitResponse response = await Task.Run(
                    () => _main.Toolkit.CadesSign(CadesInputFile, CadesPin));

                if (response.DetachedSignature != null)
                {
                    _cadesSignatureBytes = new byte[response.DetachedSignature.Count];
                    response.DetachedSignature.CopyTo(_cadesSignatureBytes, 0);
                }

                if (_cadesSignatureBytes != null)
                {
                    CadesSignature = Convert.ToBase64String(_cadesSignatureBytes);
                    SetLocalStatus(string.Format(
                        "CAdES-B signature created: {0} bytes",
                        _cadesSignatureBytes.Length), true);
                }
                else
                {
                    SetLocalStatus("CAdES-B signature created (no detached data)",
                        true);
                }

                CadesPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                HandleToolkitException("CAdES sign", tEx);
            }
            catch (Exception ex)
            {
                SetLocalStatus("CAdES sign failed: " + ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private async Task VerifyCadesAsync()
        {
            if (string.IsNullOrEmpty(CadesInputFile))
            {
                SetLocalStatus("Please select the input file used for signing",
                    false);
                return;
            }

            if (_cadesSignatureBytes == null)
            {
                SetLocalStatus("No CAdES signature to verify. Sign first.", false);
                return;
            }

            _main.SetBusy(true, "Verifying CAdES signature...");

            try
            {
                string report = await Task.Run(
                    () => _main.Toolkit.CadesVerify(CadesInputFile, _cadesSignatureBytes));

                CadesVerifyResult = report;
                SetLocalStatus("CAdES verification complete", true);
            }
            catch (ToolkitException tEx)
            {
                HandleToolkitException("CAdES verify", tEx);
            }
            catch (Exception ex)
            {
                SetLocalStatus("CAdES verify failed: " + ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private async Task SignXadesAsync()
        {
            if (string.IsNullOrEmpty(XadesInputFile))
            {
                SetLocalStatus("Please select an XML input file", false);
                return;
            }

            if (!ValidatePin(XadesPin))
            {
                return;
            }

            // Auto-generate output file name
            string dir = Path.GetDirectoryName(XadesInputFile);
            string name = Path.GetFileNameWithoutExtension(XadesInputFile);
            XadesOutputFile = Path.Combine(dir, name + "_signed.xml");

            _main.SetBusy(true, "Signing with XAdES-B...");

            try
            {
                await Task.Run(() => _main.Toolkit.XadesSign(
                    XadesInputFile, XadesOutputFile, XadesPin));

                XadesResult = "Signed XML saved to:\n" + XadesOutputFile;
                SetLocalStatus("XAdES-B signature created", true);
                XadesPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                HandleToolkitException("XAdES sign", tEx);
            }
            catch (Exception ex)
            {
                SetLocalStatus("XAdES sign failed: " + ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private async Task SignPadesAsync()
        {
            if (string.IsNullOrEmpty(PadesInputFile))
            {
                SetLocalStatus("Please select a PDF input file", false);
                return;
            }

            if (!ValidatePin(PadesPin))
            {
                return;
            }

            // Auto-generate output file name
            string dir = Path.GetDirectoryName(PadesInputFile);
            string name = Path.GetFileNameWithoutExtension(PadesInputFile);
            PadesOutputFile = Path.Combine(dir, name + "_signed.pdf");

            _main.SetBusy(true, "Signing with PAdES-B...");

            try
            {
                PadesSignParams pp = new PadesSignParams();
                pp.SignVisible = 1;
                pp.PageNumber = 1;
                // Visible signature stamp position in PDF user-space units
                // (1/72 inch). Defaults place it roughly in the lower-left area.
                pp.SignatureXAxis = 150;
                pp.SignatureYAxis = 50;

                if (!string.IsNullOrEmpty(PadesSignReason))
                {
                    pp.SignReason = PadesSignReason;
                }

                if (!string.IsNullOrEmpty(PadesSignLocation))
                {
                    pp.SignerLocation = PadesSignLocation;
                }

                if (!string.IsNullOrEmpty(PadesSignatureImage))
                {
                    pp.SignatureImage = PadesSignatureImage;
                }

                await Task.Run(() => _main.Toolkit.PadesSign(
                    PadesInputFile, PadesOutputFile, PadesPin, pp));

                PadesResult = "Signed PDF saved to:\n" + PadesOutputFile;
                SetLocalStatus("PAdES-B signature created", true);
                PadesPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                HandleToolkitException("PAdES sign", tEx);
            }
            catch (Exception ex)
            {
                SetLocalStatus("PAdES sign failed: " + ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }
    }
}
