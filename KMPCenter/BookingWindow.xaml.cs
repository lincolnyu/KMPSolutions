using KMPBookingCore;
using KMPBookingCore.UserInterface;
using static KMPBookingCore.BookingIcs;
using static KMPBookingCore.DateTimeUtils;
using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

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
            Clients.SetDataConnection(MainWindow.Connection);
        }

        private void CheckDateAndWarn()
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
            Book();
            if (SetSmsReminder.IsChecked == true)
            {
                Thread.Sleep(1000);
                SmsReminder();
            }
            if (AddToDB.IsChecked == true)
            {
                AddBookingToDB();
            }
        }

        private void AddBookingToDB()
        {
            //TODO implement it...
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
                AddValidationMessage(sb, ref r, CheckResult.Error, "No client");
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
