using KMPCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KMPAccounting.BookKeepingTabular
{
    public class BaseTransactionRowDescriptor : ITransactionRowDescriptor
    {
        protected BaseTransactionRowDescriptor(string dateTimeKey, string amountKey , IList<string> keys)
        {
            DateTimeKey = dateTimeKey;
            AmountKey = amountKey;
            Keys = keys;
        }

        public string DateTimeKey { get; }

        // The key for the amount value (balance change)
        public string AmountKey { get; }

        public IList<string> Keys { get; }

        public virtual bool EstimateHasHeader(IList<string> loadedFieldsOfFirstRow)
        {
            var amountColumnIndex = Keys.IndexOf(AmountKey);
            Debug.Assert(amountColumnIndex != -1);
            var amountColumeValue = loadedFieldsOfFirstRow[amountColumnIndex].Trim();
            return !decimal.TryParse(amountColumeValue, out _);
        }

        public virtual DateTime GetDateTime(ITransactionRow row)
        {
            return CsvUtility.ParseDateTime(row[DateTimeKey]!);
        }
    }
}
