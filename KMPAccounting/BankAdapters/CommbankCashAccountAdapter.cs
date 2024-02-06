using KMPAccounting.BookKeeping;
using KMPCommon;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KMPAccounting.BankAdapters
{
    public class CommbankCashAccountAdapter
    {
        public class CommbankCashTransactionTableDescriptor : BankTransactionTableDescriptor
        {
            public CommbankCashTransactionTableDescriptor(string tableName) : base(tableName, new CommbankCashTransactionRowDescriptor())
            {
            }
        }

        public class CommbankCashTransactionRowDescriptor : BankTransactionRowDescriptor
        {
            public CommbankCashTransactionRowDescriptor() : base("Date", "Amount", "CounterAccount", new List<string> { "Date", "Amount", "Remarks", "Balance", "CounterAccount" })
            {
                BalanceColumnName = "Balance";
            }
        }

        public class OriginalCsvReader : CsvReader
        {
            public OriginalCsvReader(string path, string tableName)
            {
                Path = path;
                TableDescriptor = new CommbankCashTransactionTableDescriptor(tableName);
            }

            public string Path { get; }
            public CommbankCashTransactionTableDescriptor TableDescriptor { get; }

            public override IEnumerable<BankTransactionRow> GetRows()
            {
                using var sr = new StreamReader(Path);
                while (!sr.EndOfStream)
                {
                    var fields = CsvUtility.GetAndBreakRow(sr).ToList();
                    var row = new BankTransactionRow(TableDescriptor);
                    row["Date"] = fields[0];
                    row["Amount"] = fields[1];
                    row["Remarks"] = fields[2];
                    row["Balance"] = fields[3];
                    yield return row;
                }
            }
        }
    }
}
