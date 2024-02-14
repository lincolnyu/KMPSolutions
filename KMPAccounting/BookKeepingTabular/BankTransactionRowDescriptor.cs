using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    public class BankTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        protected BankTransactionRowDescriptor(string dateTimeKey, string amountKey, string counterAccountKey, List<string> keys) : base(dateTimeKey, amountKey, keys)
        {
            CounterAccountKey = counterAccountKey;
        }

        // The column that indicate the account that is paired with the bank account in the trasaction
        public string CounterAccountKey { get; }

        public string? BalanceKey { get; set; }

        // Whether amount being positive means to debit the bank account.
        // NOTE: If, for example, the bank account is a loan account (liability), debiting the account is to reduce the loan debt. So if a negative amount is to reduce the loan, then 'PositiveAmountForDebit' needs to be set to false.
        public bool PositiveAmountForDebit { get; set; } = true;
    }
}
