using KMPCommon;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KMPAccounting.BookKeepingTabular
{
    public class CsvReader
    {
        #region Output Data Reset Every Call of Main Method
        
        public bool? HasHeader { get; private set; }
        public List<string>? LoadedHeader { get; private set; }

        #endregion

        #region Main Method

        public IEnumerable<TransactionRow<TTransactionRowDescriptor>> GetRows<TTransactionRowDescriptor>(StreamReader sr, TransactionTable<TTransactionRowDescriptor> tableDescriptor) where TTransactionRowDescriptor : BaseTransactionRowDescriptor
        {
            HasHeader = null;
            var index = 0;
            while (!sr.EndOfStream)
            {
                var fields = CsvUtility.GetAndBreakRow(sr).ToList();
                if (LoadedHeader == null)
                {
                    if (tableDescriptor.Header == TransactionTable<TTransactionRowDescriptor>.HeaderType.Present || (tableDescriptor.Header == TransactionTable<TTransactionRowDescriptor>.HeaderType.AutoDetect && tableDescriptor.RowDescriptor.EstimateHasHeader(fields)))
                    {
                        HasHeader = true;
                        LoadedHeader = fields;
                        // Skip the loaded header
                        fields = CsvUtility.GetAndBreakRow(sr).ToList();
                    }
                    else
                    {
                        HasHeader = false;
                        LoadedHeader = null;
                    }
                }
                var row = new TransactionRow<TTransactionRowDescriptor>(tableDescriptor, index++);
                var rowDescriptor = tableDescriptor.RowDescriptor;
                var i = 0;
                for (; i < rowDescriptor.Keys.Count && i < fields.Count; i++)
                {
                    row[rowDescriptor.Keys[i]] = fields[i];
                }
                row.ExtraColumnData.Clear();
                for (; i < fields.Count; i++)
                {
                    row.ExtraColumnData.Add(fields[i]);
                }
                yield return row;
            }
        }

        #endregion
    }
}
