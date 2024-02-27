using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class CommbankCreditCardRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCreditCardRowDescriptor() : base("Date", "Amount", Constants.CounterAccountKey, new List<string> { "Date", "Amount", Constants.TransactionDetailsKey, Constants.BalanceKey, Constants.CounterAccountKey })
        {
            BalanceKey = Constants.BalanceKey; // Optional
        }
    }
}
