using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public interface IBankTransactionRowEmitter<TTransactionRowDescriptor> where TTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        IEnumerable<TransactionRow<TTransactionRowDescriptor>> Emit();
    }
}
