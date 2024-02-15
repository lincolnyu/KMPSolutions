using KMPCommon;
using System;
using System.Collections.Generic;
using System.IO;

namespace KMPAccounting.BookKeepingTabular.InstitutionSpecifics
{
    /// <summary>
    ///  Reads text copy-pasted from the transaction list on the page and turns it to
    ///  TransactionRow base don GenericInvoiceRowDescriptor
    /// </summary>
    public class WaveRawReader
    {
        public int ReadRowCount { get; private set; }

        public WaveRowDescriptor? RowDescriptor { get; private set; }

        public IEnumerable<TransactionRow<WaveRowDescriptor>> GetRows(StreamReader sr, BaseTransactionTableDescriptor<WaveRowDescriptor> tableDescriptor, bool includeIncome)
        {
            RowDescriptor = tableDescriptor.RowDescriptor;
            DateTime? date = default;
            string description = "";
            string account = "";
            string category = "";
            decimal amount = default;
            int readStatus = 0;
            ReadRowCount = 0;
            bool isIncome = false;
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line == null)
                {
                    continue;
                }
                line = line.Trim();
                if (line == "")
                {
                    if (readStatus == 5)
                    {
                        if (includeIncome || !isIncome)
                        {
                            foreach (var r in GenerateRows(tableDescriptor, date, amount, account!, category!, description))
                            {
                                yield return r;
                            }
                        }
                        ReadRowCount++;

                        readStatus = 0;
                    }
                    continue;
                }
                switch (readStatus)
                {
                    case 0: date = ParseDate(line); 
                        if (!date.HasValue)
                        {
                            if (ReadRowCount == 0)
                            {
                                continue;
                            }
                            throw new ArgumentException($"Bad date value: {line}.");
                        }
                        break;
                    case 1: description = line; break;
                    case 2: account = line; break;
                    case 3: category = line; break;
                    case 4:
                        if (decimal.TryParse(line.TrimStart('$'), out amount))
                        {
                            // Amounts in raw txt copied from wave have lost the signs.
                            amount = Math.Abs(amount); // Still make sure it is positive first. 
                            isIncome = GuessIfIsIncome(category);
                            bool isRefund = false;
                            if (!isIncome)
                            {
                                isRefund = GuessIfIsRefund(description);
                            }
                            var positive = RowDescriptor.PositiveAmountForCashOut ^ (isIncome||isRefund);
                            if (!positive)
                            {
                                amount = -amount;
                            }
                        }
                        else
                        {
                            throw new ArgumentException($"Bad amount value: {line}.");
                        }
                        break;
                    default: 
                        throw new ArgumentException($"Unexpected value line: {line}.");
                }
                readStatus++;
            }
        }

        private static IEnumerable<TransactionRow<WaveRowDescriptor>> GenerateRows(BaseTransactionTableDescriptor<WaveRowDescriptor> tableDescriptor, DateTime? date, decimal amount, string account, string category, string description)
        {
            var rowDescriptor = tableDescriptor.RowDescriptor;

            var startIndex = description.IndexOf('{');
            var endIndex = description.IndexOf('}', startIndex + 1);
            if (startIndex >= 0 && endIndex >= 0)
            {
                for (var i = startIndex + 1; i < endIndex;)
                {
                    var j = description.IndexOfAny(new[] { '+', '-', '}' }, i + 1);
                    var subs = description.Substring(i, j - i);
                    var subamount = decimal.Parse(subs);

                    if (!rowDescriptor.PositiveAmountForCashOut)
                    { 
                        subamount = -subamount;
                    }

                    var r = new TransactionRow<WaveRowDescriptor>(tableDescriptor);
                    r[rowDescriptor.DateTimeKey] = date.ToString();
                    r[rowDescriptor.AmountKey] = subamount.ToString();
                    r[rowDescriptor.WaveAccountKey] = account!;
                    r[rowDescriptor.WaveCategoryKey] = category!.ToString();
                    r[rowDescriptor.WaveDescriptionKey] = description!;
                    r[rowDescriptor.BusinessClaimableAmountKey] = amount.ToString();

                    yield return r;

                    i = j;
                }
            }
            else
            {
                var r = new TransactionRow<WaveRowDescriptor>(tableDescriptor);
                r[rowDescriptor.DateTimeKey] = date.ToString();
                r[rowDescriptor.AmountKey] = amount.ToString();
                r[rowDescriptor.WaveAccountKey] = account!;
                r[rowDescriptor.WaveCategoryKey] = category!.ToString();
                r[rowDescriptor.WaveDescriptionKey] = description!;

                yield return r;
            }
        }

        private static bool GuessIfIsIncome(string category)
        {
            return StringUtility.ContainsCaseIgnored(category, "Payment from");
        }

        private static bool GuessIfIsRefund(string description)
        {
            return StringUtility.ContainsWholeWord(description, "refund", true);
        }

        private static DateTime? ParseDate(string sDate)
        {
            sDate = sDate.Replace(',', ' ');
            var split = sDate.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 3)
            {
                return null;
            }
            var monthStr = split[0];
            var month = MonthStrToInt(monthStr);
            var dayOfMonthStr = split[1];
            var yearStr = split[2];
            if (!month.HasValue || !int.TryParse(yearStr, out var year) || !int.TryParse(dayOfMonthStr, out var dayOfMonth))
            {
                return null;
            }

            return new DateTime(year, month.Value, dayOfMonth);
        }

        private static int? MonthStrToInt(string sMonth)
        {
            return sMonth.ToLower() switch
            {
                "jan" => 1,
                "feb" => 2,
                "mar" => 3,
                "apr" => 4,
                "may" => 5,
                "jun" => 6,
                "jul" => 7,
                "aug" => 8,
                "sep" => 9,
                "oct" => 10,
                "nov" => 11,
                "dec" => 12,
                _ => null
            };
        }
    }
}
