using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KmpCrmUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static string _dataFilePath;
        static StorageFile _storageFile;

        public MainPage()
        {
            this.InitializeComponent();
            UpdateUiEnablement();
        }

        private void UpdateUiEnablement()
        {
            BrowseDataFile.IsEnabled = !CrmData.Initialized;
            if (CrmData.Initialized)
            {
                DataFileName.Text = _dataFilePath;
            }
            ListCustomers.IsEnabled = CrmData.Initialized;
            EditCustomer.IsEnabled = CrmData.Initialized;
        }

        private void EditCustomer_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(EditCustomerPage));
        }

        private void ListCustomers_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ListCustomersPage));
        }

        private async void BrowseDataFile_Click(object sender, RoutedEventArgs e)
        {
            if (CrmData.Initialized)
            {
                BrowseDataFile.IsEnabled = false;
                return;
            }
            var fsp = new FileSavePicker();
            // Dropdown of file types the user can save the file as
            fsp.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
            // Default file name if the user does not type one in or select a file to replace
            fsp.SuggestedFileName = "CrmData";
            fsp.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            _storageFile = await fsp.PickSaveFileAsync();
            if (_storageFile != null)
            {
                var stream = await _storageFile.OpenStreamForReadAsync();
                CrmData.Initialize(new StreamReader(stream));
                _dataFilePath = _storageFile.Path;
                UpdateUiEnablement();
            }
        }
    }
}
