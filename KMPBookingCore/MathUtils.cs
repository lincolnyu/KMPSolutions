using System.Text;

namespace KMPBookingCore
{
    public static class MathUtils
    {
        public static string ToDecPlaces(this decimal val, int decplaces = 2)
        {
            var decstr = new StringBuilder();
            if (decplaces > 0)
            {
                decstr.Append('.');
                for (var i = 0; i < decplaces; i++)
                {
                    decstr.Append('0');
                }
            }
            var fmt = "{0:0" + decstr.ToString() + "}";
            return string.Format(fmt, val);
        }

        public static decimal GetDecimalOrZero(this string s)
        {
            if (decimal.TryParse(s, out var res))
            {
                return res;
            }
            return 0;
        }
    }
}
