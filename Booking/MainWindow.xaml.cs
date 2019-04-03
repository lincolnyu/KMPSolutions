using System;
using System.Threading;
using System.Windows;
using static BookingCore.BookingIcs;
using static BookingCore.DateTimeUtils;

namespace Booking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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
    }
}
