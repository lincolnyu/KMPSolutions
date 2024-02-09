using System.Collections.Generic;
using System.Diagnostics;

namespace KMPAccounting.BookKeeping
{
    public class BaseTransactionRowDescriptor
    {
        protected BaseTransactionRowDescriptor(string dateTimeKey, string amountKey , List<string> keys)
        {
            DateTimeKey = dateTimeKey;
            AmountKey = amountKey;
            Keys = keys;
        }

        public string DateTimeKey { get; set; }

        // The key for the amount value (balance change)
        public string AmountKey { get; set; }

        // The keys for the columns in the table starting at index 0 (the first column).
        // So this should at minimum include up to the last column the actual TransactionRow cares about.
        // Can use dummy key names for those columns that it doesn't care about.
        public List<string> Keys { get; }

        public virtual bool EstimateHasHeader(IList<string> loadedFieldsOfFirstRow)
        {
            var amountColumnIndex = Keys.IndexOf(AmountKey);
            Debug.Assert(amountColumnIndex != -1);
            var amountColumeValue = loadedFieldsOfFirstRow[amountColumnIndex].Trim();
            return !decimal.TryParse(amountColumeValue, out _);
        }
    }
}
