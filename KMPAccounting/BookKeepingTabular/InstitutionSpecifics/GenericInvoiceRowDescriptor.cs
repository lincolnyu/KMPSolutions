using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class GenericInvoiceRowDescriptor : InvoiceTransactionRowDescriptor
    {
        public GenericInvoiceRowDescriptor() : base("Date", "Amount", Constants.BusinessClaimableKey, Constants.CounterAccountKey, Constants.InvoiceReferenceKey, new List<string> { "Date", "Amount", Constants.RemarksKey, Constants.BusinessClaimableKey, Constants.CounterAccountKey, Constants.InvoiceReferenceKey })
        {
        }

        public string RemarksKey { get; } = Constants.RemarksKey;
    }
}
