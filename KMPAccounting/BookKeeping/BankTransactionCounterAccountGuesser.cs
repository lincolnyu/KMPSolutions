using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public abstract class BankTransactionCounterAccountGuesser
    {
        public abstract IEnumerable<BankTransactionRow> Guess(IEnumerable<BankTransactionRow> rows);
    }
}
