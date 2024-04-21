namespace KMPBusinessRelationship.Objects
{
    public class ChargedService : Service
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
    }
}
