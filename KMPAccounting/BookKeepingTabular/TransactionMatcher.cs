using KMPAccounting.BookKeepingTabular;
using KMPCommon;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KMPAccounting.InstitutionSpecifics
{
    public class TransactionMatcher
    {
        // Allow [currentDay - queueAgeBefore, currentDay + queueAgeAfter]
        public int DefaultQueueAgeNumDaysBefore { get; set; } = 0;
        public int DefaultQueueAgeNumDaysAfter { get; set; } = 7;

        public int TotalInvoices { get; private set; } = 0;
        public int MatchedInvoices { get; private set; } = 0;

        /// <summary>
        ///  Matches at the best effort the bank transactions and their invoice transactions.
        ///  The input transactions must be sorted in ascending temporal order.
        /// </summary>
        /// <param name="bankTransactionLists">Bank transctions lists.</param>
        /// <param name="invoiceTransactionList">All invoice transactions. They have to be in chronicle order.</param>
        /// <returns>Enumerates through tuples of index of source bank transaction list, bank transaction and the invoice transaction if matched</returns>
        public IEnumerable<(int, ITransactionRow?, ITransactionRow?)> Match(IList<IEnumerable<ITransactionRow>> bankTransactionLists, IEnumerable<ITransactionRow> invoiceTransactionList, Func<ITransactionRow, IList<int>?>? getPreferredAccountIndices = null, bool yieldUnmatchedInvoiceRows = false, IList<(int?,int?)?>? queueAgeNumDaysBeforeAndAfter = null)
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

                var date = invoiceTransaction.DateTime;

                // Return and remove all transactions prior to the invoice.
                for (var i = 0; i < queues.Count; i++)
                {
                    var queueAgeBefore = queueAgeNumDaysBeforeAndAfter?[i]?.Item1 ?? DefaultQueueAgeNumDaysBefore;
                    var queue = queues[i];
                    while (queue.First != null)
                    {
                        var rowDescriptor = queue.First.Value.OwnerTable.RowDescriptor;
                        var frontDateStr = queue.First.Value[rowDescriptor.DateTimeKey];
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

                // Load the transactions within the specified range around the invoice date.
                for (var i = 0; i < queues.Count; i++)
                {
                    var queueAgeBefore = queueAgeNumDaysBeforeAndAfter?[i]?.Item1 ?? DefaultQueueAgeNumDaysBefore;
                    var queueAgeAfter = queueAgeNumDaysBeforeAndAfter?[i]?.Item2 ?? DefaultQueueAgeNumDaysAfter;
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
                        else
                        {
                            yield return (i, bt, null);
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

                        if (itemAmount == amount)
                        {
                            yield return (i, item, invoiceTransaction);
                            queue.Remove(p);
                            matched = true;
                            break;
                        }
                    }
                    if (matched)
                    {
                        break;
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

        class MatchComparer : IComparer<(int, ITransactionRow?, ITransactionRow?)>
        {
            public int Compare((int, ITransactionRow?, ITransactionRow?) x, (int, ITransactionRow?, ITransactionRow?) y)
            {
                if (x.Item2 != null && y.Item2 != null)
                {
                    if (x.Item1 == y.Item1)
                    {
                        Debug.Assert(x.Item2.OwnerTable == y.Item2.OwnerTable);
                        return x.Item2.Index.CompareTo(y.Item2.Index);
                    }
                    return Compare(x.Item1, x.Item2, y.Item1, y.Item2);
                }
                else if (x.Item2 != null)
                {
                    Debug.Assert(y.Item3 != null);
                    return Compare(x.Item1, x.Item2, null, y.Item3);
                }
                else if (y.Item2 != null)
                {
                    Debug.Assert(x.Item3 != null);
                    return Compare(null, x.Item3, y.Item1, y.Item2);
                }
                else
                {
                    Debug.Assert(x.Item3 != null);
                    Debug.Assert(y.Item3 != null);
                    return x.Item3.Index.CompareTo(y.Item3.Index);
                }
            }

            private int Compare(int? bankIndexX, ITransactionRow rowX, int? bankIndexY, ITransactionRow rowY)
            {
                var c = rowX.DateTime.CompareTo(rowY.DateTime);
                if (c != 0) return c;
                // Unmatched invoice rows take precendence.
                if (bankIndexX == null) return -1;
                if (bankIndexY == null) return 1;
                return bankIndexX.Value.CompareTo(bankIndexY.Value);
            }

            public static MatchComparer Instance { get; } = new MatchComparer();
        }

        public static IEnumerable<(int, ITransactionRow?, ITransactionRow?)> OrderMatch(IEnumerable<(int, ITransactionRow?, ITransactionRow?)> input) => input.OrderBy(x => x, MatchComparer.Instance);
    }
}
