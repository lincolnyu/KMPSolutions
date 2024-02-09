using System.Collections.Generic;
using System.Diagnostics;

namespace KMPAccounting.BookKeeping
{
    public class BankTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        protected BankTransactionRowDescriptor(string dateTimeKey, string amountKey, string counterAccountKey, List<string> keys) : base(dateTimeKey, amountKey, keys)
        {
            DateTimeKey = dateTimeKey;
            CounterAccountKey = counterAccountKey;
        }

        public string CounterAccountKey { get; set; }

        public string? BalanceColumnName { get; set; }

        // Whether amount being positive means to credit the account.
        // NOTE: If, for example, the bank account is a loan account (liability), crediting the account is to reduce the loan debt. So if a negative amount is to reduce the loan, then 'PositiveAmountForCredit' needs to be set to false.
        public bool PositiveAmountForCredit { get; set; } = true;
    }
}
