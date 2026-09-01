using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for biometric authentication on server and card+biometric authentication.
    ///
    /// Connection model (important): the app Connects once via MainViewModel and keeps
    /// the PC/SC session open for the whole biometric flow (Load Finger Data -> Authenticate).
    /// Disconnecting between GetFingerData and AuthenticateBiometricOnServer drops the
    /// native-layer finger context and the authenticate call will fail with "Invalid parameter".
    /// Do not switch this VM to a Connect-per-op pattern.
    /// </summary>
    public class BiometricViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // Biometric Auth on Server
        private ObservableCollection<string> _fingerIndexList;
        private int _selectedFingerIndex;
        private int _sensorTimeout;
        private string _xmlResponse;
        private string _authResult;

        // Card + Biometric Auth
        private ObservableCollection<string> _cardBioFingerIndexList;
        private int _cardBioSelectedFingerIndex;
        private int _cardBioSensorTimeout;
        private string _cardBioXmlResponse;
        private string _cardBioAuthResult;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Commands - Biometric on Server
        public RelayCommand AuthenticateCommand { get; private set; }

        public RelayCommand LoadFingerDataCommand { get; private set; }

        // Commands - Card + Biometric
        public RelayCommand CardBioAuthenticateCommand { get; private set; }

        public RelayCommand CardBioLoadFingerDataCommand { get; private set; }

        // Biometric Auth on Server Properties
        public ObservableCollection<string> FingerIndexList
        {
            get { return _fingerIndexList; }
            set { SetProperty(ref _fingerIndexList, value); }
        }

        public int SelectedFingerIndex
        {
            get { return _selectedFingerIndex; }
            set { SetProperty(ref _selectedFingerIndex, value); }
        }

        public int SensorTimeout
        {
            get { return _sensorTimeout; }
            set { SetProperty(ref _sensorTimeout, value); }
        }

        public string XmlResponse
        {
            get { return _xmlResponse; }
            set { SetProperty(ref _xmlResponse, value); }
        }

        public string AuthResult
        {
            get { return _authResult; }
            set { SetProperty(ref _authResult, value); }
        }

        // Card + Biometric Properties
        public ObservableCollection<string> CardBioFingerIndexList
        {
            get { return _cardBioFingerIndexList; }
            set { SetProperty(ref _cardBioFingerIndexList, value); }
        }

        public int CardBioSelectedFingerIndex
        {
            get { return _cardBioSelectedFingerIndex; }
            set { SetProperty(ref _cardBioSelectedFingerIndex, value); }
        }

        public int CardBioSensorTimeout
        {
            get { return _cardBioSensorTimeout; }
            set { SetProperty(ref _cardBioSensorTimeout, value); }
        }

        public string CardBioXmlResponse
        {
            get { return _cardBioXmlResponse; }
            set { SetProperty(ref _cardBioXmlResponse, value); }
        }

        public string CardBioAuthResult
        {
            get { return _cardBioAuthResult; }
            set { SetProperty(ref _cardBioAuthResult, value); }
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

        public BiometricViewModel(MainViewModel main)
        {
            _main = main;
            _fingerIndexList = new ObservableCollection<string>();
            _cardBioFingerIndexList = new ObservableCollection<string>();
            _sensorTimeout = 30;
            _cardBioSensorTimeout = 30;
            _selectedFingerIndex = -1;
            _cardBioSelectedFingerIndex = -1;

            AuthenticateCommand = new RelayCommand(WrapAsync(AuthenticateAsync));
            LoadFingerDataCommand = new RelayCommand(WrapAsync(LoadFingerDataAsync));
            CardBioAuthenticateCommand = new RelayCommand(WrapAsync(CardBioAuthenticateAsync));
            CardBioLoadFingerDataCommand = new RelayCommand(WrapAsync(CardBioLoadFingerDataAsync));
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
                    FingerIndexList.Clear();
                    FingerIndexList.Add(data[0].FingerIndex.ToString());
                    FingerIndexList.Add(data[1].FingerIndex.ToString());
                    SelectedFingerIndex = 0;

                    SetOpStatus("Finger data loaded", true);
                }
                else
                {
                    SetOpStatus("Failed to read finger data", false);
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

        private async Task AuthenticateAsync()
        {
            if (_sensorTimeout <= 0)
            {
                SetOpStatus("Sensor timeout must be greater than zero", false);
                return;
            }

            if (_selectedFingerIndex < 0)
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

            _main.SetBusy(true, "Authenticating biometric on server...");

            try
            {
                FingerData selectedFinger = fingerData[_selectedFingerIndex];

                ToolkitResponse response = await Task.Run(() =>
                    _main.Toolkit.AuthenticateBiometricOnServer(
                        selectedFinger.FingerIndex, _sensorTimeout));

                AuthResult = !string.IsNullOrEmpty(response.Status)
                    ? response.Status
                    : response.ResponseStatus.ToString();
                XmlResponse = response.XmlString;
                SetOpStatus("Authentication result: " + AuthResult, true);
            }
            catch (ToolkitException tEx)
            {
                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    XmlResponse = tEx.VGResponse;
                }

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

        private async Task CardBioLoadFingerDataAsync()
        {
            _main.SetBusy(true, "Reading finger data from card...");

            try
            {
                FingerData[] data = await Task.Run(() => _main.Toolkit.GetFingerData());

                if (data != null && data.Length >= 2)
                {
                    CardBioFingerIndexList.Clear();
                    CardBioFingerIndexList.Add(data[0].FingerIndex.ToString());
                    CardBioFingerIndexList.Add(data[1].FingerIndex.ToString());
                    CardBioSelectedFingerIndex = 0;

                    SetOpStatus("Finger data loaded for Card + Biometric", true);
                }
                else
                {
                    SetOpStatus("Failed to read finger data", false);
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

        private async Task CardBioAuthenticateAsync()
        {
            if (_cardBioSensorTimeout <= 0)
            {
                SetOpStatus("Sensor timeout must be greater than zero", false);
                return;
            }

            if (_cardBioSelectedFingerIndex < 0)
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

            _main.SetBusy(true, "Authenticating card and biometric...");

            try
            {
                FingerData selectedFinger = fingerData[_cardBioSelectedFingerIndex];

                ToolkitResponse response = await Task.Run(() =>
                    _main.Toolkit.AuthenticateCardAndBiometric(
                        selectedFinger.FingerIndex, _cardBioSensorTimeout));

                CardBioAuthResult = !string.IsNullOrEmpty(response.Status)
                    ? response.Status
                    : response.ResponseStatus.ToString();
                CardBioXmlResponse = response.XmlString;
                SetOpStatus("Card + Biometric result: " + CardBioAuthResult, true);
            }
            catch (ToolkitException tEx)
            {
                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    CardBioXmlResponse = tEx.VGResponse;
                }

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
