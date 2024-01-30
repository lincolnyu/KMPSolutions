using System.Collections.Generic;
using System.IO;

namespace KMPAccounting.Objects.BookKeeping
{
    public class Ledger
    {
        /// <summary>
        ///  All systemwide accounting entries sorted in chronicle order
        /// </summary>
        public List<Entry> Entries { get; } = new List<Entry>();

        public void WriteToStream(StreamWriter sw)
        {
            foreach (var entry in Entries)
            {
                sw.WriteLine(entry.ToString());
            }
        }

        public void LoadFromStream(StreamReader sr, bool append=false)
        {
            if (!append)
            {
                Entries.Clear();
            }
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                var entry = EntryFactory.ParseEntry(line);
                Entries.Add(entry);
            }
        }
    }
}
