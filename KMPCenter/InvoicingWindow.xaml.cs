using KMPBookingCore;
using KMPBookingPlus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Window = System.Windows.Window;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for InvoiceGeneratorWindow.xaml
    /// </summary>
    public partial class InvoicingWindow : Window
    {
        private MainWindow MainWindow => (MainWindow)Owner;
        private List<Booking> _existingBookings = new List<Booking>();
        private Dictionary<int, Booking> _bookingCache = new Dictionary<int, Booking>();
        private Service _currentService;
        private List<Service> _invoicedServices = new List<Service>();

        public InvoicingWindow(MainWindow mw)
        {
            Owner = mw;
            InitializeComponent();

            Clients.SetDataConnection(MainWindow.Connection);
            Clients.ActiveClientChanged += OnActiveClientChanged;
            OnlyPreviousBookings.Checked += OnlyPreviousBookingsCheckedOrUnchecked;
            OnlyPreviousBookings.Unchecked += OnlyPreviousBookingsCheckedOrUnchecked;

            ServiceTotal.TextChanged += ServiceFeeComponentsChanged;
            ServiceBenefit.TextChanged += ServiceFeeComponentsChanged;
            ServiceDiscount.TextChanged += ServiceFeeComponentsChanged;
            ServiceDesc.TextChanged += ServiceDescTextChanged;
            ServiceDate.SelectedDateChanged += ServiceDateSelectedDateChanged;

            AttachBookingChk.Checked += AttachBookingCheckedUnchecked;
            AttachBookingChk.Unchecked += AttachBookingCheckedUnchecked;
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
            _existingBookings.Clear();
            if (Clients.ActiveClient != null && MainWindow.Connection != null)
            {
                var query = $"select ID, [Booking Date], Duration from Bookings where [Client ID]={Clients.ActiveClient.Id.ClientIdFromStr()} order by Bookings.[Booking Date]";
                using (var r = MainWindow.Connection.RunReaderQuery(query))
                {
                    while (r.Read())
                    {
                        var id = r.GetInt32(0);
                        var dt = r.TryGetDateTime(1);
                        if (OnlyPreviousBookings.IsChecked == true)
                        {
                            if (dt > DateTime.Now)
                            {
                                break;
                            }
                        }
                        var nummins = r.GetInt32(2);
                        ExistingBookings.Items.Add($"{dt?.ToString() ?? "Unspecified date"} for {nummins} mins");
                        var booking = GetOrCreateBooking(id);
                        booking.Client = Clients.ActiveClient;
                        booking.DateTime = dt;
                        booking.Duration = TimeSpan.FromMinutes(nummins);
                        _existingBookings.Add(booking);
                    }
                }
            }
        }

        private Booking GetOrCreateBooking(int id)
        {
            if (_bookingCache.TryGetValue(id, out var val))
            {
                return val;
            }
            var res = new Booking { Id = id };
            _bookingCache[id] = res;
            return res;
        }

        private void ServiceFromBookingClick(object sender, RoutedEventArgs e)
        {
            if (ExistingBookings.SelectedIndex >= 0)
            {
                var booking = _existingBookings[ExistingBookings.SelectedIndex];
                ServiceDesc.Text = $"Service on {booking.DateTime?.ToString() ?? "unspecified date"} for {booking.Duration.TotalMinutes} minutes.";
                ServiceDate.SelectedDate = booking.DateTime;
                AttachBookingChk.IsChecked = true;
            }
        }

        private void ServiceDescTextChanged(object sender, TextChangedEventArgs e)
        {
            ServiceDataChanged();
        }

        private void ServiceFeeComponentsChanged(object sender, TextChangedEventArgs e)
        {
            ServiceFeeComponentsChanged();
        }

        private void ServiceDateSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ServiceDataChanged();
        }

        private void ServiceDataChanged()
        {
            CurrentServiceToAdd().Detail = ServiceDesc.Text;
            CurrentServiceToAdd().Date = ServiceDate.SelectedDate;
        }

        private void ServiceFeeComponentsChanged()
        {
            CurrentServiceToAdd().Total = ServiceTotal.Text.GetDecimalOrZero();
            CurrentServiceToAdd().Owing = CurrentServiceToAdd().Total;
            CurrentServiceToAdd().Benefit = ServiceBenefit.Text.GetDecimalOrZero();
            CurrentServiceToAdd().Workout();
            ServiceCharge.Text = CurrentServiceToAdd().Balance.ToDecPlaces();
        }

        private void AttachBookingCheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (AttachBookingChk.IsChecked == true && ExistingBookings.SelectedIndex >= 0)
            {
                var booking = _existingBookings[ExistingBookings.SelectedIndex];
                CurrentServiceToAdd().Booking = booking;
                AttachBookingId.Text = $"Booking #{booking.Id.BookingIdToStr()}";
            }
            else if (AttachBookingChk.IsChecked != true)
            {
                CurrentServiceToAdd().Booking = null;
                AttachBookingId.Text = "";
            }
        }

        private Service CurrentServiceToAdd()
        {
            if (_currentService == null)
            {
                _currentService = new Service();
            }
            return _currentService;
        }

        private void AttachBookingClick(object sender, RoutedEventArgs e)
        {
            var booking = CurrentServiceToAdd().Booking;
            if (booking != null)
            {
                var client = booking.Client;
                if (client != null)
                {
                    Clients.TrySetActiveClient(client);
                    var index = _existingBookings.IndexOf(booking);
                    if (index >= 0)
                    {
                        ExistingBookings.SelectedIndex = index;
                    }
                }
            }
        }
    }
}
