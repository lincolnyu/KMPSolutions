using System;
using System.Collections.Generic;
using KMPCommon;

namespace KMPAccounting.BookKeepingTabular
{
    /// <summary>
    ///  Generate uniform rows from multiple row sources
    /// </summary>
    public class MultiTransactionRowSourceCsvCombiner
    {
        public class TypedColumnPicker
        {
            public string? Key { get; set; }

            /// <summary>
            ///  The smaller the number the higher the precedence.
            ///  A source with lower precedence will be overwritten by higher.
            /// </summary>
            public int? Precedence { get; set; }

            public Func<string, string>? Formatter { get; set; }
        }

        public class ColumnPicker
        {
            public ColumnPicker(string targetColumName)
            {
                TargetColumnName = targetColumName;
            }

            /// <summary>
            ///  Specialized pickers for certain types.
            /// </summary>
            public Dictionary<Type, TypedColumnPicker> Typed { get; } = new Dictionary<Type, TypedColumnPicker>();

            /// <summary>
            ///  Fallback picker
            /// </summary>
            /// <remarks>
            ///  If the precedence of this is not set, the precedence of Typed will be ignored.
            ///  Then it will early out if the column is set by any source.
            ///  If the precedence of this is set then all the precedence values of all the typed entry have the be set as well.
            /// </remarks>
            public TypedColumnPicker Generic { get; } = new TypedColumnPicker();

            /// <summary>
            ///  The column name in the target table.
            /// </summary>
            public string TargetColumnName { get; }

            public Func<string, string>? Formatter { get; set; }
        }

        private readonly Dictionary<string, int> targetNameToColumnIndex_ = new Dictionary<string, int>();

        public ColumnPicker? GetColumn(string targetName)
        {
            if (!targetNameToColumnIndex_.TryGetValue(targetName, out var columnIndex))
            {
                return null;
            }
            return Columns[columnIndex];
        }

        public int GetColumnIndex(Type rowDescriptorType, string key)
        {
            for (var i=0; i < Columns.Count;i++)
            {
                var col = Columns[i];
                if (col.Typed.TryGetValue(rowDescriptorType, out var val) && val.Key == key)
                {
                    return i;
                }
                if (col.Generic.Key == key)
                {
                    return i;
                }
            }
            return -1;
        }

        public MultiTransactionRowSourceCsvCombiner(List<ColumnPicker> columns) 
        {
            Columns = columns;
            UpdateColumnMap();
        }

        private void UpdateColumnMap()
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                ColumnPicker? column = Columns[i];
                targetNameToColumnIndex_[column.TargetColumnName] = i;
            }
        }

        public static MultiTransactionRowSourceCsvCombiner CreateSimpleCombiningRowDescriptors(params ITransactionRowDescriptor[] supportedRowDescriptors)
        {
            var formatter = new MultiTransactionRowSourceCsvCombiner(new List<ColumnPicker>());
            var keySet = new HashSet<string>();

            var dateKeys = new HashSet<string>();
            var amountKeys = new HashSet<string>();
            foreach (var rowDescriptor in supportedRowDescriptors)
            {
                foreach (var k in rowDescriptor.Keys)
                {
                    if (k == rowDescriptor.DateTimeKey)
                    {
                        dateKeys.Add(k);
                    }
                    else if (k == rowDescriptor.AmountKey)
                    {
                        amountKeys.Add(k);
                    }
                    else if (k != Constants.DummyKey)
                    {
                        keySet.Add(k);
                    }
                }
            }

            foreach (var k in dateKeys)
            {
                var column = new ColumnPicker(k);
                column.Generic.Key = k;
                formatter.Columns.Add(column);
            }

            foreach (var k in amountKeys)
            {
                var column = new ColumnPicker(k);
                column.Generic.Key = k;
                formatter.Columns.Add(column);
            }

            foreach (var k in keySet)
            {
                var column = new ColumnPicker(k);
                column.Generic.Key = k;
                formatter.Columns.Add(column);
            }
            formatter.UpdateColumnMap();
            return formatter;
        }

        public List<ColumnPicker> Columns { get; }

        private string GetColumnValue(ITransactionRow row, TypedColumnPicker picker, ColumnPicker column)
        {
            var val = row[picker.Key!]!.Trim();
            return picker.Formatter?.Invoke(val) ?? column.Formatter?.Invoke(val) ?? val;
        }

        public IEnumerable<string> FieldsToStrings(params ITransactionRow[] rowSouces)
        {
            static bool IsHigherPrecedence(int newValue, int? currentHighest)
            {
                if (!currentHighest.HasValue) return true;
                return newValue < currentHighest!;
            }

            foreach (var column in Columns)
            {
                string columnValue = "";
                var checkPrecedence = column.Generic.Precedence.HasValue;
                int? currentHighestPrecedence = null;
                foreach (var row in rowSouces)
                {
                    for (var t = row.GetType(); t != typeof(object); t = t.BaseType)
                    {
                        if (column.Typed.TryGetValue(row.GetType(), out var typed))
                        {
                            if (!checkPrecedence || IsHigherPrecedence(typed.Precedence!.Value, currentHighestPrecedence))
                            {
                                if (typed.Key != null)
                                {
                                    if (row[typed.Key!] != null)
                                    {
                                        columnValue = GetColumnValue(row, typed, column);
                                    }
                                }
                                if (checkPrecedence)
                                {
                                    currentHighestPrecedence = typed.Precedence!.Value;
                                }
                            }
                            break;
                        }
                    }

                    if ((checkPrecedence && IsHigherPrecedence(column.Generic.Precedence!.Value, currentHighestPrecedence)) || columnValue == "")
                    {
                        if (column.Generic.Key != null)
                        {
                            if (row[column.Generic.Key!] != null)
                            {
                                columnValue = GetColumnValue(row, column.Generic, column);
                            }
                        }
                        if (checkPrecedence)
                        {
                            currentHighestPrecedence = column.Generic.Precedence!.Value;
                        }
                    }
                    if (!checkPrecedence && columnValue != "") break;
                }
                yield return columnValue.StringToCsvField();
            }
        }
    }
}
