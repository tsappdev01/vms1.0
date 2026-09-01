using System;
using System.Windows;
using System.Windows.Controls;

using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for FamilyBookDataUserControl.xaml
    /// </summary>
    public partial class FamilyBookDataUserControl : UserControl
    {
        public FamilyBookDataUserControl()
        {
            InitializeComponent();
        }

        public void FillFamilyBookDataTextFields(CardFamilyBookData cardFamilyBookData)
        {
            int wifeCount = 0;
            int childCount = 0;

            try
            {
                if (null != cardFamilyBookData.Wives)
                    wifeCount = cardFamilyBookData.Wives.Count;
                if (null != cardFamilyBookData.Children)
                    childCount = cardFamilyBookData.Children.Count;

                #region Head Of Family Details  

                if (null != cardFamilyBookData.HeadOfFamily)
                {
                    this.HeadOfFamily_HolderIdNumberText.Text = cardFamilyBookData.HeadOfFamily.HolderIdNumber;
                    this.HeadOfFamily_FamilyIdText.Text = cardFamilyBookData.HeadOfFamily.FamilyId;
                    this.HeadOfFamily_EmirateNameArabicText.Text = cardFamilyBookData.HeadOfFamily.EmirateNameArabic;
                    this.HeadOfFamily_EmirateNameEnglishText.Text = cardFamilyBookData.HeadOfFamily.EmirateNameEnglish;
                    this.HeadOfFamily_FirstNameArabicText.Text = cardFamilyBookData.HeadOfFamily.FirstNameArabic;
                    this.HeadOfFamily_FirstNameEnglishText.Text = cardFamilyBookData.HeadOfFamily.FirstNameEnglish;
                    this.HeadOfFamily_FatherNameArabic_Text.Text = cardFamilyBookData.HeadOfFamily.FatherNameArabic;
                    this.HeadOfFamily_FatherNameEnglishText.Text = cardFamilyBookData.HeadOfFamily.FatherNameEnglish;
                    this.HeadOfFamily_GrandFatherNameArabicText.Text = cardFamilyBookData.HeadOfFamily.GrandFatherNameArabic;
                    this.HeadOfFamily_GrandFatherNameEnglishText.Text = cardFamilyBookData.HeadOfFamily.GrandFatherNameEnglish;
                    this.HeadOfFamily_TribeArabicText.Text = cardFamilyBookData.HeadOfFamily.TribeArabic;
                    this.HeadOfFamily_TribeEnglishText.Text = cardFamilyBookData.HeadOfFamily.TribeEnglish;
                    this.HeadOfFamily_ClanArabicText.Text = cardFamilyBookData.HeadOfFamily.ClanArabic;
                    this.HeadOfFamily_ClanEnglishText.Text = cardFamilyBookData.HeadOfFamily.ClanEnglish;
                    this.HeadOfFamily_NationalityCodeText.Text = cardFamilyBookData.HeadOfFamily.NationalityCode;
                    this.HeadOfFamily_NationalityArabicText.Text = cardFamilyBookData.HeadOfFamily.NationalityArabic;
                    this.HeadOfFamily_NationalityEnglishText.Text = cardFamilyBookData.HeadOfFamily.NationalityEnglish;
                    this.HeadOfFamily_GenderText.Text = cardFamilyBookData.HeadOfFamily.Gender;
                    this.HeadOfFamily_DateOfBirthText.Text = cardFamilyBookData.HeadOfFamily.DateOfBirth;
                    this.HeadOfFamily_PlaceOfBirthArabicText.Text = cardFamilyBookData.HeadOfFamily.PlaceOfBirthArabic;
                    this.HeadOfFamily_PlaceOfBirthEnglishText.Text = cardFamilyBookData.HeadOfFamily.PlaceOfBirthEnglish;
                    this.HeadOfFamily_MotherFullNameArabicText.Text = cardFamilyBookData.HeadOfFamily.MotherFullNameArabic;
                    this.HeadOfFamily_MotherFullNameEnglishText.Text = cardFamilyBookData.HeadOfFamily.MotherFullNameEnglish;
                }

                #endregion

                if (wifeCount > 0)
                {
                    #region Wife1 Details

                    if (0 < wifeCount--)
                    {
                        Wife1GroupBox.Visibility = Visibility.Visible;

                        Wife1_WifeIdnText.Text = cardFamilyBookData.Wives[0].WifeIdn;
                        Wife1_FullNameArabicText.Text = cardFamilyBookData.Wives[0].FullNameArabic;
                        Wife1_FullNameEnglishText.Text = cardFamilyBookData.Wives[0].FullNameEnglish;
                        Wife1_NationalityCodeText.Text = cardFamilyBookData.Wives[0].NationalityCode;
                        Wife1_NationalityArabicText.Text = cardFamilyBookData.Wives[0].NationalityArabic;
                        Wife1_NationalityEnglishText.Text = cardFamilyBookData.Wives[0].NationalityEnglish;
                    }
                    #endregion

                    #region Wife2 Details
                    if (0 < wifeCount--)
                    {
                        Wife2GroupBox.Visibility = Visibility.Visible;

                        Wife2_WifeIdnText.Text = cardFamilyBookData.Wives[1].WifeIdn;
                        Wife2_FullNameArabicText.Text = cardFamilyBookData.Wives[1].FullNameArabic;
                        Wife2_FullNameEnglishText.Text = cardFamilyBookData.Wives[1].FullNameEnglish;
                        Wife2_NationalityCodeText.Text = cardFamilyBookData.Wives[1].NationalityCode;
                        Wife2_NationalityArabicText.Text = cardFamilyBookData.Wives[1].NationalityArabic;
                        Wife2_NationalityEnglishText.Text = cardFamilyBookData.Wives[1].NationalityEnglish;
                    }
                    #endregion

                    #region Wife3 Details
                    if (0 < wifeCount--)
                    {
                        Wife3GroupBox.Visibility = Visibility.Visible;

                        Wife3_WifeIdnText.Text = cardFamilyBookData.Wives[3].WifeIdn;
                        Wife3_FullNameArabicText.Text = cardFamilyBookData.Wives[3].FullNameArabic;
                        Wife3_FullNameEnglishText.Text = cardFamilyBookData.Wives[3].FullNameEnglish;
                        Wife3_NationalityCodeText.Text = cardFamilyBookData.Wives[3].NationalityCode;
                        Wife3_NationalityArabicText.Text = cardFamilyBookData.Wives[3].NationalityArabic;
                        Wife3_NationalityEnglishText.Text = cardFamilyBookData.Wives[3].NationalityEnglish;
                    }
                    #endregion

                    #region Wife4 Details

                    if (0 < wifeCount--)
                    {
                        Wife4GroupBox.Visibility = Visibility.Visible;

                        Wife4_WifeIdnText.Text = cardFamilyBookData.Wives[4].WifeIdn;
                        Wife4_FullNameArabicText.Text = cardFamilyBookData.Wives[4].FullNameArabic;
                        Wife4_FullNameEnglishText.Text = cardFamilyBookData.Wives[4].FullNameEnglish;
                        Wife4_NationalityCodeText.Text = cardFamilyBookData.Wives[4].NationalityCode;
                        Wife4_NationalityArabicText.Text = cardFamilyBookData.Wives[4].NationalityArabic;
                        Wife4_NationalityEnglishText.Text = cardFamilyBookData.Wives[4].NationalityEnglish;
                    }
                    #endregion
                }

                if (childCount > 0)
                {
                    #region Child1 Details
                    if (0 < childCount--)
                    {
                        Child1GroupBox.Visibility = Visibility.Visible;

                        Child1_ChildIdnText.Text = cardFamilyBookData.Children[0].ChildIdn;
                        Child1_FirstNameArabicText.Text = cardFamilyBookData.Children[0].FirstNameArabic;
                        Child1_FirstNameEnglishText.Text = cardFamilyBookData.Children[0].FirstNameEnglish;
                        Child1_GenderText.Text = cardFamilyBookData.Children[0].Gender;
                        Child1_DateOfBirthText.Text = cardFamilyBookData.Children[0].DateOfBirth;
                        Child1_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[0].PlaceOfBirthArabic;
                        Child1_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[0].PlaceOfBirthEnglish;
                        Child1_MotherIdnText.Text = cardFamilyBookData.Children[0].MotherIdn;
                        Child1_MotherFullNameArabicText.Text = cardFamilyBookData.Children[0].MotherFullNameArabic;
                        Child1_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[0].MotherFullNameEnglish;
                    }
                    #endregion

                    #region Child2 Details
                    if (0 < childCount--)
                    {
                        Child2GroupBox.Visibility = Visibility.Visible;

                        Child2_ChildIdnText.Text = cardFamilyBookData.Children[1].ChildIdn;
                        Child2_FirstNameArabicText.Text = cardFamilyBookData.Children[1].FirstNameArabic;
                        Child2_FirstNameEnglishText.Text = cardFamilyBookData.Children[1].FirstNameEnglish;
                        Child2_GenderText.Text = cardFamilyBookData.Children[1].Gender;
                        Child2_DateOfBirthText.Text = cardFamilyBookData.Children[1].DateOfBirth;
                        Child2_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[1].PlaceOfBirthArabic;
                        Child2_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[1].PlaceOfBirthEnglish;
                        Child2_MotherIdnText.Text = cardFamilyBookData.Children[1].MotherIdn;
                        Child2_MotherFullNameArabicText.Text = cardFamilyBookData.Children[1].MotherFullNameArabic;
                        Child2_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[1].MotherFullNameEnglish;
                    }
                    #endregion

                    #region Child3 Details

                    if (0 < childCount--)
                    {
                        Child3GroupBox.Visibility = Visibility.Visible;

                        Child3_ChildIdnText.Text = cardFamilyBookData.Children[2].ChildIdn;
                        Child3_FirstNameArabicText.Text = cardFamilyBookData.Children[2].FirstNameArabic;
                        Child3_FirstNameEnglishText.Text = cardFamilyBookData.Children[2].FirstNameEnglish;
                        Child3_GenderText.Text = cardFamilyBookData.Children[2].Gender;
                        Child3_DateOfBirthText.Text = cardFamilyBookData.Children[2].DateOfBirth;
                        Child3_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[2].PlaceOfBirthArabic;
                        Child3_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[2].PlaceOfBirthEnglish;
                        Child3_MotherIdnText.Text = cardFamilyBookData.Children[2].MotherIdn;
                        Child3_MotherFullNameArabicText.Text = cardFamilyBookData.Children[2].MotherFullNameArabic;
                        Child3_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[2].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child4 Details

                    if (0 < childCount--)
                    {
                        Child4GroupBox.Visibility = Visibility.Visible;

                        Child4_ChildIdnText.Text = cardFamilyBookData.Children[3].ChildIdn;
                        Child4_FirstNameArabicText.Text = cardFamilyBookData.Children[3].FirstNameArabic;
                        Child4_FirstNameEnglishText.Text = cardFamilyBookData.Children[3].FirstNameEnglish;
                        Child4_GenderText.Text = cardFamilyBookData.Children[3].Gender;
                        Child4_DateOfBirthText.Text = cardFamilyBookData.Children[3].DateOfBirth;
                        Child4_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[3].PlaceOfBirthArabic;
                        Child4_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[3].PlaceOfBirthEnglish;
                        Child4_MotherIdnText.Text = cardFamilyBookData.Children[3].MotherIdn;
                        Child4_MotherFullNameArabicText.Text = cardFamilyBookData.Children[3].MotherFullNameArabic;
                        Child4_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[3].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child5 Details

                    if (0 < childCount--)
                    {
                        Child5GroupBox.Visibility = Visibility.Visible;

                        Child5_ChildIdnText.Text = cardFamilyBookData.Children[4].ChildIdn;
                        Child5_FirstNameArabicText.Text = cardFamilyBookData.Children[4].FirstNameArabic;
                        Child5_FirstNameEnglishText.Text = cardFamilyBookData.Children[4].FirstNameEnglish;
                        Child5_GenderText.Text = cardFamilyBookData.Children[4].Gender;
                        Child5_DateOfBirthText.Text = cardFamilyBookData.Children[4].DateOfBirth;
                        Child5_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[4].PlaceOfBirthArabic;
                        Child5_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[4].PlaceOfBirthEnglish;
                        Child5_MotherIdnText.Text = cardFamilyBookData.Children[4].MotherIdn;
                        Child5_MotherFullNameArabicText.Text = cardFamilyBookData.Children[4].MotherFullNameArabic;
                        Child5_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[4].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child6 Details

                    if (0 < childCount--)
                    {
                        Child6GroupBox.Visibility = Visibility.Visible;

                        Child6_ChildIdnText.Text = cardFamilyBookData.Children[5].ChildIdn;
                        Child6_FirstNameArabicText.Text = cardFamilyBookData.Children[5].FirstNameArabic;
                        Child6_FirstNameEnglishText.Text = cardFamilyBookData.Children[5].FirstNameEnglish;
                        Child6_GenderText.Text = cardFamilyBookData.Children[5].Gender;
                        Child6_DateOfBirthText.Text = cardFamilyBookData.Children[5].DateOfBirth;
                        Child6_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[5].PlaceOfBirthArabic;
                        Child6_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[5].PlaceOfBirthEnglish;
                        Child6_MotherIdnText.Text = cardFamilyBookData.Children[5].MotherIdn;
                        Child6_MotherFullNameArabicText.Text = cardFamilyBookData.Children[5].MotherFullNameArabic;
                        Child6_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[5].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child7 Details

                    if (0 < childCount--)
                    {
                        Child7GroupBox.Visibility = Visibility.Visible;

                        Child7_ChildIdnText.Text = cardFamilyBookData.Children[6].ChildIdn;
                        Child7_FirstNameArabicText.Text = cardFamilyBookData.Children[6].FirstNameArabic;
                        Child7_FirstNameEnglishText.Text = cardFamilyBookData.Children[6].FirstNameEnglish;
                        Child7_GenderText.Text = cardFamilyBookData.Children[6].Gender;
                        Child7_DateOfBirthText.Text = cardFamilyBookData.Children[6].DateOfBirth;
                        Child7_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[6].PlaceOfBirthArabic;
                        Child7_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[6].PlaceOfBirthEnglish;
                        Child7_MotherIdnText.Text = cardFamilyBookData.Children[6].MotherIdn;
                        Child7_MotherFullNameArabicText.Text = cardFamilyBookData.Children[6].MotherFullNameArabic;
                        Child7_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[6].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child8 Details

                    if (0 < childCount--)
                    {
                        Child8GroupBox.Visibility = Visibility.Visible;

                        Child8_ChildIdnText.Text = cardFamilyBookData.Children[7].ChildIdn;
                        Child8_FirstNameArabicText.Text = cardFamilyBookData.Children[7].FirstNameArabic;
                        Child8_FirstNameEnglishText.Text = cardFamilyBookData.Children[7].FirstNameEnglish;
                        Child8_GenderText.Text = cardFamilyBookData.Children[7].Gender;
                        Child8_DateOfBirthText.Text = cardFamilyBookData.Children[7].DateOfBirth;
                        Child8_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[7].PlaceOfBirthArabic;
                        Child8_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[7].PlaceOfBirthEnglish;
                        Child8_MotherIdnText.Text = cardFamilyBookData.Children[7].MotherIdn;
                        Child8_MotherFullNameArabicText.Text = cardFamilyBookData.Children[7].MotherFullNameArabic;
                        Child8_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[7].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child9 Details

                    if (0 < childCount--)
                    {
                        Child9GroupBox.Visibility = Visibility.Visible;

                        Child9_ChildIdnText.Text = cardFamilyBookData.Children[8].ChildIdn;
                        Child9_FirstNameArabicText.Text = cardFamilyBookData.Children[8].FirstNameArabic;
                        Child9_FirstNameEnglishText.Text = cardFamilyBookData.Children[8].FirstNameEnglish;
                        Child9_GenderText.Text = cardFamilyBookData.Children[8].Gender;
                        Child9_DateOfBirthText.Text = cardFamilyBookData.Children[8].DateOfBirth;
                        Child9_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[8].PlaceOfBirthArabic;
                        Child9_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[8].PlaceOfBirthEnglish;
                        Child9_MotherIdnText.Text = cardFamilyBookData.Children[8].MotherIdn;
                        Child9_MotherFullNameArabicText.Text = cardFamilyBookData.Children[8].MotherFullNameArabic;
                        Child9_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[8].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child10 Details

                    if (0 < childCount--)
                    {
                        Child10GroupBox.Visibility = Visibility.Visible;

                        Child10_ChildIdnText.Text = cardFamilyBookData.Children[9].ChildIdn;
                        Child10_FirstNameArabicText.Text = cardFamilyBookData.Children[9].FirstNameArabic;
                        Child10_FirstNameEnglishText.Text = cardFamilyBookData.Children[9].FirstNameEnglish;
                        Child10_GenderText.Text = cardFamilyBookData.Children[9].Gender;
                        Child10_DateOfBirthText.Text = cardFamilyBookData.Children[9].DateOfBirth;
                        Child10_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[9].PlaceOfBirthArabic;
                        Child10_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[9].PlaceOfBirthEnglish;
                        Child10_MotherIdnText.Text = cardFamilyBookData.Children[9].MotherIdn;
                        Child10_MotherFullNameArabicText.Text = cardFamilyBookData.Children[9].MotherFullNameArabic;
                        Child10_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[9].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child11 Details

                    if (0 < childCount--)
                    {
                        Child11GroupBox.Visibility = Visibility.Visible;

                        Child11_ChildIdnText.Text = cardFamilyBookData.Children[10].ChildIdn;
                        Child11_FirstNameArabicText.Text = cardFamilyBookData.Children[10].FirstNameArabic;
                        Child11_FirstNameEnglishText.Text = cardFamilyBookData.Children[10].FirstNameEnglish;
                        Child11_GenderText.Text = cardFamilyBookData.Children[10].Gender;
                        Child11_DateOfBirthText.Text = cardFamilyBookData.Children[10].DateOfBirth;
                        Child11_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[10].PlaceOfBirthArabic;
                        Child11_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[10].PlaceOfBirthEnglish;
                        Child11_MotherIdnText.Text = cardFamilyBookData.Children[10].MotherIdn;
                        Child11_MotherFullNameArabicText.Text = cardFamilyBookData.Children[10].MotherFullNameArabic;
                        Child11_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[10].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child12 Details

                    if (0 < childCount--)
                    {
                        Child12GroupBox.Visibility = Visibility.Visible;

                        Child12_ChildIdnText.Text = cardFamilyBookData.Children[11].ChildIdn;
                        Child12_FirstNameArabicText.Text = cardFamilyBookData.Children[11].FirstNameArabic;
                        Child12_FirstNameEnglishText.Text = cardFamilyBookData.Children[11].FirstNameEnglish;
                        Child12_GenderText.Text = cardFamilyBookData.Children[11].Gender;
                        Child12_DateOfBirthText.Text = cardFamilyBookData.Children[11].DateOfBirth;
                        Child12_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[11].PlaceOfBirthArabic;
                        Child12_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[11].PlaceOfBirthEnglish;
                        Child12_MotherIdnText.Text = cardFamilyBookData.Children[11].MotherIdn;
                        Child12_MotherFullNameArabicText.Text = cardFamilyBookData.Children[11].MotherFullNameArabic;
                        Child12_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[11].MotherFullNameEnglish;
                    }
                    #endregion

                    #region Child13 Details

                    if (0 < childCount--)
                    {
                        Child13GroupBox.Visibility = Visibility.Visible;

                        Child13_ChildIdnText.Text = cardFamilyBookData.Children[12].ChildIdn;
                        Child13_FirstNameArabicText.Text = cardFamilyBookData.Children[12].FirstNameArabic;
                        Child13_FirstNameEnglishText.Text = cardFamilyBookData.Children[12].FirstNameEnglish;
                        Child13_GenderText.Text = cardFamilyBookData.Children[12].Gender;
                        Child13_DateOfBirthText.Text = cardFamilyBookData.Children[12].DateOfBirth;
                        Child13_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[12].PlaceOfBirthArabic;
                        Child13_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[12].PlaceOfBirthEnglish;
                        Child13_MotherIdnText.Text = cardFamilyBookData.Children[12].MotherIdn;
                        Child13_MotherFullNameArabicText.Text = cardFamilyBookData.Children[12].MotherFullNameArabic;
                        Child13_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[12].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child14 Details

                    if (0 < childCount--)
                    {
                        Child14GroupBox.Visibility = Visibility.Visible;

                        Child14_ChildIdnText.Text = cardFamilyBookData.Children[13].ChildIdn;
                        Child14_FirstNameArabicText.Text = cardFamilyBookData.Children[13].FirstNameArabic;
                        Child14_FirstNameEnglishText.Text = cardFamilyBookData.Children[13].FirstNameEnglish;
                        Child14_GenderText.Text = cardFamilyBookData.Children[13].Gender;
                        Child14_DateOfBirthText.Text = cardFamilyBookData.Children[13].DateOfBirth;
                        Child14_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[13].PlaceOfBirthArabic;
                        Child14_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[13].PlaceOfBirthEnglish;
                        Child14_MotherIdnText.Text = cardFamilyBookData.Children[13].MotherIdn;
                        Child14_MotherFullNameArabicText.Text = cardFamilyBookData.Children[13].MotherFullNameArabic;
                        Child14_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[13].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child15 Details

                    if (0 < childCount--)
                    {
                        Child15GroupBox.Visibility = Visibility.Visible;

                        Child15_ChildIdnText.Text = cardFamilyBookData.Children[14].ChildIdn;
                        Child15_FirstNameArabicText.Text = cardFamilyBookData.Children[14].FirstNameArabic;
                        Child15_FirstNameEnglishText.Text = cardFamilyBookData.Children[14].FirstNameEnglish;
                        Child15_GenderText.Text = cardFamilyBookData.Children[14].Gender;
                        Child15_DateOfBirthText.Text = cardFamilyBookData.Children[14].DateOfBirth;
                        Child15_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[14].PlaceOfBirthArabic;
                        Child15_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[14].PlaceOfBirthEnglish;
                        Child15_MotherIdnText.Text = cardFamilyBookData.Children[14].MotherIdn;
                        Child15_MotherFullNameArabicText.Text = cardFamilyBookData.Children[14].MotherFullNameArabic;
                        Child15_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[14].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child16 Details

                    if (0 < childCount--)
                    {
                        Child16GroupBox.Visibility = Visibility.Visible;

                        Child16_ChildIdnText.Text = cardFamilyBookData.Children[15].ChildIdn;
                        Child16_FirstNameArabicText.Text = cardFamilyBookData.Children[15].FirstNameArabic;
                        Child16_FirstNameEnglishText.Text = cardFamilyBookData.Children[15].FirstNameEnglish;
                        Child16_GenderText.Text = cardFamilyBookData.Children[15].Gender;
                        Child16_DateOfBirthText.Text = cardFamilyBookData.Children[15].DateOfBirth;
                        Child16_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[15].PlaceOfBirthArabic;
                        Child16_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[15].PlaceOfBirthEnglish;
                        Child16_MotherIdnText.Text = cardFamilyBookData.Children[15].MotherIdn;
                        Child16_MotherFullNameArabicText.Text = cardFamilyBookData.Children[15].MotherFullNameArabic;
                        Child16_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[15].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child17 Details

                    if (0 < childCount--)
                    {
                        Child17GroupBox.Visibility = Visibility.Visible;

                        Child17_ChildIdnText.Text = cardFamilyBookData.Children[16].ChildIdn;
                        Child17_FirstNameArabicText.Text = cardFamilyBookData.Children[16].FirstNameArabic;
                        Child17_FirstNameEnglishText.Text = cardFamilyBookData.Children[16].FirstNameEnglish;
                        Child17_GenderText.Text = cardFamilyBookData.Children[16].Gender;
                        Child17_DateOfBirthText.Text = cardFamilyBookData.Children[16].DateOfBirth;
                        Child17_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[16].PlaceOfBirthArabic;
                        Child17_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[16].PlaceOfBirthEnglish;
                        Child17_MotherIdnText.Text = cardFamilyBookData.Children[16].MotherIdn;
                        Child17_MotherFullNameArabicText.Text = cardFamilyBookData.Children[16].MotherFullNameArabic;
                        Child17_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[16].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child18 Details

                    if (0 < childCount--)
                    {
                        Child18GroupBox.Visibility = Visibility.Visible;

                        Child18_ChildIdnText.Text = cardFamilyBookData.Children[17].ChildIdn;
                        Child18_FirstNameArabicText.Text = cardFamilyBookData.Children[17].FirstNameArabic;
                        Child18_FirstNameEnglishText.Text = cardFamilyBookData.Children[17].FirstNameEnglish;
                        Child18_GenderText.Text = cardFamilyBookData.Children[17].Gender;
                        Child18_DateOfBirthText.Text = cardFamilyBookData.Children[17].DateOfBirth;
                        Child18_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[17].PlaceOfBirthArabic;
                        Child18_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[17].PlaceOfBirthEnglish;
                        Child18_MotherIdnText.Text = cardFamilyBookData.Children[17].MotherIdn;
                        Child18_MotherFullNameArabicText.Text = cardFamilyBookData.Children[17].MotherFullNameArabic;
                        Child18_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[17].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child19 Details

                    if (0 < childCount--)
                    {
                        Child19GroupBox.Visibility = Visibility.Visible;

                        Child19_ChildIdnText.Text = cardFamilyBookData.Children[18].ChildIdn;
                        Child19_FirstNameArabicText.Text = cardFamilyBookData.Children[18].FirstNameArabic;
                        Child19_FirstNameEnglishText.Text = cardFamilyBookData.Children[18].FirstNameEnglish;
                        Child19_GenderText.Text = cardFamilyBookData.Children[18].Gender;
                        Child19_DateOfBirthText.Text = cardFamilyBookData.Children[18].DateOfBirth;
                        Child19_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[18].PlaceOfBirthArabic;
                        Child19_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[18].PlaceOfBirthEnglish;
                        Child19_MotherIdnText.Text = cardFamilyBookData.Children[18].MotherIdn;
                        Child19_MotherFullNameArabicText.Text = cardFamilyBookData.Children[18].MotherFullNameArabic;
                        Child19_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[18].MotherFullNameEnglish;
                    }

                    #endregion

                    #region Child20 Details

                    if (0 < childCount--)
                    {
                        Child20GroupBox.Visibility = Visibility.Visible;

                        Child20_ChildIdnText.Text = cardFamilyBookData.Children[19].ChildIdn;
                        Child20_FirstNameArabicText.Text = cardFamilyBookData.Children[19].FirstNameArabic;
                        Child20_FirstNameEnglishText.Text = cardFamilyBookData.Children[19].FirstNameEnglish;
                        Child20_GenderText.Text = cardFamilyBookData.Children[19].Gender;
                        Child20_DateOfBirthText.Text = cardFamilyBookData.Children[19].DateOfBirth;
                        Child20_PlaceOfBirthArabicText.Text = cardFamilyBookData.Children[19].PlaceOfBirthArabic;
                        Child20_PlaceOfBirthEnglishText.Text = cardFamilyBookData.Children[19].PlaceOfBirthEnglish;
                        Child20_MotherIdnText.Text = cardFamilyBookData.Children[19].MotherIdn;
                        Child20_MotherFullNameArabicText.Text = cardFamilyBookData.Children[19].MotherFullNameArabic;
                        Child20_MotherFullNameEnglishText.Text = cardFamilyBookData.Children[19].MotherFullNameEnglish;
                    }

                    #endregion
                }

                FamilyBookXmlResponse.Text = cardFamilyBookData.XmlString;
                ScrollViewerFamilyBookData.ScrollToTop();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ClearFamilyBookDataTextFields()
        {
            #region HeadOfFamily

            HeadOfFamily_HolderIdNumberText.Text = "";
            HeadOfFamily_FamilyIdText.Text = "";
            HeadOfFamily_EmirateNameArabicText.Text = "";
            HeadOfFamily_EmirateNameEnglishText.Text = "";
            HeadOfFamily_FirstNameArabicText.Text = "";
            HeadOfFamily_FirstNameEnglishText.Text = "";
            HeadOfFamily_FatherNameEnglishText.Text = "";
            HeadOfFamily_FatherNameArabic_Text.Text = "";
            HeadOfFamily_GrandFatherNameArabicText.Text = "";
            HeadOfFamily_GrandFatherNameEnglishText.Text = "";
            HeadOfFamily_TribeArabicText.Text = "";
            HeadOfFamily_TribeEnglishText.Text = "";
            HeadOfFamily_ClanArabicText.Text = "";
            HeadOfFamily_ClanEnglishText.Text = "";
            HeadOfFamily_NationalityCodeText.Text = "";
            HeadOfFamily_NationalityArabicText.Text = "";
            HeadOfFamily_NationalityEnglishText.Text = "";
            HeadOfFamily_GenderText.Text = "";
            HeadOfFamily_DateOfBirthText.Text = "";
            HeadOfFamily_PlaceOfBirthArabicText.Text = "";
            HeadOfFamily_PlaceOfBirthEnglishText.Text = "";
            HeadOfFamily_MotherFullNameArabicText.Text = "";
            HeadOfFamily_MotherFullNameEnglishText.Text = "";

            #endregion

            #region Wife1

            Wife1_WifeIdnText.Text = "";
            Wife1_FullNameArabicText.Text = "";
            Wife1_FullNameEnglishText.Text = "";
            Wife1_NationalityCodeText.Text = "";
            Wife1_NationalityArabicText.Text = "";
            Wife1_NationalityEnglishText.Text = "";
            Wife1GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Wife2

            Wife2_WifeIdnText.Text = "";
            Wife2_FullNameArabicText.Text = "";
            Wife2_FullNameEnglishText.Text = "";
            Wife2_NationalityCodeText.Text = "";
            Wife2_NationalityArabicText.Text = "";
            Wife2_NationalityEnglishText.Text = "";
            Wife2GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Wife3

            Wife3_WifeIdnText.Text = "";
            Wife3_FullNameArabicText.Text = "";
            Wife3_FullNameEnglishText.Text = "";
            Wife3_NationalityCodeText.Text = "";
            Wife3_NationalityArabicText.Text = "";
            Wife3_NationalityEnglishText.Text = "";
            Wife3GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Wife4

            Wife4_WifeIdnText.Text = "";
            Wife4_FullNameArabicText.Text = "";
            Wife4_FullNameEnglishText.Text = "";
            Wife4_NationalityCodeText.Text = "";
            Wife4_NationalityArabicText.Text = "";
            Wife4_NationalityEnglishText.Text = "";
            Wife4GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child1

            Child1_ChildIdnText.Text = "";
            Child1_FirstNameArabicText.Text = "";
            Child1_FirstNameEnglishText.Text = "";
            Child1_GenderText.Text = "";
            Child1_DateOfBirthText.Text = "";
            Child1_PlaceOfBirthArabicText.Text = "";
            Child1_PlaceOfBirthEnglishText.Text = "";
            Child1_MotherIdnText.Text = "";
            Child1_MotherFullNameArabicText.Text = "";
            Child1_MotherFullNameEnglishText.Text = "";
            Child1GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child2

            Child2_ChildIdnText.Text = "";
            Child2_FirstNameArabicText.Text = "";
            Child2_FirstNameEnglishText.Text = "";
            Child2_GenderText.Text = "";
            Child2_DateOfBirthText.Text = "";
            Child2_PlaceOfBirthArabicText.Text = "";
            Child2_PlaceOfBirthEnglishText.Text = "";
            Child2_MotherIdnText.Text = "";
            Child2_MotherFullNameArabicText.Text = "";
            Child2_MotherFullNameEnglishText.Text = "";
            Child2GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child3

            Child3_ChildIdnText.Text = "";
            Child3_FirstNameArabicText.Text = "";
            Child3_FirstNameEnglishText.Text = "";
            Child3_GenderText.Text = "";
            Child3_DateOfBirthText.Text = "";
            Child3_PlaceOfBirthArabicText.Text = "";
            Child3_PlaceOfBirthEnglishText.Text = "";
            Child3_MotherIdnText.Text = "";
            Child3_MotherFullNameArabicText.Text = "";
            Child3_MotherFullNameEnglishText.Text = "";
            Child3GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child4

            Child4_ChildIdnText.Text = "";
            Child4_FirstNameArabicText.Text = "";
            Child4_FirstNameEnglishText.Text = "";
            Child4_GenderText.Text = "";
            Child4_DateOfBirthText.Text = "";
            Child4_PlaceOfBirthArabicText.Text = "";
            Child4_PlaceOfBirthEnglishText.Text = "";
            Child4_MotherIdnText.Text = "";
            Child4_MotherFullNameArabicText.Text = "";
            Child4_MotherFullNameEnglishText.Text = "";
            Child4GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child5

            Child5_ChildIdnText.Text = "";
            Child5_FirstNameArabicText.Text = "";
            Child5_FirstNameEnglishText.Text = "";
            Child5_GenderText.Text = "";
            Child5_DateOfBirthText.Text = "";
            Child5_PlaceOfBirthArabicText.Text = "";
            Child5_PlaceOfBirthEnglishText.Text = "";
            Child5_MotherIdnText.Text = "";
            Child5_MotherFullNameArabicText.Text = "";
            Child5_MotherFullNameEnglishText.Text = "";
            Child5GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child6

            Child6_ChildIdnText.Text = "";
            Child6_FirstNameArabicText.Text = "";
            Child6_FirstNameEnglishText.Text = "";
            Child6_GenderText.Text = "";
            Child6_DateOfBirthText.Text = "";
            Child6_PlaceOfBirthArabicText.Text = "";
            Child6_PlaceOfBirthEnglishText.Text = "";
            Child6_MotherIdnText.Text = "";
            Child6_MotherFullNameArabicText.Text = "";
            Child6_MotherFullNameEnglishText.Text = "";
            Child6GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child7

            Child7_ChildIdnText.Text = "";
            Child7_FirstNameArabicText.Text = "";
            Child7_FirstNameEnglishText.Text = "";
            Child7_GenderText.Text = "";
            Child7_DateOfBirthText.Text = "";
            Child7_PlaceOfBirthArabicText.Text = "";
            Child7_PlaceOfBirthEnglishText.Text = "";
            Child7_MotherIdnText.Text = "";
            Child7_MotherFullNameArabicText.Text = "";
            Child7_MotherFullNameEnglishText.Text = "";
            Child7GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child8

            Child8_ChildIdnText.Text = "";
            Child8_FirstNameArabicText.Text = "";
            Child8_FirstNameEnglishText.Text = "";
            Child8_GenderText.Text = "";
            Child8_DateOfBirthText.Text = "";
            Child8_PlaceOfBirthArabicText.Text = "";
            Child8_PlaceOfBirthEnglishText.Text = "";
            Child8_MotherIdnText.Text = "";
            Child8_MotherFullNameArabicText.Text = "";
            Child8_MotherFullNameEnglishText.Text = "";
            Child8GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child9

            Child9_ChildIdnText.Text = "";
            Child9_FirstNameArabicText.Text = "";
            Child9_FirstNameEnglishText.Text = "";
            Child9_GenderText.Text = "";
            Child9_DateOfBirthText.Text = "";
            Child9_PlaceOfBirthArabicText.Text = "";
            Child9_PlaceOfBirthEnglishText.Text = "";
            Child9_MotherIdnText.Text = "";
            Child9_MotherFullNameArabicText.Text = "";
            Child9_MotherFullNameEnglishText.Text = "";
            Child9GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child10

            Child10_ChildIdnText.Text = "";
            Child10_FirstNameArabicText.Text = "";
            Child10_FirstNameEnglishText.Text = "";
            Child10_GenderText.Text = "";
            Child10_DateOfBirthText.Text = "";
            Child10_PlaceOfBirthArabicText.Text = "";
            Child10_PlaceOfBirthEnglishText.Text = "";
            Child10_MotherIdnText.Text = "";
            Child10_MotherFullNameArabicText.Text = "";
            Child10_MotherFullNameEnglishText.Text = "";
            Child10GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child11

            Child11_ChildIdnText.Text = "";
            Child11_FirstNameArabicText.Text = "";
            Child11_FirstNameEnglishText.Text = "";
            Child11_GenderText.Text = "";
            Child11_DateOfBirthText.Text = "";
            Child11_PlaceOfBirthArabicText.Text = "";
            Child11_PlaceOfBirthEnglishText.Text = "";
            Child11_MotherIdnText.Text = "";
            Child11_MotherFullNameArabicText.Text = "";
            Child11_MotherFullNameEnglishText.Text = "";
            Child11GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child12

            Child12_ChildIdnText.Text = "";
            Child12_FirstNameArabicText.Text = "";
            Child12_FirstNameEnglishText.Text = "";
            Child12_GenderText.Text = "";
            Child12_DateOfBirthText.Text = "";
            Child12_PlaceOfBirthArabicText.Text = "";
            Child12_PlaceOfBirthEnglishText.Text = "";
            Child12_MotherIdnText.Text = "";
            Child12_MotherFullNameArabicText.Text = "";
            Child12_MotherFullNameEnglishText.Text = "";
            Child12GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child13

            Child13_ChildIdnText.Text = "";
            Child13_FirstNameArabicText.Text = "";
            Child13_FirstNameEnglishText.Text = "";
            Child13_GenderText.Text = "";
            Child13_DateOfBirthText.Text = "";
            Child13_PlaceOfBirthArabicText.Text = "";
            Child13_PlaceOfBirthEnglishText.Text = "";
            Child13_MotherIdnText.Text = "";
            Child13_MotherFullNameArabicText.Text = "";
            Child13_MotherFullNameEnglishText.Text = "";
            Child13GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child14

            Child14_ChildIdnText.Text = "";
            Child14_FirstNameArabicText.Text = "";
            Child14_FirstNameEnglishText.Text = "";
            Child14_GenderText.Text = "";
            Child14_DateOfBirthText.Text = "";
            Child14_PlaceOfBirthArabicText.Text = "";
            Child14_PlaceOfBirthEnglishText.Text = "";
            Child14_MotherIdnText.Text = "";
            Child14_MotherFullNameArabicText.Text = "";
            Child14_MotherFullNameEnglishText.Text = "";
            Child14GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child15

            Child15_ChildIdnText.Text = "";
            Child15_FirstNameArabicText.Text = "";
            Child15_FirstNameEnglishText.Text = "";
            Child15_GenderText.Text = "";
            Child15_DateOfBirthText.Text = "";
            Child15_PlaceOfBirthArabicText.Text = "";
            Child15_PlaceOfBirthEnglishText.Text = "";
            Child15_MotherIdnText.Text = "";
            Child15_MotherFullNameArabicText.Text = "";
            Child15_MotherFullNameEnglishText.Text = "";
            Child15GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child16

            Child16_ChildIdnText.Text = "";
            Child16_FirstNameArabicText.Text = "";
            Child16_FirstNameEnglishText.Text = "";
            Child16_GenderText.Text = "";
            Child16_DateOfBirthText.Text = "";
            Child16_PlaceOfBirthArabicText.Text = "";
            Child16_PlaceOfBirthEnglishText.Text = "";
            Child16_MotherIdnText.Text = "";
            Child16_MotherFullNameArabicText.Text = "";
            Child16_MotherFullNameEnglishText.Text = "";
            Child16GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child17

            Child17_ChildIdnText.Text = "";
            Child17_FirstNameArabicText.Text = "";
            Child17_FirstNameEnglishText.Text = "";
            Child17_GenderText.Text = "";
            Child17_DateOfBirthText.Text = "";
            Child17_PlaceOfBirthArabicText.Text = "";
            Child17_PlaceOfBirthEnglishText.Text = "";
            Child17_MotherIdnText.Text = "";
            Child17_MotherFullNameArabicText.Text = "";
            Child17_MotherFullNameEnglishText.Text = "";
            Child17GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child18

            Child18_ChildIdnText.Text = "";
            Child18_FirstNameArabicText.Text = "";
            Child18_FirstNameEnglishText.Text = "";
            Child18_GenderText.Text = "";
            Child18_DateOfBirthText.Text = "";
            Child18_PlaceOfBirthArabicText.Text = "";
            Child18_PlaceOfBirthEnglishText.Text = "";
            Child18_MotherIdnText.Text = "";
            Child18_MotherFullNameArabicText.Text = "";
            Child18_MotherFullNameEnglishText.Text = "";
            Child18GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child19

            Child19_ChildIdnText.Text = "";
            Child19_FirstNameArabicText.Text = "";
            Child19_FirstNameEnglishText.Text = "";
            Child19_GenderText.Text = "";
            Child19_DateOfBirthText.Text = "";
            Child19_PlaceOfBirthArabicText.Text = "";
            Child19_PlaceOfBirthEnglishText.Text = "";
            Child19_MotherIdnText.Text = "";
            Child19_MotherFullNameArabicText.Text = "";
            Child19_MotherFullNameEnglishText.Text = "";
            Child19GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            #region Child20

            Child20_ChildIdnText.Text = "";
            Child20_FirstNameArabicText.Text = "";
            Child20_FirstNameEnglishText.Text = "";
            Child20_GenderText.Text = "";
            Child20_DateOfBirthText.Text = "";
            Child20_PlaceOfBirthArabicText.Text = "";
            Child20_PlaceOfBirthEnglishText.Text = "";
            Child20_MotherIdnText.Text = "";
            Child20_MotherFullNameArabicText.Text = "";
            Child20_MotherFullNameEnglishText.Text = "";
            Child20GroupBox.Visibility = Visibility.Collapsed;

            #endregion

            FamilyBookXmlResponse.Text = "";
            ScrollViewerFamilyBookData.ScrollToTop();
        }
    }
}
