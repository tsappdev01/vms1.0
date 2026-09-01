using System;
using System.Windows;
using System.Windows.Controls;

using AE.EmiratesId.IdCard;

namespace EIDAToolkitApp.UserControls
{
    /// <summary>
    /// Interaction logic for ResponseValidationUserControl.xaml
    /// </summary>
    public partial class ResponseValidationUserControl : UserControl
    {
        public ResponseValidationUserControl()
        {
            InitializeComponent();
        }

        public void FillResponseValidationTextFields(ToolkitResponseData toolkitResponseData)
        {
            ServiceText.Text = toolkitResponseData.Service;
            ActionText.Text = toolkitResponseData.Action;
            RequestIDText.Text = toolkitResponseData.RequsetId;
            NonceText.Text = toolkitResponseData.Nonce;
            CorrelationIDText.Text = toolkitResponseData.CorrelationId;
            CSNText.Text = toolkitResponseData.CSN;
            CardNumber_ValidationText.Text = toolkitResponseData.CardNumber;
            IDNumber_Validation_Text.Text = toolkitResponseData.IdNumber;
            TimeStampText.Text = toolkitResponseData.TimeStamp;
            ValidityIntervalText.Text = toolkitResponseData.ValidityInterval.ToString();
        }

        public void ClearResponseValidationTextFields()
        {
            ServiceText.Text = "";
            ActionText.Text = "";
            RequestIDText.Text = "";
            NonceText.Text = "";
            CorrelationIDText.Text = "";
            CSNText.Text = "";
            CardNumber_ValidationText.Text = "";
            IDNumber_Validation_Text.Text = "";
            TimeStampText.Text = "";
            ValidityIntervalText.Text = "";
        }

        private void BrowseCertificateDataFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension
                fileDialog.Multiselect = false;
                fileDialog.Filter = "Certificate (*.cer)|*.cer|Certificate (*.cert)|*.cert";

                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> value = fileDialog.ShowDialog();

                // Get the selected file name
                CertificateDataFilePathText.Text = "";
                if (value == true)
                {
                    string file = fileDialog.FileName;
                    CertificateDataFilePathText.Text = file;
                }
            }
            catch (Exception ex)
            {
                CertificateDataFilePathText.Text = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void BrowseCertificateChainFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create OpenFileDialog 
                Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();

                // Set filter for file extension
                fileDialog.Multiselect = false;
                fileDialog.Filter = "Certificate (*.cer)|*.cer|Certificate (*.cert)|*.cert";

                // Display OpenFileDialog by calling ShowDialog method 
                Nullable<bool> value = fileDialog.ShowDialog();

                // Get the selected file name
                CertificateChainFilePathText.Text = "";
                if (value == true)
                {
                    string file = fileDialog.FileName;
                    CertificateChainFilePathText.Text = file;
                }
            }
            catch (Exception ex)
            {
                CertificateChainFilePathText.Text = "";
                MessageBox.Show(ex.Message, "Error");
            }
        }

    }
}
