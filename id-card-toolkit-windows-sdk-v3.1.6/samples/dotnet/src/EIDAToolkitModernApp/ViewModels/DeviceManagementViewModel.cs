using System;
using System.Threading.Tasks;
using System.Windows.Media;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for toolkit-level VG operations that do not require a card:
    /// Register Device, Get Device ID
    /// </summary>
    public class DeviceManagementViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // Device ID
        private string _deviceId;

        // Register Device
        private string _regUserId;
        private string _regPassword;
        private string _regDeviceRefId;
        private string _regDeviceRegistrationId;
        private string _regXmlResponse;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Commands
        public RelayCommand GetDeviceIdCommand { get; private set; }

        public RelayCommand RegisterDeviceCommand { get; private set; }

        // Device ID Properties
        public string DeviceId
        {
            get { return _deviceId; }
            set { SetProperty(ref _deviceId, value); }
        }

        // Register Device Properties
        public string RegUserId
        {
            get { return _regUserId; }
            set { SetProperty(ref _regUserId, value); }
        }

        public string RegPassword
        {
            get { return _regPassword; }
            set { SetProperty(ref _regPassword, value); }
        }

        public string RegDeviceRefId
        {
            get { return _regDeviceRefId; }
            set { SetProperty(ref _regDeviceRefId, value); }
        }

        public string RegDeviceRegistrationId
        {
            get { return _regDeviceRegistrationId; }
            set { SetProperty(ref _regDeviceRegistrationId, value); }
        }

        public string RegXmlResponse
        {
            get { return _regXmlResponse; }
            set { SetProperty(ref _regXmlResponse, value); }
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

        public DeviceManagementViewModel(MainViewModel main)
        {
            _main = main;

            GetDeviceIdCommand = new RelayCommand(WrapAsync(GetDeviceIdAsync));
            RegisterDeviceCommand = new RelayCommand(WrapAsync(RegisterDeviceAsync));
        }

        private void SetOpStatus(string message, bool isSuccess)
        {
            OperationStatus = message;
            OperationStatusColor = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
            _main.SetStatus(message, isSuccess);
        }

        private async Task GetDeviceIdAsync()
        {
            _main.SetBusy(true, "Getting device ID...");

            try
            {
                string deviceId = await Task.Run(() => _main.Toolkit.GetDeviceId());

                if (!string.IsNullOrEmpty(deviceId))
                {
                    DeviceId = deviceId;
                    SetOpStatus("Device ID retrieved successfully", true);
                }
                else
                {
                    SetOpStatus("Device ID is null or empty", false);
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

        private async Task RegisterDeviceAsync()
        {
            if (string.IsNullOrEmpty(RegUserId) ||
                string.IsNullOrEmpty(RegPassword) ||
                string.IsNullOrEmpty(RegDeviceRefId))
            {
                SetOpStatus("Please enter User ID, Password, and Device Reference ID", false);
                return;
            }

            _main.SetBusy(true, "Registering device...");

            try
            {
                RegisterDeviceResponse response = await Task.Run(
                    () => _main.Toolkit.RegisterDevice(RegUserId, RegPassword, RegDeviceRefId));

                RegDeviceRegistrationId = response.DeviceRegistrationId;
                RegXmlResponse = response.XmlString;
                SetOpStatus("Device registered: " + response.ResponseStatus.ToString(), true);
            }
            catch (ToolkitException tEx)
            {
                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    RegXmlResponse = tEx.VGResponse;
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
