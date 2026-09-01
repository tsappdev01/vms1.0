
using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for PublicDataUsingNFCUserControl.xaml
    /// </summary>
    public partial class PublicDataUsingNFCUserControl : UserControl
    {
        public PublicDataUsingNFCUserControl()
        {
            InitializeComponent();
        }

        public void FillPublicUsingNFCDataTextFields(CardPublicData cardPublicDataObj)
        {

            NFC_IdNumberText.Text = cardPublicDataObj.IdNumber;
            NFC_CardNumberText.Text = cardPublicDataObj.CardNumber;

            #region NonModifiableData

            if (null != cardPublicDataObj.NonModifiablePublicData)
            {
                NFC_IdTypeText.Text = cardPublicDataObj.NonModifiablePublicData.IdType;
                NFC_IssueDateText.Text = cardPublicDataObj.NonModifiablePublicData.IssueDate;
                NFC_ExpiryDateText.Text = cardPublicDataObj.NonModifiablePublicData.ExpiryDate;
                NFC_TitleArabicText.Text = cardPublicDataObj.NonModifiablePublicData.TitleArabic;
                NFC_FullNameArabicText.Text = cardPublicDataObj.NonModifiablePublicData.FullNameArabic;
                NFC_TitleEnglishText.Text = cardPublicDataObj.NonModifiablePublicData.TitleEnglish;
                NFC_FullNameEnglishText.Text = cardPublicDataObj.NonModifiablePublicData.FullNameEnglish;
                NFC_GenderText.Text = cardPublicDataObj.NonModifiablePublicData.Gender;
                NFC_NationalityArabicText.Text = cardPublicDataObj.NonModifiablePublicData.NationalityArabic;
                NFC_NationalityEnglishText.Text = cardPublicDataObj.NonModifiablePublicData.NationalityEnglish;
                NFC_NationalityCodeText.Text = cardPublicDataObj.NonModifiablePublicData.NationalityCode;
                NFC_DateOfBirthText.Text = cardPublicDataObj.NonModifiablePublicData.DateOfBirth;
                NFC_PlaceOfBirthArabicText.Text = cardPublicDataObj.NonModifiablePublicData.PlaceOfBirthArabic;
                NFC_PlaceOfBirthEnglishText.Text = cardPublicDataObj.NonModifiablePublicData.PlaceOfBirthEnglish;
            }

            #endregion

            #region ModifiableData

            if (null != cardPublicDataObj.ModifiablePublicData)
            {
                NFC_OccupationCodeText.Text = cardPublicDataObj.ModifiablePublicData.OccupationCode.ToString();
                NFC_OccupationArabicText.Text = cardPublicDataObj.ModifiablePublicData.OccupationArabic;
                NFC_OccupationEnglishText.Text = cardPublicDataObj.ModifiablePublicData.OccupationEnglish;
                NFC_FamilyIdText.Text = cardPublicDataObj.ModifiablePublicData.FamilyId;
                NFC_OccupationTypeArabicText.Text = cardPublicDataObj.ModifiablePublicData.OccupationTypeArabic;
                NFC_OccupationTypeEnglishText.Text = cardPublicDataObj.ModifiablePublicData.OccupationTypeEnglish;
                NFC_OccupationFieldCodeText.Text = cardPublicDataObj.ModifiablePublicData.OccupationFieldCode;
                NFC_CompanyNameArabicText.Text = cardPublicDataObj.ModifiablePublicData.CompanyNameArabic;
                NFC_CompanyNameEnglishText.Text = cardPublicDataObj.ModifiablePublicData.CompanyNameEnglish;
                NFC_MaritalStatusCodeText.Text = cardPublicDataObj.ModifiablePublicData.MaritalStatusCode;
                NFC_HusbandIdNumberText.Text = cardPublicDataObj.ModifiablePublicData.HusbandIdNumber;
                NFC_SponsorTypeCodeText.Text = cardPublicDataObj.ModifiablePublicData.SponsorTypeCode;
                NFC_SponserUnifiedNumberText.Text = cardPublicDataObj.ModifiablePublicData.SponsorUnifiedNumber;
                NFC_SponsorNameText.Text = cardPublicDataObj.ModifiablePublicData.SponsorName;
                NFC_ResidencyTypeCodeText.Text = cardPublicDataObj.ModifiablePublicData.ResidencyTypeCode;
                NFC_ResidencyNumberText.Text = cardPublicDataObj.ModifiablePublicData.ResidencyNumber;
                NFC_ResidencyExpiryDateText.Text = cardPublicDataObj.ModifiablePublicData.ResidencyExpiryDate;
                NFC_PassportNumberText.Text = cardPublicDataObj.ModifiablePublicData.PassportNumber;
                NFC_PassportTypeCodeText.Text = cardPublicDataObj.ModifiablePublicData.PassportTypeCode;
                NFC_PassportCountryCodeText.Text = cardPublicDataObj.ModifiablePublicData.PassportCountryCode;
                NFC_PassportCountryArabicText.Text = cardPublicDataObj.ModifiablePublicData.PassportCountryArabic;
                NFC_PassportCountryEnglishText.Text = cardPublicDataObj.ModifiablePublicData.PassportCountryEnglish;
                NFC_PassportIssueDateText.Text = cardPublicDataObj.ModifiablePublicData.PassportIssueDate;
                NFC_PassportExpiryDateText.Text = cardPublicDataObj.ModifiablePublicData.PassportExpiryDate;
                NFC_QualificationLevelCodeText.Text = cardPublicDataObj.ModifiablePublicData.QualificationLevelCode;
                NFC_QualificationLevelArabicText.Text = cardPublicDataObj.ModifiablePublicData.QualificationLevelArabic;
                NFC_QualificationLevelEnglishText.Text = cardPublicDataObj.ModifiablePublicData.QualificationLevelEnglish;
                NFC_DegreeDescriptionArabicText.Text = cardPublicDataObj.ModifiablePublicData.DegreeDescriptionArabic;
                NFC_DegreeDescriptionEnglishText.Text = cardPublicDataObj.ModifiablePublicData.DegreeDescriptionEnglish;
                NFC_FieldOfStudyCodeText.Text = cardPublicDataObj.ModifiablePublicData.FieldOfStudyCode;
                NFC_FieldOfStudyArabicText.Text = cardPublicDataObj.ModifiablePublicData.FieldOfStudyArabic;
                NFC_FieldOfStudyEnglishText.Text = cardPublicDataObj.ModifiablePublicData.FieldOfStudyEnglish;
                NFC_PlaceOfStudyArabicText.Text = cardPublicDataObj.ModifiablePublicData.PlaceOfStudyArabic;
                NFC_PlaceOfStudyEnglishText.Text = cardPublicDataObj.ModifiablePublicData.PlaceOfStudyEnglish;
                NFC_DateOfGraduationText.Text = cardPublicDataObj.ModifiablePublicData.DateOfGraduation;
                NFC_MotherFullNameArabicText.Text = cardPublicDataObj.ModifiablePublicData.MotherFullNameArabic;
                NFC_MotherFullNameEnglishText.Text = cardPublicDataObj.ModifiablePublicData.MotherFullNameEnglish;
            }

            #endregion

            #region Home Address

            if (null != cardPublicDataObj.HomeAddress)
            {
                NFC_Home_AddressTypeCodeText.Text = cardPublicDataObj.HomeAddress.AddressTypeCode;
                NFC_Home_LocationCodeText.Text = cardPublicDataObj.HomeAddress.LocationCode;
                NFC_Home_EmirateCodeText.Text = cardPublicDataObj.HomeAddress.EmirateCode;
                NFC_Home_EmirateArabicText.Text = cardPublicDataObj.HomeAddress.EmirateArabic;
                NFC_Home_EmirateEnglishText.Text = cardPublicDataObj.HomeAddress.EmirateEnglish;
                NFC_Home_CityCodeText.Text = cardPublicDataObj.HomeAddress.CityCode;
                NFC_Home_CityArabicText.Text = cardPublicDataObj.HomeAddress.CityArabic;
                NFC_Home_CityEnglishText.Text = cardPublicDataObj.HomeAddress.CityEnglish;
                NFC_Home_StreetArabicText.Text = cardPublicDataObj.HomeAddress.StreetArabic;
                NFC_Home_StreetEnglishText.Text = cardPublicDataObj.HomeAddress.StreetEnglish;
                NFC_Home_PoBoxText.Text = cardPublicDataObj.HomeAddress.PoBox;
                NFC_Home_AreaCodeText.Text = cardPublicDataObj.HomeAddress.AreaCode;
                NFC_Home_AreaArabicText.Text = cardPublicDataObj.HomeAddress.AreaArabic;
                NFC_Home_AreaEnglishText.Text = cardPublicDataObj.HomeAddress.AreaEnglish;
                NFC_Home_BuildingNameArabicText.Text = cardPublicDataObj.HomeAddress.BuildingNameArabic;
                NFC_Home_BuildingNameEnglishText.Text = cardPublicDataObj.HomeAddress.BuildingNameEnglish;
                NFC_Home_LandPhoneNumberText.Text = cardPublicDataObj.HomeAddress.LandPhoneNumber;
                NFC_Home_MobilePhoneNumberText.Text = cardPublicDataObj.HomeAddress.MobilePhoneNumber;
                NFC_Home_EmailText.Text = cardPublicDataObj.HomeAddress.Email;
                NFC_Home_FlatNumberText.Text = cardPublicDataObj.HomeAddress.FlatNumber;
            }

            #endregion

            #region Work Address

            if (null != cardPublicDataObj.WorkAddress)
            {
                NFC_Work_AddressTypeCodeText.Text = cardPublicDataObj.WorkAddress.AddressTypeCode;
                NFC_Work_LocationCodeText.Text = cardPublicDataObj.WorkAddress.LocationCode;
                NFC_Work_EmirateCodeText.Text = cardPublicDataObj.WorkAddress.EmirateCode;
                NFC_Work_EmirateArabicText.Text = cardPublicDataObj.WorkAddress.EmirateArabic;
                NFC_Work_EmirateEnglishText.Text = cardPublicDataObj.WorkAddress.EmirateEnglish;
                NFC_Work_CityCodeText.Text = cardPublicDataObj.WorkAddress.CityCode;
                NFC_Work_CityArabicText.Text = cardPublicDataObj.WorkAddress.CityArabic;
                NFC_Work_CityEnglishText.Text = cardPublicDataObj.WorkAddress.CityEnglish;
                NFC_Work_StreetArabicText.Text = cardPublicDataObj.WorkAddress.StreetArabic;
                NFC_Work_StreetEnglishText.Text = cardPublicDataObj.WorkAddress.StreetEnglish;
                NFC_Work_PoBoxText.Text = cardPublicDataObj.WorkAddress.PoBox;
                NFC_Work_AreaCodeText.Text = cardPublicDataObj.WorkAddress.AreaCode;
                NFC_Work_AreaArabicText.Text = cardPublicDataObj.WorkAddress.AreaArabic;
                NFC_Work_AreaEnglishText.Text = cardPublicDataObj.WorkAddress.AreaEnglish;
                NFC_Work_BuildingNameArabicText.Text = cardPublicDataObj.WorkAddress.BuildingNameArabic;
                NFC_Work_BuildingNameEnglishText.Text = cardPublicDataObj.WorkAddress.BuildingNameEnglish;
                NFC_Work_LandPhoneNumberText.Text = cardPublicDataObj.WorkAddress.LandPhoneNumber;
                NFC_Work_MobilePhoneNumberText.Text = cardPublicDataObj.WorkAddress.MobilePhoneNumber;
                NFC_Work_EmailText.Text = cardPublicDataObj.WorkAddress.Email;
                NFC_Work_CompanyNameArabicText.Text = cardPublicDataObj.WorkAddress.CompanyNameArabic;
                NFC_Work_CompanyNameEnglishText.Text = cardPublicDataObj.WorkAddress.CompanyNameEnglish;
            }

            #endregion

            #region ReadPhotography

            if (null != cardPublicDataObj.CardHolderPhoto)
            {
                byte[] photoBytes = cardPublicDataObj.CardHolderPhoto.ToArray();
                BitmapImage bitmapPhoto = new BitmapImage();
                bitmapPhoto.BeginInit();
                bitmapPhoto.StreamSource = new MemoryStream(photoBytes);
                bitmapPhoto.EndInit();
                NFC_PublicDataImage.Source = bitmapPhoto;
            }

            #endregion

            #region ReadSignatueImage

            if (null != cardPublicDataObj.HolderSignatureImage)
            {
                byte[] signatureBytes = cardPublicDataObj.HolderSignatureImage.ToArray();
                BitmapImage bitmapSignature = new BitmapImage();
                bitmapSignature.BeginInit();
                bitmapSignature.StreamSource = new MemoryStream(signatureBytes);
                bitmapSignature.EndInit();
                NFC_SignatureDataImage.Source = bitmapSignature;
            }

            #endregion

            CardDataUsingNfcXmlResponse.Text = cardPublicDataObj.XmlString;
            ScrollViewerNfcPublicData.ScrollToTop();
        }

        public void ClearPublicUsingNFCDataTextFields()
        {
            try
            {
                NFC_IdNumberText.Text = "";
                NFC_CardNumberText.Text = "";

                // Non-Modifiable Data
                NFC_IdTypeText.Text = "";
                NFC_IssueDateText.Text = "";
                NFC_ExpiryDateText.Text = "";
                NFC_TitleArabicText.Text = "";
                NFC_TitleEnglishText.Text = "";
                NFC_FullNameArabicText.Text = "";
                NFC_FullNameEnglishText.Text = "";
                NFC_GenderText.Text = "";
                NFC_NationalityArabicText.Text = "";
                NFC_NationalityEnglishText.Text = "";
                NFC_NationalityCodeText.Text = "";
                NFC_DateOfBirthText.Text = "";
                NFC_PlaceOfBirthArabicText.Text = "";
                NFC_PlaceOfBirthEnglishText.Text = "";

                // Modifiable Data
                NFC_OccupationCodeText.Text = "";
                NFC_OccupationArabicText.Text = "";
                NFC_OccupationEnglishText.Text = "";
                NFC_OccupationTypeArabicText.Text = "";
                NFC_OccupationTypeEnglishText.Text = "";
                NFC_OccupationFieldCodeText.Text = "";
                NFC_CompanyNameArabicText.Text = "";
                NFC_CompanyNameEnglishText.Text = "";
                NFC_MaritalStatusCodeText.Text = "";
                NFC_FamilyIdText.Text = "";
                NFC_HusbandIdNumberText.Text = "";
                NFC_SponsorTypeCodeText.Text = "";
                NFC_SponserUnifiedNumberText.Text = "";
                NFC_SponsorNameText.Text = "";
                NFC_ResidencyTypeCodeText.Text = "";
                NFC_ResidencyNumberText.Text = "";
                NFC_ResidencyExpiryDateText.Text = "";
                NFC_PassportNumberText.Text = "";
                NFC_PassportTypeCodeText.Text = "";
                NFC_PassportCountryCodeText.Text = "";
                NFC_PassportCountryArabicText.Text = "";
                NFC_PassportCountryEnglishText.Text = "";
                NFC_PassportIssueDateText.Text = "";
                NFC_PassportExpiryDateText.Text = "";
                NFC_QualificationLevelCodeText.Text = "";
                NFC_QualificationLevelArabicText.Text = "";
                NFC_QualificationLevelEnglishText.Text = "";
                NFC_DegreeDescriptionArabicText.Text = "";
                NFC_DegreeDescriptionEnglishText.Text = "";
                NFC_FieldOfStudyCodeText.Text = "";
                NFC_FieldOfStudyArabicText.Text = "";
                NFC_FieldOfStudyEnglishText.Text = "";
                NFC_PlaceOfStudyArabicText.Text = "";
                NFC_PlaceOfStudyEnglishText.Text = "";
                NFC_DateOfGraduationText.Text = "";
                NFC_MotherFullNameArabicText.Text = "";
                NFC_MotherFullNameEnglishText.Text = "";

                // Home Address
                NFC_Home_AddressTypeCodeText.Text = "";
                NFC_Home_LocationCodeText.Text = "";
                NFC_Home_EmirateCodeText.Text = "";
                NFC_Home_EmirateArabicText.Text = "";
                NFC_Home_EmirateEnglishText.Text = "";
                NFC_Home_CityCodeText.Text = "";
                NFC_Home_CityArabicText.Text = "";
                NFC_Home_CityEnglishText.Text = "";
                NFC_Home_StreetArabicText.Text = "";
                NFC_Home_StreetEnglishText.Text = "";
                NFC_Home_PoBoxText.Text = "";
                NFC_Home_AreaCodeText.Text = "";
                NFC_Home_AreaArabicText.Text = "";
                NFC_Home_AreaEnglishText.Text = "";
                NFC_Home_BuildingNameArabicText.Text = "";
                NFC_Home_BuildingNameEnglishText.Text = "";
                NFC_Home_FlatNumberText.Text = "";
                NFC_Home_LandPhoneNumberText.Text = "";
                NFC_Home_MobilePhoneNumberText.Text = "";
                NFC_Home_EmailText.Text = "";

                // Work Address
                NFC_Work_AddressTypeCodeText.Text = "";
                NFC_Work_LocationCodeText.Text = "";
                NFC_Work_CompanyNameArabicText.Text = "";
                NFC_Work_CompanyNameEnglishText.Text = "";
                NFC_Work_EmirateCodeText.Text = "";
                NFC_Work_EmirateArabicText.Text = "";
                NFC_Work_EmirateEnglishText.Text = "";
                NFC_Work_CityCodeText.Text = "";
                NFC_Work_CityArabicText.Text = "";
                NFC_Work_CityEnglishText.Text = "";
                NFC_Work_StreetArabicText.Text = "";
                NFC_Work_StreetEnglishText.Text = "";
                NFC_Work_PoBoxText.Text = "";
                NFC_Work_AreaCodeText.Text = "";
                NFC_Work_AreaArabicText.Text = "";
                NFC_Work_AreaEnglishText.Text = "";
                NFC_Work_BuildingNameArabicText.Text = "";
                NFC_Work_BuildingNameEnglishText.Text = "";
                NFC_Work_LandPhoneNumberText.Text = "";
                NFC_Work_MobilePhoneNumberText.Text = "";
                NFC_Work_EmailText.Text = "";

                // Photo and Signature
                NFC_PublicDataImage.Source = null;
                NFC_SignatureDataImage.Source = null;

                // Xml Response
                CardDataUsingNfcXmlResponse.Text = "";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
