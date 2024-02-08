namespace KMPAccounting.BookKeeping
{
    public class BankTransactionTableDescriptor
    {
        public BankTransactionTableDescriptor(string tableName, BankTransactionRowDescriptor rowDescriptor)
        {
            TableName = tableName;
            RowDescriptor = rowDescriptor;
        }

        public string TableName { get; }

        public BankTransactionRowDescriptor RowDescriptor { get; }

        public string BaseAccountName { get; set; } = "";
        public string CounterAccountPrefix { get; set; } = "";
    }

    public class BankTransactionTableDescriptor<TRowDescriptor> : BankTransactionTableDescriptor where TRowDescriptor : BankTransactionRowDescriptor, new()
    {
        public BankTransactionTableDescriptor(string tableName) : base(tableName, new TRowDescriptor())
        {
        }
    }
}
