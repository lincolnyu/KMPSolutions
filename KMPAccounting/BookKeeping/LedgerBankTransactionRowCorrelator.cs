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

        public IEnumerable<Transaction> Correlate()
        {
            foreach (var row in Emitter.Emit())
            {
                var table = row.OwnerTable;
                var rowDescriptor = table.RowDescriptor;
                var dateTimeStr = row.GetValue(rowDescriptor.DateTimeColumnName);
                var baseAccountName = row.GetValue(rowDescriptor.BaseAccountColumnName);
                var counterAccountName = row.GetValue(rowDescriptor.CounterAccountColumnName);
                var amountStr = row.GetValue(rowDescriptor.AmountColumnName);
                var amount = decimal.Parse(amountStr);

                var dateTime = CsvUtility.ParseDateTime(dateTimeStr);

                if (rowDescriptor.PositiveAmountForCredit)
                {
                    if (amount > 0)
                    {
                        yield return new Transaction(dateTime, new AccountNodeReference(baseAccountName) , new AccountNodeReference (counterAccountName), amount);
                    }
                    else
                    {
                        yield return new Transaction(dateTime, new AccountNodeReference(counterAccountName), new AccountNodeReference(baseAccountName), -amount);
                    }
                }
                else
                {
                    if (amount > 0)
                    {
                        yield return new Transaction(dateTime, new AccountNodeReference(counterAccountName), new AccountNodeReference(baseAccountName), amount);
                    }
                    else
                    {
                        yield return new Transaction(dateTime, new AccountNodeReference(baseAccountName), new AccountNodeReference(counterAccountName), -amount);
                    }
                }
            }
        }
    }
}
