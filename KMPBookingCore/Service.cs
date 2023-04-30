namespace KMPBookingCore
{
    public class Service : Event
    {
        private string _serviceTitle;
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

        public string ServiceTitle
        {
            get => _serviceTitle; set
            {
                _serviceTitle = value;
                RaiseEventChanged(nameof(ServiceTitle));
            }
        }
        public Receipt Receipt
        {
            get => _receipt; set
            {
                _receipt = value;
                RaiseEventChanged(nameof(Receipt));
            }
        }
        public string Detail
        {
            get => _detail; set
            {
                _detail = value;
                RaiseEventChanged(nameof(Detail));
            }
        }
        public decimal TotalFee
        {
            get => _totalFee; set
            {
                _totalFee = value;
                RaiseEventChanged(nameof(TotalFee));
            }
        }
        public decimal Owing
        {
            get => _owing; set
            {
                _owing = value;
                RaiseEventChanged(nameof(Owing));
            }
        }
        public decimal Benefit
        {
            get => _benefit; set
            {
                _benefit = value;
                RaiseEventChanged(nameof(Benefit));
            }
        }
        public decimal Gap
        {
            get => _gap; set
            {
                _gap = value;
                RaiseEventChanged(nameof(Gap));
            }
        }
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
        public Booking Booking
        {
            get => _booking; set
            {
                _booking = value;
                RaiseEventChanged(nameof(Booking));
            }
        }

        public void Calculate()
        {
            Gap = Owing - Benefit;
            Balance = Gap * (1 - Discount / 100);
        }
    }
}
