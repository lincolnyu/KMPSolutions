using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    public class InvoiceTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        protected InvoiceTransactionRowDescriptor(string dateTimeKey, string amountKey, string businessClaimableAmountKey, string invoiceReferenceKey, List<string> keys) : base(dateTimeKey, amountKey, keys)
        {
            BusinessClaimableAmountKey = businessClaimableAmountKey;
            InvoiceReferenceKey = invoiceReferenceKey;
        }

        public string BusinessClaimableAmountKey { get; }

        /// <summary>
        ///  The column that has the information of the invoice in agreed format.
        /// </summary>
        public string InvoiceReferenceKey { get; }

        // Whether amount being positive indicates income the account that has changed due to the invoice.
        public bool PositiveAmountForIncome { get; set; } = true;
    }
}
