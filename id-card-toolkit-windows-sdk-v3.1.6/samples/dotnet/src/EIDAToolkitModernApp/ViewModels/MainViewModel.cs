using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

using AE.EmiratesId.IdCard;

using EIDAToolkitModernApp.Services;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// Main ViewModel controlling navigation and connection state
    /// </summary>
    public class MainViewModel : BaseViewModel
    {
        private readonly ToolkitService _toolkit;

        private BaseViewModel _currentView;
        private string _currentPageName;
        private string _statusMessage;
        private SolidColorBrush _statusColor;
        private string _currentPage;
        private bool _isConnected;
        private bool _isInitialized;
        private string _readerName;
        private string _toolkitVersion;
        private bool _isBusy;
        private string _busyMessage;
        private bool _processMode;
        private ObservableCollection<string> _readerList;
        private int _selectedReaderIndex;

        // Child ViewModels
        public PublicDataViewModel PublicDataVM { get; private set; }

        public CardOperationsViewModel CardOperationsVM { get; private set; }

        public PinOperationsViewModel PinOperationsVM { get; private set; }

        public BiometricViewModel BiometricVM { get; private set; }

        public NfcOperationsViewModel NfcOperationsVM { get; private set; }

        public PkiSigningViewModel PkiSigningVM { get; private set; }

        public AdESSigningViewModel AdESSigningVM { get; private set; }

        public DeviceManagementViewModel DeviceManagementVM { get; private set; }

        public ToolkitUtilitiesViewModel ToolkitUtilitiesVM { get; private set; }

        // Commands
        public RelayCommand InitializeCommand { get; private set; }

        public RelayCommand CleanupCommand { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }

        public RelayCommand DisconnectCommand { get; private set; }

        public RelayCommand FindReaderCommand { get; private set; }

        public RelayCommand NavigateCommand { get; private set; }

        // Properties
        public BaseViewModel CurrentView
        {
            get { return _currentView; }
            set { SetProperty(ref _currentView, value); }
        }

        public string CurrentPageName
        {
            get { return _currentPageName; }
            set { SetProperty(ref _currentPageName, value); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        public SolidColorBrush StatusColor
        {
            get { return _statusColor; }
            set { SetProperty(ref _statusColor, value); }
        }

        public string CurrentPage
        {
            get { return _currentPage; }
            set { SetProperty(ref _currentPage, value); }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }

        public bool IsInitialized
        {
            get { return _isInitialized; }
            set { SetProperty(ref _isInitialized, value); }
        }

        public string ReaderName
        {
            get { return _readerName; }
            set { SetProperty(ref _readerName, value); }
        }

        public string ToolkitVersion
        {
            get { return _toolkitVersion; }
            set { SetProperty(ref _toolkitVersion, value); }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set { SetProperty(ref _isBusy, value); }
        }

        public string BusyMessage
        {
            get { return _busyMessage; }
            set { SetProperty(ref _busyMessage, value); }
        }

        public bool ProcessMode
        {
            get { return _processMode; }
            set { SetProperty(ref _processMode, value); }
        }

        public ObservableCollection<string> ReaderList
        {
            get { return _readerList; }
            set { SetProperty(ref _readerList, value); }
        }

        public int SelectedReaderIndex
        {
            get { return _selectedReaderIndex; }
            set
            {
                if (SetProperty(ref _selectedReaderIndex, value))
                {
                    if (value >= 0 && _toolkit.IsInitialized)
                    {
                        try
                        {
                            _toolkit.SelectReader(value);
                            ReaderName = _toolkit.ReaderName;
                        }
                        catch (Exception ex)
                        {
                            SetStatus(ex.Message, false);
                        }
                    }
                }
            }
        }

        public ToolkitService Toolkit
        {
            get { return _toolkit; }
        }

        public MainViewModel()
        {
            _toolkit = new ToolkitService();
            _processMode = true;
            _selectedReaderIndex = -1;
            _readerList = new ObservableCollection<string>();

            // Initialize child ViewModels
            PublicDataVM = new PublicDataViewModel(this);
            CardOperationsVM = new CardOperationsViewModel(this);
            PinOperationsVM = new PinOperationsViewModel(this);
            BiometricVM = new BiometricViewModel(this);
            NfcOperationsVM = new NfcOperationsViewModel(this);
            PkiSigningVM = new PkiSigningViewModel(this);
            AdESSigningVM = new AdESSigningViewModel(this);
            DeviceManagementVM = new DeviceManagementViewModel(this);
            ToolkitUtilitiesVM = new ToolkitUtilitiesViewModel(this);

            // Initialize commands
            InitializeCommand = new RelayCommand(WrapAsync(InitializeAsync));
            CleanupCommand = new RelayCommand(WrapAsync(CleanupAsync));
            ConnectCommand = new RelayCommand(WrapAsync(ConnectAsync));
            DisconnectCommand = new RelayCommand(WrapAsync(DisconnectAsync));
            FindReaderCommand = new RelayCommand(WrapAsync(FindReaderAsync));
            NavigateCommand = new RelayCommand(o => Navigate((string)o));

            // Default view
            CurrentView = CardOperationsVM;
            CurrentPageName = "Card Operations";
            CurrentPage = "CardOperations";

            StatusMessage = "Ready";
            StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6E6E6E"));
        }

        public void SetStatus(string message, bool isSuccess)
        {
            StatusMessage = message;
            StatusColor = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
        }

        public void SetStatusInfo(string message)
        {
            StatusMessage = message;
            StatusColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0078D4"));
        }

        public void SetBusy(bool busy, string message = "")
        {
            IsBusy = busy;
            BusyMessage = message;
        }

        private void Navigate(string page)
        {
            CurrentPage = page;

            switch (page)
            {
                case "CardOperations":
                    CurrentView = CardOperationsVM;
                    CurrentPageName = "Card Operations";
                    break;

                case "PublicData":
                    CurrentView = PublicDataVM;
                    CurrentPageName = "Public Data";
                    break;

                case "PinOperations":
                    CurrentView = PinOperationsVM;
                    CurrentPageName = "PIN Operations";
                    break;

                case "Biometric":
                    CurrentView = BiometricVM;
                    CurrentPageName = "Biometric Authentication";
                    break;


                case "NfcOperations":
                    CurrentView = NfcOperationsVM;
                    CurrentPageName = "NFC Operations";
                    break;

                case "PkiSigning":
                    CurrentView = PkiSigningVM;
                    CurrentPageName = "PKI & Signing";
                    break;

                case "AdESSigning":
                    CurrentView = AdESSigningVM;
                    CurrentPageName = "AdES Signing";
                    break;

                case "DeviceManagement":
                    CurrentView = DeviceManagementVM;
                    CurrentPageName = "Device Management";
                    break;

                case "ToolkitUtilities":
                    CurrentView = ToolkitUtilitiesVM;
                    CurrentPageName = "Toolkit Utilities";
                    break;
            }
        }

        private async Task InitializeAsync()
        {
            SetBusy(true, "Initializing toolkit...");

            try
            {
                await Task.Run(() => _toolkit.Initialize(_processMode));
                IsInitialized = true;

                try
                {
                    string version = await Task.Run(() => _toolkit.GetToolkitVersion());
                    ToolkitVersion = version;
                    SetStatus("Toolkit initialized (v" + version + ")", true);
                }
                catch
                {
                    SetStatus("Toolkit initialized", true);
                }
            }
            catch (ToolkitException tEx)
            {
                SetStatus("Init failed: " + tEx.Message, false);
            }
            catch (Exception ex)
            {
                SetStatus("Init failed: " + ex.Message, false);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task CleanupAsync()
        {
            SetBusy(true, "Cleaning up...");

            try
            {
                await Task.Run(() => _toolkit.Cleanup());

                IsInitialized = false;
                IsConnected = false;
                ReaderName = string.Empty;
                ToolkitVersion = string.Empty;
                ReaderList.Clear();
                SelectedReaderIndex = -1;

                // Recreate all child ViewModels to clear previous session data
                CardOperationsVM = new CardOperationsViewModel(this);
                PublicDataVM = new PublicDataViewModel(this);
                PinOperationsVM = new PinOperationsViewModel(this);
                BiometricVM = new BiometricViewModel(this);
                NfcOperationsVM = new NfcOperationsViewModel(this);
                PkiSigningVM = new PkiSigningViewModel(this);
                AdESSigningVM = new AdESSigningViewModel(this);
                DeviceManagementVM = new DeviceManagementViewModel(this);
                ToolkitUtilitiesVM = new ToolkitUtilitiesViewModel(this);

                // Reset to default page
                Navigate("CardOperations");

                SetStatus("Toolkit cleaned up", true);
            }
            catch (Exception ex)
            {
                SetStatus("Cleanup failed: " + ex.Message, false);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task FindReaderAsync()
        {
            if (!_toolkit.IsInitialized)
            {
                SetStatus("Please initialize the toolkit first", false);
                return;
            }

            SetBusy(true, "Searching for readers...");

            try
            {
                CardReader[] readers = await Task.Run(() => _toolkit.ListReaders());

                Application.Current.Dispatcher.Invoke(() =>
                {
                    ReaderList.Clear();

                    foreach (var reader in readers)
                    {
                        ReaderList.Add(reader.Name);
                    }
                });

                if (readers.Length > 0)
                {
                    // Reset index to force PropertyChanged even if already 0
                    _selectedReaderIndex = -1;
                    SelectedReaderIndex = 0;
                    SetStatus("Found " + readers.Length + " reader(s)", true);
                }
                else
                {
                    SetStatus("No smart card readers found", false);
                }
            }
            catch (ToolkitException tEx)
            {
                SetStatus("Reader search failed: " + tEx.Message, false);
            }
            catch (Exception ex)
            {
                SetStatus("Reader search failed: " + ex.Message, false);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task ConnectAsync()
        {
            if (!_toolkit.IsInitialized)
            {
                SetStatus("Please initialize the toolkit first", false);
                return;
            }

            if (string.IsNullOrEmpty(ReaderName))
            {
                // Try auto-detect
                SetBusy(true, "Finding Emirates ID card...");

                try
                {
                    await Task.Run(() => _toolkit.GetReaderWithEmiratesId());
                    ReaderName = _toolkit.ReaderName;
                }
                catch (ToolkitException tEx)
                {
                    SetStatus("No Emirates ID card found: " + tEx.Message, false);
                    SetBusy(false);
                    return;
                }
                catch (Exception ex)
                {
                    SetStatus("Reader detection failed: " + ex.Message, false);
                    SetBusy(false);
                    return;
                }
            }

            SetBusy(true, "Connecting to card...");

            try
            {
                await Task.Run(() => _toolkit.Connect());

                IsConnected = true;
                SetStatus("Connected to " + ReaderName, true);
            }
            catch (ToolkitException tEx)
            {
                SetStatus("Connection failed: " + tEx.Message, false);
            }
            catch (Exception ex)
            {
                SetStatus("Connection failed: " + ex.Message, false);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private async Task DisconnectAsync()
        {
            SetBusy(true, "Disconnecting...");

            try
            {
                await Task.Run(() => _toolkit.Disconnect());

                IsConnected = false;
                SetStatus("Disconnected", true);
            }
            catch (Exception ex)
            {
                SetStatus("Disconnect failed: " + ex.Message, false);
            }
            finally
            {
                SetBusy(false);
            }
        }

        public void CleanupOnClose()
        {
            try { _toolkit?.Disconnect(); } catch { }

            try { _toolkit?.Cleanup(); } catch { }
        }
    }
}
