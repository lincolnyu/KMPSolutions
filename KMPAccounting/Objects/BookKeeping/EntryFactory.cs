using KMPCommon;
using System;

namespace KMPAccounting.Objects.BookKeeping
{
    public class EntryFactory
    {
        public static Entry ParseEntry(string line)
        {
            int p = 0;
            int newp;
            line.GetNextWord('|', p, out newp, out var timestampStr);
            p = newp + 1;

            var timestamp = CsvUtility.ParseTimestamp(timestampStr);

            line.GetNextWord('|', p, out newp, out var type);

            p = newp + 1;
            var content = line.Substring(p);
            switch (type)
            {
                case "PackedTransaction":
                    return PackedTransaction.ParseLine(timestamp, content);
                case "Transaction":
                    return Transaction.ParseLine(timestamp, content);
                case "OpenAccount":
                    return OpenAccount.ParseLine(timestamp, content);
                default:
                    throw new ArgumentException($"Unknown entry type {type}");
            }
        }
    }
}
