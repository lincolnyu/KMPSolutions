namespace KMPAccounting.BookKeeping
{
    public class BankTransactionRowDescriptor
    {
        protected BankTransactionRowDescriptor(string dateTimeColumnName, string amountColumnName, string baseAccountColumnName, string counterAccountColumnName)
        {
            DateTimeColumnName = dateTimeColumnName;
            AmountColumnName = amountColumnName;
            BaseAccountColumnName = baseAccountColumnName;
            CounterAccountColumnName = counterAccountColumnName;
        }

        public string DateTimeColumnName { get; set; }
        public string AmountColumnName { get; set; }
        public string BaseAccountColumnName { get; set; }
        public string CounterAccountColumnName { get; set; }
        public string? BalanceColumnName { get; set; }

        public bool PositiveAmountForCredit { get; set; } = true;
    }
}
