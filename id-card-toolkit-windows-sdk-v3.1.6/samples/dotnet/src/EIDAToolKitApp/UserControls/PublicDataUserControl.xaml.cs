using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for PublicDataUserControl.xaml
    /// </summary>
    public partial class PublicDataUserControl : UserControl
    {
        public PublicDataUserControl()
        {
            InitializeComponent();
        }

        public void FillPublicDataTextFields(CardPublicData cardPublicDataObj)
        {
            try
            {
                IdNumberText.Text = cardPublicDataObj.IdNumber;
                CardNumberText.Text = cardPublicDataObj.CardNumber;

                #region NonModifiableData

                if (null != cardPublicDataObj.NonModifiablePublicData)
                {
                    IdTypeText.Text = cardPublicDataObj.NonModifiablePublicData.IdType;
                    IssueDateText.Text = cardPublicDataObj.NonModifiablePublicData.IssueDate;
                    ExpiryDateText.Text = cardPublicDataObj.NonModifiablePublicData.ExpiryDate;
                    TitleArabicText.Text = cardPublicDataObj.NonModifiablePublicData.TitleArabic;
                    FullNameArabicText.Text = cardPublicDataObj.NonModifiablePublicData.FullNameArabic;
                    TitleEnglishText.Text = cardPublicDataObj.NonModifiablePublicData.TitleEnglish;
                    FullNameEnglishText.Text = cardPublicDataObj.NonModifiablePublicData.FullNameEnglish;
                    GenderText.Text = cardPublicDataObj.NonModifiablePublicData.Gender;
                    NationalityArabicText.Text = cardPublicDataObj.NonModifiablePublicData.NationalityArabic;
                    NationalityEnglishText.Text = cardPublicDataObj.NonModifiablePublicData.NationalityEnglish;
                    NationalityCodeText.Text = cardPublicDataObj.NonModifiablePublicData.NationalityCode;
                    DateOfBirthText.Text = cardPublicDataObj.NonModifiablePublicData.DateOfBirth;
                    PlaceOfBirthArabicText.Text = cardPublicDataObj.NonModifiablePublicData.PlaceOfBirthArabic;
                    PlaceOfBirthEnglishText.Text = cardPublicDataObj.NonModifiablePublicData.PlaceOfBirthEnglish;
                }

                #endregion

                #region ModifiableData

                if (null != cardPublicDataObj.ModifiablePublicData)
                {
                    OccupationCodeText.Text = cardPublicDataObj.ModifiablePublicData.OccupationCode.ToString();
                    OccupationArabicText.Text = cardPublicDataObj.ModifiablePublicData.OccupationArabic;
                    OccupationEnglishText.Text = cardPublicDataObj.ModifiablePublicData.OccupationEnglish;
                    FamilyIdText.Text = cardPublicDataObj.ModifiablePublicData.FamilyId;
                    OccupationTypeArabicText.Text = cardPublicDataObj.ModifiablePublicData.OccupationTypeArabic;
                    OccupationTypeEnglishText.Text = cardPublicDataObj.ModifiablePublicData.OccupationTypeEnglish;
                    OccupationFieldCodeText.Text = cardPublicDataObj.ModifiablePublicData.OccupationFieldCode;
                    CompanyNameArabicText.Text = cardPublicDataObj.ModifiablePublicData.CompanyNameArabic;
                    CompanyNameEnglishText.Text = cardPublicDataObj.ModifiablePublicData.CompanyNameEnglish;
                    MaritalStatusCodeText.Text = cardPublicDataObj.ModifiablePublicData.MaritalStatusCode;
                    HusbandIdNumberText.Text = cardPublicDataObj.ModifiablePublicData.HusbandIdNumber;
                    SponsorTypeCodeText.Text = cardPublicDataObj.ModifiablePublicData.SponsorTypeCode;
                    SponserUnifiedNumberText.Text = cardPublicDataObj.ModifiablePublicData.SponsorUnifiedNumber;
                    SponsorNameText.Text = cardPublicDataObj.ModifiablePublicData.SponsorName;
                    ResidencyTypeCodeText.Text = cardPublicDataObj.ModifiablePublicData.ResidencyTypeCode;
                    ResidencyNumberText.Text = cardPublicDataObj.ModifiablePublicData.ResidencyNumber;
                    ResidencyExpiryDateText.Text = cardPublicDataObj.ModifiablePublicData.ResidencyExpiryDate;
                    PassportNumberText.Text = cardPublicDataObj.ModifiablePublicData.PassportNumber;
                    PassportTypeCodeText.Text = cardPublicDataObj.ModifiablePublicData.PassportTypeCode;
                    PassportCountryCodeText.Text = cardPublicDataObj.ModifiablePublicData.PassportCountryCode;
                    PassportCountryArabicText.Text = cardPublicDataObj.ModifiablePublicData.PassportCountryArabic;
                    PassportCountryEnglishText.Text = cardPublicDataObj.ModifiablePublicData.PassportCountryEnglish;
                    PassportIssueDateText.Text = cardPublicDataObj.ModifiablePublicData.PassportIssueDate;
                    PassportExpiryDateText.Text = cardPublicDataObj.ModifiablePublicData.PassportExpiryDate;
                    QualificationLevelCodeText.Text = cardPublicDataObj.ModifiablePublicData.QualificationLevelCode;
                    QualificationLevelArabicText.Text = cardPublicDataObj.ModifiablePublicData.QualificationLevelArabic;
                    QualificationLevelEnglishText.Text = cardPublicDataObj.ModifiablePublicData.QualificationLevelEnglish;
                    DegreeDescriptionArabicText.Text = cardPublicDataObj.ModifiablePublicData.DegreeDescriptionArabic;
                    DegreeDescriptionEnglishText.Text = cardPublicDataObj.ModifiablePublicData.DegreeDescriptionEnglish;
                    FieldOfStudyCodeText.Text = cardPublicDataObj.ModifiablePublicData.FieldOfStudyCode;
                    FieldOfStudyArabicText.Text = cardPublicDataObj.ModifiablePublicData.FieldOfStudyArabic;
                    FieldOfStudyEnglishText.Text = cardPublicDataObj.ModifiablePublicData.FieldOfStudyEnglish;
                    PlaceOfStudyArabicText.Text = cardPublicDataObj.ModifiablePublicData.PlaceOfStudyArabic;
                    PlaceOfStudyEnglishText.Text = cardPublicDataObj.ModifiablePublicData.PlaceOfStudyEnglish;
                    DateOfGraduationText.Text = cardPublicDataObj.ModifiablePublicData.DateOfGraduation;
                    MotherFullNameArabicText.Text = cardPublicDataObj.ModifiablePublicData.MotherFullNameArabic;
                    MotherFullNameEnglishText.Text = cardPublicDataObj.ModifiablePublicData.MotherFullNameEnglish;
                }

                #endregion

                #region Home Address

                if (null != cardPublicDataObj.HomeAddress)
                {
                    Home_AddressTypeCodeText.Text = cardPublicDataObj.HomeAddress.AddressTypeCode;
                    Home_LocationCodeText.Text = cardPublicDataObj.HomeAddress.LocationCode;
                    Home_EmirateCodeText.Text = cardPublicDataObj.HomeAddress.EmirateCode;
                    Home_EmirateArabicText.Text = cardPublicDataObj.HomeAddress.EmirateArabic;
                    Home_EmirateEnglishText.Text = cardPublicDataObj.HomeAddress.EmirateEnglish;
                    Home_CityCodeText.Text = cardPublicDataObj.HomeAddress.CityCode;
                    Home_CityArabicText.Text = cardPublicDataObj.HomeAddress.CityArabic;
                    Home_CityEnglishText.Text = cardPublicDataObj.HomeAddress.CityEnglish;
                    Home_StreetArabicText.Text = cardPublicDataObj.HomeAddress.StreetArabic;
                    Home_StreetEnglishText.Text = cardPublicDataObj.HomeAddress.StreetEnglish;
                    Home_PoBoxText.Text = cardPublicDataObj.HomeAddress.PoBox;
                    Home_AreaCodeText.Text = cardPublicDataObj.HomeAddress.AreaCode;
                    Home_AreaArabicText.Text = cardPublicDataObj.HomeAddress.AreaArabic;
                    Home_AreaEnglishText.Text = cardPublicDataObj.HomeAddress.AreaEnglish;
                    Home_BuildingNameArabicText.Text = cardPublicDataObj.HomeAddress.BuildingNameArabic;
                    Home_BuildingNameEnglishText.Text = cardPublicDataObj.HomeAddress.BuildingNameEnglish;
                    Home_LandPhoneNumberText.Text = cardPublicDataObj.HomeAddress.LandPhoneNumber;
                    Home_MobilePhoneNumberText.Text = cardPublicDataObj.HomeAddress.MobilePhoneNumber;
                    Home_EmailText.Text = cardPublicDataObj.HomeAddress.Email;
                    Home_FlatNumberText.Text = cardPublicDataObj.HomeAddress.FlatNumber;
                }
                #endregion

                #region Work Address

                if (null != cardPublicDataObj.WorkAddress)
                {
                    Work_AddressTypeCodeText.Text = cardPublicDataObj.WorkAddress.AddressTypeCode;
                    Work_LocationCodeText.Text = cardPublicDataObj.WorkAddress.LocationCode;
                    Work_EmirateCodeText.Text = cardPublicDataObj.WorkAddress.EmirateCode;
                    Work_EmirateArabicText.Text = cardPublicDataObj.WorkAddress.EmirateArabic;
                    Work_EmirateEnglishText.Text = cardPublicDataObj.WorkAddress.EmirateEnglish;
                    Work_CityCodeText.Text = cardPublicDataObj.WorkAddress.CityCode;
                    Work_CityArabicText.Text = cardPublicDataObj.WorkAddress.CityArabic;
                    Work_CityEnglishText.Text = cardPublicDataObj.WorkAddress.CityEnglish;
                    Work_StreetArabicText.Text = cardPublicDataObj.WorkAddress.StreetArabic;
                    Work_StreetEnglishText.Text = cardPublicDataObj.WorkAddress.StreetEnglish;
                    Work_PoBoxText.Text = cardPublicDataObj.WorkAddress.PoBox;
                    Work_AreaCodeText.Text = cardPublicDataObj.WorkAddress.AreaCode;
                    Work_AreaArabicText.Text = cardPublicDataObj.WorkAddress.AreaArabic;
                    Work_AreaEnglishText.Text = cardPublicDataObj.WorkAddress.AreaEnglish;
                    Work_BuildingNameArabicText.Text = cardPublicDataObj.WorkAddress.BuildingNameArabic;
                    Work_BuildingNameEnglishText.Text = cardPublicDataObj.WorkAddress.BuildingNameEnglish;
                    Work_LandPhoneNumberText.Text = cardPublicDataObj.WorkAddress.LandPhoneNumber;
                    Work_MobilePhoneNumberText.Text = cardPublicDataObj.WorkAddress.MobilePhoneNumber;
                    Work_EmailText.Text = cardPublicDataObj.WorkAddress.Email;
                    Work_CompanyNameArabicText.Text = cardPublicDataObj.WorkAddress.CompanyNameArabic;
                    Work_CompanyNameEnglishText.Text = cardPublicDataObj.WorkAddress.CompanyNameEnglish;
                }

                #endregion

                #region ReadPhotography

                if (null != cardPublicDataObj.CardHolderPhoto)
                {
                    byte[] photoBytes = cardPublicDataObj.CardHolderPhoto.ToArray();
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(photoBytes);
                    bitmap.EndInit();
                    this.PublicDataImage.Source = bitmap;
                }

                #endregion

                #region ReadSignatureImage

                if (null != cardPublicDataObj.HolderSignatureImage)
                {
                    byte[] signatureBytes = cardPublicDataObj.HolderSignatureImage.ToArray();
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(signatureBytes);
                    bitmap.EndInit();
                    this.SignatureDataImage.Source = bitmap;
                }

                #endregion

                CardDataXmlResponse.Text = cardPublicDataObj.XmlString;
                ScrollViewerPublicData.ScrollToTop();
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Clear the public data fields
        /// </summary>
        public void ClearPublicDataTextFields()
        {
            try
            {
                IdNumberText.Text = "";
                CardNumberText.Text = "";

                // Non-Modifiable Data
                IdTypeText.Text = "";
                IssueDateText.Text = "";
                ExpiryDateText.Text = "";
                TitleArabicText.Text = "";
                TitleEnglishText.Text = "";
                FullNameArabicText.Text = "";
                FullNameEnglishText.Text = "";
                GenderText.Text = "";
                NationalityArabicText.Text = "";
                NationalityEnglishText.Text = "";
                NationalityCodeText.Text = "";
                DateOfBirthText.Text = "";
                PlaceOfBirthArabicText.Text = "";
                PlaceOfBirthEnglishText.Text = "";

                // Modifiable Data
                OccupationCodeText.Text = "";
                OccupationArabicText.Text = "";
                OccupationEnglishText.Text = "";
                OccupationTypeArabicText.Text = "";
                OccupationTypeEnglishText.Text = "";
                OccupationFieldCodeText.Text = "";
                CompanyNameArabicText.Text = "";
                CompanyNameEnglishText.Text = "";
                MaritalStatusCodeText.Text = "";
                FamilyIdText.Text = "";
                HusbandIdNumberText.Text = "";
                SponsorTypeCodeText.Text = "";
                SponserUnifiedNumberText.Text = "";
                SponsorNameText.Text = "";
                ResidencyTypeCodeText.Text = "";
                ResidencyNumberText.Text = "";
                ResidencyExpiryDateText.Text = "";
                PassportNumberText.Text = "";
                PassportTypeCodeText.Text = "";
                PassportCountryCodeText.Text = "";
                PassportCountryArabicText.Text = "";
                PassportCountryEnglishText.Text = "";
                PassportIssueDateText.Text = "";
                PassportExpiryDateText.Text = "";
                QualificationLevelCodeText.Text = "";
                QualificationLevelArabicText.Text = "";
                QualificationLevelEnglishText.Text = "";
                DegreeDescriptionArabicText.Text = "";
                DegreeDescriptionEnglishText.Text = "";
                FieldOfStudyCodeText.Text = "";
                FieldOfStudyArabicText.Text = "";
                FieldOfStudyEnglishText.Text = "";
                PlaceOfStudyArabicText.Text = "";
                PlaceOfStudyEnglishText.Text = "";
                DateOfGraduationText.Text = "";
                MotherFullNameArabicText.Text = "";
                MotherFullNameEnglishText.Text = "";

                // Home Address
                Home_AddressTypeCodeText.Text = "";
                Home_LocationCodeText.Text = "";
                Home_EmirateCodeText.Text = "";
                Home_EmirateArabicText.Text = "";
                Home_EmirateEnglishText.Text = "";
                Home_CityCodeText.Text = "";
                Home_CityArabicText.Text = "";
                Home_CityEnglishText.Text = "";
                Home_StreetArabicText.Text = "";
                Home_StreetEnglishText.Text = "";
                Home_PoBoxText.Text = "";
                Home_AreaCodeText.Text = "";
                Home_AreaArabicText.Text = "";
                Home_AreaEnglishText.Text = "";
                Home_BuildingNameArabicText.Text = "";
                Home_BuildingNameEnglishText.Text = "";
                Home_FlatNumberText.Text = "";
                Home_LandPhoneNumberText.Text = "";
                Home_MobilePhoneNumberText.Text = "";
                Home_EmailText.Text = "";

                // Work Address
                Work_AddressTypeCodeText.Text = "";
                Work_LocationCodeText.Text = "";
                Work_CompanyNameArabicText.Text = "";
                Work_CompanyNameEnglishText.Text = "";
                Work_EmirateCodeText.Text = "";
                Work_EmirateArabicText.Text = "";
                Work_EmirateEnglishText.Text = "";
                Work_CityCodeText.Text = "";
                Work_CityArabicText.Text = "";
                Work_CityEnglishText.Text = "";
                Work_StreetArabicText.Text = "";
                Work_StreetEnglishText.Text = "";
                Work_PoBoxText.Text = "";
                Work_AreaCodeText.Text = "";
                Work_AreaArabicText.Text = "";
                Work_AreaEnglishText.Text = "";
                Work_BuildingNameArabicText.Text = "";
                Work_BuildingNameEnglishText.Text = "";
                Work_LandPhoneNumberText.Text = "";
                Work_MobilePhoneNumberText.Text = "";
                Work_EmailText.Text = "";

                // Photo and Signature
                PublicDataImage.Source = null;
                SignatureDataImage.Source = null;

                // Xml Response
                CardDataXmlResponse.Text = "";
                ScrollViewerPublicData.ScrollToTop();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
