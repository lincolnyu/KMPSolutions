using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class SuperRowDescriptor : BaseTransactionRowDescriptor
    {
        protected SuperRowDescriptor(string dateTimeKey, string amountKey, IList<string> keys) : base(dateTimeKey, amountKey, keys)
        {
        }

        public static decimal? GetAmount(ITransactionRow row) => row.GetDecimalValue(row.OwnerTable.RowDescriptor.AmountKey);
    }
}
