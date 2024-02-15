using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    public class InvoiceTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        protected InvoiceTransactionRowDescriptor(string dateTimeKey, string amountKey, string businessClaimableAmountKey, string counterAccountKey, string invoiceReferenceKey, List<string> keys) : base(dateTimeKey, amountKey, keys)
        {
            BusinessClaimableAmountKey = businessClaimableAmountKey;
            CounterAccountKey = counterAccountKey;
            InvoiceReferenceKey = invoiceReferenceKey;
        }

        public string BusinessClaimableAmountKey { get; }

        /// <summary>
        ///  Specifiies the counter accounts and the split details.
        ///  This needs to be manually input after an automatic prefill at best effort.
        ///  This should be able to be used directly by the bank transaction rows.
        /// </summary>
        public string CounterAccountKey { get; }

        /// <summary>
        ///  The column that has the information of the invoice in agreed format.
        /// </summary>
        public string InvoiceReferenceKey { get; }

        // Whether amount being positive indicates income the account that has changed due to the invoice.
        public bool PositiveAmountForIncome { get; set; } = true;
    }
}
