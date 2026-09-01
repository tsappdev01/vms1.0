using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for Reset PIN, Unblock PIN, PKI Certificates operations
    /// </summary>
    public class PinOperationsViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // Reset PIN
        private string _resetPin;
        private string _resetConfirmPin;
        private int _resetSensorTimeout;
        private ObservableCollection<string> _resetFingerIndexList;
        private int _resetSelectedFingerIndex;
        private string _resetPinXml;
        private string _resetPinResult;

        // Unblock PIN
        private string _unblockPin;
        private string _unblockConfirmPin;
        private int _unblockSensorTimeout;
        private ObservableCollection<string> _unblockFingerIndexList;
        private int _unblockSelectedFingerIndex;
        private string _unblockPinXml;
        private string _unblockPinResult;

        // PKI Certificates
        private string _certPin;
        private string _authCertificate;
        private string _signCertificate;
        private string _pkiCertXml;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Card PIN must be 4-16 digits on the EID card.
        private const int MinPinLength = 4;
        private const int MaxPinLength = 16;

        // Commands
        public RelayCommand ResetPinCommand { get; private set; }

        public RelayCommand UnblockPinCommand { get; private set; }

        public RelayCommand GetCertificatesCommand { get; private set; }

        public RelayCommand LoadFingerDataCommand { get; private set; }

        // Reset PIN Properties
        public string ResetPin
        {
            get { return _resetPin; }
            set { SetProperty(ref _resetPin, value); }
        }

        public string ResetConfirmPin
        {
            get { return _resetConfirmPin; }
            set { SetProperty(ref _resetConfirmPin, value); }
        }

        public int ResetSensorTimeout
        {
            get { return _resetSensorTimeout; }
            set { SetProperty(ref _resetSensorTimeout, value); }
        }

        public ObservableCollection<string> ResetFingerIndexList
        {
            get { return _resetFingerIndexList; }
            set { SetProperty(ref _resetFingerIndexList, value); }
        }

        public int ResetSelectedFingerIndex
        {
            get { return _resetSelectedFingerIndex; }
            set { SetProperty(ref _resetSelectedFingerIndex, value); }
        }

        public string ResetPinXml
        {
            get { return _resetPinXml; }
            set { SetProperty(ref _resetPinXml, value); }
        }

        public string ResetPinResult
        {
            get { return _resetPinResult; }
            set { SetProperty(ref _resetPinResult, value); }
        }

        // Unblock PIN Properties
        public string UnblockPin
        {
            get { return _unblockPin; }
            set { SetProperty(ref _unblockPin, value); }
        }

        public string UnblockConfirmPin
        {
            get { return _unblockConfirmPin; }
            set { SetProperty(ref _unblockConfirmPin, value); }
        }

        public int UnblockSensorTimeout
        {
            get { return _unblockSensorTimeout; }
            set { SetProperty(ref _unblockSensorTimeout, value); }
        }

        public ObservableCollection<string> UnblockFingerIndexList
        {
            get { return _unblockFingerIndexList; }
            set { SetProperty(ref _unblockFingerIndexList, value); }
        }

        public int UnblockSelectedFingerIndex
        {
            get { return _unblockSelectedFingerIndex; }
            set { SetProperty(ref _unblockSelectedFingerIndex, value); }
        }

        public string UnblockPinXml
        {
            get { return _unblockPinXml; }
            set { SetProperty(ref _unblockPinXml, value); }
        }

        public string UnblockPinResult
        {
            get { return _unblockPinResult; }
            set { SetProperty(ref _unblockPinResult, value); }
        }

        // PKI Certificate Properties
        public string CertPin
        {
            get { return _certPin; }
            set { SetProperty(ref _certPin, value); }
        }

        public string AuthCertificate
        {
            get { return _authCertificate; }
            set { SetProperty(ref _authCertificate, value); }
        }

        public string SignCertificate
        {
            get { return _signCertificate; }
            set { SetProperty(ref _signCertificate, value); }
        }

        public string PkiCertXml
        {
            get { return _pkiCertXml; }
            set { SetProperty(ref _pkiCertXml, value); }
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

        public PinOperationsViewModel(MainViewModel main)
        {
            _main = main;
            _resetFingerIndexList = new ObservableCollection<string>();
            _unblockFingerIndexList = new ObservableCollection<string>();
            _resetSensorTimeout = 30;
            _unblockSensorTimeout = 30;
            _resetSelectedFingerIndex = -1;
            _unblockSelectedFingerIndex = -1;

            ResetPinCommand = new RelayCommand(WrapAsync(ResetPinAsync));
            UnblockPinCommand = new RelayCommand(WrapAsync(UnblockPinAsync));
            GetCertificatesCommand = new RelayCommand(WrapAsync(GetCertificatesAsync));
            LoadFingerDataCommand = new RelayCommand(WrapAsync(LoadFingerDataAsync));
        }

        private void SetOpStatus(string message, bool isSuccess)
        {
            OperationStatus = message;
            OperationStatusColor = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
            _main.SetStatus(message, isSuccess);
        }

        private async Task LoadFingerDataAsync()
        {
            _main.SetBusy(true, "Reading finger data from card...");

            try
            {
                FingerData[] data = await Task.Run(() => _main.Toolkit.GetFingerData());

                if (data != null && data.Length >= 2)
                {
                    ResetFingerIndexList.Clear();
                    UnblockFingerIndexList.Clear();

                    ResetFingerIndexList.Add(data[0].FingerIndex.ToString());
                    ResetFingerIndexList.Add(data[1].FingerIndex.ToString());
                    UnblockFingerIndexList.Add(data[0].FingerIndex.ToString());
                    UnblockFingerIndexList.Add(data[1].FingerIndex.ToString());

                    ResetSelectedFingerIndex = 0;
                    UnblockSelectedFingerIndex = 0;

                    SetOpStatus("Finger data loaded successfully", true);
                }
                else
                {
                    SetOpStatus("Failed to read finger data from card", false);
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

        private async Task ResetPinAsync()
        {
            if (string.IsNullOrEmpty(_resetPin))
            {
                SetOpStatus("Please enter the PIN", false);
                return;
            }

            if (string.IsNullOrEmpty(_resetConfirmPin))
            {
                SetOpStatus("Please enter the confirm PIN", false);
                return;
            }

            if (_resetPin != _resetConfirmPin)
            {
                SetOpStatus("PIN does not match", false);
                return;
            }

            if ((_resetPin.Length < MinPinLength) || (_resetPin.Length > MaxPinLength))
            {
                SetOpStatus("PIN must be between "
                    + MinPinLength + " and " + MaxPinLength + " digits", false);
                return;
            }

            if (_resetSensorTimeout <= 0)
            {
                SetOpStatus("Sensor timeout must be greater than zero", false);
                return;
            }

            if (_resetSelectedFingerIndex < 0)
            {
                SetOpStatus("Please select a finger index", false);
                return;
            }

            FingerData[] fingerData = _main.Toolkit.FingerDataArray;

            if (fingerData == null || fingerData.Length == 0)
            {
                SetOpStatus("Please load finger data from card first", false);
                return;
            }

            _main.SetBusy(true, "Resetting PIN...");

            try
            {
                FingerData selectedFinger = fingerData[_resetSelectedFingerIndex];

                ToolkitResponse response = await Task.Run(() =>
                    _main.Toolkit.ResetPin(_resetPin, selectedFinger, _resetSensorTimeout));

                ResetPinResult = !string.IsNullOrEmpty(response.Status)
                    ? response.Status : response.ResponseStatus.ToString();
                ResetPinXml = response.XmlString;
                SetOpStatus("PIN reset: " + ResetPinResult, true);

                // Drop plaintext PIN from memory now that the op is done
                ResetPin = string.Empty;
                ResetConfirmPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    ResetPinXml = tEx.VGResponse;
                }

                if (ExceptionType.CardPinError == tEx.ExceptionType)
                {
                    SetOpStatus(tEx.AttemptsLeft + " card block attempts left", false);
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

        private async Task UnblockPinAsync()
        {
            if (string.IsNullOrEmpty(_unblockPin))
            {
                SetOpStatus("Please enter the PIN", false);
                return;
            }

            if (string.IsNullOrEmpty(_unblockConfirmPin))
            {
                SetOpStatus("Please enter the confirm PIN", false);
                return;
            }

            if (_unblockPin != _unblockConfirmPin)
            {
                SetOpStatus("PIN does not match", false);
                return;
            }

            if ((_unblockPin.Length < MinPinLength) || (_unblockPin.Length > MaxPinLength))
            {
                SetOpStatus("PIN must be between "
                    + MinPinLength + " and " + MaxPinLength + " digits", false);
                return;
            }

            if (_unblockSensorTimeout <= 0)
            {
                SetOpStatus("Sensor timeout must be greater than zero", false);
                return;
            }

            if (_unblockSelectedFingerIndex < 0)
            {
                SetOpStatus("Please select a finger index", false);
                return;
            }

            FingerData[] fingerData = _main.Toolkit.FingerDataArray;

            if (fingerData == null || fingerData.Length == 0)
            {
                SetOpStatus("Please load finger data from card first", false);
                return;
            }

            _main.SetBusy(true, "Unblocking PIN...");

            try
            {
                FingerData selectedFinger = fingerData[_unblockSelectedFingerIndex];

                ToolkitResponse response = await Task.Run(() =>
                    _main.Toolkit.UnblockPin(_unblockPin, selectedFinger, _unblockSensorTimeout));

                UnblockPinXml = response.XmlString;
                UnblockPinResult = !string.IsNullOrEmpty(response.Status)
                    ? response.Status : response.ResponseStatus.ToString();
                SetOpStatus("PIN unblocked: " + UnblockPinResult, true);

                // Drop plaintext PIN from memory now that the op is done
                UnblockPin = string.Empty;
                UnblockConfirmPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    UnblockPinXml = tEx.VGResponse;
                }

                if (ExceptionType.CardPinError == tEx.ExceptionType)
                {
                    SetOpStatus(tEx.AttemptsLeft + " card block attempts left", false);
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

        private async Task GetCertificatesAsync()
        {
            if (string.IsNullOrEmpty(_certPin))
            {
                SetOpStatus("Please enter the PIN", false);
                return;
            }

            if ((_certPin.Length < MinPinLength) || (_certPin.Length > MaxPinLength))
            {
                SetOpStatus("PIN must be between "
                    + MinPinLength + " and " + MaxPinLength + " digits", false);
                return;
            }

            _main.SetBusy(true, "Reading PKI certificates...");

            try
            {
                CardCertificates certs = await Task.Run(() =>
                    _main.Toolkit.GetPkiCertificates(_certPin));

                if (certs.AuthenticationCertificate != null)
                {
                    AuthCertificate = Convert.ToBase64String(certs.AuthenticationCertificate.ToArray());
                }

                if (certs.SigningCertificate != null)
                {
                    SignCertificate = Convert.ToBase64String(certs.SigningCertificate.ToArray());
                }

                PkiCertXml = certs.XmlString;
                SetOpStatus("PKI certificates read: " + certs.ResponseStatus.ToString(), true);

                // Drop plaintext PIN from memory now that the op is done
                CertPin = string.Empty;
            }
            catch (ToolkitException tEx)
            {
                AuthCertificate = null;
                SignCertificate = null;

                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    PkiCertXml = tEx.VGResponse;
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
                AuthCertificate = null;
                SignCertificate = null;
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }
    }
}
