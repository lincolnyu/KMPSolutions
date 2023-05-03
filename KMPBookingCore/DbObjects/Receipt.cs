using KMPBookingCore.Database;

namespace KMPBookingCore.DbObjects
{
    [DBClass]
    public class Receipt : Event
    {
        private string _diagnosis;
        private string _claimNumber;
        private string _healthFund;
        private string _membershipNumber;
        private decimal _totalDue;
        private decimal _paymentReceived;
        private double _discount;
        private decimal _balance;

        public Receipt()
        {
            Type = "Receipt";
        }

        [DBField]
        public string Diagnosis
        {
            get => _diagnosis; set
            {
                _diagnosis = value;
                RaiseEventChanged(nameof(Diagnosis));
            }
        }

        [DBField]
        public string ClaimNumber
        {
            get => _claimNumber; set
            {
                _claimNumber = value;
                RaiseEventChanged(nameof(ClaimNumber));
            }
        }

        [DBField]
        public string HealthFund
        {
            get => _healthFund; set
            {
                _healthFund = value;
                RaiseEventChanged(nameof(HealthFund));
            }
        }

        [DBField]
        public string MembershipNumber
        {
            get => _membershipNumber; set
            {
                _membershipNumber = value;
                RaiseEventChanged(nameof(MembershipNumber));
            }
        }

        [DBField]
        public decimal TotalDue
        {
            get => _totalDue; set
            {
                _totalDue = value;
                RaiseEventChanged(nameof(TotalDue));
            }
        }

        [DBField]
        public decimal PaymentReceived
        {
            get => _paymentReceived; set
            {
                _paymentReceived = value;
                RaiseEventChanged(nameof(PaymentReceived));
            }
        }

        [DBField]
        public double Discount
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
            Balance = (decimal)((double)(TotalDue - PaymentReceived) * (1 - Discount));
        }
    }
}
