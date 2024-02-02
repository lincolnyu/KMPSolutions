using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using KMPCommon;
using System.Collections.Generic;

namespace KMPAccounting.BookKeeping
{
    public class LedgerBankTransactionRowCorrelator
    {
        public LedgerBankTransactionRowCorrelator(IBankTransactionRowEmitter emitter)
        {
            Emitter = emitter;
        }

        public IBankTransactionRowEmitter Emitter { get; }

        public IEnumerable<(BankTransactionRow, List<Transaction>)> Correlate()
        {
            foreach (var row in Emitter.Emit())
            {
                var table = row.OwnerTable;
                foreach (var rowDescriptor in table.RowDescriptors)
                {
                    var dateTimeStr = row[rowDescriptor.DateTimeKey];

                    var baseAccountName = table.BaseAccountPrefix + row[rowDescriptor.BaseAccountKey];

                    var amountStr = row[rowDescriptor.AccountKey];
                    var amount = decimal.Parse(amountStr);

                    var counterAccountsStr = row[rowDescriptor.CounterAccountKey];
                    var counterAccounts = ParseCounterAccounts(counterAccountsStr, amount);

                    var dateTime = CsvUtility.ParseDateTime(dateTimeStr);
                     
                    var transactions = new List<Transaction>();
                    if (rowDescriptor.PositiveAmountForCredit)
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
        }

        private IEnumerable<(string, decimal)> ParseCounterAccounts(string counterAccountsStr, decimal totalAmount)
        {
            var counterAccounts = counterAccountsStr.Split(';');
            if (counterAccounts.Length > 2)
            {
                foreach (var ca in counterAccounts)
                {
                    var caSplit = ca.Split('=');
                    yield return (caSplit[0], decimal.Parse(caSplit[1]));
                }
            }
            else
            {
                yield return (counterAccounts[0], totalAmount);
            }
        }
    }
}
