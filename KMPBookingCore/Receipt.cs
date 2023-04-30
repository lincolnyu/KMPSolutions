namespace KMPBookingCore
{
    public class Receipt : Event
    {
        private string diagnosis;
        private string claimNumber;
        private string healthFund;
        private string membershipNumber;
        private decimal totalDue;
        private decimal paymentReceived;
        private double discount;
        private decimal balance;

        public Receipt()
        {
            Type = "Receipt";
        }

        public string Diagnosis
        {
            get => diagnosis; set
            {
                diagnosis = value;
                RaiseEventChanged(nameof(Diagnosis));
            }
        }
        public string ClaimNumber
        {
            get => claimNumber; set
            {
                claimNumber = value;
                RaiseEventChanged(nameof(ClaimNumber));
            }
        }
        public string HealthFund
        {
            get => healthFund; set
            {
                healthFund = value;
                RaiseEventChanged(nameof(HealthFund));
            }
        }
        public string MembershipNumber
        {
            get => membershipNumber; set
            {
                membershipNumber = value;
                RaiseEventChanged(nameof(MembershipNumber));
            }
        }
        public decimal TotalDue
        {
            get => totalDue; set
            {
                totalDue = value;
                RaiseEventChanged(nameof(TotalDue));
            }
        }
        public decimal PaymentReceived
        {
            get => paymentReceived; set
            {
                paymentReceived = value;
                RaiseEventChanged(nameof(PaymentReceived));
            }
        }
        public double Discount
        {
            get => discount; set
            {
                discount = value;
                RaiseEventChanged(nameof(Discount));
            }
        }
        public decimal Balance
        {
            get => balance; set
            {
                balance = value;
                RaiseEventChanged(nameof(Balance));
            }
        }

        public void Calculate()
        {
            Balance = (decimal)((double)(TotalDue - PaymentReceived) * (1 - Discount));
        }
    }
}
