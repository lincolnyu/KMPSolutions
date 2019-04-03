using System;
using System.Linq;
using System.Text;

namespace BookingCore
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

        private static string PullDigits(string s)
        {
            var sb = new StringBuilder();
            foreach (var c in s.TakeWhile(x => char.IsDigit(x)))
            {
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
