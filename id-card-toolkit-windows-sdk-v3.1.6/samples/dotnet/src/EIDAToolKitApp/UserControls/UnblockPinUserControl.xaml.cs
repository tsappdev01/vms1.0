using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for UnblockPinUserControl.xaml
    /// </summary>
    public partial class UnblockPinUserControl : UserControl
    {
        public UnblockPinUserControl()
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
