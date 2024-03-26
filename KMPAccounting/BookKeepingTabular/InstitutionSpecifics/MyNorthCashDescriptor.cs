namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class MyNorthCashDescriptor : SuperRowDescriptor
    {
        /// <summary>
        ///  Constructor. Using precise column names have the benefits such as being able to detect header.
        /// </summary>
        public MyNorthCashDescriptor() : base(Constants.DateTimeKey, "Amount ($)", new[] { Constants.DateTimeKey, "Transaction Type", "Description", "Amount ($)", "Running Balance ($)" })
        {
        }
    }
}
