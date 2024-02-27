using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class NABCashRowDescriptor : BankTransactionRowDescriptor
    {
        public NABCashRowDescriptor() : base(Constants.DateTimeKey, Constants.AmountKey, Constants.CounterAccountKey, new List<string> { Constants.DateTimeKey, Constants.AmountKey, "Account Number", "_", "Transaction Type", Constants.TransactionDetailsKey, Constants.BalanceKey, "Category", "Merchant Name", Constants.CounterAccountKey })
        {
            BalanceKey = Constants.BalanceKey;
            CategoryKey = "Category";
            MerchantNameKey = "Merchant Name";
        }

        public string CategoryKey { get; }
        public string MerchantNameKey { get; }
    }
}
