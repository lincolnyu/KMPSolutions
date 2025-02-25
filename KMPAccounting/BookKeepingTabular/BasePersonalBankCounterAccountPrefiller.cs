﻿using System.Collections.Generic;
using System.Diagnostics;
using KMPAccounting.Objects.AccountCreation;

namespace KMPAccounting.BookKeepingTabular
{
    public abstract class BasePersonalBankCounterAccountPrefiller
    {
        protected BasePersonalBankCounterAccountPrefiller(AccountPath businessLiabilityAccountGroup, string personalFallbackCounterAccount)
        {
            BusinessOwingPersonalAccountGroup = businessLiabilityAccountGroup;
            PersonalFallbackCounterAccount = personalFallbackCounterAccount;
        }

        public AccountPath BusinessOwingPersonalAccountGroup { get; }
        public string PersonalFallbackCounterAccount { get; }

        public void Prefill(ITransactionRow row, ITransactionRow? invoiceRow, bool overwrite, bool populateFallbackCounterAccount = false) 
        {
            var rowDescriptor = row.OwnerTable.RowDescriptor;
            var counterAccountKey = ((BankTransactionRowDescriptor)rowDescriptor).CounterAccountKey;

            if (!overwrite && row[counterAccountKey] != null && !string.IsNullOrWhiteSpace(row[counterAccountKey]))
            {
                return;
            }

            var assignmentList = new List<string>();

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
                        assignmentList.Add($"{businessAccount}={businessAmount}");
                        claimedAmount += businessAmount;
                    }
                }
            }

            
            var amount = row.GetDecimalValue(rowDescriptor.AmountKey)!.Value;
            if (claimedAmount != amount)
            {
                var remainingAmount = amount - claimedAmount;

                var details = row[Constants.TransactionDetailsKey]!.Trim();

                if (!Prefill(row, details, claimedAmount, remainingAmount, assignmentList) && populateFallbackCounterAccount)
                {
                    // TODO should probably have an expense account and income account
                    assignmentList.Add($"{PersonalFallbackCounterAccount}={remainingAmount}");
                }
            }

            row[counterAccountKey] = string.Join(Constants.CommonSeparator, assignmentList);
        }

        /// <summary>
        ///  Special prefill after the invoiced business components filled by BaseBusinessBankCounterAccountPrefiller.Prefill<>() 
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="transactionDetails">TransactionDetails field from the row.</param>
        /// <param name="claimedAmount">The amount that has been claimed in the invoiced business components.</param>
        /// <param name="remainingAmount">The remaining amount (The total transaction amount minus claimed amount).</param>
        /// <param name="assignmentList">The account assignment list that includes the claimed and is to be updated for the remaining.</param>
        /// <param name="populateFallbackCounterAccount">Whether can fill the remaining using fallback. But the implementer doesn't have to.</param>
        /// <returns>If the remaining is fully filled.</returns>
        protected bool Prefill(ITransactionRow row, string transactionDetails, decimal claimedAmount, decimal remainingAmount, List<string> assignmentList)
        {
            if (PrefillGeneric(row, transactionDetails, claimedAmount, remainingAmount, assignmentList))
            {
                return true;
            }
            Debug.Assert(assignmentList.Count == 0);
            if (PrefillSpecificPostGeneric(row, transactionDetails, claimedAmount, remainingAmount, assignmentList))
            {
                return true;
            }

            return false;
        }

        protected virtual bool PrefillSpecificDuringGeneric(ITransactionRow row, string transactionDetails, decimal claimedAmount, decimal remainingAmount, List<string> assignmentList, string title)
        {
            return false;
        }

        protected abstract bool PrefillGeneric(ITransactionRow row, string transactionDetails, decimal claimedAmount, decimal remainingAmount, List<string> assignmentList);

        protected abstract bool PrefillSpecificPostGeneric(ITransactionRow row, string transactionDetails, decimal claimedAmount, decimal remainingAmount, List<string> assignmentList);
    }
}
