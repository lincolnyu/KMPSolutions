using System;

namespace KMPBookingCore.Database
{
    public class DbDateOnlyAttribute : Attribute
    {
        public DbDateOnlyAttribute() { }
        public DbDateOnlyAttribute(bool isDateOnly)
        {
            IsDateOnly = isDateOnly;
        }
        public bool IsDateOnly { private set; get; } = true;
    }
}
