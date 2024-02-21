using KMPCommon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KMPAccounting.BookKeepingTabular
{
    public class TransactionRow<TTransactionRowDescriptor> : ITransactionRow where TTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        public TransactionRow(TransactionTable<TTransactionRowDescriptor> ownerTable, int index)
        {
            OwnerTable = ownerTable;
            Index = index;
        }

        public IEnumerable<(string, string)> GetKeyAndValuePairs()
        {
            foreach (var kvp in KeyValueMap)
            {
                yield return (kvp.Key, kvp.Value);
            }
        }

        public bool KeyHasValue(string key) => KeyValueMap.ContainsKey(key);

        public string this[string key]
        {
            get
            {
                return KeyValueMap[key];
            }
            set
            {
                KeyValueMap[key] = value;
            }
        }

        public int Index { get; set;  }

        public Dictionary<string, string> KeyValueMap { get; } = new Dictionary<string, string>();

        public IList<string> ExtraColumnData { get; } = new List<string>();

        public TransactionTable<TTransactionRowDescriptor> OwnerTable { get; }

        public int? OriginalRowNumber { get; }

        ITransactionTable ITransactionRow.OwnerTable => OwnerTable;

        public DateTime DateTime => OwnerTable.RowDescriptor.GetDateTime(this);

        public override string ToString()
        {
            return string.Join(',', OwnerTable.RowDescriptor.Keys.Select(k =>
            KeyHasValue(k) ? CsvUtility.StringToCsvField(this[k]) : ""));
        }

        public virtual bool MergableWith(ITransactionRow that)
        {
            if (DateTime != that.DateTime) return false;

            var thisAmount = this.GetDecimalValue(OwnerTable.RowDescriptor.AmountKey);
            var thatAmount = that.GetDecimalValue(OwnerTable.RowDescriptor.AmountKey);

            return thisAmount == thatAmount;
        }

        public virtual void MergeFrom(ITransactionRow other)
        {
            foreach (var (k, v) in ((TransactionRow<TTransactionRowDescriptor>)other).KeyValueMap)
            {
                if (!string.IsNullOrEmpty(v))
                {
                    this[k] = v;
                }
            }
        }
    }
}
