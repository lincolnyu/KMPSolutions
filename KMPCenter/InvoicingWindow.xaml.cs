using KMPCenter.Data;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Window = System.Windows.Window;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for InvoiceGeneratorWindow.xaml
    /// </summary>
    public partial class InvoicingWindow : Window
    {
        private MainWindow _mainwindow;

        public InvoicingWindow(MainWindow mw)
        {
            _mainwindow = mw;
            InitializeComponent();
        }

        public string InvoiceTemplatePath => _mainwindow.InvoiceTemplatePath.Text;

        private void GenerateClick(object sender, RoutedEventArgs e)
        {
            Generate(@"c:\temp\output.docx", "Test client", "Test client No.", "Test claim no.", "test diagnosis detail",
                "test receipt no.", DateTime.Today, "test healthfund", "test membership no.", new Service[]{ }, 
                30, 10, 20);
        }

        //https://www.c-sharpcorner.com/UploadFile/muralidharan.d/how-to-create-word-document-using-C-Sharp/
        void Generate(string outputfilename,
            string clientName, string clientNo, string claimNo, string diagnosis, 
            string receiptNo, DateTime date, string healthFund, string memberNo, IList<Service> services, 
            decimal paymentReceived, decimal discount, decimal balance)
        {
            try
            {
                //Create an instance for word app  
                var winword = new Microsoft.Office.Interop.Word.Application();

                //Set animation status for word application  

                winword.ShowAnimation = false;

                //Set status for word application is to be visible or not.  
                winword.Visible = false;
                object readOnly = true;
                object isVisible = false;

                //Create a missing variable for missing value  
                object missing = System.Reflection.Missing.Value;
                object infilename = InvoiceTemplatePath;
                var document = winword.Documents.Open(infilename, missing, readOnly, 
                    missing, missing, missing, missing, missing, missing, missing, 
                    missing, isVisible, missing, missing, missing, missing);
                var t = document.Tables.Cast<Table>().FirstOrDefault();
                t.Cell(2, 2).Range.Text = clientName;
                t.Cell(3, 2).Range.Text = clientNo;
                t.Cell(4, 2).Range.Text = claimNo;
                t.Cell(5, 2).Range.Text = diagnosis;
                t.Cell(2, 4).Range.Text = receiptNo;
                t.Cell(3, 4).Range.Text = date.ToShortDateString();
                t.Cell(4, 4).Range.Text = healthFund;
                t.Cell(5, 4).Range.Text = memberNo;

                var due = services.Sum(x => x.Balance);
                t.Cell(11, 3).Range.Text = due.ToString();
                t.Cell(12, 3).Range.Text = paymentReceived.ToString();
                t.Cell(13, 3).Range.Text = $"{discount}%";
                t.Cell(14, 3).Range.Text = balance.ToString();

                object rref = t.Cell(9, 1);
                t.Rows.Add(ref rref);
                t.Rows.Add(ref rref);
                t.Rows.Add(ref rref);
                t.Rows.Add(ref rref);
                t.Rows.Add(ref rref);

                object filename = outputfilename;
                document.SaveAs2(ref filename);
                document.Close(ref missing, ref missing, ref missing);
                document = null;
                winword.Quit(ref missing, ref missing, ref missing);
                winword = null;
                App.ShowMessage("Invoice generated successfully!");
            }
            catch (Exception ex)
            {
                App.ShowMessage(ex.Message);
            }
        }
    }
}
