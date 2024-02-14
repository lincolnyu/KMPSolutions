namespace KMPAccounting.BookKeepingTabular
{
    public class BankTransactionTableDescriptor<TRowDescriptor>: BaseTransactionTableDescriptor<TRowDescriptor> where TRowDescriptor : BankTransactionRowDescriptor, new()
    {
        public BankTransactionTableDescriptor(string tableName) : base(tableName, new TRowDescriptor())
        {
        }

        public string BaseAccountName { get; set; } = "";

        public string CounterAccountPrefix { get; set; } = "";
    }
}
