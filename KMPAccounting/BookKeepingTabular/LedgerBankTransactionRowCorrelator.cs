using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using KMPCommon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KMPAccounting.BookKeepingTabular
{
    public static class LedgerBankTransactionRowCorrelator
    {
        public abstract class AccompanyingTransactionsDector
        {
            public abstract SimpleTransaction? Detect(DateTime dateTime, string counterAccountName, decimal counterAccountAmount);
        }
        
        public static IEnumerable<SimpleTransaction> CorrelateToMultipleTransaction(ITransactionRow row, AccompanyingTransactionsDector? detector = null)
        {
            var table = (IBankTransactionTable)row.OwnerTable;
            var rowDescriptor = (BankTransactionRowDescriptor)table.RowDescriptor;
            var dateTimeStr = row[rowDescriptor.DateTimeKey];

            var baseAccountName = table.BaseAccountName;

            var amountStr = row[rowDescriptor.AmountKey];
            var amount = decimal.Parse(amountStr);

            var counterAccountsStr = row[rowDescriptor.CounterAccountKey];
            var counterAccounts = ParseCounterAccounts(counterAccountsStr, amount);

            var dateTime = CsvUtility.ParseDateTime(dateTimeStr);

            var transactions = new List<SimpleTransaction>();
            var accompanyingTransactions = new List<SimpleTransaction>();

            foreach (var (counterAccountName, counterAccountAmount) in counterAccounts)
            {
                var actualCounterAccountName = table.CounterAccountPrefix + counterAccountName;

                var accompanyingTrasaction = detector?.Detect(dateTime, actualCounterAccountName, counterAccountAmount);
                if (accompanyingTrasaction != null)
                {
                    accompanyingTransactions.Add(accompanyingTrasaction);
                }

                if (counterAccountAmount > 0)
                {
                    transactions.Add(new SimpleTransaction(dateTime, new AccountNodeReference(baseAccountName), new AccountNodeReference(actualCounterAccountName), counterAccountAmount));
                }
                else
                {
                    transactions.Add(new SimpleTransaction(dateTime, new AccountNodeReference(actualCounterAccountName), new AccountNodeReference(baseAccountName), -counterAccountAmount));
                }
            }

            foreach (var t in transactions)
            {
                yield return t;
            }
            foreach (var t in  accompanyingTransactions)
            {
                yield return t;
            }
        }

        public static IEnumerable<Entry> CorrelateToSingleTransaction(ITransactionRow row, AccompanyingTransactionsDector? detector = null) 
        {
            var table = (IBankTransactionTable)row.OwnerTable;
            var rowDescriptor = (BankTransactionRowDescriptor)table.RowDescriptor;
            var dateTimeStr = row[rowDescriptor.DateTimeKey];

            var baseAccountName = table.BaseAccountName;

            var amountStr = row[rowDescriptor.AmountKey];
            var amount = CsvUtility.ParseDecimalValue(amountStr!)!.Value;

            if (amount == 0)
            {
                // TODO special cases.
                // No transaction to create for amount 0.
                yield break;
            }

            var counterAccountsStr = row[rowDescriptor.CounterAccountKey];
            var counterAccounts = ParseCounterAccounts(counterAccountsStr!, amount).ToArray();

            var dateTime = CsvUtility.ParseDateTime(dateTimeStr!);
            var accompanyingTransactions = new List<SimpleTransaction>();

            if (counterAccounts.Length > 1)
            {
                var compositeTransaction = new CompositeTransaction(dateTime);

                foreach (var (counterAccountName, counterAccountAmount) in counterAccounts)
                {
                    var actualCounterAccountName = table.CounterAccountPrefix + counterAccountName;

                    var accompanying = detector?.Detect(dateTime, actualCounterAccountName, counterAccountAmount);
                    if (accompanying != null)
                    {
                        accompanyingTransactions.Add(accompanying);
                    }

                    if (counterAccountAmount > 0)
                    {
                        compositeTransaction.Credited.Add((new AccountNodeReference(actualCounterAccountName), counterAccountAmount));
                    }
                    else
                    {
                        compositeTransaction.Debited.Add((new AccountNodeReference(actualCounterAccountName), -counterAccountAmount));
                    }
                }

                if (amount > 0)
                {
                    compositeTransaction.Debited.Add((new AccountNodeReference(baseAccountName), amount));
                }
                else
                {
                    compositeTransaction.Credited.Add((new AccountNodeReference(baseAccountName), -amount));
                }

                yield return compositeTransaction;
            }
            else
            {
                var (counterAccountName, counterAccountAmount) = counterAccounts[0];
                var actualCounterAccountName = table.CounterAccountPrefix + counterAccountName;

                var accompanying = detector?.Detect(dateTime, actualCounterAccountName, counterAccountAmount);
                if (accompanying != null)
                {
                    accompanyingTransactions.Add(accompanying);
                }

                SimpleTransaction singleTransaction;
                if (counterAccountAmount > 0)
                {
                    singleTransaction = new SimpleTransaction(dateTime, new AccountNodeReference(baseAccountName), new AccountNodeReference(actualCounterAccountName), counterAccountAmount);
                }
                else
                {
                    singleTransaction = new SimpleTransaction(dateTime, new AccountNodeReference(actualCounterAccountName), new AccountNodeReference(baseAccountName), -counterAccountAmount);
                }

                yield return singleTransaction;
            }

            foreach (var accompanyingTransaction in accompanyingTransactions)
            {
                yield return accompanyingTransaction;
            }
        }

        private static IEnumerable<(string, decimal)> ParseCounterAccounts(string counterAccountsStr, decimal totalAmount)
        {
            var counterAccounts = counterAccountsStr.Split(';');
            if (counterAccounts.Length > 1)
            {
                decimal added = 0;

                for (var i = 0; i < counterAccounts.Length; ++i)
                {
                    var ca = counterAccounts[i];
                    var caSplit = ca.Split('=');
                    var valStr = caSplit[1].Trim();
                    decimal val;
                    if (valStr.EndsWith('%'))
                    {
                        var perc = decimal.Parse(valStr[..^1]) / 100;
                        val = totalAmount * perc;
                    }
                    else
                    {
                        val = decimal.Parse(valStr);
                    }
                    if (i == counterAccounts.Length-1)
                    {
                        if (Math.Abs(added + val - totalAmount) >= 0.1m)
                        {
                            throw new ArgumentException($"Counter account setting '{counterAccountsStr}' doesn't add up to total amount {totalAmount}.");
                        }
                        val = totalAmount - added;
                    }
                    else
                    {
                        added += val;
                    }
                    yield return (caSplit[0], val);
                }
            }
            else
            {
                var ca = counterAccounts[0];
                var caSplit = ca.Split('=');
                yield return (caSplit[0], totalAmount);
            }
        }
    }
}
