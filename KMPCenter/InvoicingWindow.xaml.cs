using KMPBookingCore;
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
            var (res, errorMsg) = InvoiceUtils.GenerateInvoice(@"c:\temp\output.docx", InvoiceTemplatePath, "Test client", 
                "Test client No.", "Test claim no.", "test diagnosis detail",
                "test receipt no.", DateTime.Today, "test healthfund", "test membership no.", svcs.ToList(), 
                30, 10, 20);
            if (res)
            {
                App.ShowMessage("Invoice successfully generated.");
            }
            else
            {
                App.ShowMessage($"Invoice generation failed:\n{errorMsg}");
            }
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
    }
}
