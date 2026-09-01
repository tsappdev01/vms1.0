using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using AE.EmiratesId.IdCard;
using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitModernApp.ViewModels
{
    /// <summary>
    /// ViewModel for reading and displaying card public data
    /// </summary>
    public class PublicDataViewModel : BaseViewModel
    {
        private readonly MainViewModel _main;

        // Read options
        private bool _readNonModifiable = true;
        private bool _readModifiable = true;
        private bool _readPhoto = true;
        private bool _readSignature = true;
        private bool _readAddress = true;

        // Identity fields
        private string _idNumber;
        private string _cardNumber;

        // Non-Modifiable
        private string _idType;
        private string _issueDate;
        private string _expiryDate;
        private string _titleArabic;
        private string _titleEnglish;
        private string _fullNameArabic;
        private string _fullNameEnglish;
        private string _gender;
        private string _nationalityArabic;
        private string _nationalityEnglish;
        private string _nationalityCode;
        private string _dateOfBirth;
        private string _placeOfBirthArabic;
        private string _placeOfBirthEnglish;

        // Modifiable
        private string _occupationArabic;
        private string _occupationEnglish;
        private string _familyId;
        private string _companyNameArabic;
        private string _companyNameEnglish;
        private string _maritalStatusCode;
        private string _sponsorName;
        private string _residencyNumber;
        private string _residencyExpiryDate;
        private string _passportNumber;
        private string _passportCountryEnglish;
        private string _passportExpiryDate;
        private string _motherFullNameArabic;
        private string _motherFullNameEnglish;

        // Address
        private string _homeEmirateEnglish;
        private string _homeCityEnglish;
        private string _homeStreetEnglish;
        private string _homePoBox;
        private string _homeAreaEnglish;
        private string _homeBuildingNameEnglish;
        private string _homePhoneNumber;
        private string _homeMobileNumber;
        private string _homeEmail;
        private string _workEmirateEnglish;
        private string _workCityEnglish;
        private string _workStreetEnglish;
        private string _workPoBox;

        // Photo
        private BitmapImage _cardHolderPhoto;
        private BitmapImage _signatureImage;

        // XML Response
        private string _xmlResponse;

        // Status
        private string _operationStatus;
        private SolidColorBrush _operationStatusColor;

        // Commands
        public RelayCommand ReadPublicDataCommand { get; private set; }

        public RelayCommand ClearDataCommand { get; private set; }

        // Read Options
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

        public bool ReadSignature
        {
            get { return _readSignature; }
            set { SetProperty(ref _readSignature, value); }
        }

        public bool ReadAddress
        {
            get { return _readAddress; }
            set { SetProperty(ref _readAddress, value); }
        }

        // Identity
        public string IdNumber
        {
            get { return _idNumber; }
            set { SetProperty(ref _idNumber, value); }
        }

        public string CardNumber
        {
            get { return _cardNumber; }
            set { SetProperty(ref _cardNumber, value); }
        }

        // Non-Modifiable
        public string IdType
        {
            get { return _idType; }
            set { SetProperty(ref _idType, value); }
        }

        public string IssueDate
        {
            get { return _issueDate; }
            set { SetProperty(ref _issueDate, value); }
        }

        public string ExpiryDate
        {
            get { return _expiryDate; }
            set { SetProperty(ref _expiryDate, value); }
        }

        public string TitleArabic
        {
            get { return _titleArabic; }
            set { SetProperty(ref _titleArabic, value); }
        }

        public string TitleEnglish
        {
            get { return _titleEnglish; }
            set { SetProperty(ref _titleEnglish, value); }
        }

        public string FullNameArabic
        {
            get { return _fullNameArabic; }
            set { SetProperty(ref _fullNameArabic, value); }
        }

        public string FullNameEnglish
        {
            get { return _fullNameEnglish; }
            set { SetProperty(ref _fullNameEnglish, value); }
        }

        public string Gender
        {
            get { return _gender; }
            set { SetProperty(ref _gender, value); }
        }

        public string NationalityArabic
        {
            get { return _nationalityArabic; }
            set { SetProperty(ref _nationalityArabic, value); }
        }

        public string NationalityEnglish
        {
            get { return _nationalityEnglish; }
            set { SetProperty(ref _nationalityEnglish, value); }
        }

        public string NationalityCode
        {
            get { return _nationalityCode; }
            set { SetProperty(ref _nationalityCode, value); }
        }

        public string DateOfBirth
        {
            get { return _dateOfBirth; }
            set { SetProperty(ref _dateOfBirth, value); }
        }

        public string PlaceOfBirthArabic
        {
            get { return _placeOfBirthArabic; }
            set { SetProperty(ref _placeOfBirthArabic, value); }
        }

        public string PlaceOfBirthEnglish
        {
            get { return _placeOfBirthEnglish; }
            set { SetProperty(ref _placeOfBirthEnglish, value); }
        }

        // Modifiable
        public string OccupationArabic
        {
            get { return _occupationArabic; }
            set { SetProperty(ref _occupationArabic, value); }
        }

        public string OccupationEnglish
        {
            get { return _occupationEnglish; }
            set { SetProperty(ref _occupationEnglish, value); }
        }

        public string FamilyId
        {
            get { return _familyId; }
            set { SetProperty(ref _familyId, value); }
        }

        public string CompanyNameArabic
        {
            get { return _companyNameArabic; }
            set { SetProperty(ref _companyNameArabic, value); }
        }

        public string CompanyNameEnglish
        {
            get { return _companyNameEnglish; }
            set { SetProperty(ref _companyNameEnglish, value); }
        }

        public string MaritalStatusCode
        {
            get { return _maritalStatusCode; }
            set { SetProperty(ref _maritalStatusCode, value); }
        }

        public string SponsorName
        {
            get { return _sponsorName; }
            set { SetProperty(ref _sponsorName, value); }
        }

        public string ResidencyNumber
        {
            get { return _residencyNumber; }
            set { SetProperty(ref _residencyNumber, value); }
        }

        public string ResidencyExpiryDate
        {
            get { return _residencyExpiryDate; }
            set { SetProperty(ref _residencyExpiryDate, value); }
        }

        public string PassportNumber
        {
            get { return _passportNumber; }
            set { SetProperty(ref _passportNumber, value); }
        }

        public string PassportCountryEnglish
        {
            get { return _passportCountryEnglish; }
            set { SetProperty(ref _passportCountryEnglish, value); }
        }

        public string PassportExpiryDate
        {
            get { return _passportExpiryDate; }
            set { SetProperty(ref _passportExpiryDate, value); }
        }

        public string MotherFullNameArabic
        {
            get { return _motherFullNameArabic; }
            set { SetProperty(ref _motherFullNameArabic, value); }
        }

        public string MotherFullNameEnglish
        {
            get { return _motherFullNameEnglish; }
            set { SetProperty(ref _motherFullNameEnglish, value); }
        }

        // Address
        public string HomeEmirateEnglish
        {
            get { return _homeEmirateEnglish; }
            set { SetProperty(ref _homeEmirateEnglish, value); }
        }

        public string HomeCityEnglish
        {
            get { return _homeCityEnglish; }
            set { SetProperty(ref _homeCityEnglish, value); }
        }

        public string HomeStreetEnglish
        {
            get { return _homeStreetEnglish; }
            set { SetProperty(ref _homeStreetEnglish, value); }
        }

        public string HomePoBox
        {
            get { return _homePoBox; }
            set { SetProperty(ref _homePoBox, value); }
        }

        public string HomeAreaEnglish
        {
            get { return _homeAreaEnglish; }
            set { SetProperty(ref _homeAreaEnglish, value); }
        }

        public string HomeBuildingNameEnglish
        {
            get { return _homeBuildingNameEnglish; }
            set { SetProperty(ref _homeBuildingNameEnglish, value); }
        }

        public string HomePhoneNumber
        {
            get { return _homePhoneNumber; }
            set { SetProperty(ref _homePhoneNumber, value); }
        }

        public string HomeMobileNumber
        {
            get { return _homeMobileNumber; }
            set { SetProperty(ref _homeMobileNumber, value); }
        }

        public string HomeEmail
        {
            get { return _homeEmail; }
            set { SetProperty(ref _homeEmail, value); }
        }

        public string WorkEmirateEnglish
        {
            get { return _workEmirateEnglish; }
            set { SetProperty(ref _workEmirateEnglish, value); }
        }

        public string WorkCityEnglish
        {
            get { return _workCityEnglish; }
            set { SetProperty(ref _workCityEnglish, value); }
        }

        public string WorkStreetEnglish
        {
            get { return _workStreetEnglish; }
            set { SetProperty(ref _workStreetEnglish, value); }
        }

        public string WorkPoBox
        {
            get { return _workPoBox; }
            set { SetProperty(ref _workPoBox, value); }
        }

        // Photo
        public BitmapImage CardHolderPhoto
        {
            get { return _cardHolderPhoto; }
            set { SetProperty(ref _cardHolderPhoto, value); }
        }

        public BitmapImage SignatureImage
        {
            get { return _signatureImage; }
            set { SetProperty(ref _signatureImage, value); }
        }

        // XML
        public string XmlResponse
        {
            get { return _xmlResponse; }
            set { SetProperty(ref _xmlResponse, value); }
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

        public PublicDataViewModel(MainViewModel main)
        {
            _main = main;

            ReadPublicDataCommand = new RelayCommand(WrapAsync(ReadPublicDataAsync));
            ClearDataCommand = new RelayCommand(() => ClearAllFields());
        }

        private void SetOpStatus(string message, bool isSuccess)
        {
            OperationStatus = message;
            OperationStatusColor = isSuccess
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#107C10"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D13438"));
            _main.SetStatus(message, isSuccess);
        }

        private void ClearAllFields()
        {
            IdNumber = null;
            CardNumber = null;
            IdType = null;
            IssueDate = null;
            ExpiryDate = null;
            TitleArabic = null;
            TitleEnglish = null;
            FullNameArabic = null;
            FullNameEnglish = null;
            Gender = null;
            NationalityArabic = null;
            NationalityEnglish = null;
            NationalityCode = null;
            DateOfBirth = null;
            PlaceOfBirthArabic = null;
            PlaceOfBirthEnglish = null;
            OccupationArabic = null;
            OccupationEnglish = null;
            FamilyId = null;
            CompanyNameArabic = null;
            CompanyNameEnglish = null;
            MaritalStatusCode = null;
            SponsorName = null;
            ResidencyNumber = null;
            ResidencyExpiryDate = null;
            PassportNumber = null;
            PassportCountryEnglish = null;
            PassportExpiryDate = null;
            MotherFullNameArabic = null;
            MotherFullNameEnglish = null;
            HomeEmirateEnglish = null;
            HomeCityEnglish = null;
            HomeStreetEnglish = null;
            HomePoBox = null;
            HomeAreaEnglish = null;
            HomeBuildingNameEnglish = null;
            HomePhoneNumber = null;
            HomeMobileNumber = null;
            HomeEmail = null;
            WorkEmirateEnglish = null;
            WorkCityEnglish = null;
            WorkStreetEnglish = null;
            WorkPoBox = null;
            CardHolderPhoto = null;
            SignatureImage = null;
            XmlResponse = null;
            OperationStatus = null;
        }

        private async Task ReadPublicDataAsync()
        {
            _main.SetBusy(true, "Reading public data...");

            try
            {
                CardPublicData data = await Task.Run(() =>
                    _main.Toolkit.ReadPublicData(
                        _readNonModifiable, _readModifiable,
                        _readPhoto, _readSignature, _readAddress));

                FillFields(data);
                SetOpStatus("Public data read: " + data.ResponseStatus.ToString(), true);
            }
            catch (ToolkitException tEx)
            {
                ClearAllFields();
                SetOpStatus(tEx.Message, false);
            }
            catch (Exception ex)
            {
                ClearAllFields();
                SetOpStatus(ex.Message, false);
            }
            finally
            {
                _main.SetBusy(false);
            }
        }

        private void FillFields(CardPublicData data)
        {
            IdNumber = data.IdNumber;
            CardNumber = data.CardNumber;

            if (data.NonModifiablePublicData != null)
            {
                IdType = data.NonModifiablePublicData.IdType;
                IssueDate = data.NonModifiablePublicData.IssueDate;
                ExpiryDate = data.NonModifiablePublicData.ExpiryDate;
                TitleArabic = data.NonModifiablePublicData.TitleArabic;
                TitleEnglish = data.NonModifiablePublicData.TitleEnglish;
                FullNameArabic = data.NonModifiablePublicData.FullNameArabic;
                FullNameEnglish = data.NonModifiablePublicData.FullNameEnglish;
                Gender = data.NonModifiablePublicData.Gender;
                NationalityArabic = data.NonModifiablePublicData.NationalityArabic;
                NationalityEnglish = data.NonModifiablePublicData.NationalityEnglish;
                NationalityCode = data.NonModifiablePublicData.NationalityCode;
                DateOfBirth = data.NonModifiablePublicData.DateOfBirth;
                PlaceOfBirthArabic = data.NonModifiablePublicData.PlaceOfBirthArabic;
                PlaceOfBirthEnglish = data.NonModifiablePublicData.PlaceOfBirthEnglish;
            }

            if (data.ModifiablePublicData != null)
            {
                OccupationArabic = data.ModifiablePublicData.OccupationArabic;
                OccupationEnglish = data.ModifiablePublicData.OccupationEnglish;
                FamilyId = data.ModifiablePublicData.FamilyId;
                CompanyNameArabic = data.ModifiablePublicData.CompanyNameArabic;
                CompanyNameEnglish = data.ModifiablePublicData.CompanyNameEnglish;
                MaritalStatusCode = data.ModifiablePublicData.MaritalStatusCode;
                SponsorName = data.ModifiablePublicData.SponsorName;
                ResidencyNumber = data.ModifiablePublicData.ResidencyNumber;
                ResidencyExpiryDate = data.ModifiablePublicData.ResidencyExpiryDate;
                PassportNumber = data.ModifiablePublicData.PassportNumber;
                PassportCountryEnglish = data.ModifiablePublicData.PassportCountryEnglish;
                PassportExpiryDate = data.ModifiablePublicData.PassportExpiryDate;
                MotherFullNameArabic = data.ModifiablePublicData.MotherFullNameArabic;
                MotherFullNameEnglish = data.ModifiablePublicData.MotherFullNameEnglish;
            }

            if (data.HomeAddress != null)
            {
                HomeEmirateEnglish = data.HomeAddress.EmirateEnglish;
                HomeCityEnglish = data.HomeAddress.CityEnglish;
                HomeStreetEnglish = data.HomeAddress.StreetEnglish;
                HomePoBox = data.HomeAddress.PoBox;
                HomeAreaEnglish = data.HomeAddress.AreaEnglish;
                HomeBuildingNameEnglish = data.HomeAddress.BuildingNameEnglish;
                HomePhoneNumber = data.HomeAddress.LandPhoneNumber;
                HomeMobileNumber = data.HomeAddress.MobilePhoneNumber;
                HomeEmail = data.HomeAddress.Email;
            }

            if (data.WorkAddress != null)
            {
                WorkEmirateEnglish = data.WorkAddress.EmirateEnglish;
                WorkCityEnglish = data.WorkAddress.CityEnglish;
                WorkStreetEnglish = data.WorkAddress.StreetEnglish;
                WorkPoBox = data.WorkAddress.PoBox;
            }

            if (data.CardHolderPhoto != null)
            {
                byte[] photoBytes = data.CardHolderPhoto.ToArray();
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = new MemoryStream(photoBytes);
                bitmap.EndInit();
                bitmap.Freeze();
                CardHolderPhoto = bitmap;
            }

            if (data.HolderSignatureImage != null)
            {
                // HolderSignatureImage is typically a TIFF (CCITT-compressed) payload.
                // Most WIC pipelines decode it, but a few codecs fail -- guard so that
                // an unreadable signature does not wipe all the public-data fields
                // we just populated above.
                try
                {
                    byte[] sigBytes = data.HolderSignatureImage.ToArray();
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = new MemoryStream(sigBytes);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    SignatureImage = bitmap;
                }
                catch
                {
                    SignatureImage = null;
                }
            }

            XmlResponse = data.XmlString;
        }
    }
}
