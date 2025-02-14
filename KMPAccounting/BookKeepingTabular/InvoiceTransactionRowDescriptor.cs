using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    /// <summary>
    ///  Descriptor that describes a invoice transaction row.
    /// </summary>
    /// <remarks>
    ///  Positive amount always stands for income. To simplify implementation this is not made configurable.
    /// </remarks>
    public class InvoiceTransactionRowDescriptor : BaseTransactionRowDescriptor
    {
        protected InvoiceTransactionRowDescriptor(string dateTimeKey, string amountKey, string businessClaimableAmountKey, string invoiceReferenceKey, List<string> keys) : base(dateTimeKey, amountKey, keys)
        {
            BusinessClaimableKey = businessClaimableAmountKey;
            InvoiceReferenceKey = invoiceReferenceKey;
        }

        public string BusinessClaimableKey { get; }

        /// <summary>
        ///  The column that has the information of the invoice in agreed format.
        /// </summary>
        public string InvoiceReferenceKey { get; }
    }
}
