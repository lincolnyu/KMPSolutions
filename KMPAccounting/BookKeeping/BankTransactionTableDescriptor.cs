﻿namespace KMPAccounting.BookKeeping
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
    }
}
