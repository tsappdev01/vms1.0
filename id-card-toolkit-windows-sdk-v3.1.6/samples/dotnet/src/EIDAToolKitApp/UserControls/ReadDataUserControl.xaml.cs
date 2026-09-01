using System;
using System.Collections.Generic;
using System.Windows.Controls;

using AE.EmiratesId.IdCard.DataModels;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for ReadDataUserControl.xaml
    /// </summary>
    public partial class ReadDataUserControl : UserControl
    {
        public ReadDataUserControl()
        {
            InitializeComponent();
        }

        public void FillDataContainerTextFields(DataContainer dataContainerObj)
        {
            try
            {
                if (dataContainerObj.HealthDataContainer != null)
                {
                    int allergyTypeCount = 0;
                    int diagnosisTypeCount = 0;

                    HealthDataContainer healthDataContainerObj = dataContainerObj.HealthDataContainer;

                    #region AllergyResourceType

                    if (healthDataContainerObj.AllergyResources != null)
                    {
                        allergyTypeCount = healthDataContainerObj.AllergyResources.Count;

                        List<AllergyResource> allergyResources = new List<AllergyResource>(healthDataContainerObj.AllergyResources);
                        if (allergyTypeCount-- > 0)
                        {
                            AllergyType1_AllergyDisplay_Text.Text = allergyResources[0].AllergyDisplay;
                            AllergyType1_AllergyRecordedDate_Text.Text = allergyResources[0].AllergyRecordedDate;
                            AllergyType1GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (allergyTypeCount-- > 0)
                        {
                            AllergyType2_AllergyDisplay_Text.Text = allergyResources[1].AllergyDisplay;
                            AllergyType2_AllergyRecordedDate_Text.Text = allergyResources[1].AllergyRecordedDate;
                            AllergyType2GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (allergyTypeCount-- > 0)
                        {
                            AllergyType3_AllergyDisplay_Text.Text = allergyResources[2].AllergyDisplay;
                            AllergyType3_AllergyRecordedDate_Text.Text = allergyResources[2].AllergyRecordedDate;
                            AllergyType3GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (allergyTypeCount-- > 0)
                        {
                            AllergyType4_AllergyDisplay_Text.Text = allergyResources[3].AllergyDisplay;
                            AllergyType4_AllergyRecordedDate_Text.Text = allergyResources[3].AllergyRecordedDate;
                            AllergyType4GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (allergyTypeCount-- > 0)
                        {
                            AllergyType5_AllergyDisplay_Text.Text = allergyResources[4].AllergyDisplay;
                            AllergyType5_AllergyRecordedDate_Text.Text = allergyResources[4].AllergyRecordedDate;
                            AllergyType5GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (allergyTypeCount-- > 0)
                        {
                            AllergyType6_AllergyDisplay_Text.Text = allergyResources[5].AllergyDisplay;
                            AllergyType6_AllergyRecordedDate_Text.Text = allergyResources[5].AllergyRecordedDate;
                            AllergyType6GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }
                    }

                    #endregion

                    #region DiagnosisResourceType

                    if (healthDataContainerObj.DiagnosisResources != null)
                    {
                        diagnosisTypeCount = healthDataContainerObj.DiagnosisResources.Count;

                        List<DiagnosisResource> diagnosisResourcesObj = new List<DiagnosisResource>(healthDataContainerObj.DiagnosisResources);
                        if (diagnosisTypeCount-- > 0)
                        {
                            DiagnosisType1_DiagnosisCode_Text.Text = diagnosisResourcesObj[0].DiagnosisCode;
                            DiagnosisType1_DiagnosisDescription_Text.Text = diagnosisResourcesObj[0].DiagnosisDescription;
                            DiagnosisType1_DiagnosisRecordedDate_Text.Text = diagnosisResourcesObj[0].DiagnosisRecordedDate;
                            DiagnosisType1GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (diagnosisTypeCount-- > 0)
                        {
                            DiagnosisType2_DiagnosisCode_Text.Text = diagnosisResourcesObj[1].DiagnosisCode;
                            DiagnosisType2_DiagnosisDescription_Text.Text = diagnosisResourcesObj[1].DiagnosisDescription;
                            DiagnosisType2_DiagnosisRecordedDate_Text.Text = diagnosisResourcesObj[1].DiagnosisRecordedDate;
                            DiagnosisType2GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (diagnosisTypeCount-- > 0)
                        {
                            DiagnosisType3_DiagnosisCode_Text.Text = diagnosisResourcesObj[2].DiagnosisCode;
                            DiagnosisType3_DiagnosisDescription_Text.Text = diagnosisResourcesObj[2].DiagnosisDescription;
                            DiagnosisType3_DiagnosisRecordedDate_Text.Text = diagnosisResourcesObj[2].DiagnosisRecordedDate;
                            DiagnosisType3GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (diagnosisTypeCount-- > 0)
                        {
                            DiagnosisType4_DiagnosisCode_Text.Text = diagnosisResourcesObj[3].DiagnosisCode;
                            DiagnosisType4_DiagnosisDescription_Text.Text = diagnosisResourcesObj[3].DiagnosisDescription;
                            DiagnosisType4_DiagnosisRecordedDate_Text.Text = diagnosisResourcesObj[3].DiagnosisRecordedDate;
                            DiagnosisType4GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (diagnosisTypeCount-- > 0)
                        {
                            DiagnosisType5_DiagnosisCode_Text.Text = diagnosisResourcesObj[4].DiagnosisCode;
                            DiagnosisType5_DiagnosisDescription_Text.Text = diagnosisResourcesObj[4].DiagnosisDescription;
                            DiagnosisType5_DiagnosisRecordedDate_Text.Text = diagnosisResourcesObj[4].DiagnosisRecordedDate;
                            DiagnosisType5GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (diagnosisTypeCount-- > 0)
                        {
                            DiagnosisType6_DiagnosisCode_Text.Text = diagnosisResourcesObj[5].DiagnosisCode;
                            DiagnosisType6_DiagnosisDescription_Text.Text = diagnosisResourcesObj[5].DiagnosisDescription;
                            DiagnosisType6_DiagnosisRecordedDate_Text.Text = diagnosisResourcesObj[5].DiagnosisRecordedDate;
                            DiagnosisType6GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }

                        if (diagnosisTypeCount-- > 0)
                        {
                            DiagnosisType7_DiagnosisCode_Text.Text = diagnosisResourcesObj[6].DiagnosisCode;
                            DiagnosisType7_DiagnosisDescription_Text.Text = diagnosisResourcesObj[6].DiagnosisDescription;
                            DiagnosisType7_DiagnosisRecordedDate_Text.Text = diagnosisResourcesObj[6].DiagnosisRecordedDate;
                            DiagnosisType7GroupBox.Visibility = System.Windows.Visibility.Visible;
                        }
                    }

                    #endregion

                    #region BloodGroup

                    if (healthDataContainerObj.BloodGroupResource != null)
                    {
                        BloodGroupResource bloodGroupResourceObj = healthDataContainerObj.BloodGroupResource;
                        BloodGroupType_BloodGroup_Text.Text = bloodGroupResourceObj.BloodGroup;
                        BloodGroupType_RecordedDate_Text.Text = bloodGroupResourceObj.RecordedDate;
                        BloodGroupTypeGroupBox.Visibility = System.Windows.Visibility.Visible;
                    }

                    #endregion

                    #region Insurance

                    if (healthDataContainerObj.InsuranceResource != null)
                    {
                        InsuranceResource insuranceResourceObj = healthDataContainerObj.InsuranceResource;
                        InsuranceType_InsuranceName_Text.Text = insuranceResourceObj.InsuranceName;
                        InsuranceType_InsuranceNumber_Text.Text = insuranceResourceObj.InsuranceNumber;
                        InsuranceType_InsuranceValidityStartDate_Text.Text = insuranceResourceObj.InsuranceValidityStartDate;
                        InsuranceType_InsuranceValidityEndDate_Text.Text = insuranceResourceObj.InsuranceValidityEndDate;
                        InsuranceTypeGroupBox.Visibility = System.Windows.Visibility.Visible;
                    }

                    #endregion

                    if (healthDataContainerObj.OrganDonar != null)
                        OrganDonor_Text.Text = healthDataContainerObj.OrganDonar;
                }

                ReadDataXmlResponse.Text = dataContainerObj.XmlString;
                ScrollViewerReadData.ScrollToTop();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ClearDataContainerTextFields()
        {
            AllergyType1_AllergyDisplay_Text.Text = "";
            AllergyType1_AllergyRecordedDate_Text.Text = "";
            AllergyType1GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            AllergyType2_AllergyDisplay_Text.Text = "";
            AllergyType2_AllergyRecordedDate_Text.Text = "";
            AllergyType2GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            AllergyType3_AllergyDisplay_Text.Text = "";
            AllergyType3_AllergyRecordedDate_Text.Text = "";
            AllergyType3GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            AllergyType4_AllergyDisplay_Text.Text = "";
            AllergyType4_AllergyRecordedDate_Text.Text = "";
            AllergyType4GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            AllergyType5_AllergyDisplay_Text.Text = "";
            AllergyType5_AllergyRecordedDate_Text.Text = "";
            AllergyType5GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            AllergyType6_AllergyDisplay_Text.Text = "";
            AllergyType6_AllergyRecordedDate_Text.Text = "";
            AllergyType6GroupBox.Visibility = System.Windows.Visibility.Collapsed;

            DiagnosisType1_DiagnosisCode_Text.Text = "";
            DiagnosisType1_DiagnosisDescription_Text.Text = "";
            DiagnosisType1_DiagnosisRecordedDate_Text.Text = "";
            DiagnosisType1GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            DiagnosisType2_DiagnosisCode_Text.Text = "";
            DiagnosisType2_DiagnosisDescription_Text.Text = "";
            DiagnosisType2_DiagnosisRecordedDate_Text.Text = "";
            DiagnosisType2GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            DiagnosisType3_DiagnosisCode_Text.Text = "";
            DiagnosisType3_DiagnosisDescription_Text.Text = "";
            DiagnosisType3_DiagnosisRecordedDate_Text.Text = "";
            DiagnosisType3GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            DiagnosisType4_DiagnosisCode_Text.Text = "";
            DiagnosisType4_DiagnosisDescription_Text.Text = "";
            DiagnosisType4_DiagnosisRecordedDate_Text.Text = "";
            DiagnosisType4GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            DiagnosisType5_DiagnosisCode_Text.Text = "";
            DiagnosisType5_DiagnosisDescription_Text.Text = "";
            DiagnosisType5_DiagnosisRecordedDate_Text.Text = "";
            DiagnosisType5GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            DiagnosisType6_DiagnosisCode_Text.Text = "";
            DiagnosisType6_DiagnosisDescription_Text.Text = "";
            DiagnosisType6_DiagnosisRecordedDate_Text.Text = "";
            DiagnosisType6GroupBox.Visibility = System.Windows.Visibility.Collapsed;
            DiagnosisType7_DiagnosisCode_Text.Text = "";
            DiagnosisType7_DiagnosisDescription_Text.Text = "";
            DiagnosisType7_DiagnosisRecordedDate_Text.Text = "";
            DiagnosisType7GroupBox.Visibility = System.Windows.Visibility.Collapsed;

            BloodGroupType_BloodGroup_Text.Text = "";
            BloodGroupType_RecordedDate_Text.Text = "";
            BloodGroupTypeGroupBox.Visibility = System.Windows.Visibility.Collapsed;

            InsuranceType_InsuranceName_Text.Text = "";
            InsuranceType_InsuranceNumber_Text.Text = "";
            InsuranceType_InsuranceValidityStartDate_Text.Text = "";
            InsuranceType_InsuranceValidityEndDate_Text.Text = "";
            InsuranceTypeGroupBox.Visibility = System.Windows.Visibility.Collapsed;

            OrganDonor_Text.Text = "";

            ReadDataXmlResponse.Text = "";
            ScrollViewerReadData.ScrollToTop();
        }

        private void ReadDataFileTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OrganDonor_Text.Text = string.Empty;
            ReadDataXmlResponse.Text = string.Empty;
        }
    }
}
