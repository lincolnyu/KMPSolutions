using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Collections.Generic;
using BookingCore;
using static BookingCore.BookingIcs;
using static BookingCore.DateTimeUtils;

namespace Booking
{
    class AutoResetSuppressor
    {
        public bool Suppressing { get; private set; } = false;

        public void Run(Action proc)
        {
            if (!Suppressing)
            {
                Suppressing = true;
                proc();
                Suppressing = false;
            }
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetTitle();
            LoadSettings();
        }

        TimeSpan DEFAULT_EVENT_DURATION = TimeSpan.FromMinutes(30);

        private (DateTime?, TimeSpan?) GetBookingTemporalInfo()
        {
            TimeSpan? duration = null;
            if (int.TryParse(Duration.Text, out var dur))
            {
                duration = TimeSpan.FromMinutes(dur);
            }

            if (!BookingDate.SelectedDate.HasValue)
            {
                //TODO SelectedDate bad
                return (null, duration);
            }

            var bookingDate = BookingDate.SelectedDate.Value.Date;
            var bookingTime = LaunderTime(BookingTime.Text);
            if (!bookingTime.HasValue)
            {
                //TODO booking time bad
                return (null, duration);
            }
            var bookingDateTime = CreateDateTime(bookingDate, bookingTime.Value);

            return (bookingDateTime, duration);
        }

        void Book()
        {
            (var bookingDateTime, var duration) = GetBookingTemporalInfo();
            if (!bookingDateTime.HasValue)
            {
                //TODO bad booking time
                return;
            }
            if (!duration.HasValue)
            {
                duration = DEFAULT_EVENT_DURATION;
            }
            var ics = GenerateBookingIcs(ClientName.Text, ClientNumber.Text, bookingDateTime.Value,
                duration.Value);
            if (ics != null)
            {
                LaunchIcs(ics);
            }
            else
            {
                //TODO Event ICS bad
            }
        }

        DateTime? GetSmsTime()
        {
            if (!SmsDate.SelectedDate.HasValue)
            {
                return null;
            }
            var smsDate = SmsDate.SelectedDate.Value.Date;
            var smsTime = LaunderTime(SmsTime.Text);
            if (!smsTime.HasValue)
            {
                // TODO default time?
                return null;
            }
            return CreateDateTime(smsDate, smsTime.Value);
        }

        void SmsReminder()
        {
            (var bookingDateTime, var duration) = GetBookingTemporalInfo();
            if (!bookingDateTime.HasValue)
            {
                //TODO bad booking time
                return;
            }
            if (!duration.HasValue)
            {
                duration = DEFAULT_EVENT_DURATION;
            }

            var smsDateTime = GetSmsTime();
            if (!smsDateTime.HasValue)
            {
                //TODO SMS time bad
                return;
            }

            var smsics = GenerateSmsIcs(ClientName.Text, ClientNumber.Text,
                bookingDateTime.Value, duration.Value, smsDateTime.Value);
            if (smsics != null)
            {
                LaunchIcs(smsics);
            }
            else
            {
                //TODO SMS ICS bad
            }
        }

        private void BookClicked(object sender, RoutedEventArgs e)
        {
            var sms = SetSmsReminder.IsChecked == true;
            (var msg, var r) = CheckDate(sms ? Tolerance.AllowIncompleteSmsDate : Tolerance.Neither);
            if (r == CheckResult.Error)
            {
                MessageBox.Show($"Error: {msg}. Unable to book.", Title);
                return;
            }
            else if (r == CheckResult.Warning)
            {
                if (MessageBox.Show($"Warning: {msg}. Are you sure to continue?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }
            }
            Book();
            if (SetSmsReminder.IsChecked == true)
            {
                Thread.Sleep(1000);
                SmsReminder();
            }
        }

        private void DayBeforeChecked(object sender, RoutedEventArgs e)
        {
            UpdateSmsDate();
        }

        private void BookingDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CheckDateAndWarn();
            UpdateSmsDate();
        }

        private void UpdateSmsDate()
        {
            if (BookingDate.SelectedDate.HasValue)
            {
                if (DayBefore.IsChecked == true)
                {
                    SmsDate.SelectedDate = BookingDate.SelectedDate.Value.Subtract(TimeSpan.FromDays(1));
                }
            }
        }

        private void SmsTimeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CheckDateAndWarn();
        }

        private void SmsDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CheckDateAndWarn();
        }

        private void BookingTimeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            CheckDateAndWarn();
        }

        void CheckDateAndWarn()
        {
            (var msg, var e) = CheckDate(Tolerance.AllowBothIncomplete);
            if (e == CheckResult.Warning)
            {
                MessageBox.Show($"Warning: {msg}", Title);
            }
            else if (e == CheckResult.Error)
            {
                MessageBox.Show($"Error: {msg}", Title);
            }
        }

        enum CheckResult
        {
            OK,
            Warning,
            Error
        }

        enum Tolerance
        {
            Neither,
            AllowBothIncomplete,
            AllowIncompleteSmsDate
        }

        (string, CheckResult) CheckDate(Tolerance tol)
        {
            if (!IsInitialized)
            {
                return ("", CheckResult.OK);
            }
            (var bd, var dur) = GetBookingTemporalInfo();
            if (bd.HasValue)
            {
                if (bd.Value <= DateTime.Now)
                {
                    return ("Booking time is in the past", CheckResult.Error);
                }
            }
            else if (tol != Tolerance.AllowBothIncomplete)
            {
                return ("Missing booking date", CheckResult.Error);
            }

            var smsTime = GetSmsTime();
            if (smsTime.HasValue)
            {
                if ((bd.Value - smsTime.Value).TotalHours < 4)
                {
                    return ("SMS time is too late", CheckResult.Warning);
                }
            }
            else if (tol == Tolerance.Neither)
            {
                return ("Missing SMS reminder date", CheckResult.Error);
            }
            return ("", CheckResult.OK);
        }

        static void AddDistinctAlphabetic(ItemCollection items, string newItem)
        {
            var a = new string[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                a[i] = items[i].ToString();
            }
            var at = Array.BinarySearch<string>(a, newItem);
            if (at < 0)
            {
                items.Insert(-at - 1, newItem);
            }
        }

        bool LoadCustomerData(string filepath)
        {
            try
            {
                if (!File.Exists(filepath))
                {
                    MessageBox.Show("Error: The data file does not exist.", Title);
                    return false;
                }

                var fi = new FileInfo(filepath);
                using (var p = new ExcelPackage(fi))
                {
                    var ws = p.Workbook.Worksheets["Details"];

                    _clients.Clear();
                    ClientName.Items.Clear();
                    ClientMedicare.Items.Clear();
                    ClientNumber.Items.Clear();
                    for (var i = 2; i <= ws.Cells.Rows; i++)
                    {
                        var medi = ws.Cells[i, 1];
                        if (medi.Text.Trim().Length == 0) break;
                        var firstName = ws.Cells[i, 2];
                        var surname = ws.Cells[i, 3];
                        var name = FormCommaSeparateName(firstName.Text, surname.Text);
                        var phone = ws.Cells[i, 6];

                        AddDistinctAlphabetic(ClientName.Items, name);
                        AddDistinctAlphabetic(ClientMedicare.Items, medi.Text);
                        AddDistinctAlphabetic(ClientNumber.Items, phone.Text);

                        var dob = ws.Cells[i, 5];
                        var gen = ws.Cells[i, 4];
                        var client = new ClientRecord
                        {
                            FirstName = firstName.Text,
                            Surname = surname.Text,
                            MedicareNumber = medi.Text,
                            PhoneNumber = phone.Text,
                            Gender = ClientRecord.ParseGender(gen.Text),
                        };
                        if (DateTime.TryParse(dob.Text, out var dt))
                        {
                            client.DOB = dt;
                        }
                        _clients.Add(client);
                    }
                }

                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show($"Error: Failed to load the data file. Details:\n{e.Message}.", Title);
                return false;
            }
        }

        private void LoadSettings()
        {
            DataFilePath.Text = Properties.Settings.Default.DataFilePath;
            if (DataFilePath.Text != null && DataFilePath.Text.Trim().Length > 0 &&  File.Exists(DataFilePath.Text))
            {
                LoadCustomerData(DataFilePath.Text);
            }
        }

        void SetTitle()
        {
            var ver = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            var verstr = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            Title = $"KMP Booking (Ver {verstr})";
        }

        private void LoadDataClick(object sender, RoutedEventArgs e)
        {
            if (LoadCustomerData(DataFilePath.Text))
            {
                Properties.Settings.Default.DataFilePath = DataFilePath.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void BrowseDataPathClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (File.Exists(DataFilePath.Text))
            {
                ofd.InitialDirectory = Path.GetDirectoryName(DataFilePath.Text);
            }
            if (ofd.ShowDialog() == true)
            {
                DataFilePath.Text = ofd.FileName;
            }
        }

        static string FormCommaSeparateName(string firstName, string surname)
            => $"{surname}, {firstName}";

        private static (string /* first name*/, string /* surname */) SplitCommaSeparatedName(string name)
        {
            name = name.Trim();
            var segs = name.Split(',');
            if (segs.Length > 1)
            {
                return (segs[1].Trim(), segs[0].Trim());
            }
            // Treated as FirstName LastName
            var space = name.IndexOf(' ');
            if (space >= 0)
            {
                var fn = name.Substring(0, space);
                var sn = name.Substring(space + 1).Trim();
                return (fn, sn);
            }
            else
            {
                return (name, "");
            }
        }

        private IEnumerable<string> RecordsToStrings(IList<ClientRecord> clients)
        {
            foreach (var c in clients)
            {
                yield return $"{FormCommaSeparateName(c.FirstName, c.Surname)} (Medicare#{c.MedicareNumber}, Phone#{c.PhoneNumber})";
            }
        }

        private void SearchByName(string name, bool updateSourceField=false)
        {
            _suppressSearch.Run(() =>
            {
                (var fn, var sn) = SplitCommaSeparatedName(name);
                var clients = _clients.FindByName(fn, sn).OrderBy(x => x.MedicareNumber).ToList();
                if (clients.Count == 0)
                {
                    MessageBox.Show("Error: Client not found.", Title);
                }
                else
                {
                    ClientRecord client = null;
                    if (clients.Count > 1)
                    {
                        var dc = new DuplicateClients($"Multiple clients found with name {name}", RecordsToStrings(clients));
                        if (dc.ShowDialog() == true)
                        {
                            client = clients[dc.SelectedIndex];
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        client = clients[0];
                    }
                    if (updateSourceField)
                    {
                        ClientName.Text = name;
                    }
                    ClientMedicare.Text = client.MedicareNumber;
                    ClientNumber.Text = client.PhoneNumber;
                }
            });
        }

        private void SearchByMedi(string medi, bool updateSourceField = false)
        {
            _suppressSearch.Run(() =>
            {
                var client = _clients.FindByMedicareNumber(medi);
                if (client != null)
                {
                    ClientName.Text = FormCommaSeparateName(client.FirstName, client.Surname);
                    ClientNumber.Text = client.PhoneNumber;
                    if (updateSourceField)
                    {
                        ClientMedicare.Text = medi;
                    }
                }
                else
                {
                    MessageBox.Show("Error: Client not found.", Title);
                }
            });
        }

        private void SearchByPhone(string phone, bool updateSourceField = false)
        {
            _suppressSearch.Run(() =>
            {
                var clients = _clients.FindByPhoneNumber(phone).OrderBy(x => x.MedicareNumber).ToList();
                if (clients.Count == 0)
                {
                    MessageBox.Show("Error: Client not found.", Title);
                }
                else
                {
                    ClientRecord client;
                    if (clients.Count > 1)
                    {
                        var dc = new DuplicateClients($"Multiple clients found with phone number {phone}", RecordsToStrings(clients));
                        if (dc.ShowDialog() == true)
                        {
                            client = clients[dc.SelectedIndex];
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        client = clients[0];
                    }
                    ClientMedicare.Text = client.MedicareNumber;
                    ClientName.Text = FormCommaSeparateName(client.FirstName, client.Surname);
                    if (updateSourceField)
                    {
                        ClientNumber.Text = phone;
                    }
                }
            });
        }

        private void SearchByNameClick(object sender, RoutedEventArgs e)
        {
            SearchByName(ClientName.Text);
        }

        private void SearchByPhoneClick(object sender, RoutedEventArgs e)
        {
            SearchByPhone(ClientNumber.Text);
        }

        private void SearchByMediClick(object sender, RoutedEventArgs e)
        {
            SearchByMedi(ClientMedicare.Text);
        }

        private void ClientNumberKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByPhone(ClientNumber.Text);
            }
        }

        private void ClientNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByName(ClientName.Text);
            }
        }

        private void ClientMediKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByMedi(ClientMedicare.Text);
            }
        }

        private void ClientMediSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByMedi(e.AddedItems[0].ToString());
            }
        }

        private void ClientNameSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByName(e.AddedItems[0].ToString());
            }
        }

        private void ClientNumberSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByPhone(e.AddedItems[0].ToString());
            }
        }

        ClientRecords _clients = new ClientRecords();
        AutoResetSuppressor _suppressSearch = new AutoResetSuppressor();
    }
}
