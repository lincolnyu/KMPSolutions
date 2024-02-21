namespace KMPAccounting.BookKeepingTabular
{
    public class BankTransactionTable<TRowDescriptor> : TransactionTable<TRowDescriptor>, IBankTransactionTable where TRowDescriptor : BankTransactionRowDescriptor, new()
    {
        public BankTransactionTable(string tableName) : base(tableName, new TRowDescriptor())
        {
        }

        public BankTransactionTable(string tableName, TRowDescriptor rowDescriptor) : base(tableName, rowDescriptor)
        {
        }

        public string BaseAccountName { get; set; } = "";

        public string CounterAccountPrefix { get; set; } = "";
    }
}
