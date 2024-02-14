using System;
using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    public interface ITransactionRowDescriptor
    {
        public string DateTimeKey { get; }

        // The key for the amount value (balance change)
        public string AmountKey { get; }

        // The keys for the columns in the table starting at index 0 (the first column).
        // So this should at minimum include up to the last column the actual TransactionRow cares about.
        // Can use any unique key names for those columns that it doesn't care about.
        // They need to be unique because all these values will only be identified by the key in the KeyValueMap of TransactionRow.
        public IList<string> Keys { get; }

        public DateTime GetDateTime(ITransactionRow row);
    }
}
