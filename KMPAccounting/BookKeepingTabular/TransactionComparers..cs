using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KMPAccounting.BookKeepingTabular
{
    public static class TransactionComparers
    {
        public class IndexSecond : IComparer<ITransactionRow>
        {
            public int Compare(ITransactionRow x, ITransactionRow y)
            {
                Debug.Assert(x.OwnerTable == y.OwnerTable);
                var c = x.DateTime.CompareTo(y.DateTime);
                if (c != 0) return c;

                return x.Index.CompareTo(y.Index);
            }

            public static IndexSecond Instance { get; } = new IndexSecond();
        }

        public class RegardlessOfIndex : IComparer<ITransactionRow>
        {
            public int Compare(ITransactionRow x, ITransactionRow y)
            {
                var c = x.DateTime.CompareTo(y.DateTime);
                if (c != 0) return c;

                var amountX = x.GetDecimalValue(x.OwnerTable.RowDescriptor.AmountKey);
                var amountY = y.GetDecimalValue(y.OwnerTable.RowDescriptor.AmountKey);

                return -Math.Abs(amountX).CompareTo(Math.Abs(amountY));   // Larger amount first
            }

            public static RegardlessOfIndex Instance { get; } = new RegardlessOfIndex();
        }
    }   
}
