using System;

namespace KMPBookingCore
{
    public class Booking
    {
        public int Id;
        public Client Client;
        public DateTime? DateTime;
        public TimeSpan Duration;
    }
}
