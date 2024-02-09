namespace KMPAccounting.BookKeeping
{
    public class BaseTransactionTableDescriptor<TTransactionRowDescriptor> where TTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        public enum HeaderType
        {
            Absent,
            Present,
            AutoDetect
        }

        protected BaseTransactionTableDescriptor(string tableName, TTransactionRowDescriptor rowDescriptor) 
        {
            TableName = tableName;
            RowDescriptor = rowDescriptor;
        }

        public string TableName { get; }

        public TTransactionRowDescriptor RowDescriptor { get; }

        public HeaderType Header { get; set; } = HeaderType.AutoDetect;
    }
}
