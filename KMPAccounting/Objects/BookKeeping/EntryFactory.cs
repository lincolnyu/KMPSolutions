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
            return type switch
            {
                "CompositeTransaction" => CompositeTransaction.ParseLine(timestamp, content),
                "Transaction" => Transaction.ParseLine(timestamp, content),
                "OpenAccount" => OpenAccount.ParseLine(timestamp, content),
                _ => throw new ArgumentException($"Unknown entry type {type}"),
            };
        }
    }
}
