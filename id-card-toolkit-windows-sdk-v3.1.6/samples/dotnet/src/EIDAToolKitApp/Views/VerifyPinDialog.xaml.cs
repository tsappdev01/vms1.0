using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace EIDAToolkitApp.Views
{
    /// <summary>
    /// Interaction logic for VerifyPinDialog.xaml
    /// </summary>
    public partial class VerifyPinDialog : UserControl
    {
        private bool _hideRequest = false;
        private bool _result = false;

        /// <summary>
        /// Default constructor initializes the components
        /// </summary>
        public VerifyPinDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Show the dialog with animation 
        /// </summary>
        /// <param name="Msg1">
        /// Type of info message to be displayed in the dialog
        /// </param>
        /// <param name="Msg2">Message</param>
        public bool ShowDialog()
        {
            Visibility = Visibility.Visible;
            DoubleAnimation da1 = new DoubleAnimation();
            da1.Duration = new Duration(TimeSpan.FromSeconds(0.4));
            Storyboard sb1 = new Storyboard();
            sb1.Children.Add(da1);
            Storyboard.SetTarget(da1, verifyPinDialogBox);
            DependencyProperty[] propertyChain1 =
            new DependencyProperty[] { StackPanel.OpacityProperty };
            string thePath1 = "(0)";
            PropertyPath myPropertyPath1 = new PropertyPath(thePath1, propertyChain1);
            Storyboard.SetTargetProperty(da1, myPropertyPath1);

            da1.From = 0;
            da1.To = 1;
            sb1.Begin();

            PasswordText.Focus();

            _hideRequest = false;
            while (!_hideRequest)
            {
                // HACK: Stop the thread if the application is about to close
                if (this.Dispatcher.HasShutdownStarted ||
                    this.Dispatcher.HasShutdownFinished)
                {
                    break;
                }

                // HACK: Simulate "DoEvents"
                this.Dispatcher.Invoke(
                    DispatcherPriority.Background,
                    new ThreadStart(delegate { }));
                Thread.Sleep(20);
            }

            return _result;
        }

        /// <summary>
        /// Hide the dialog
        /// </summary>
        public async void HideDialog()
        {
            DoubleAnimation da1 = new DoubleAnimation();
            da1.Duration = new Duration(TimeSpan.FromSeconds(0.4));
            Storyboard sb1 = new Storyboard();
            sb1.Children.Add(da1);
            Storyboard.SetTarget(da1, verifyPinDialogBox);
            DependencyProperty[] propertyChain1 =
            new DependencyProperty[] { StackPanel.OpacityProperty };
            string thePath1 = "(0)";
            PropertyPath myPropertyPath1 = new PropertyPath(thePath1, propertyChain1);
            Storyboard.SetTargetProperty(da1, myPropertyPath1);

            da1.To = 0;
            sb1.Begin();

            await Task.Delay(400);
            _hideRequest = true;
            Visibility = Visibility.Hidden;
        }

        /// <summary>
        /// Routine occurs when OK button of the dilaog is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VerifyPINBtn_Click(object sender, RoutedEventArgs e)
        {
            _result = true;
            this.HideDialog();
        }

        /// <summary>
        /// Routine occurs when password text changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void password_Text_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordText.Password.Length > 0)
                verifyPINBtn.IsEnabled = true;
            else
                verifyPINBtn.IsEnabled = false;
        }

        /// <summary>
        /// Routine occurs when Cancel button of the dilaog is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelBtn_Click(object sender, RoutedEventArgs e)
        {
            _result = false;
            this.HideDialog();
        }

        /// <summary>
        /// Validate the input control without entering non-numeric values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (e.Handled = regex.IsMatch(e.Text))
            {
                MessageBox.Show("Must enter numueric values", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void PasswordText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (PasswordText.Password.Length == 0)
                    e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                _result = false;
                this.HideDialog();
                e.Handled = true;
            }
        }
    }
}
