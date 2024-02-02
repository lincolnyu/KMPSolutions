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
            public CommbankCashTransactionTableDescriptor(string tableName) : base(tableName, new List<BankTransactionRowDescriptor> { new CommbankCashTransactionRowDescriptor() })
            {
            }
        }

        public class CommbankCashTransactionRowDescriptor : BankTransactionRowDescriptor
        {
            public CommbankCashTransactionRowDescriptor() : base("Date", "Amount", "BaseAccount", "CounterAccount", new List<string> { "Date", "Amount", "Remarks", "Balance", "BaseAccount", "CounterAccount" })
            {
                BalanceColumnName = "Balance";
            }
        }

        public class OriginalCsvReader : BaseCsvReader
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
                    row["Balance"] = fields[2];
                    yield return row;
                }
            }
        }
    }
}
