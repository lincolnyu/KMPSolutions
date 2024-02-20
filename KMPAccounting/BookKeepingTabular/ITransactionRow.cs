using System;
using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    public interface ITransactionRow
    {
        public IEnumerable<(string, string)> GetKeyAndValuePairs();

        /// <summary>
        ///  Index in the table (OwnerTable)
        /// </summary>
        public int Index { get; set; }

        public bool KeyHasValue(string key);

        public string this[string key] { get; set; }

        public IList<string> ExtraColumnData { get; }

        /// <summary>
        ///  The time the transaction occurs. It has to refer to the column of RowDescriptor.DateTimeKey
        /// </summary>
        public DateTime DateTime { get; }

        public ITransactionTable OwnerTable { get; }

        public bool MergableWith(ITransactionRow other);
        public void MergeFrom(ITransactionRow other);
    }
}
