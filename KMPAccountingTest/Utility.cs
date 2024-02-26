using KMPAccounting.BookKeepingTabular;
using System.Runtime.CompilerServices;

namespace KMPAccountingTest
{
    public static class Utility
    {
        public static string GetThisFolderPath([CallerFilePath] string path = null)
        {
            return Path.GetDirectoryName(path);
        }

        public static string GetThisFilePath([CallerFilePath] string path = null)
        {
            return path;
        }

        public static IEnumerable<TTransactionRow> AssertChangeToAscendingInDate<TTransactionRow>(this IEnumerable<TTransactionRow> input) where TTransactionRow : ITransactionRow
        {
            DateTime? last = null;
            return input.Reverse().Select(x =>
            {
                if (last != null)
                {
                    Assert.That(last, Is.LessThanOrEqualTo(x.DateTime));
                }
                last = x.DateTime;
                return x;
            }).ResetIndex();
        }

        public static IEnumerable<TTransactionRow> ChangeToAscendingInDate<TTransactionRow>(this IEnumerable<TTransactionRow> input) where TTransactionRow : ITransactionRow
        {
            // Assuming it's in descending order
            return input.Reverse().ResetIndex().OrderBy(x=>x, TransactionComparers.IndexSecond.Instance).ResetIndex();
        }
    }
}
