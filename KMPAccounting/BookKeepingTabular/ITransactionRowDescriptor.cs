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
        // They need to be unique because all these values will only be identified by their keys in the TransactionRow.KeyValueMap if these values need to be accessed. If they are not required and dummy keys (Constants.DummyKey highly recommended as this may be an agreed key name for keys to be ignored) are provided then the values won't be retrievable where duplicate keys are used and the storage of these values is undefined.
        public IList<string> Keys { get; }

        public DateTime GetDateTime(ITransactionRow row);
    }
}
