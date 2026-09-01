using System;
using System.Threading.Tasks;
using System.Windows.Media;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for card-dependent operations that require a connected reader:
    /// CSN, Card Status, Interface Type
    /// </summary>
    public class CardOperationsViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // CSN
        private string _csn;

        // Card Status
        private string _cardStatus;
        private string _cardStatusXml;

        // Interface Type
        private string _interfaceType;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Commands
        public RelayCommand GetCsnCommand { get; private set; }

        public RelayCommand CheckCardStatusCommand { get; private set; }

        public RelayCommand GetInterfaceTypeCommand { get; private set; }

        // CSN Properties
        public string Csn
        {
            get { return _csn; }
            set { SetProperty(ref _csn, value); }
        }

        // Card Status Properties
        public string CardStatus
        {
            get { return _cardStatus; }
            set { SetProperty(ref _cardStatus, value); }
        }

        public string CardStatusXml
        {
            get { return _cardStatusXml; }
            set { SetProperty(ref _cardStatusXml, value); }
        }

        // Interface Type Properties
        public string InterfaceType
        {
            get { return _interfaceType; }
            set { SetProperty(ref _interfaceType, value); }
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

        public CardOperationsViewModel(MainViewModel main)
        {
            _main = main;

            GetCsnCommand = new RelayCommand(WrapAsync(GetCsnAsync));
            CheckCardStatusCommand = new RelayCommand(WrapAsync(CheckCardStatusAsync));
            GetInterfaceTypeCommand = new RelayCommand(WrapAsync(GetInterfaceTypeAsync));
        }

        private void SetOpStatus(string message, bool isSuccess)
        {
            OperationStatus = message;
            OperationStatusColor = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
            _main.SetStatus(message, isSuccess);
        }

        private async Task GetCsnAsync()
        {
            _main.SetBusy(true, "Reading card serial number...");

            try
            {
                string csn = await Task.Run(() => _main.Toolkit.GetCSN());

                if (!string.IsNullOrEmpty(csn))
                {
                    Csn = csn;
                    SetOpStatus("CSN read successfully", true);
                }
                else
                {
                    SetOpStatus("CSN value is null or empty", false);
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

        private async Task CheckCardStatusAsync()
        {
            _main.SetBusy(true, "Checking card status...");

            try
            {
                ToolkitResponse response = await Task.Run(() => _main.Toolkit.CheckCardStatus());

                CardStatus = !string.IsNullOrEmpty(response.Status)
                    ? response.Status
                    : response.ResponseStatus.ToString();
                CardStatusXml = response.XmlString;
                SetOpStatus("Card status: " + CardStatus, true);
            }
            catch (ToolkitException tEx)
            {
                CardStatus = tEx.Message;
                if (!string.IsNullOrEmpty(tEx.VGResponse))
                {
                    CardStatusXml = tEx.VGResponse;
                }

                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                CardStatus = ex.Message;
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private async Task GetInterfaceTypeAsync()
        {
            _main.SetBusy(true, "Getting interface type...");

            try
            {
                string ifType = await Task.Run(() => _main.Toolkit.GetInterfaceType());

                InterfaceType = ifType;
                SetOpStatus("Interface type: " + ifType, true);
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
