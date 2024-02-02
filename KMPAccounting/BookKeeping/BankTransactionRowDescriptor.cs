using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public class BankTransactionRowDescriptor
    {
        protected BankTransactionRowDescriptor(string dateTimeColumnKey, string amountColumnKey, string baseAccountKey, string counterAccountKeyName, List<string> keys)
        {
            DateTimeKey = dateTimeColumnKey;
            AccountKey = amountColumnKey;
            BaseAccountKey = baseAccountKey;
            CounterAccountKey = counterAccountKeyName;
            Keys  = keys;
        }

        public string DateTimeKey { get; set; }
        public string AccountKey { get; set; }
        public string BaseAccountKey { get; set; }
        public string CounterAccountKey { get; set; }
        public string? BalanceColumnName { get; set; }

        public bool PositiveAmountForCredit { get; set; } = true;

        public List<string> Keys { get; }
    }
}
