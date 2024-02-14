namespace KMPAccounting.BookKeepingTabular
{
    public interface ITransactionTableDescriptor
    {
        public ITransactionRowDescriptor RowDescriptor { get; }
    }
}
