using System.Collections.Generic;

namespace KMPAccounting.Objects.BookKeeping
{
    public class Ledger
    {
        /// <summary>
        ///  All systemwide accounting entries sorted in chronicle order
        /// </summary>
        public List<Entry> Entries { get; } = new List<Entry>();
    }
}
