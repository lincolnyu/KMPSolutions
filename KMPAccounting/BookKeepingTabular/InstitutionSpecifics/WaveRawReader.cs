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

        public string FallbackBusinessAccount { get; set; } = "Uncategorized";

        public IEnumerable<TransactionRow<WaveRowDescriptor>> GetRows(StreamReader sr, TransactionTable<WaveRowDescriptor> tableDescriptor, bool includeIncome)
        {
            RowDescriptor = tableDescriptor.RowDescriptor;
            DateTime? date = default;
            string description = "";
            string account = "";
            string category = "";
            decimal amount = default;
            int readStatus = 0;
            ReadRowCount = 0;
            string inferredBusinessAccount = "";
            bool isIncome = false;
            var rowIndex = 0;
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
                            foreach (var r in GenerateRows(tableDescriptor, date, amount, account!, category!, description, inferredBusinessAccount, rowIndex))
                            {
                                yield return r;
                                rowIndex++;
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
                            inferredBusinessAccount = InferBusinessAccount(category);
                            isIncome = inferredBusinessAccount == KMPSpecifics.AccountConstants.Business.AccountSuffixes.Income;
                            bool isRefund = false;
                            if (!isIncome)
                            {
                                isRefund = GuessIfIsRefund(description);
                            }
                            var positive = isIncome||isRefund;
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

        private IEnumerable<TransactionRow<WaveRowDescriptor>> GenerateRows(TransactionTable<WaveRowDescriptor> table, DateTime? date, decimal amount, string account, string category, string description, string inferredBusinessAccount, int rowIndex)
        {
            var rowDescriptor = table.RowDescriptor;

            var startIndex = description.IndexOf('{');
            var endIndex = description.IndexOf('}', startIndex + 1);

            if (startIndex >= 0 && endIndex >= 0)
            {
                // Format:
                // 1. +/-<trn_amount1>[:<busi_account1>=<busi_amount1>;<busi_account2>=<busi_amount2>..]+/-<trn_amount2>[...
                // 2. <trn_amount>
                // The business account names used here should be the unified name that describe the business expense type and then they will be added under any appropriate account groups
                int nextI;
                decimal claimedBusinessAmount = 0;
                for (var i = startIndex + 1; i < endIndex; i = nextI)
                {
                    var j = description.IndexOfAny(new[] { '+', '-', '}' }, i + 1);
                    nextI = j;
                    var substr = description[i..j];
                    decimal subTotalAmount;
                    string businessClaimable = "";
                    string? message = null;
                    if (substr.Contains(Constants.CommonPrompt))
                    {
                        var ss = substr.Split(Constants.CommonPrompt);
                        subTotalAmount = decimal.Parse(ss[0]);

                        // If business component breakdown is specified the user is responsible for ensuring they add up to Amount.
                        if (ParseBusinessClaimable(ss[1], inferredBusinessAccount, out businessClaimable, out var claimedAmount))
                        {
                            claimedBusinessAmount += claimedAmount;
                        }
                        else
                        {
                            message = "Error: bad business claimable format.";
                        }
                    }
                    else
                    {
                        subTotalAmount = decimal.Parse(substr);

                        if (nextI >= endIndex && claimedBusinessAmount != amount)
                        {
                            // Last one
                            // TODO log a warning
                            message = "Warning: claimable business not specified or does not add up.";
                            businessClaimable = inferredBusinessAccount + "=" + (amount - claimedBusinessAmount).ToString();
                        }
                    }

                    // Income in invoice is always positive. The convention of business claimable strings is the opposite.
                    subTotalAmount = -subTotalAmount;

                    var r = new TransactionRow<WaveRowDescriptor>(table, rowIndex++);
                    r[rowDescriptor.DateTimeKey] = date.ToString();
                    r[rowDescriptor.AmountKey] = subTotalAmount.ToString();
                    r[rowDescriptor.WaveAccountKey] = account!;
                    r[rowDescriptor.WaveCategoryKey] = category!.ToString();
                    r[rowDescriptor.WaveDescriptionKey] = description!;
                    r[rowDescriptor.BusinessClaimableKey] = businessClaimable;
                    if (message != null)
                    {
                        r[Constants.MessageKey] = message;
                    }

                    yield return r;
                }
            }
            else
            {
                var r = new TransactionRow<WaveRowDescriptor>(table, rowIndex);
                r[rowDescriptor.DateTimeKey] = date.ToString();
                r[rowDescriptor.AmountKey] = amount.ToString();
                r[rowDescriptor.WaveAccountKey] = account!;
                r[rowDescriptor.WaveCategoryKey] = category!.ToString();
                r[rowDescriptor.WaveDescriptionKey] = description!;
                r[rowDescriptor.BusinessClaimableKey] = inferredBusinessAccount + "=" + amount.ToString();

                yield return r;
            }
        }

        private static bool GuessIfIsRefund(string description)
        {
            return StringUtility.ContainsWholeWord(description, "refund", true);
        }

        private string InferBusinessAccount(string category)
        {
            category = category.Trim();
            if (category.ContainsCaseIgnored("Payment from"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Income;
            }
            if (category.ContainsWholeWord("Office Supplies"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.OfficeSupplies;
            }
            else if (category.ContainsWholeWord("Medical Supplies"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.MedicalSupplies;
            }
            else if (category.ContainsCaseIgnored("Fuel"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Fuel;
            }
            else if (category.ContainsWholeWord("Vehicle") || category.ContainsWholeWord("Vehicles"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Vehicles;
            }
            else if (category.ContainsWholeWord("Uniform") || category.ContainsWholeWord("Uniforms"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Uniforms;
            }
            else if (category.ContainsWholeWord("Subscription") || category.ContainsWholeWord("Subscriptions"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Subscriptions;
            }
            else if (category.ContainsWholeWord("Advertising") || category.ContainsWholeWord("Promotion"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Promotion;
            }
            else if (category.ContainsWholeWord("Computer"))
            {
                // TODO update...
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Computer;
            }
            else if (category.ContainsWholeWord("Repairs"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Maintenance;
            }
            else if (category.ContainsWholeWord("Cleaning"))
            {
                return KMPSpecifics.AccountConstants.Business.AccountSuffixes.Cleaning;
            }
            return FallbackBusinessAccount;
        }

        private bool ParseBusinessClaimable(string businessSubstring, string buisnessAccount, out string businessClaimable, out decimal claimedAmount)
        {
            claimedAmount = 0;
            var slist = new List<string>();
            foreach (var s in businessSubstring.Split(Constants.CommonSeparator))
            {
                var ss = s.Split('=');
                if (ss.Length > 2)
                {
                    businessClaimable = "";
                    return false;
                }
                string account;
                decimal amount = 0;
                if (ss.Length == 2)
                {
                    account = ss[0];
                    amount = decimal.Parse(ss[1]);
                }
                else
                {
                    account = buisnessAccount;
                    amount = decimal.Parse(ss[0]);
                }

                // Income in invoice is always positive. The convention of business claimable strings is the opposite.
                amount = -amount;
                slist.Add($"{account}={amount}");
                claimedAmount += amount;
            }
            businessClaimable = string.Join(Constants.CommonSeparator, slist);
            return true;
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
