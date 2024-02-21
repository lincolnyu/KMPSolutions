﻿namespace KMPAccounting.BookKeepingTabular
{
    public class BankTransactionTable<TRowDescriptor>: TransactionTable<TRowDescriptor> where TRowDescriptor : BankTransactionRowDescriptor, new()
    {
        public BankTransactionTable(string tableName) : base(tableName, new TRowDescriptor())
        {
        }

        public string BaseAccountName { get; set; } = "";

        public string CounterAccountPrefix { get; set; } = "";
    }
}
