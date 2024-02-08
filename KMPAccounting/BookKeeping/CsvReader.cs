using KMPCommon;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KMPAccounting.BookKeeping
{
    public static class CsvReader
    {
        public static IEnumerable<BankTransactionRow> GetRows(StreamReader sr, BankTransactionTableDescriptor tableDescriptor)
        {
            while (!sr.EndOfStream)
            {
                var fields = CsvUtility.GetAndBreakRow(sr).ToList();
                var row = new BankTransactionRow(tableDescriptor);
                var rowDescriptor = tableDescriptor.RowDescriptor;
                var i = 0;
                for (; i < rowDescriptor.Keys.Count && i < fields.Count; i++)
                {
                    row[rowDescriptor.Keys[i]] = fields[i];
                }
                row.ExtraColumnData.Clear();
                for (; i < fields.Count; i++)
                {
                    row.ExtraColumnData.Add(fields[i]);
                }
                yield return row;
            }
        }
    }
}
