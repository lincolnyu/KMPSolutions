using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public abstract class CsvReader
    {
        public abstract IEnumerable<BankTransactionRow> GetRows();
    }
}
