using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class CommbankCreditCardRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCreditCardRowDescriptor() : base(Constants.DateTimeKey, Constants.AmountKey, Constants.CounterAccountKey, new List<string> { Constants.DateTimeKey, Constants.AmountKey, Constants.TransactionDetailsKey, Constants.BalanceKey, Constants.CounterAccountKey })
        {
            BalanceKey = Constants.BalanceKey; // Optional
        }
    }
}
