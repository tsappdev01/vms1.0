using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for NFCAuthFieldsUserControl.xaml
    /// </summary>
    public partial class NFCAuthFieldsUserControl : UserControl
    {
        public NFCAuthFieldsUserControl()
        {
            InitializeComponent();
        }

        public void ClearNFCAuthTextFields()
        {
            SetCardNumberText.Text = "";
            SetDateOfBirthDayText.Text = "";
            SetDateOfBirthMonthText.Text = "";
            SetDateOfBirthYearText.Text = "";
            SetExpiryDateDayText.Text = "";
            SetExpiryDateMonthText.Text = "";
            SetExpiryDateYearText.Text = "";
        }

        /// <summary>
        /// Validate the input control without entering non-numeric values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "\r")
            {
                e.Handled = true;
                return;
            }

            Regex regex = new Regex("[^0-9]+");
            if (e.Handled = regex.IsMatch(e.Text))
            {
                MessageBox.Show("Must enter numueric values", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Routine called when NFCField_DateOfBirth_Day_Text when key is pressed on keyboard 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDateOfBirthDayText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetDateOfBirthDayText.Text.Length >= 2)
            {
                SetDateOfBirthMonthText.Focus();
            }
        }

        /// <summary>
        /// Routine called when NFCField_DateOfBirth_Month_Text when key is pressed on keyboard 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDateOfBirthMonthText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetDateOfBirthMonthText.Text.Length >= 2)
            {
                SetDateOfBirthYearText.Focus();
            }
        }

        /// <summary>
        /// Routine called when NFCField_DateOfBirth_Year_Text when key is pressed on keyboard 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetDateOfBirthYearText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetDateOfBirthYearText.Text.Length >= 2)
            {
                SetExpiryDateDayText.Focus();
            }
        }

        /// <summary>
        /// Routine called when NFCField_ExpiryDate_Day_Text when key is pressed on keyboard 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetExpiryDateDayText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetExpiryDateDayText.Text.Length >= 2)
            {
                SetExpiryDateMonthText.Focus();
            }
        }

        /// <summary>
        /// Routine called when NFCField_ExpiryDate_Month_Text when key is pressed on keyboard 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetExpiryDateMonthText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetExpiryDateMonthText.Text.Length >= 2)
            {
                SetExpiryDateYearText.Focus();
            }
        }

        /// <summary>
        /// Routine called when NFCField_ExpiryDate_Year_Text when key is pressed on keyboard 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetExpiryDateYearText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SetExpiryDateYearText.Text.Length >= 2)
            {
                this.Focus();
            }
        }
    }
}
