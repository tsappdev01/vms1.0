using System.Windows;
using System.Windows.Controls;

using Microsoft.Win32;

using EIDAToolkitModernApp.ViewModels;

namespace EIDAToolkitModernApp.Views
{
    public partial class AdESSigningView : UserControl
    {
        public AdESSigningView()
        {
            InitializeComponent();
        }

        // CAdES file browse
        private void BrowseCadesInput_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select file to sign with CAdES",
                Filter = "All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var vm = DataContext as AdESSigningViewModel;

                if (vm != null)
                {
                    vm.CadesInputFile = dlg.FileName;
                }
            }
        }

        // XAdES file browse
        private void BrowseXadesInput_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select XML file to sign with XAdES",
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var vm = DataContext as AdESSigningViewModel;

                if (vm != null)
                {
                    vm.XadesInputFile = dlg.FileName;
                }
            }
        }

        // PAdES file browse
        private void BrowsePadesInput_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select PDF file to sign with PAdES",
                Filter = "PDF Files (*.pdf)|*.pdf|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var vm = DataContext as AdESSigningViewModel;

                if (vm != null)
                {
                    vm.PadesInputFile = dlg.FileName;
                }
            }
        }

        // PAdES signature image browse
        private void BrowsePadesImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select signature image",
                Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png|All Files (*.*)|*.*"
            };

            if (dlg.ShowDialog() == true)
            {
                var vm = DataContext as AdESSigningViewModel;

                if (vm != null)
                {
                    vm.PadesSignatureImage = dlg.FileName;
                }
            }
        }

        // PIN PasswordBox handlers (WPF doesn't support binding on PasswordBox)
        private void CadesPinBox_PasswordChanged(
            object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AdESSigningViewModel;

            if (vm != null)
            {
                vm.CadesPin = CadesPinBox.Password;
            }
        }

        private void XadesPinBox_PasswordChanged(
            object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AdESSigningViewModel;

            if (vm != null)
            {
                vm.XadesPin = XadesPinBox.Password;
            }
        }

        private void PadesPinBox_PasswordChanged(
            object sender, RoutedEventArgs e)
        {
            var vm = DataContext as AdESSigningViewModel;

            if (vm != null)
            {
                vm.PadesPin = PadesPinBox.Password;
            }
        }
    }
}
