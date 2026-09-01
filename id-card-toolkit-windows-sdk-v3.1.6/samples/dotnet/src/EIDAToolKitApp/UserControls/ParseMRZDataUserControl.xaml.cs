using System.Windows.Controls;

using AE.EmiratesId.IdCard;


namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for ParseMRZDataUserControl.xaml
    /// </summary>
    public partial class ParseMRZDataUserControl : UserControl
    {
        public ParseMRZDataUserControl()
        {
            InitializeComponent();
        }

        public void FillParseMRZDataTextFields(MRZData mrzDataAttributes)
        {
            DocumentType_MRZData_Text.Text = mrzDataAttributes.DocumentType;
            IssuedCountry_MRZData_Text.Text = mrzDataAttributes.IssuedCountry;
            CardNumber_MRZData_Text.Text = mrzDataAttributes.CardNumber;
            IdNumber_MRZData_Text.Text = mrzDataAttributes.IdNumber;
            DateOfBirth_MRZData_Text.Text = mrzDataAttributes.DateOfBirth;
            Gender_MRZData_Text.Text = mrzDataAttributes.Gender;
            CardExpiryDate_MRZData_Text.Text = mrzDataAttributes.CardExpiryDate;
            Nationality_MRZData_Text.Text = mrzDataAttributes.Nationality;
            FullName_MRZData_Text.Text = mrzDataAttributes.FullName;
        }
               
        public void ClearParseMRZDataTextFields()
        {
            DocumentType_MRZData_Text.Text = "";
            IssuedCountry_MRZData_Text.Text = "";
            CardNumber_MRZData_Text.Text = "";
            IdNumber_MRZData_Text.Text = "";
            DateOfBirth_MRZData_Text.Text = "";
            Gender_MRZData_Text.Text = "";
            CardExpiryDate_MRZData_Text.Text = "";
            Nationality_MRZData_Text.Text = "";
            FullName_MRZData_Text.Text = "";

        }
    }
}
