using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace Booking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        enum BookingStatus
        {
            Pending,
            Confirmed,
            Cancelled,
            Rescheduled, // Not used at the moment
            Completed
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        void AddSms(StringBuilder sb, string clientName, string clientPhoneNumber, 
            DateTime bookingTime)
        {
            sb.Append("---- SMS ----\\n");
            sb.Append($"Dear {GetLastName(clientName)}\\, ");
            sb.Append("this is just a reminder of your appointment with Kinetic Mobile Physio at ");
            sb.Append($"{bookingTime.ToShortTimeString()} {bookingTime.ToShortDateString()}. ");
            sb.Append("To confirm please reply Y. To cancel or reschedule call 0400693696.");
        }

        void SetContent(StringBuilder sb, string clientName, string clientPhoneNumber,
            DateTime bookingTime)
        {
            AddSms(sb, clientName, clientPhoneNumber, bookingTime);
        }

        static string DateTimeToStr(DateTime dt)
            => string.Format("{0:D4}{1:D2}{2:D2}T{3:D2}{4:D2}{5:D2}",
                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

        static string StatusToCompactStr(BookingStatus status)
            => status.ToString();

        static string GetLastName(string name)
            => name.Trim().Split(' ').Where(x => x.Length > 0).Last();

        static string ClientNameToCompactStr(string clientName)
        {
            var segs = clientName.Trim().Split(' ').Where(x=>x.Length > 0);
            var sb = new StringBuilder();
            string proc(string input) => input[0] + input.Substring(1).ToLower();
            foreach (var seg in segs)
            {
                sb.Append(proc(seg));
            }
            return sb.ToString();
        }

        string GenerateBookingIcs(string clientName, string clientPhoneNumber, 
            DateTime bookingTime, TimeSpan duration, BookingStatus status = BookingStatus.Pending)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//hacksw/handcal//NONSGML v1.0//EN");
            //sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine("UID:bocaijia@hotmail.com");
            sb.AppendLine($"DTSTAMP:{DateTimeToStr(DateTime.Now)}");    // Creation time
            sb.AppendLine($"DTSTART:{DateTimeToStr(bookingTime)}");     // Event start time
            sb.AppendLine($"DTEND:{DateTimeToStr(bookingTime + duration)}"); // Event end time
            sb.AppendLine($"SUMMARY:KMPBooking{StatusToCompactStr(status)} - Client{ClientNameToCompactStr(clientName)} - Phone{clientPhoneNumber}");
            // TODO implement it...
            sb.Append("DESCRIPTION:");
            SetContent(sb, clientName, clientPhoneNumber, bookingTime);
            sb.AppendLine("LOCATION:Unit 12 66-80 Totterdell Street Belconnen ACT 2617");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }

        void LaunchBookingIcs(string ics)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                
                var isf = isoStore.CreateFile("temp.ics");
                using (var sw = new StreamWriter(isf))
                {
                    sw.Write(ics);
                }
                var path = isf.GetType().GetField("m_FullPath",
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(isf).ToString();
                File.Copy(path, @"D:\temp\out.ics", true);
                System.Diagnostics.Process.Start(path);
            }
        }

        private void BookClicked(object sender, RoutedEventArgs e)
        {
            if (BookingDate.SelectedDate.HasValue)
            {
                var bookingDate = BookingDate.SelectedDate.Value.Date;
                var bookingTime = DateTime.Parse(BookingTime.Text);
                var bookingDateTime = new DateTime(bookingDate.Year, bookingDate.Month, bookingDate.Day,
                    bookingTime.Hour, bookingTime.Minute, bookingTime.Second);
                var duration = TimeSpan.FromMinutes(30);
                if (int.TryParse(Duration.Text, out var dur))
                {
                    duration = TimeSpan.FromMinutes(dur);
                }
                var ics = GenerateBookingIcs(ClientName.Text, 
                    ClientNumber.Text, bookingDateTime, duration);
                if (ics != null)
                {
                    LaunchBookingIcs(ics);
                }
            }
            else
            {

            }
        }
    }
}
