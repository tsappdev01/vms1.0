using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for AuthBioOnServerUserControl.xaml
    /// </summary>
    public partial class AuthBioOnServerUserControl : UserControl
    {
        public AuthBioOnServerUserControl()
        {
            InitializeComponent();
        }

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
    }
}
