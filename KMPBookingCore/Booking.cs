using System;

namespace KMPBookingCore
{
    public class Booking
    {
        public int Id;
        public ClientRecord Client;
        public DateTime? DateTime;
        public TimeSpan Duration;
    }
}
