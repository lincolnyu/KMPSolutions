using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class CommbankCashRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCashRowDescriptor() : base("Date", "Amount", Constants.CounterAccountKey, new List<string> { "Date", "Amount", Constants.TransactionDetailsKey, "Balance", Constants.CounterAccountKey })
        {
            BalanceKey = "Balance";
        }

        public string TransactionDetailsKey { get; } = Constants.TransactionDetailsKey;
    }
}
