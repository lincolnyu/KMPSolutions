namespace KMPAccounting.BookKeepingTabular
{
    public class BaseTransactionTableDescriptor<TTransactionRowDescriptor> : ITransactionTableDescriptor where TTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        public enum HeaderType
        {
            Absent,
            Present,
            AutoDetect
        }

        public BaseTransactionTableDescriptor(string tableName, TTransactionRowDescriptor rowDescriptor) 
        {
            TableName = tableName;
            RowDescriptor = rowDescriptor;
        }

        public string TableName { get; }

        public TTransactionRowDescriptor RowDescriptor { get; }

        public HeaderType Header { get; set; } = HeaderType.AutoDetect;

        ITransactionRowDescriptor ITransactionTableDescriptor.RowDescriptor => RowDescriptor;
    }
}
