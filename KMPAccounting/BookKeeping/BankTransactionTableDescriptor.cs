using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public class BankTransactionTableDescriptor
    {
        public BankTransactionTableDescriptor(string tableName, List<BankTransactionRowDescriptor> rowDescriptors)
        {
            TableName = tableName;
            RowDescriptors = rowDescriptors;
        }

        public string TableName { get; }

        // Each row of the table may produce multiple transaction row entries
        public List<BankTransactionRowDescriptor> RowDescriptors { get; }

        public string BaseAccountPrefix { get; set; } = "";
        public string CounterAccountPrefix { get; set; } = "";
    }
}
