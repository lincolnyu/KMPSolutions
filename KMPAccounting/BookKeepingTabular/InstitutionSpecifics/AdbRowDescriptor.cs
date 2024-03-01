using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class AdbRowDescriptor
        : BankTransactionRowDescriptor
    {
        public AdbRowDescriptor() : base(Constants.DateTimeKey, Constants.AmountKey, Constants.CounterAccountKey, new List<string> { Constants.DateTimeKey, Constants.TransactionDetailsKey, Constants.AmountKey, Constants.BalanceKey, Constants.CounterAccountKey })
        {
            BalanceKey = Constants.BalanceKey;
        }

        public string TransactionDetailsKey { get; } = Constants.TransactionDetailsKey;
    }
}
