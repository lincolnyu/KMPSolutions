using System;

namespace KMPBookingCore
{
    public class Service
    {
        public int Id;
        public Receipt Receipt;
        public DateTime? Date;
        public string Detail;
        public decimal Total;
        public decimal Owing;
        public decimal Benefit;
        public decimal Gap;
        public decimal Discount;
        public decimal Balance;
        public Booking Booking;

        public void Workout()
        {
            Gap = Owing - Benefit;
            Balance = Gap * (1 - Discount / 100);
        }
    }
}
