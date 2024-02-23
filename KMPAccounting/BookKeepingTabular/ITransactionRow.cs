using System;

namespace KMPAccounting.BookKeepingTabular
{
    public interface ITransactionRow
    {
        /// <summary>
        ///  Index in the table (OwnerTable).
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///  Returns the value for the key if it exists.
        /// </summary>
        /// <param name="key">The key to get value for.</param>
        /// <returns>The value or null.</returns>
        public string? this[string key] { get; set; }

        /// <summary>
        ///  The time the transaction occurs. It has to refer to the column of RowDescriptor.DateTimeKey.
        /// </summary>
        public DateTime DateTime { get; }

        public ITransactionTable OwnerTable { get; }

        public bool MergableWith(ITransactionRow other);
        public void MergeFrom(ITransactionRow other);
    }
}
