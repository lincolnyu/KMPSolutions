using KMPAccounting.BookKeepingTabular;
using NUnit.Framework.Constraints;
using System.Runtime.CompilerServices;

namespace KMPAccounting.Test
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

        public static string? CompareTextFiles(string dirSourceFiles, DirectoryInfo dirTargetFiles)
        {
            foreach (var file in dirTargetFiles.GetFiles())
            {
                if (file.Extension == ".bak") continue;
                var fileName = file.Name;
                var contentTarget = file.OpenText().ReadToEnd();
                var sourceFile = Path.Combine(dirSourceFiles, fileName);
                if (!File.Exists(sourceFile))
                {
                    return $"File '{sourceFile}' is expected but does not exist.";
                }
                using var sf = new StreamReader(sourceFile);
                var contentSource = sf.ReadToEnd();
                if (contentSource != contentTarget)
                {
                    return $"File '{sourceFile}' and target '{file.FullName}' differ.";
                }
            }
            return null;
        }
    }
}
