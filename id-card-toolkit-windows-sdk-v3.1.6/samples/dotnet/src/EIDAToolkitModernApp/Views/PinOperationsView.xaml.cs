using System.Windows.Controls;

using EIDAToolkitModernApp.ViewModels;

namespace EIDAToolkitModernApp.Views
{
    public partial class PinOperationsView : UserControl
    {
        public PinOperationsView()
        {
            InitializeComponent();
        }

        // PasswordBox cannot be data-bound directly (security by design),
        // so we use code-behind event handlers to update the ViewModel.

        private void ResetPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PinOperationsViewModel vm)
            {
                vm.ResetPin = ((PasswordBox)sender).Password;
            }
        }

        private void ResetConfirmPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PinOperationsViewModel vm)
            {
                vm.ResetConfirmPin = ((PasswordBox)sender).Password;
            }
        }

        private void UnblockPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PinOperationsViewModel vm)
            {
                vm.UnblockPin = ((PasswordBox)sender).Password;
            }
        }

        private void UnblockConfirmPinBox_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is PinOperationsViewModel vm)
            {
                vm.UnblockConfirmPin = ((PasswordBox)sender).Password;
            }
        }

    }
}
