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

        public string? this[string key]
        {
            get
            {
                if (KeyValueMap.TryGetValue(key, out var val))
                {
                    return val;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    KeyValueMap[key] = value;
                }
                else
                {
                    KeyValueMap.Remove(key);
                }
            }
        }

        public int Index { get; set;  }

        public Dictionary<string, string> KeyValueMap { get; } = new Dictionary<string, string>();

        public TransactionTable<TTransactionRowDescriptor> OwnerTable { get; }

        public int? OriginalRowNumber { get; }

        ITransactionTable ITransactionRow.OwnerTable => OwnerTable;

        public DateTime DateTime => OwnerTable.RowDescriptor.GetDateTime(this);

        public override string ToString()
        {
            return string.Join(',', OwnerTable.RowDescriptor.Keys.Select(k => this[k]?.StringToCsvField()?? ""));
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
