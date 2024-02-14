using KMPCommon;
using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class NABCashRowDescriptor : BankTransactionRowDescriptor
    {
        public NABCashRowDescriptor() : base("Date", "Amount", Constants.CounterAccountKey, new List<string> { "Date", "Amount", "Account Number", "Transaction Type", "Transaction Details", "Balance", "Category", "Merchant Name", Constants.CounterAccountKey })
        {
            BalanceKey = "Balance";
        }
    }
}
