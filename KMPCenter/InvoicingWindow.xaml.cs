using Microsoft.Office.Interop.Word;
using System;
using System.Windows;
using Window = System.Windows.Window;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for InvoiceGeneratorWindow.xaml
    /// </summary>
    public partial class InvoicingWindow : Window
    {
        public InvoicingWindow()
        {
            InitializeComponent();
        }

        private void GenerateClick(object sender, RoutedEventArgs e)
        {
            Generate();
        }

        //https://www.c-sharpcorner.com/UploadFile/muralidharan.d/how-to-create-word-document-using-C-Sharp/
        void Generate()
        {
            try
            {
                //Create an instance for word app  
                var winword = new Microsoft.Office.Interop.Word.Application();

                //Set animation status for word application  
                winword.ShowAnimation = false;

                //Set status for word application is to be visible or not.  
                winword.Visible = false;

                //Create a missing variable for missing value  
                object missing = System.Reflection.Missing.Value;

                //TODO ...
            }
            catch (Exception ex)
            {
                App.ShowMessage(ex.Message);
            }
        }
    }
}
