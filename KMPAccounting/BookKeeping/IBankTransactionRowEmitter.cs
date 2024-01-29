using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public interface IBankTransactionRowEmitter
    {
        IEnumerable<BankTransactionRow> Emit();
    }
}
