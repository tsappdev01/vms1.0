using System;
using System.Threading.Tasks;
using System.Windows.Media;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for NFC authentication and NFC public data read operations
    /// </summary>
    public class NfcOperationsViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // NFC Auth Parameters
        private string _cardNumber;
        private string _dobYear;
        private string _dobMonth;
        private string _dobDay;
        private string _expiryYear;
        private string _expiryMonth;
        private string _expiryDay;
        private bool _nfcAuthSet;

        // NFC Read Public Data checkboxes
        private bool _readNonModifiable;
        private bool _readModifiable;
        private bool _readPhoto;
        private bool _readSignatureImage;
        private bool _readAddress;

        // NFC Read Public Data results
        private string _nfcIdNumber;
        private string _nfcCardNumber;
        private string _nfcFullNameEn;
        private string _nfcFullNameAr;
        private string _nfcNationality;
        private string _nfcGender;
        private string _nfcDateOfBirth;
        private string _nfcExpiryDate;
        private string _nfcResponseStatus;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Commands
        public RelayCommand SetNfcAuthCommand { get; private set; }

        public RelayCommand ClearNfcAuthCommand { get; private set; }

        public RelayCommand ReadPublicDataNfcCommand { get; private set; }

        // NFC Auth Properties
        public string CardNumber
        {
            get { return _cardNumber; }
            set { SetProperty(ref _cardNumber, value); }
        }

        public string DobYear
        {
            get { return _dobYear; }
            set { SetProperty(ref _dobYear, value); }
        }

        public string DobMonth
        {
            get { return _dobMonth; }
            set { SetProperty(ref _dobMonth, value); }
        }

        public string DobDay
        {
            get { return _dobDay; }
            set { SetProperty(ref _dobDay, value); }
        }

        public string ExpiryYear
        {
            get { return _expiryYear; }
            set { SetProperty(ref _expiryYear, value); }
        }

        public string ExpiryMonth
        {
            get { return _expiryMonth; }
            set { SetProperty(ref _expiryMonth, value); }
        }

        public string ExpiryDay
        {
            get { return _expiryDay; }
            set { SetProperty(ref _expiryDay, value); }
        }

        public bool NfcAuthSet
        {
            get { return _nfcAuthSet; }
            set { SetProperty(ref _nfcAuthSet, value); }
        }

        // NFC Read checkboxes
        public bool ReadNonModifiable
        {
            get { return _readNonModifiable; }
            set { SetProperty(ref _readNonModifiable, value); }
        }

        public bool ReadModifiable
        {
            get { return _readModifiable; }
            set { SetProperty(ref _readModifiable, value); }
        }

        public bool ReadPhoto
        {
            get { return _readPhoto; }
            set { SetProperty(ref _readPhoto, value); }
        }

        public bool ReadSignatureImage
        {
            get { return _readSignatureImage; }
            set { SetProperty(ref _readSignatureImage, value); }
        }

        public bool ReadAddress
        {
            get { return _readAddress; }
            set { SetProperty(ref _readAddress, value); }
        }

        // NFC Read results
        public string NfcIdNumber
        {
            get { return _nfcIdNumber; }
            set { SetProperty(ref _nfcIdNumber, value); }
        }

        public string NfcCardNumber
        {
            get { return _nfcCardNumber; }
            set { SetProperty(ref _nfcCardNumber, value); }
        }

        public string NfcFullNameEn
        {
            get { return _nfcFullNameEn; }
            set { SetProperty(ref _nfcFullNameEn, value); }
        }

        public string NfcFullNameAr
        {
            get { return _nfcFullNameAr; }
            set { SetProperty(ref _nfcFullNameAr, value); }
        }

        public string NfcNationality
        {
            get { return _nfcNationality; }
            set { SetProperty(ref _nfcNationality, value); }
        }

        public string NfcGender
        {
            get { return _nfcGender; }
            set { SetProperty(ref _nfcGender, value); }
        }

        public string NfcDateOfBirth
        {
            get { return _nfcDateOfBirth; }
            set { SetProperty(ref _nfcDateOfBirth, value); }
        }

        public string NfcExpiryDate
        {
            get { return _nfcExpiryDate; }
            set { SetProperty(ref _nfcExpiryDate, value); }
        }

        public string NfcResponseStatus
        {
            get { return _nfcResponseStatus; }
            set { SetProperty(ref _nfcResponseStatus, value); }
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

        public NfcOperationsViewModel(MainViewModel main)
        {
            _main = main;
            _readNonModifiable = true;
            _readModifiable = true;
            _readPhoto = true;
            _readSignatureImage = true;
            _readAddress = true;

            SetNfcAuthCommand = new RelayCommand(WrapAsync(SetNfcAuthAsync));
            ClearNfcAuthCommand = new RelayCommand(() => ClearNfcAuth());
            ReadPublicDataNfcCommand = new RelayCommand(WrapAsync(ReadPublicDataNfcAsync));
        }

        private void SetOpStatus(string message, bool isSuccess)
        {
            OperationStatus = message;
            OperationStatusColor = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
            _main.SetStatus(message, isSuccess);
        }

        private async Task SetNfcAuthAsync()
        {
            if (string.IsNullOrEmpty(CardNumber) ||
                string.IsNullOrEmpty(DobYear) ||
                string.IsNullOrEmpty(DobMonth) ||
                string.IsNullOrEmpty(DobDay) ||
                string.IsNullOrEmpty(ExpiryYear) ||
                string.IsNullOrEmpty(ExpiryMonth) ||
                string.IsNullOrEmpty(ExpiryDay))
            {
                SetOpStatus("Please fill all NFC authentication fields", false);
                return;
            }

            _main.SetBusy(true, "Setting NFC authentication parameters...");

            try
            {
                string dateOfBirth = DobYear + DobMonth + DobDay;
                string expiryDate = ExpiryYear + ExpiryMonth + ExpiryDay;

                await Task.Run(() =>
                    _main.Toolkit.SetNfcAuthenticationParameters(
                        CardNumber, dateOfBirth, expiryDate));

                NfcAuthSet = true;
                SetOpStatus("NFC authentication parameters set successfully", true);
            }
            catch (ToolkitException tEx)
            {
                NfcAuthSet = false;
                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                NfcAuthSet = false;
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private void ClearNfcAuth()
        {
            CardNumber = string.Empty;
            DobYear = string.Empty;
            DobMonth = string.Empty;
            DobDay = string.Empty;
            ExpiryYear = string.Empty;
            ExpiryMonth = string.Empty;
            ExpiryDay = string.Empty;
            NfcAuthSet = false;
            ClearResults();
            OperationStatus = string.Empty;
        }

        private void ClearResults()
        {
            NfcIdNumber = string.Empty;
            NfcCardNumber = string.Empty;
            NfcFullNameEn = string.Empty;
            NfcFullNameAr = string.Empty;
            NfcNationality = string.Empty;
            NfcGender = string.Empty;
            NfcDateOfBirth = string.Empty;
            NfcExpiryDate = string.Empty;
            NfcResponseStatus = string.Empty;
        }

        private async Task ReadPublicDataNfcAsync()
        {
            if (!NfcAuthSet)
            {
                SetOpStatus("Please set NFC authentication parameters first", false);
                return;
            }

            _main.SetBusy(true, "Reading public data using NFC...");

            try
            {
                CardPublicData data = await Task.Run(() =>
                    _main.Toolkit.ReadPublicData(
                        _readNonModifiable, _readModifiable,
                        _readPhoto, _readSignatureImage, _readAddress));

                if (data != null)
                {
                    NfcIdNumber = data.IdNumber;
                    NfcCardNumber = data.CardNumber;

                    if (data.NonModifiablePublicData != null)
                    {
                        NfcFullNameEn = data.NonModifiablePublicData.FullNameEnglish;
                        NfcFullNameAr = data.NonModifiablePublicData.FullNameArabic;
                        NfcNationality = data.NonModifiablePublicData.NationalityEnglish;
                        NfcGender = data.NonModifiablePublicData.Gender;
                        NfcDateOfBirth = data.NonModifiablePublicData.DateOfBirth;
                        NfcExpiryDate = data.NonModifiablePublicData.ExpiryDate;
                    }

                    NfcResponseStatus = data.ResponseStatus.ToString();
                    SetOpStatus("NFC public data read: " + NfcResponseStatus, true);
                }
                else
                {
                    SetOpStatus("No public data returned", false);
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
    }
}
