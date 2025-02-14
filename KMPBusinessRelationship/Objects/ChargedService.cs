namespace KMPBusinessRelationship.Objects
{
    public class ChargedService : Service<ChargedService>
    {
        public Invoice? Invoice { get; set; }

        public string Details { get; set; } = "";

        public decimal TotalFee { get; set; }
        public decimal Owing { get; set; }
        public decimal Benefit { get; set; }
        public decimal Gap { get; set; }
        public decimal Discount { get; set; }
        public decimal Balance { get; set; }

        public void Calculate()
        {
            Gap = Owing - Benefit;
            Balance = Gap * (1 - Discount / 100);
        }

        public override bool Equals(ChargedService other)
        {
            if (!base.Equals(other)) return false;
            if (Invoice?.Id != other.Invoice?.Id) return false;
            if (Details != other.Details) return false;
            if (TotalFee != other.TotalFee) return false;
            if (Owing != other.Owing) return false;
            if (Benefit != other.Benefit) return false;
            if (Gap != other.Gap) return false;
            if (Discount != other.Discount) return false;
            if (Balance != other.Balance) return false;
            return true;
        }
    }
}
