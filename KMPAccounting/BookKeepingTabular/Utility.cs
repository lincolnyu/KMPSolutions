using System.Collections.Generic;
using System.Linq;

namespace KMPAccounting.BookKeepingTabular
{
    public static class Utility
    {
        public static decimal GetDecimalValue(this ITransactionRow row, string key)
        {
            return decimal.Parse(row[key]);
        }

        public static IEnumerable<TTransactionRow> ResetIndex<TTransactionRow>(this IEnumerable<TTransactionRow> rows) where TTransactionRow: ITransactionRow
        {
            var index = 0;
            return rows.Select(x => { x.Index = index++; return x; });
        }
    }
}
