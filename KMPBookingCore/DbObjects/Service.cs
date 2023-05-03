using System.Collections.Generic;
using KMPBookingCore.Database;

namespace KMPBookingCore.DbObjects
{
    [DBClass]
    public class Service : Event
    {
        private string _serviceContent;
        private Receipt _receipt;
        private string _detail;
        private decimal _totalFee;
        private decimal _owing;
        private decimal _benefit;
        private decimal _gap;
        private decimal _discount;
        private decimal _balance;
        private Booking _booking;

        public Service()
        {
            Type = "Service";
        }

        [DBField]
        public string ServiceContent
        {
            get => _serviceContent; set
            {
                _serviceContent = value;
                RaiseEventChanged(nameof(ServiceContent));
            }
        }
        [DBField]
        public Receipt Receipt
        {
            get => _receipt; set
            {
                _receipt = value;
                RaiseEventChanged(nameof(Receipt));
            }
        }
        [DBField]
        public Booking Booking
        {
            get => _booking; set
            {
                _booking = value;
                RaiseEventChanged(nameof(Booking));
            }
        }
        [DBField]
        public decimal TotalFee
        {
            get => _totalFee; set
            {
                _totalFee = value;
                RaiseEventChanged(nameof(TotalFee));
            }
        }
        [DBField]
        public decimal Owing
        {
            get => _owing; set
            {
                _owing = value;
                RaiseEventChanged(nameof(Owing));
            }
        }
        [DBField]
        public decimal Benefit
        {
            get => _benefit; set
            {
                _benefit = value;
                RaiseEventChanged(nameof(Benefit));
            }
        }
        [DBField]
        public decimal Gap
        {
            get => _gap; set
            {
                _gap = value;
                RaiseEventChanged(nameof(Gap));
            }
        }
        [DBField]
        public decimal Discount
        {
            get => _discount; set
            {
                _discount = value;
                RaiseEventChanged(nameof(Discount));
            }
        }
        public decimal Balance
        {
            get => _balance; set
            {
                _balance = value;
                RaiseEventChanged(nameof(Balance));
            }
        }
      
        public void Calculate()
        {
            Gap = Owing - Benefit;
            Balance = Gap * (1 - Discount / 100);
        }
}
}
