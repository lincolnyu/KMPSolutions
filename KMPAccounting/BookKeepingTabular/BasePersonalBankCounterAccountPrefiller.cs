﻿using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    public abstract class BasePersonalBankCounterAccountPrefiller
    {
        protected BasePersonalBankCounterAccountPrefiller(AccountGroup businessLiabilityAccountGroup, string personalFallbackCouterAccount)
        {
            BusinessOwingPersonalAccountGroup = businessLiabilityAccountGroup;
            PersonalFallbackCounterAccount = personalFallbackCouterAccount;
        }

        public AccountGroup BusinessOwingPersonalAccountGroup { get; }
        public string PersonalFallbackCounterAccount { get; }

        public void Prefill<TRowDescriptor>(TransactionRow<TRowDescriptor> row, ITransactionRow? invoiceRow, bool overwrite, bool populateFallbackCounterAccount = false) where TRowDescriptor : BankTransactionRowDescriptor
        {
            var rowDescriptor = row.OwnerTable.RowDescriptor;
            var counterAccountKey = rowDescriptor.CounterAccountKey;

            if (!overwrite && row[counterAccountKey] != null && !string.IsNullOrWhiteSpace(row[counterAccountKey]))
            {
                return;
            }

            var slist = new List<string>();

            decimal claimedAmount = 0;

            if (invoiceRow != null)
            {
                // Invoice should contain all the information other than personal use.
                var invoiceRowDescriptor = (InvoiceTransactionRowDescriptor)invoiceRow.OwnerTable.RowDescriptor;

                var businessClaimable = invoiceRow[invoiceRowDescriptor.BusinessClaimableKey];

                if (!string.IsNullOrEmpty(businessClaimable))
                {
                    var bcEntries = businessClaimable.Split(Constants.CommonSeparator);
                    foreach (var entry in bcEntries)
                    {
                        var es = entry.Split('=');

                        var businessAccount = BusinessOwingPersonalAccountGroup + es[0];
                        // It should always be expense
                        var businessAmount = decimal.Parse(es[1]);
                        slist.Add($"{businessAccount}={businessAmount}");
                        claimedAmount += businessAmount;
                    }
                }
            }

            var amount = decimal.Parse(row[rowDescriptor.AmountKey]);
            if (claimedAmount != amount)
            {
                var remainingAmount = amount - claimedAmount;

                var details = row[Constants.TransactionDetailsKey]!.Trim();

                if (!Prefill(row, details, claimedAmount, remainingAmount, slist) && populateFallbackCounterAccount)
                {
                    slist.Add($"{PersonalFallbackCounterAccount}={remainingAmount}");
                }
            }

            row[counterAccountKey] = string.Join(Constants.CommonSeparator, slist);
        }

        protected abstract bool Prefill(ITransactionRow row, string transactionDetails, decimal claimedAmount, decimal remainingAmount, List<string> slist);
    }
}
