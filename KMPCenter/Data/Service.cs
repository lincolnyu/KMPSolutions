using System;

namespace KMPCenter.Data
{
    public class Service
    {
        public DateTime Date;
        public string Detail;
        public decimal Total;
        public decimal Owing;
        public decimal Benefit;
        public decimal Gap;
        public decimal Discount;
        public decimal Balance;

        internal object ToDecPlaces(int v)
        {
            throw new NotImplementedException();
        }
    }
}
