﻿using KMPBookingCore;
using KMPBookingCore.DbObjects;
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
        private readonly List<Booking> _existingBookings = new List<Booking>();
        private readonly Dictionary<int, Booking> _bookingCache = new Dictionary<int, Booking>();
        private Service _currentService;
        private readonly List<Service> _invoicedServices = new List<Service>();

        private readonly HashSet<Service> _servicesToAdd = new HashSet<Service>();
        private readonly Dictionary<int, Service> _servicesToUpdate = new Dictionary<int, Service>();
        private Receipt _currentReceipt;

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

            AddingChk.Checked += AddingServiceCheckedUnchecked;
            AddingChk.Unchecked += AddingServiceCheckedUnchecked;
            EditingChk.Checked += EditingServiceCheckedUnchecked;
            EditingChk.Unchecked += EditingServiceCheckedUnchecked;
        }

        private void EditingServiceCheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (EditingChk.IsChecked == true)
            {
                AddingChk.IsChecked = false;
                UpdateServiceBtn.Content = "Update Service";
            }
        }

        private void AddingServiceCheckedUnchecked(object sender, RoutedEventArgs e)
        {
            if (AddingChk.IsChecked == true)
            {
                EditingChk.IsChecked = false;
                UpdateServiceBtn.Content = "Add Service";
            }
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
                    EventDate = date,
                    ServiceContent = svcdetail.ToString(),
                    TotalFee = total,
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
                var query = $"select ID, [Booking Date], Duration from Bookings where [Client ID]={Clients.ActiveClient.MedicareNumber} order by Bookings.[Booking Date]";
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
                        booking.EventDate = dt;
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
                ServiceDesc.Text = $"Service on {booking.EventDate?.ToString() ?? "unspecified date"} for {booking.Duration.TotalMinutes} minutes.";
                ServiceDate.SelectedDate = booking.EventDate;
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
            CurrentServiceToAdd().ServiceContent = ServiceDesc.Text;
            CurrentServiceToAdd().EventDate = ServiceDate.SelectedDate;
        }

        private void ServiceFeeComponentsChanged()
        {
            CurrentServiceToAdd().TotalFee = ServiceTotal.Text.GetDecimalOrZero();
            CurrentServiceToAdd().Owing = CurrentServiceToAdd().TotalFee;
            CurrentServiceToAdd().Benefit = ServiceBenefit.Text.GetDecimalOrZero();
            CurrentServiceToAdd().Calculate();
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
                _currentService = new Service
                {
                    Receipt = CreateOrGetCurrentReceipt()
                };
                EditingChk.IsEnabled = false;
                AddingChk.IsChecked = true;
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

        private void UpdateServiceClick(object sender, RoutedEventArgs e)
        {
            if (_currentService == null)
            {
                // TODO Error msg...
                return;
            }
            if (AddingChk.IsChecked == true)
            {
                AddService();
            }
            else if (EditingChk.IsChecked == true)
            {
                EditService();
            }
        }

        private void AddService()
        {
            _invoicedServices.Add(_currentService);
            InvoicedServices.Items.Add($"{_currentService.EventDate?.ToShortDateString()??"Unspecified date"}: '{_currentService.ServiceContent}' ${_currentService.TotalFee.ToDecPlaces()}");

            _servicesToAdd.Add(_currentService);

            ResetCurrentService();
        }

        private void UpdateToDb()
        {
            var cmdreceipts = AccessUtils.CreateInsert("Receipts", new (string, string)[]{
                // TODO
            });
            MainWindow.Connection.RunNonQuery(cmdreceipts);

            foreach (var sa in _servicesToAdd)
            {
                var cmdsvc = AccessUtils.CreateInsert("Services", new (string, string)[]{
                    ("Service", sa.ServiceContent),
                    ("Receipt ID", sa.Receipt.Id.ToString()),
                    ("Booking ID", sa.Booking?.Id.ToString()),
                    ("Service Date", sa.EventDate.ToDbDate()),
                    ("Total Fee", sa.TotalFee.ToDecPlaces()),
                    ("Owing", sa.Owing.ToDecPlaces()),
                    ("Benefit", sa.Benefit.ToDecPlaces()),
                    ("Gap", sa.Gap.ToDecPlaces()),
                    ("Discount", sa.Discount.ToDecPlaces()),
                    ("Balance", sa.Balance.ToDecPlaces())
                });
                MainWindow.Connection.RunNonQuery(cmdsvc);
            }

            foreach (var kvp in _servicesToUpdate)
            {
                var suid = kvp.Key;
                var su = kvp.Value;
                var cmdsvc = AccessUtils.CreateUpdate("Services", new (string, string)[]
                {
                    ("Service", su.ServiceContent),
                    ("Receipt ID", su.Receipt.Id.ToString()),
                    ("Booking ID", su.Booking?.Id.ToString()),
                    ("Service Date", su.EventDate.ToDbDate()),
                    ("Total Fee", su.TotalFee.ToDecPlaces()),
                    ("Owing", su.Owing.ToDecPlaces()),
                    ("Benefit", su.Benefit.ToDecPlaces()),
                    ("Gap", su.Gap.ToDecPlaces()),
                    ("Discount", su.Discount.ToDecPlaces()),
                    ("Balance", su.Balance.ToDecPlaces())
                }, $"ID = {suid}");
                MainWindow.Connection.RunNonQuery(cmdsvc);
            }
        }

        private Receipt CreateOrGetCurrentReceipt()
        {
            if (_currentReceipt == null)
            {
                _currentReceipt = new Receipt();
            }
            return _currentReceipt;
        }

        private void EditService()
        {

        }

        private void ResetCurrentService()
        {
            InvoicedServices.SelectedIndex = -1;
            _currentService = null;
            ServiceDesc.Text = "";
            ServiceDate.SelectedDate = null;
            ServiceTotal.Text = "";
            ServiceBenefit.Text = "";
            ServiceDiscount.Text = "";
            ServiceCharge.Text = "";
        }
    }
}
