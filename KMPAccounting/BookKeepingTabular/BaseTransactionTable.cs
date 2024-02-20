namespace KMPAccounting.BookKeepingTabular
{
    public class BaseTransactionTable
        <TTransactionRowDescriptor> : ITransactionTable where TTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        public enum HeaderType
        {
            Absent,
            Present,
            AutoDetect
        }

        public BaseTransactionTable(string tableName, TTransactionRowDescriptor rowDescriptor) 
        {
            TableName = tableName;
            RowDescriptor = rowDescriptor;
        }

        public string TableName { get; }

        public TTransactionRowDescriptor RowDescriptor { get; }

        public HeaderType Header { get; set; } = HeaderType.AutoDetect;

        ITransactionRowDescriptor ITransactionTable.RowDescriptor => RowDescriptor;
    }
}
