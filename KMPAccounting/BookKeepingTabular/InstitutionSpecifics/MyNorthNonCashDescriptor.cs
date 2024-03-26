namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class MyNorthNonCashDescriptor : SuperRowDescriptor
    {
        /// <summary>
        ///  Constructor. Using precise column names have the benefits such as being able to detect header.
        /// </summary>
        public MyNorthNonCashDescriptor() : base("Transaction Date", "Amount ($)", new[] { "Transaction Date", "Settlement Date", "Transaction Type", "Investment", "Description", "Quantity", "Unit Price", "Amount ($)" })
        {
        }
    }
}
