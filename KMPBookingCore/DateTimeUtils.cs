using System;
using System.Linq;
using System.Text;

namespace KMPBookingCore
{
    public static class DateTimeUtils
    {
        public static DateTime? LaunderTime(string s)
        {
            if (DateTime.TryParse(s, out var dt))
            {
                return dt;
            }
            s = s.Trim().ToUpper();
            var pm = s.Contains("P");
            var segs = s.Split(':', '.', ' ');
            var hrstr = segs.Length > 0 ? PullDigits(segs[0]) : "0";
            var minstr = segs.Length > 1 ? PullDigits(segs[1]) : "0";
            var secstr = segs.Length > 2 ? PullDigits(segs[2]) : "0";
            var rdt = DateTime.Today;
            if (int.TryParse(hrstr, out var hrs))
            {
                if (hrs < 0) hrs = 0;
                else if (hrs > 12) hrs = 12;
                if (pm && hrs < 12)
                {
                    hrs += 12;
                }
                else if (!pm && hrs == 12)
                {
                    hrs = 0;
                }
                rdt = rdt.AddHours(hrs);
            }
            else
            {
                return null;
            }
            if (int.TryParse(minstr, out var mins))
            {
                if (mins < 0) mins = 0;
                else if (mins > 59) mins = 59;
                rdt = rdt.AddMinutes(mins);
            }
            if (int.TryParse(secstr, out var secs))
            {
                if (secs < 0) secs = 0;
                else if (secs > 59) secs = 59;
                rdt = rdt.AddSeconds(secs);
            }
            return rdt;
        }

        public static int DayDiff(DateTime d1, DateTime d2)
        {
            var yr1 = d1.Year;
            var yr2 = d2.Year;
            var doy1 = d1.DayOfYear;
            var doy2 = d2.DayOfYear;
            if (yr1 == yr2)
            {
                return doy2 - doy1;
            }
            var yddiff = new DateTime(yr2, 1, 1) - new DateTime(yr1, 1, 1);
            return yddiff.Days + doy2 - doy1;
        }

        public static (string, bool) GetDayDescription(DateTime cur, DateTime refd)
        {
            var dd = DayDiff(cur, refd);
            bool hasdow = false;
            string daydesc;
            if (dd == 0)
            {
                daydesc = "today";
            }
            else if (dd > 0)
            {
                if (dd == 1)
                {
                    daydesc = "tomorrow";
                }
                else if (dd == 2)
                {
                    daydesc = "the day after tomorrow";
                }
                else if (dd < 7)
                {
                    daydesc = "coming " + cur.DayOfWeek.ToString();
                    hasdow = true;
                }
                else
                {
                    daydesc = $"in {dd} days";
                }
            }
            else
            {
                if (dd == -1)
                {
                    daydesc = "yesterday";
                }
                else if (dd == -2)
                {
                    daydesc = "the day before yesterday";
                }
                else
                {
                    daydesc = $"{-dd} days ago";
                }
            }
            return (daydesc, hasdow);
        }

        public static DateTime CreateDateTime(DateTime date, DateTime time)
            => new DateTime(date.Year, date.Month, date.Day,
               time.Hour, time.Minute, time.Second);


        private static string PullDigits(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s.TakeWhile(x => char.IsDigit(x)))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }

        public static string ToDbDate(this DateTime? date)
        {
            if (date.HasValue)
            {
                //return date.Value.ToString("dd/MM/yyyy");
                return $"\"{date.Value.ToShortDateString()}\"";
            }
            else
            {
                return "NULL";
            }
        }

        public static string ToDbDateTime(this DateTime? date)
        {
            if (date.HasValue)
            {
                return $"\"{date.Value.ToString()}\"";
            }
            else
            {
                return "NULL";
            }
        }
    }
}
