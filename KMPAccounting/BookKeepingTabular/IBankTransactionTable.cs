namespace KMPAccounting.BookKeepingTabular
{
    public interface IBankTransactionTable : ITransactionTable
    {
        public string BaseAccountName { get; set; }

        public string CounterAccountPrefix { get; set; }
    }
}
