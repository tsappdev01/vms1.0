using System.Windows.Controls;

using EIDAToolkitModernApp.ViewModels;

namespace EIDAToolkitModernApp.Views
{
    public partial class DeviceManagementView : UserControl
    {
        public DeviceManagementView()
        {
            InitializeComponent();
        }

        private void RegPasswordBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as DeviceManagementViewModel;

            if (vm != null)
            {
                vm.RegPassword = RegPasswordBox.Password;
            }
        }
    }
}
