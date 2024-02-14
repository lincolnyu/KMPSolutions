using KMPCommon;
using System;

namespace KMPAccounting.BookKeepingTabular
{
    public static class Utility
    {
        public static decimal GetDecimalValue(this ITransactionRow row, string key)
        {
            return decimal.Parse(row[key]);
        }
    }
}
