using System;
using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    public class TransactionRowCsvFormatter
    {
        public class TypedColumnPicker
        {
            public string? Key { get; set; }
            public int? ExtraIndex { get; set; }
        }

        public class ColumnPicker
        {
            public ColumnPicker(string targetColumName)
            {
                TargetColumnName = targetColumName;
            }
            public Dictionary<Type, TypedColumnPicker> Typed { get; } = new Dictionary<Type, TypedColumnPicker>();
            public TypedColumnPicker Generic { get; } = new TypedColumnPicker();
            public string TargetColumnName { get; }
        }

        public TransactionRowCsvFormatter(List<ColumnPicker> columns) 
        {
            Columns = columns;
        }

        public static TransactionRowCsvFormatter CreateSimpleCombiningRowDescriptors(params ITransactionRowDescriptor[] supportedRowDescriptors)
        {
            var formatter = new TransactionRowCsvFormatter(new List<ColumnPicker>());
            var keySet = new HashSet<string>();
            foreach (var rowDescriptor in supportedRowDescriptors)
            {
                foreach (var k in rowDescriptor.Keys)
                {
                    if (k != rowDescriptor.AmountKey && k != rowDescriptor.DateTimeKey)
                    {
                        keySet.Add(k);
                    }
                }
            }

            var dateColumn = new ColumnPicker(Constants.DateTimeKey);
            dateColumn.Generic.Key = Constants.DateTimeKey;
            var amountColumn = new ColumnPicker(Constants.AmountKey);
            amountColumn.Generic.Key = Constants.AmountKey;
            formatter.Columns.Add(dateColumn);
            formatter.Columns.Add(amountColumn);

            foreach (var k in keySet)
            {
                var column = new ColumnPicker(k);
                column.Generic.Key = k;
                formatter.Columns.Add(column);
            }

            return formatter;
        }

        public List<ColumnPicker> Columns { get; }

        public IEnumerable<string> FieldsToStrings(params ITransactionRow[] rowSouces)
        {
            foreach (var column in Columns)
            {
                string columnValue = "";
                foreach (var row in rowSouces)
                {
                    for (var t = row.GetType(); t != typeof(object); t = t.BaseType)
                    {
                        if (column.Typed.TryGetValue(row.GetType(), out var typed))
                        {
                            if (typed.Key != null)
                            {
                                if (row.KeyHasValue(typed.Key!))
                                {
                                    columnValue = row[typed.Key!].Trim();
                                }
                            }             
                            else
                            {
                                columnValue = row.ExtraColumnData[typed.ExtraIndex!.Value].Trim();
                            }
                            break;
                        }
                    }
                    if (columnValue == "")
                    {
                        if (column.Generic.Key != null)
                        {
                            if (row.KeyHasValue(column.Generic.Key!))
                            {
                                columnValue = row[column.Generic.Key!].Trim();
                            }
                        }
                        else
                        {
                            columnValue = row.ExtraColumnData[column.Generic.ExtraIndex!.Value].Trim();
                        }
                    }
                    if (columnValue != "") break;
                }
                yield return columnValue;
            }
        }
    }
}
