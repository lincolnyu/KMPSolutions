using Microsoft.Office.Interop.Word;
using System;
using System.Linq;
using System.Windows;
using System.Collections;
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
                object readOnly = false;
                object isVisible = false;

                //Create a missing variable for missing value  
                object missing = System.Reflection.Missing.Value;
                object infilename = @"D:\temp\invoice-template.docx";
                var document = winword.Documents.Open(infilename, missing, readOnly, missing, missing, missing, missing, missing, missing, missing, missing, isVisible, missing, missing, missing, missing);
                var t = document.Tables.Cast<Table>().FirstOrDefault();
                t.Cell(2, 2).Range.Text = "ABN-12345678";
                
                //TODO ...
                object filename = @"d:\temp\output.docx";
                document.SaveAs2(ref filename);
                document.Close(ref missing, ref missing, ref missing);
                document = null;
                winword.Quit(ref missing, ref missing, ref missing);
                winword = null;
                MessageBox.Show("Document created successfully !");

            }
            catch (Exception ex)
            {
                App.ShowMessage(ex.Message);
            }
        }
    }
}
