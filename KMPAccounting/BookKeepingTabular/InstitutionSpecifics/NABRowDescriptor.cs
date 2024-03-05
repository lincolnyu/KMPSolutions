using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    public class NABRowDescriptor : BankTransactionRowDescriptor
    {
        public NABRowDescriptor() : base(Constants.DateTimeKey, Constants.AmountKey, Constants.CounterAccountKey, new List<string> { Constants.DateTimeKey, Constants.AmountKey, "Account Number", "_", "Transaction Type", Constants.TransactionDetailsKey, Constants.BalanceKey, "Category", "Merchant Name", Constants.CounterAccountKey })
        {
            BalanceKey = Constants.BalanceKey;
            TypeKey = "Transaction Type";
            CategoryKey = "Category";
            MerchantNameKey = "Merchant Name";
        }

        public string TypeKey { get; }
        public string CategoryKey { get; }
        public string MerchantNameKey { get; }
    }
}
