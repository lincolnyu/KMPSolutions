using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using KMPCommon;
using System;
using System.Collections.Generic;

namespace KMPAccounting.BookKeepingTabular
{
    public class LedgerBankTransactionRowCorrelator
    {
        public IEnumerable<(ITransactionRow, List<Transaction>)> Correlate<TTransactionRowDescriptor>(IEnumerable<ITransactionRow> rows) where TTransactionRowDescriptor :  BankTransactionRowDescriptor, new()
        {
            foreach (var row in rows)
            {
                var table = (BankTransactionTableDescriptor<TTransactionRowDescriptor>)row.OwnerTable;
                var rowDescriptor = table.RowDescriptor;
                var dateTimeStr = row[rowDescriptor.DateTimeKey];

                var baseAccountName = table.BaseAccountName;

                var amountStr = row[rowDescriptor.AmountKey];
                var amount = decimal.Parse(amountStr);

                var counterAccountsStr = row[rowDescriptor.CounterAccountKey];
                var counterAccounts = ParseCounterAccounts(counterAccountsStr, amount);

                var dateTime = CsvUtility.ParseDateTime(dateTimeStr);

                var transactions = new List<Transaction>();
                if (rowDescriptor.PositiveAmountForDebit)
                {
                    foreach (var (counterAccountName, counterAccountAmount) in counterAccounts)
                    {
                        var actualCounterAccountName = table.CounterAccountPrefix + counterAccountName;
                        if (counterAccountAmount > 0)
                        {
                            transactions.Add(new Transaction(dateTime, new AccountNodeReference(baseAccountName), new AccountNodeReference(actualCounterAccountName), counterAccountAmount));
                        }
                        else
                        {
                            transactions.Add(new Transaction(dateTime, new AccountNodeReference(actualCounterAccountName), new AccountNodeReference(baseAccountName), -counterAccountAmount));
                        }
                    }
                }
                else
                {
                    foreach (var (counterAccountName, counterAccountAmount) in counterAccounts)
                    {
                        var actualCounterAccountName = table.CounterAccountPrefix + counterAccountName;
                        if (counterAccountAmount > 0)
                        {
                            transactions.Add(new Transaction(dateTime, new AccountNodeReference(actualCounterAccountName), new AccountNodeReference(baseAccountName), counterAccountAmount));
                        }
                        else
                        {
                            transactions.Add(new Transaction(dateTime, new AccountNodeReference(baseAccountName), new AccountNodeReference(actualCounterAccountName), -counterAccountAmount));
                        }
                    }
                }
                yield return (row, transactions);
            }
        }

        private IEnumerable<(string, decimal)> ParseCounterAccounts(string counterAccountsStr, decimal totalAmount)
        {
            var counterAccounts = counterAccountsStr.Split(';');
            if (counterAccounts.Length > 2)
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
                yield return (counterAccounts[0], totalAmount);
            }
        }
    }
}
