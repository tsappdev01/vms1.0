using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace EIDAToolkitApp.Views
{
    /// <summary>
    /// Interaction logic for ErrorMsgDialog.xaml
    /// </summary>
    public partial class ErrorDialog : UserControl
    {
        /// <summary>
        /// Default constructor initializes the components
        /// </summary>
        public ErrorDialog()
        {
            InitializeComponent();
            Visibility = Visibility.Hidden;
        } 
        
        /// <summary>
        /// Show the dialog with animation 
        /// </summary>
        /// <param name="Msg1">Type of info message to be displayed in the dialog</param>
        /// <param name="Msg2">Message</param>
        public void ShowDialog(string Msg1, string Msg2)
        {
            typeOfMsgTxtBlck.Text = Msg1;
            TxtBx.Text = Msg2;
            Visibility = Visibility.Visible;
            DoubleAnimation da1 = new DoubleAnimation();
            da1.Duration = new Duration(TimeSpan.FromSeconds(0.4));
            Storyboard sb1 = new Storyboard();
            sb1.Children.Add(da1);
            Storyboard.SetTarget(da1, errorMsgDialogBox);
            DependencyProperty[] propertyChain1 =
            new DependencyProperty[] { StackPanel.OpacityProperty };
            string thePath1 = "(0)";
            PropertyPath myPropertyPath1 = new PropertyPath(thePath1, propertyChain1);
            Storyboard.SetTargetProperty(da1, myPropertyPath1);

            da1.From = 0;
            da1.To = 1;
            sb1.Begin();
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
            Storyboard.SetTarget(da1, errorMsgDialogBox);
            DependencyProperty[] propertyChain1 =
            new DependencyProperty[] { StackPanel.OpacityProperty };
            string thePath1 = "(0)";
            PropertyPath myPropertyPath1 = new PropertyPath(thePath1, propertyChain1);
            Storyboard.SetTargetProperty(da1, myPropertyPath1);

            da1.To = 0;
            sb1.Begin();

            await Task.Delay(400);
            Visibility = Visibility.Hidden;
        } 
    } 
} 
