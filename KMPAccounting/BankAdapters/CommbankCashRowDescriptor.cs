using KMPAccounting.BookKeeping;
using System.Collections.Generic;

namespace KMPAccounting.BankAdapters
{
    public class CommbankCashRowDescriptor : BankTransactionRowDescriptor
    {
        public CommbankCashRowDescriptor() : base("Date", "Amount", "CounterAccount", new List<string> { "Date", "Amount", "Remarks", "Balance", "CounterAccount" })
        {
            BalanceColumnName = "Balance";
        }
    }
}
