using KMPBookingCore;
using KMPBookingPlus;
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

        private MainWindow MainWindow => (MainWindow)Owner;

        public InvoicingWindow(MainWindow mw)
        {
            Owner = mw;
            InitializeComponent();

            Clients.SetDataConnection(MainWindow.Connection);
            Clients.ActiveClientChanged += OnActiveClientChanged;
            OnlyPreviousBookings.Checked += OnlyPreviousBookingsCheckedOrUnchecked;
            OnlyPreviousBookings.Unchecked += OnlyPreviousBookingsCheckedOrUnchecked;
        }

        private void OnActiveClientChanged()
        {
            UpdateBookingList();
        }

        public string InvoiceTemplatePath => MainWindow.InvoiceTemplatePath.Text;

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

        private void OnlyPreviousBookingsCheckedOrUnchecked(object sender, RoutedEventArgs e)
        {
            UpdateBookingList();
        }

        private void UpdateBookingList()
        {
            ExistingBookings.Items.Clear();
            if (Clients.ActiveClient != null && MainWindow.Connection != null)
            {
                var query = $"select [Booking Date], Duration from Bookings where [Client ID]={Clients.ActiveClient.Id.ClientIdFromStr()} order by Bookings.[Booking Date]";
                using (var r = MainWindow.Connection.RunReaderQuery(query))
                {
                    while (r.Read())
                    {
                        var dt = r.TryGetDateTime(0);
                        if (OnlyPreviousBookings.IsChecked == true)
                        {
                            if (dt > DateTime.Now)
                            {
                                break;
                            }
                        }
                        var nummins = r.GetInt32(1);
                        ExistingBookings.Items.Add($"{dt?.ToString() ?? "Unspecified date"} for {nummins} mins");
                    }
                }
            }
        }
    }
}
