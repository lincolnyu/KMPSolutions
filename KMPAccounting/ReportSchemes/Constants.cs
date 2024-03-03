namespace KMPAccounting.ReportSchemes
{
    public static class Constants
    {
        public const string Income = "Equity.Income";               // Credit
        public const string Deduction = "Equity.Deduction";         // Debit
        public const string Expense = "Equity.Expense";             // Debit

        public const string TaxWithheld = "Liability.TaxWithheld";  // Debit
        public const string TaxReturn = "Liability.TaxReturn";      // Credit

        public static readonly string EquityMain = $"Equity.{Objects.Constants.MainNodeName}";
    }
}
