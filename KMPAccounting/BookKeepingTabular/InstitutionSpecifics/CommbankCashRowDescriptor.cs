using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class CommbankCashRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCashRowDescriptor() : base("Date", "Amount", "CounterAccount", new List<string> { "Date", "Amount", Constants.RemarksKey, "Balance", "CounterAccount" })
        {
            BalanceKey = "Balance";
        }

        public string RemarksKey { get; } = Constants.RemarksKey;
    }
}
