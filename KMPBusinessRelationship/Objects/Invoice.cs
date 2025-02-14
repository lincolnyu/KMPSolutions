using System;

namespace KMPBusinessRelationship.Objects
{
    public class Invoice : Event<Invoice>
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

        public override bool Equals(Invoice other)
        {
            if (Client.Id != other.Client.Id) return false;
            if (Diagnosis != other.Diagnosis) return false;
            if (ClaimNumber != other.ClaimNumber) return false;
            if (HealthFund != other.HealthFund) return false;
            if (MembershipNumber != other.MembershipNumber) return false;
            if (TotalDue != other.TotalDue) return false;
            if (PaymentReceived != other.PaymentReceived) return false;
            if (Discount != other.Discount) return false;
            if (Balance != other.Balance) return false;
            return true;
        }
    }
}