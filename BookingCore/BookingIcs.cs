using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Reflection;
using System.Text;
using static BookingCore.DateTimeUtils;

namespace BookingCore
{
    public static class BookingIcs
    {
        public enum BookingStatus
        {
            Pending,
            Confirmed,
            Cancelled,
            Rescheduled, // Not used at the moment
            Completed
        }

        const string UID = "BOCAIJIA@HOTMAIL.COM";

        public static string GenerateIcs(string uid, DateTime creationTime, 
            DateTime startTime, DateTime endTime, string summary, string description,
            string location)
        {
            var sb = new StringBuilder();
            sb.AppendLine("BEGIN:VCALENDAR");
            sb.AppendLine("VERSION:2.0");
            sb.AppendLine("PRODID:-//hacksw/handcal//NONSGML v1.0//EN");
            //sb.AppendLine("CALSCALE:GREGORIAN");
            sb.AppendLine("BEGIN:VEVENT");
            sb.AppendLine($"UID:{uid}");
            sb.AppendLine($"DTSTAMP:{DateTimeToStr(creationTime)}");   // Event (calendar item) creation time
            sb.AppendLine($"DTSTART:{DateTimeToStr(startTime)}");       // Event start time
            sb.AppendLine($"DTEND:{DateTimeToStr(endTime)}");           // Event end time
            sb.AppendLine($"SUMMARY:{summary}");            // Title
            sb.AppendLine($"DESCRIPTION:{description}");    // Content
            sb.AppendLine($"LOCATION:{location}");
            sb.AppendLine("END:VEVENT");
            sb.AppendLine("END:VCALENDAR");
            return sb.ToString();
        }

        public static string GenerateBookingIcs(string clientName, string clientPhoneNumber,
           DateTime bookingTime, TimeSpan duration, BookingStatus status = BookingStatus.Pending)
            => GenerateIcs(UID,
                DateTime.Now,
                bookingTime,
                bookingTime + duration,
                $"KMPBooking [{StatusToCompactStr(status)}] - {ClientNameToCompactStr(clientName)} ({clientPhoneNumber})",
                "",
                "Unit 12 66-80 Totterdell Street Belconnen ACT 2617");

        public static string GenerateSmsIcs(string clientName, string clientPhoneNumber,
            DateTime bookingTime, TimeSpan duration, DateTime smsReminderTime)
        {
            var sms = GenerateSms(clientName, clientPhoneNumber, bookingTime, smsReminderTime);
            return GenerateIcs(UID, DateTime.Now, smsReminderTime, smsReminderTime,
                $"KMPSMS - {ClientNameToCompactStr(clientName)} ({clientPhoneNumber})",
                sms, "");
        }

        public static void LaunchIcs(string ics)
        {
            using (var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User 
                | IsolatedStorageScope.Domain | IsolatedStorageScope.Assembly, null, null))
            {
                var isf = isoStore.CreateFile("temp.ics");
                using (var sw = new StreamWriter(isf))
                {
                    sw.Write(ics);
                }
                var path = isf.GetType().GetField("m_FullPath",
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(isf).ToString();
                System.Diagnostics.Process.Start(path);
            }
        }

        static string GenerateSms(string clientName, string clientPhoneNumber,
            DateTime bookingTime, DateTime? smsReminderTime)
        {
            string daydesc = null;
            bool hasdow = false;
            if (smsReminderTime.HasValue)
            {;
                (daydesc, hasdow) = GetDayDescription(smsReminderTime.Value, bookingTime);
            }

            var sb = new StringBuilder();
            sb.Append($"Dear {GetFirstName(clientName)}\\, ");
            sb.Append("This is just a reminder of your appointment with Kinetic Mobile Physio");
            if (daydesc != null)
            {
                sb.Append(" ");
                sb.Append(daydesc);
                sb.Append(" (");
                if (!hasdow)
                {
                    sb.Append($"{bookingTime.DayOfWeek} ");
                }
                sb.Append($"{bookingTime.ToShortDateString()})");
            }
            else
            {
                sb.Append($" on {bookingTime.DayOfWeek} {bookingTime.ToShortDateString()}");
            }
            sb.Append($" at {bookingTime.ToShortTimeString()}.");
            sb.Append(" Please reply Y to confirm or call 0400693696 for rescheduling or other queries.");
            return sb.ToString();
        }

        static string DateTimeToStr(DateTime dt)
            => string.Format("{0:D4}{1:D2}{2:D2}T{3:D2}{4:D2}{5:D2}",
                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);

        static string StatusToCompactStr(BookingStatus status)
            => status.ToString();

        static string GetLastName(string name)
            => name.Trim().Split(',').Where(x => x.Length > 0).First().Trim();

        static string GetFirstName(string name)
            => name.Trim().Split(',').Where(x => x.Length > 0).Last().Trim();

        static string ClientNameToCompactStr(string clientName)
        {
            var segs = clientName.Trim().Split(' ').Where(x => x.Length > 0);
            var sb = new StringBuilder();
            string proc(string input) => input[0] + input.Substring(1).ToLower();
            foreach (var seg in segs)
            {
                sb.Append(proc(seg));
                sb.Append(' ');
            }
            sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }
    }
}
