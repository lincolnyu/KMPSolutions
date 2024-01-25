namespace KMPAccounting.Objects.Reports
{
    public class PnlReport
    {
        public decimal Income { get; set; }
        public decimal Deduction { get; set; }

        public decimal TaxableIncome => Income - Deduction;

        public decimal TaxWithheld { get; set; }

        public decimal Tax { get; set; }

        public decimal TaxReturn => TaxWithheld - Tax; // Negative for tax payable

        public decimal PostTaxIncome => TaxableIncome - Tax;
    }
}
