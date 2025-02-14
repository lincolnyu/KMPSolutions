using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class CommbankCashRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCashRowDescriptor() : base(Constants.DateTimeKey, Constants.AmountKey, Constants.CounterAccountKey, new List<string> { Constants.DateTimeKey, Constants.AmountKey, Constants.TransactionDetailsKey, Constants.BalanceKey, Constants.CounterAccountKey })
        {
            BalanceKey = Constants.BalanceKey;
        }

        public string TransactionDetailsKey { get; } = Constants.TransactionDetailsKey;
    }
}
