using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public class InvoiceTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        protected InvoiceTransactionRowDescriptor(string dateTimeKey, string amountKey, List<string> keys) : base(dateTimeKey, amountKey, keys)
        {
        }

        /// <summary>
        ///  Specifiies the counter accounts and the split details.
        ///  This needs to be manually input after an automatic prefill at best effort.
        ///  This should be able to be used directly by the bank transaction rows.
        /// </summary>
        public string CounterAccountKey { get; set; }
    }
}
