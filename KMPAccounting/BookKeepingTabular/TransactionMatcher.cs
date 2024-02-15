using KMPAccounting.BookKeepingTabular;
using KMPCommon;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KMPAccounting.InstitutionSpecifics
{
    public class TransactionMatcher
    {
        // Allow [currentDay - queueAgeBefore, currentDay + queueAgeAfter]
        public int DefaultQueueAgeNumDaysBefore { get; set; } = 3;
        public int DefaultQueueAgeNumDaysAfter { get; set; } = 7;

        public int TotalInvoices { get; private set; } = 0;
        public int MatchedInvoices { get; private set; } = 0;

        /// <summary>
        ///  Matches at the best effort the bank transactions and their invoice transactions
        /// </summary>
        /// <param name="bankTransactionLists">Bank transctions lists.</param>
        /// <param name="invoiceTransactionList">All invoice transactions. They have to be in chronicle order.</param>
        /// <returns>Enumerates through tuples of index of source bank transaction list, bank transaction and the invoice transaction if matched</returns>
        public IEnumerable<(int, ITransactionRow?, ITransactionRow?)> Match(IList<IEnumerable<ITransactionRow>> bankTransactionLists, IEnumerable<ITransactionRow> invoiceTransactionList, Func<ITransactionRow, IList<int>?>? getPreferredAccountIndices = null, bool yieldUnmatchedInvoiceRows = false, IList<(int,int)>? queueAgeNumDaysBeforeAndAfter = null)
        {
            TotalInvoices = 0;
            MatchedInvoices = 0;

            var bankTransactionListCount = bankTransactionLists.Count;
            var queues = new List<LinkedList<ITransactionRow>>();
            var btEnumerators = new List<IEnumerator<ITransactionRow>?>();
            
            for (var i = 0; i < bankTransactionListCount; i++)
            {
                queues.Add(new LinkedList<ITransactionRow>());

                var bt = bankTransactionLists[i].GetEnumerator();
                if (!bt.MoveNext())
                {
                    bt = null;
                }
                btEnumerators.Add(bt);
            }

            foreach (var invoiceTransaction in invoiceTransactionList)
            {
                var invoiceRowDescriptor = (InvoiceTransactionRowDescriptor)invoiceTransaction.OwnerTable.RowDescriptor;

                var positiveForCashOut = invoiceRowDescriptor.PositiveAmountForCashOut;

                var date = invoiceTransaction.DateTime;

                // Load the transactions within the specified range around the invoice date.
                for (var i = 0; i < queues.Count; i++)
                {
                    var queueAgeBefore = queueAgeNumDaysBeforeAndAfter?[i].Item1 ?? DefaultQueueAgeNumDaysBefore;
                    var queueAgeAfter = queueAgeNumDaysBeforeAndAfter?[i].Item2 ?? DefaultQueueAgeNumDaysAfter;
                    var queue = queues[i];
                    while (btEnumerators[i] != null)
                    {
                        var bt = btEnumerators[i]!.Current;
                        var dt = bt.DateTime;
                        if (dt.Date > date.Date.AddDays(queueAgeAfter))
                        {
                            break;
                        }
                        else if (dt >= date.Date.AddDays(-queueAgeBefore))
                        {
                            queue.AddLast(bt);
                        }
                        if (!btEnumerators[i]!.MoveNext())
                        {
                            btEnumerators[i] = null;
                        }
                    }
                }

                var preferredAccounts = getPreferredAccountIndices?.Invoke(invoiceTransaction)?? Enumerable.Range(0, bankTransactionListCount);

                var amount = invoiceTransaction.GetDecimalValue(invoiceTransaction.OwnerTable.RowDescriptor.AmountKey);
                var matched = false;
                foreach (var i in preferredAccounts)
                {
                    var queue = queues[i];
                    for (var p = queue.First; p != null; p = p.Next)
                    {
                        var item = p.Value;
                        var itemAmount = item.GetDecimalValue(item.OwnerTable.RowDescriptor.AmountKey);

                        var equalAmount = positiveForCashOut ? itemAmount == -amount : itemAmount == amount;

                        if (equalAmount)
                        {
                            yield return (i, item, invoiceTransaction);
                            queue.Remove(p);
                            matched = true;
                            break;
                        }
                    }
                }

                TotalInvoices++;
                if (matched)
                {
                    MatchedInvoices++;
                }
                else if (yieldUnmatchedInvoiceRows)
                {
                    yield return (-1, null, invoiceTransaction);
                }

                // Return and remove all transactions prior to the invoice.
                for (var i = 0; i < queues.Count; i++)
                {
                    var queueAgeBefore = queueAgeNumDaysBeforeAndAfter?[i].Item1 ?? DefaultQueueAgeNumDaysBefore;
                    var queue = queues[i];
                    while (queue.First != null)
                    {
                        var frontDateStr = queue.First.Value[invoiceRowDescriptor.DateTimeKey];
                        var frontDate = CsvUtility.ParseDateTime(frontDateStr);
                        if (frontDate.Date < date.Date.AddDays(-queueAgeBefore))
                        {
                            yield return (i, queue.First.Value, null);
                            queue.RemoveFirst();
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // Return all remaining transactions.
            for (var i = 0; i < queues.Count; i++)
            {
                var queue = queues[i];
                foreach (var entry in queue)
                {
                    yield return (i, entry, null);
                }
            }
        }
    }
}
