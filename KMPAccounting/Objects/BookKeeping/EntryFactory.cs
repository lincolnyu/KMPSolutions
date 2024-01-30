using KMPCommon;
using System;

namespace KMPAccounting.Objects.BookKeeping
{
    public class EntryFactory
    {
        public static Entry ParseEntry(string line)
        {
            var sep0 = line.IndexOf('|');
            var timestamp = line.Substring(0, sep0);
            
            var dtTimestamp = CsvUtility.ParseTimestamp(timestamp);
            
            var sep1= line.IndexOf('|', sep0 + 1);
            var type = line.Substring(sep0 + 1, sep1 - sep0 - 1);
            var content = line.Substring(sep0 + 1);
            switch (type)
            {
                case "PackedTransaction":
                    return PackedTransaction.ParseLine(dtTimestamp, content);
                case "Transaction":
                    return Transaction.ParseLine(dtTimestamp, content);
                case "OpenAccount":
                    return OpenAccount.ParseLine(dtTimestamp, content);
                default:
                    throw new ArgumentException($"Unknown entry type {type}");
            }
        }
    }
}
