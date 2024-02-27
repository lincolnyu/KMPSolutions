using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class CommbankCashRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCashRowDescriptor() : base("Date", "Amount", Constants.CounterAccountKey, new List<string> { "Date", "Amount", Constants.TransactionDetailsKey, Constants.BalanceKey, Constants.CounterAccountKey })
        {
            BalanceKey = Constants.BalanceKey;
        }

        public string TransactionDetailsKey { get; } = Constants.TransactionDetailsKey;
    }
}
