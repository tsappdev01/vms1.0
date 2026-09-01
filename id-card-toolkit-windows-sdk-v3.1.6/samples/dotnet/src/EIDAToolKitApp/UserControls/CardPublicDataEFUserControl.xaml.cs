using System.Windows.Controls;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for CardPublicDataEFUserControl.xaml
    /// </summary>
    public partial class CardPublicDataEFUserControl : UserControl
    {
        public CardPublicDataEFUserControl()
        {
            InitializeComponent();
        }

        private void CardPublicDataEFTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EFDataText.Text = string.Empty;
            ValidateSignatureCheckBox.IsChecked = false;
        }
    }
}
