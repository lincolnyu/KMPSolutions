using KMPAccounting.BookKeeping;
using System.Collections.Generic;

namespace KMPAccounting.BankSpecifics
{
    public class CommbankCreditCardRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCreditCardRowDescriptor() : base("Date", "Amount", "CounterAccount", new List<string> { "Date", "Amount", "Remarks", "CounterAccount" })
        {
        }
    }
}
