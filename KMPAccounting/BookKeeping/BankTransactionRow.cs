using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public abstract class BankTransactionRow
    {
        public abstract IEnumerable<(string, string)> GetColumnAndValuePairs();
        public abstract string GetValue(string columnName);

        public abstract BankTransactionTableDescriptor OwnerTable { get; }

        public int? OriginalRowNumber { get; }
    }
}
