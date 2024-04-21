namespace KMPBusinessRelationship.Objects
{
    public class Invoice : Event
    {
        public Client Client { get; set; }

        public string Diagnosis { get; set; } = "";
        public string ClaimNumber { get; set; } = "";
        public string HealthFund { get; set; } = "";
        public string MembershipNumber { get; set; } = "";
        public decimal TotalDue { get; set; }
        public decimal PaymentReceived { get; set; }
        public double Discount { get; set; }
        public decimal Balance { get; set; }
    }
}