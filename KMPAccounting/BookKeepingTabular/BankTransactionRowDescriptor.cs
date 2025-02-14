using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    /// <summary>
    ///  Bank transaction row descriptor
    /// </summary>
    /// <remarks>
    ///  Positive amount always represents debit. This is made not configurable to simplify implementation.
    /// </remarks>
    public class BankTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        protected BankTransactionRowDescriptor(string dateTimeKey, string amountKey, string counterAccountKey, List<string> keys) : base(dateTimeKey, amountKey, keys)
        {
            CounterAccountKey = counterAccountKey;
        }

        // The column that indicate the account that is paired with the bank account in the trasaction
        public string CounterAccountKey { get; }

        public string? BalanceKey { get; set; }
    }
}
