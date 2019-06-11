using KMPBookingCore;
using KMPBookingCore.UserInterface;
using static KMPBookingCore.BookingIcs;
using static KMPBookingCore.DateTimeUtils;
using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using KMPBookingPlus;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for Booking.xaml
    /// </summary>
    public partial class BookingWindow : Window
    {
        private enum CheckResult
        {
            OK,
            Warning,
            Error
        }

        private enum Tolerance
        {
            Neither,
            AllowBothIncomplete,
            AllowIncompleteSmsDate
        }

        private MainWindow MainWindow => (MainWindow)Owner;
        private StackBottomActioner _checkDateActioner;
        private TimeSpan DEFAULT_EVENT_DURATION = TimeSpan.FromMinutes(30);

        public BookingWindow(MainWindow mw)
        {
            Owner = mw;
            _checkDateActioner = new StackBottomActioner(CheckDateAndWarn);
            InitializeComponent();
            Clients.ErrorReporter = s =>
            {
                App.ShowMessage(s);
            };
            Clients.SetDataConnection(MainWindow.Connection);
        }

        private void CheckDateAndWarn()
        {
            (var msg, var e) = CheckDate(Tolerance.AllowBothIncomplete);
            if (e == CheckResult.Warning)
            {
                App.ShowMessage($"Warning: {msg}");
            }
            else if (e == CheckResult.Error)
            {
                App.ShowMessage($"Error: {msg}");
            }
        }

        private void BookingDateChanged(object sender, SelectionChangedEventArgs e)
        {
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
                UpdateSmsDate();
            }
        }

        private void BookingTimeSelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void DayBeforeChecked(object sender, RoutedEventArgs e)
        {
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
                UpdateSmsDate();
            }
        }

        private void SmsTimeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            using (new StackBottomActioner.Guard(_checkDateActioner))
            {
            }
        }

        private void BookClicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateAndProceed())
            {
                return;
            }
            if (AddToDB.IsChecked == true)
            {
                if (HasClash())
                {
                    return;
                }
                AddBookingToDB();
            }
            Book();
            if (SetSmsReminder.IsChecked == true)
            {
                Thread.Sleep(1000);
                SmsReminder();
            }
        }

        private bool HasClash()
        {
            var (start, dur) = GetBookingTemporalInfo();
            if (!start.HasValue) return false;
            var end = start.Value + (dur.HasValue? dur.Value : TimeSpan.Zero);
            var query = "select Clients.ID, Clients.[Client Name], Bookings.[Booking Date], Bookings.Duration from Bookings inner join Clients on Bookings.[Client ID] = Clients.ID order by Bookings.[Booking Date]";
            var sb = new StringBuilder("Has clash with:\n");
            var hasClash = false;
            using (var r = MainWindow.Connection.RunReaderQuery(query))
            {
                while (r.Read())
                {
                    var cid = r.GetInt32(0).ClientIdToStr();
                    var cname = r.GetString(1);
                    var estart = r.TryGetDateTime(2);
                    var nummins = r.GetInt32(3);
                    var edur = TimeSpan.FromMinutes(nummins);
                    if (estart > end)
                    {
                        break;
                    }

                    var eend = estart + edur;
                    bool clash = false;
                    if (edur > TimeSpan.Zero)
                    {
                        clash = (start > estart && start < eend
                            || end > estart && end < eend
                            || start == estart && end > estart
                            || start < eend && end == eend
                            || start < estart && end > eend);
                    }
                    else
                    {
                        clash = (estart > start && estart < end
                            || estart == start && estart == end);
                    }
                    if (clash)
                    {
                        sb.AppendLine($"#{cid}: {cname} at {estart.ToString()} for {edur.TotalMinutes} mins");
                        hasClash = true;
                    }
                }
            }
            if (hasClash)
            {
                App.ShowMessage(sb.ToString());
            }
            return hasClash;
        }

        private void AddBookingToDB()
        {
            var fields = new StringBuilder();
            var values= new StringBuilder();
            fields.Append("[Client ID], ");
            values.Append($"{Clients.ActiveClient.Id}, ");
            var (bookingTime, dur) = GetBookingTemporalInfo();
            fields.Append("[Booking Date], ");
            values.Append($"{bookingTime.ToDbDateTime()}, ");
            if (dur.HasValue)
            {
                fields.Append($"Duration, ");
                values.Append($"{dur.Value.TotalMinutes}, ");
            }
            fields.Append($"[SMS Date]");
            values.Append($"{GetSmsTime().ToDbDateTime()}");
            var cmd = $"insert into Bookings ({fields}) values ({values})";
            MainWindow.Connection.RunNonQuery(cmd.ToString());
        }

        private void SmsReminder()
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

            var client = Clients.ActiveClient;
            if (client == null)
            {
                App.ShowMessage("Error: No client selected.");
                return;
            }
            var smsics = GenerateSmsIcs(client.ClientFormalName(), client.PhoneNumber, client.MedicareNumber,
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

        private DateTime? GetSmsTime()
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


        private bool ValidateAndProceed()
        {
            var sb = new StringBuilder();
            var sms = SetSmsReminder.IsChecked == true;
            var r = CheckResult.OK;
            var client = Clients.ActiveClient;
            if (client == null)
            {
                App.ShowMessage($"Unable to book. No client");
                return false;
            }

            if (string.IsNullOrWhiteSpace(client.ClientFormalName()))
            {
                AddValidationMessage(sb, ref r, CheckResult.Error, "Missing client name");
            }

            if (string.IsNullOrWhiteSpace(client.MedicareNumber))
            {
                AddValidationMessage(sb, ref r, CheckResult.Warning, "Missing client medicare number");
            }

            (var msg, var rr) = CheckDate(sms ? Tolerance.AllowIncompleteSmsDate : Tolerance.Neither);
            AddValidationMessage(sb, ref r, rr, msg);

            if (SetSmsReminder.IsChecked == true && string.IsNullOrWhiteSpace(client.PhoneNumber)
                && r != CheckResult.Error)
            {
                AddValidationMessage(sb, ref r, CheckResult.Warning, "Phone number not provided, SMS event will be incomplete.");
            }

            if (r == CheckResult.Error)
            {
                App.ShowMessage($"Unable to book. Error details:\n{sb.ToString()}");
                return false;
            }
            else if (r == CheckResult.Warning)
            {
                if (MessageBox.Show($"Warning:\n{sb.ToString()}Are you sure to continue?", MainWindow.Title, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return false;
                }
            }

            return true;
        }

        private static void AddValidationMessage(StringBuilder sb,
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

        private (string, CheckResult) CheckDate(Tolerance tol)
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

        private void Book()
        {
            (var bookingDateTime, var duration) = GetBookingTemporalInfo();
            if (!bookingDateTime.HasValue)
            {
                App.ShowMessage("Error: Bad booking time.");
                return;
            }
            if (!duration.HasValue)
            {
                duration = DEFAULT_EVENT_DURATION;
            }
            var client = Clients.ActiveClient;
            if (client == null)
            {
                App.ShowMessage("Error: No client selected.");
                return;
            }
            var ics = GenerateBookingIcs(client.ClientFormalName(), client.MedicareNumber, client.PhoneNumber,
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
    }
}
