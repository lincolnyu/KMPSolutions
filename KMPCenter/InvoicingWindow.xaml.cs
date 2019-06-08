using KMPCenter.Data;
using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Window = System.Windows.Window;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for InvoiceGeneratorWindow.xaml
    /// </summary>
    public partial class InvoicingWindow : Window
    {
        static object missing = System.Reflection.Missing.Value;

        private MainWindow _mainwindow;

        public InvoicingWindow(MainWindow mw)
        {
            _mainwindow = mw;
            InitializeComponent();
        }

        public string InvoiceTemplatePath => _mainwindow.InvoiceTemplatePath.Text;

        private void GenerateClick(object sender, RoutedEventArgs e)
        {
            var svcs = GenerateRandomServices(24);
            Generate(@"c:\temp\output.docx", "Test client", "Test client No.", "Test claim no.", "test diagnosis detail",
                "test receipt no.", DateTime.Today, "test healthfund", "test membership no.", svcs.ToList(), 
                30, 10, 20);
        }
        
        private IEnumerable<Service> GenerateRandomServices(int count)
        {
            var rand = new Random();
            for (var i = 0; i < count; i++)
            {
                var date = DateTime.Today;
                var svcdetail = new StringBuilder("Sample Service");
                var stringlen = rand.Next(3, 50);
                for (var j = 0; j < stringlen; j++)
                {
                    var c = rand.Next(0, 26);
                    svcdetail.Append('A' + c);
                }
                decimal total = (decimal)(rand.NextDouble() * 100);
                yield return new Service {
                    Date = date,
                    Detail = svcdetail.ToString(),
                    Total = total,
                    Owing = total,
                    Benefit = 0,
                    Discount = 0,
                    Gap = 0,
                    Balance = total
                };
            }
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

                var rn = 8;
                foreach (var svc in services)
                {
                    t.Cell(rn, 1).Range.Text = svc.Date.ToShortDateString();
                    t.Cell(rn, 2).Range.Text = svc.Detail;
                    t.Cell(rn, 3).Range.Text = svc.Total.ToString();
                    t.Cell(rn, 4).Range.Text = svc.Owing.ToString();
                    t.Cell(rn, 5).Range.Text = svc.Benefit.ToString();
                    t.Cell(rn, 6).Range.Text = svc.Gap.ToString();
                    t.Cell(rn, 7).Range.Text = $"{svc.Discount}%";
                    t.Cell(rn, 8).Range.Text = svc.Balance.ToString();

                    t.Rows.Add(ref rref);
                    rn++;
                }

                var origPagesCount = GetPagesCount(document);
                while (true)
                {
                    t.Rows.Add(ref rref);
                    var currPageCount = GetPagesCount(document);
                    if (currPageCount > origPagesCount)
                    {
                        document.Undo();
                        break;
                    }
                }

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

        private static int GetPagesCount(Document document)
        {
            var PagesCountStat = WdStatistic.wdStatisticPages;
            int pagesCount = document.ComputeStatistics(PagesCountStat, ref missing);
            return pagesCount;
        }
    }
}
