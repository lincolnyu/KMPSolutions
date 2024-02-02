using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public abstract class BaseCsvReader
    {
        public abstract IEnumerable<BankTransactionRow> GetRows();
    }
}
