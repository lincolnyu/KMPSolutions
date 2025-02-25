﻿using KMPCommon;
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

        #region Main MethodS

        public IEnumerable<TransactionRow<TTransactionRowDescriptor>> GetRows<TTransactionRowDescriptor>(StreamReader sr, TransactionTable<TTransactionRowDescriptor> tableDescriptor, bool ignoreDummyKey=true, bool closeStream = true) where TTransactionRowDescriptor : BaseTransactionRowDescriptor
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
                    if (!ignoreDummyKey || rowDescriptor.Keys[i] != Constants.DummyKey)
                    {
                        row[rowDescriptor.Keys[i]] = fields[i];
                    }
                }
                yield return row;
            }
            if (closeStream)
            {
                sr.Close();
            }
        }

        public IEnumerable<(TransactionRow<TTransactionRowDescriptor>, List<string>)> GetRows<TTransactionRowDescriptor>(StreamReader sr, TransactionTable<TTransactionRowDescriptor> tableDescriptor, IList<int> fieldsSelector, bool hasHeader, bool ignoreDummyKey = true, bool closeStream = true) where TTransactionRowDescriptor : BaseTransactionRowDescriptor
        {
            HasHeader = null;
            var index = 0;
            while (!sr.EndOfStream)
            {
                var originalFields = CsvUtility.GetAndBreakRow(sr).ToList();
                var fields = new List<string>();

                foreach (var s in fieldsSelector)
                {
                    if (s >= 0)
                    {
                        fields.Add(originalFields[s]);
                    }
                    else
                    {
                        fields.Add("");
                    }
                }

                if (LoadedHeader == null)
                {
                    if (hasHeader)
                    {
                        HasHeader = true;
                        LoadedHeader = fields;

                        // Skip the loaded header
                        originalFields = CsvUtility.GetAndBreakRow(sr).ToList();

                        fields = new List<string>();
                        foreach (var s in fieldsSelector)
                        {
                            if (s >= 0)
                            {
                                fields.Add(originalFields[s]);
                            }
                            else
                            {
                                fields.Add("");
                            }
                        }
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
                    if (!ignoreDummyKey || rowDescriptor.Keys[i] != Constants.DummyKey)
                    {
                        row[rowDescriptor.Keys[i]] = fields[i];
                    }
                }
                yield return (row, originalFields);
            }
            if (closeStream)
            {
                sr.Close();
            }
        }

        #endregion
    }
}
