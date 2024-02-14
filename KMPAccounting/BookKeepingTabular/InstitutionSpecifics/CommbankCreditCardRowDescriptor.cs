using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class CommbankCreditCardRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCreditCardRowDescriptor() : base("Date", "Amount", Constants.CounterAccountKey, new List<string> { "Date", "Amount", Constants.RemarksKey, Constants.CounterAccountKey })
        {
        }
    }
}
