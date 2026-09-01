using System.Windows.Controls;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for UpdateDataUserControl.xaml
    /// </summary>
    public partial class UpdateDataUserControl : UserControl
    {
        public UpdateDataUserControl()
        {
            InitializeComponent();
        }

        private void UpdateDataFileTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateDataXmlResponse.Text = string.Empty;
        }
    }
}
