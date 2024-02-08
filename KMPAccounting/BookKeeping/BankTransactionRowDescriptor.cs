using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public class BankTransactionRowDescriptor
    {
        protected BankTransactionRowDescriptor(string dateTimeKey, string amountKey, string counterAccountKey, List<string> keys)
        {
            DateTimeKey = dateTimeKey;
            AmountKey = amountKey;
            CounterAccountKey = counterAccountKey;
            Keys  = keys;
        }

        public string DateTimeKey { get; set; }
        
        // The key for the amount value (balance change)
        public string AmountKey { get; set; }
        
        public string CounterAccountKey { get; set; }

        public string? BalanceColumnName { get; set; }

        // Positive amount is to credit the account.
        // NOTE: If, for example, the bank account is a loan account (liability), crediting the account is to reduce the loan debt. So if a negative amount is to reduce the loan, then 'PositiveAmountForCredit' needs to be set to false.
        public bool PositiveAmountForCredit { get; set; } = true;

        // The keys for the columns in the table starting at index 0 (the first column).
        // So this should at minimum include up to the last column the BankTransactionRow cares about.
        // Can use dummy key names for those columns that it doesn't care about.
        public List<string> Keys { get; }
    }
}
