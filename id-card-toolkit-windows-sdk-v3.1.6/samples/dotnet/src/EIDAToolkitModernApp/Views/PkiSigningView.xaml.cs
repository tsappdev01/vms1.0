using System.Windows.Controls;

using EIDAToolkitModernApp.ViewModels;

namespace EIDAToolkitModernApp.Views
{
    public partial class PkiSigningView : UserControl
    {
        public PkiSigningView()
        {
            InitializeComponent();
        }

        private void PkiPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as PkiSigningViewModel;

            if (vm != null)
            {
                vm.PkiPin = PkiPinBox.Password;
            }
        }

        private void SignAuthPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as PkiSigningViewModel;

            if (vm != null)
            {
                vm.SignAuthPin = SignAuthPinBox.Password;
            }
        }

        private void SignDataPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as PkiSigningViewModel;

            if (vm != null)
            {
                vm.SignDataPin = SignDataPinBox.Password;
            }
        }

        private void CertPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as PkiSigningViewModel;

            if (vm != null)
            {
                vm.CertPin = CertPinBox.Password;
            }
        }

        private void FamilyBookPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            var vm = DataContext as PkiSigningViewModel;

            if (vm != null)
            {
                vm.FamilyBookPin = FamilyBookPinBox.Password;
            }
        }
    }
}
