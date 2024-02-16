using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class GenericInvoiceRowDescriptor : InvoiceTransactionRowDescriptor
    {
        public GenericInvoiceRowDescriptor() : base("Invoice Date", "Amount", Constants.BusinessClaimableKey, Constants.InvoiceReferenceKey, new List<string> { "Invoice Date", "Amount", Constants.TransactionDetailsKey, Constants.BusinessClaimableKey, Constants.InvoiceReferenceKey })
        {
        }

        public string TransactionDetailsKey { get; } = Constants.TransactionDetailsKey;
    }
}
