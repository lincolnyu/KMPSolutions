using KMPCommon;
using System.Collections.Generic;
using System.Linq;

namespace KMPAccounting.BookKeeping
{
    public class BankTransactionRow
    {
        public BankTransactionRow(BankTransactionTableDescriptor ownerTable)
        {
            OwnerTable = ownerTable;
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

        public Dictionary<string, string> KeyValueMap { get; } = new Dictionary<string, string>();

        public List<string> ExtraColumnData { get; } = new List<string>();

        public BankTransactionTableDescriptor OwnerTable { get; }

        public int? OriginalRowNumber { get; }

        public ReceiptOrInvoice? Receipt { get; set; }

        public override string ToString()
        {
            return string.Join(',', OwnerTable.RowDescriptor.Keys.Select(k => CsvUtility.StringToCsvField(this[k])));
        }
    }
}
