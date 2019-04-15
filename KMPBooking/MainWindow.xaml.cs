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
using KMPBookingCore;
using static KMPBookingCore.BookingIcs;
using static KMPBookingCore.DateTimeUtils;
using System.Text;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Deployment.Application;

namespace KMPBooking
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
            _checkDateActioner = new StackBottomActioner(CheckDateAndWarn);
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
            var ics = GenerateBookingIcs(ClientName.Text, ClientNumber.Text, ClientMedicare.Text, 
                bookingDateTime.Value, duration.Value);
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

            var smsics = GenerateSmsIcs(ClientName.Text, ClientNumber.Text, ClientMedicare.Text,
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

        static void AddValidationMessage(StringBuilder sb, 
            ref CheckResult currentCr, CheckResult msgCr, string msg)
        {
            if (msgCr < currentCr)
            {
                return;
            }
            if (msgCr > currentCr)
            {
                currentCr = msgCr;
                sb.Clear();
            }
            sb.Append("- ");
            sb.Append(msg);
            if (!msg.EndsWith("."))
            {
                sb.Append(".");
            }
            sb.AppendLine();
        }

        private bool ValidateAndProceed()
        {
            var sb = new StringBuilder();
            var sms = SetSmsReminder.IsChecked == true;
            var r = CheckResult.OK;
            if (string.IsNullOrWhiteSpace(ClientName.Text))
            {
                AddValidationMessage(sb, ref r, CheckResult.Error, "Missing client name");
            }

            if (string.IsNullOrWhiteSpace(ClientMedicare.Text))
            {
                AddValidationMessage(sb, ref r, CheckResult.Warning, "Missing client medicare number");
            }

            (var msg, var rr) = CheckDate(sms ? Tolerance.AllowIncompleteSmsDate : Tolerance.Neither);
            AddValidationMessage(sb, ref r, rr, msg);

            if (SetSmsReminder.IsChecked == true && string.IsNullOrWhiteSpace(ClientNumber.Text) 
                && r != CheckResult.Error)
            {
                AddValidationMessage(sb, ref r, CheckResult.Warning, "Phone number not provided, SMS event will be incomplete.");
            }

            if (r == CheckResult.Error)
            {
                MessageBox.Show($"Unable to book. Error details:\n{sb.ToString()}", Title);
                return false;
            }
            else if (r == CheckResult.Warning)
            {
                if (MessageBox.Show($"Warning:\n{sb.ToString()}Are you sure to continue?", Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }

        private void BookClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateAndProceed())
            {
                return;
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
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
                UpdateSmsDate();
            }
        }

        private void BookingDateChanged(object sender, SelectionChangedEventArgs e)
        {
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
                UpdateSmsDate();
            }
        }

        private void UpdateSmsDate()
        {
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
                if (BookingDate.SelectedDate.HasValue)
                {
                    if (DayBefore.IsChecked == true)
                    {
                        SmsDate.SelectedDate = BookingDate.SelectedDate.Value.Subtract(TimeSpan.FromDays(1));
                    }
                }
            }
        }

        private void SmsTimeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
            }
        }

        private void SmsDateChanged(object sender, SelectionChangedEventArgs e)
        {
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
            }
        }

        private void BookingTimeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
            }
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

                var smsTime = GetSmsTime();
                if (smsTime.HasValue)
                {
                    if (bd.Value <= smsTime.Value)
                    {
                        return ("SMS time is past booking time", CheckResult.Error);
                    }
                    else if ((bd.Value - smsTime.Value).TotalHours < 6)
                    {
                        return ("SMS time is too late", CheckResult.Warning);
                    }
                }
                else if (tol == Tolerance.Neither)
                {
                    return ("Missing SMS reminder date", CheckResult.Error);
                }
            }
            else if (tol != Tolerance.AllowBothIncomplete)
            {
                return ("Missing booking date", CheckResult.Error);
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
                    MessageBox.Show("Error: The data file does not exist. Please provide a valid one.\nIf this is the first time to run a new version, it may not be initially available.", Title);
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
                        var medi = ws.Cells[i, 1].Text.Trim();
                        if (medi.Length == 0) break;
                        var firstName = ws.Cells[i, 2].Text.LaunderSpaceSeparateString();
                        var surname = ws.Cells[i, 3].Text.LaunderSpaceSeparateString();
                        var name = FormCommaSeparateName(firstName, surname);
                        var phone = ws.Cells[i, 6].Text;

                        AddDistinctAlphabetic(ClientMedicare.Items, medi);
                        if (name.Length > 0)
                        {
                            AddDistinctAlphabetic(ClientName.Items, name);
                        }
                        if (phone.Length > 0)
                        {
                            AddDistinctAlphabetic(ClientNumber.Items, phone);
                        }

                        var dob = ws.Cells[i, 5].Text.Trim();
                        var gen = ws.Cells[i, 4].Text.Trim();
                        var client = new ClientRecord
                        {
                            FirstName = firstName,
                            Surname = surname,
                            MedicareNumber = medi,
                            PhoneNumber = phone,
                            Gender = ClientRecord.ParseGender(gen),
                        };
                        if (DateTime.TryParse(dob, out var dt))
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
            LoadDataFile(false);
        }

        void SetTitle()
        {
            (var ver, var deployed) = GetVersion();
            var verstr = $"{ver.Major}.{ver.Minor}.{ver.Build}";
            Title = $"KMP Booking ({(deployed ? "" : "Assembly ")}Ver {verstr})";
        }

        static (Version, bool /*is network deployed*/) GetVersion(bool tryDeployed = true)
        {
            var networkDeployed = ApplicationDeployment.IsNetworkDeployed;
            if (networkDeployed && tryDeployed)
            {
                return (ApplicationDeployment.CurrentDeployment.CurrentVersion, true);
            }
            else
            {
                return (System.Reflection.Assembly.GetExecutingAssembly().GetName().Version, networkDeployed);
            }
        }

        private void LoadDataFile(bool save)
        {
            if (LoadCustomerData(DataFilePath.Text))
            {
                if (save)
                {
                    Properties.Settings.Default.DataFilePath = DataFilePath.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void LoadDataClick(object sender, RoutedEventArgs e)
            => LoadDataFile(true);

        private void ShowInExplorerClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DataFilePath.Text))
            {
                var dn = Path.GetDirectoryName(DataFilePath.Text);
                if (Directory.Exists(dn))
                {
                    Process.Start("explorer.exe", dn);
                    return;
                }
            }
            MessageBox.Show("Error: Unable to locate the file.", Title);
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
                LoadDataFile(true);
            }
        }

        private IEnumerable<string> RecordsToStrings(IList<ClientRecord> clients)
        {
            foreach (var c in clients)
            {
                yield return $"{FormCommaSeparateName(c.FirstName, c.Surname)} (Medicare#{c.MedicareNumber}, Phone#{c.PhoneNumber})";
            }
        }

        private void SearchByName(string name)
        {
            _suppressSearch.Run(() =>
            {
                var clients = _clients.FindNameContaining(name).OrderBy(x => x.MedicareNumber).ToList();
                if (clients.Count == 0)
                {
                    MessageBox.Show("Error: Client not found.", Title);
                }
                else
                {
                    ClientRecord client = null;
                    if (clients.Count > 1)
                    {
                        var dc = new DuplicateClients($"Multiple clients found with name containing '{name}'", RecordsToStrings(clients))
                        {
                            Owner = this,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        if (dc.ShowDialog() == true && dc.SelectedIndex >= 0)
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
                    ClientNumber.Text = client.PhoneNumber;
                }
            });
        }

        private void SearchByMedi(string medi)
        {
            _suppressSearch.Run(() =>
            {
                var clients = _clients.FindByMedicareNumberContaining(medi)
                    .OrderBy(x => x.MedicareNumber).ToList();
                if (clients.Count == 0)
                {
                    MessageBox.Show("Error: Client not found.", Title);
                }
                ClientRecord client = null;
                if (clients.Count > 1)
                {
                    var dc = new DuplicateClients($"Multiple clients found with medicare number containing '{medi}'", RecordsToStrings(clients))
                    {
                        Owner = this,
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    if (dc.ShowDialog() == true && dc.SelectedIndex >= 0)
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
                ClientNumber.Text = client.PhoneNumber;
            });
        }

        private void SearchByPhone(string phone)
        {
            _suppressSearch.Run(() =>
            {
                var clients = _clients.FindByPhoneNumberContaining(phone).OrderBy(x => x.MedicareNumber).ToList();
                if (clients.Count == 0)
                {
                    MessageBox.Show("Error: Client not found.", Title);
                }
                else
                {
                    ClientRecord client;
                    if (clients.Count > 1)
                    {
                        var dc = new DuplicateClients($"Multiple clients found with phone number containing '{phone}'", RecordsToStrings(clients))
                        {
                            Owner = this,
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        };
                        if (dc.ShowDialog() == true && dc.SelectedIndex >= 0)
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
                    ClientNumber.Text = client.PhoneNumber;
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

        private void DataFilePath_Drop(object sender, DragEventArgs e)
        {
            var data = (DataObject)e.Data;
            if (data.ContainsFileDropList())
            {
                string[] rawFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (rawFiles.Length > 0)
                {
                    DataFilePath.Text = rawFiles[0];
                    LoadDataFile(true);
                }
            }
        }

        private void DataFilePath_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void DataFilePath_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void HomePageLinkRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        class StackBottomActioner
        {
            public StackBottomActioner(Action finalAction)
            {
                _finalAction = finalAction;
            }
            public class Guard : IDisposable
            {
                public Guard(StackBottomActioner checker)
                {
                    _actioner = checker;
                    _actioner._stackDepth++;
                }

                public void Dispose()
                {
                    if (_actioner != null)
                    {
                        _actioner._stackDepth--;
                        if (_actioner._stackDepth == 0)
                        {
                            _actioner._finalAction?.Invoke();
                        }
                        _actioner = null;
                    }
                }

                private StackBottomActioner _actioner;
            }
            private int _stackDepth;
            private Action _finalAction;
        }

        ClientRecords _clients = new ClientRecords();
        AutoResetSuppressor _suppressSearch = new AutoResetSuppressor();
        private StackBottomActioner _checkDateActioner;
    }
}
