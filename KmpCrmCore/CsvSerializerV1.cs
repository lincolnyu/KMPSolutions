using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KmpCrmCore
{
    using DateOnly = DateTime;

    public class CsvSerializerV1
    {
        public CrmRepository Deserialize(StreamReader sr)
        {
            var crm = new CrmRepository();
            var medicareAnonCount = 1;
            while (!sr.EndOfStream)
            {
                var split = sr.GetAndBreakRow(true).ToList();
                while (split.Count < 14)
                {
                    split.Add("");
                }
                if (split[0] == "Medicare Number") continue;
                var medicareNumber = split[0];
                if (string.IsNullOrEmpty(medicareNumber))
                {
                    medicareNumber = "ANON_" + medicareAnonCount.ToString();
                    medicareAnonCount++;
                }   
                if (crm.FindOrAddCustomer(medicareNumber, out var customer))
                {
                    throw new ArgumentException($"Duplicate customer not supported, medicare number: {medicareNumber}.");
                }
                customer.MedicareNumber = medicareNumber;
                customer.FirstName = split[1];
                customer.Surname = split[2];
                customer.Gender= split[3];
                if (DateOnly.TryParse(split[4], out var dob))
                {
                    customer.DateOfBirth = dob;
                }
                customer.PhoneNumber = split[5];
                customer.Address = split[6];
                var expectedServicesCount = split[7].ParseInt();
                var currentCount = split[8].ParseInt();
                customer.InitialLetter = split[9].ParseYes();
                customer.InitialLetter.Comments = customer.InitialLetter.Comments.TrimStart(' ', ':', '-');
                ParseSeenOn(expectedServicesCount, currentCount, split[10], customer);
                var gpName = split[11];
                var gpProviderNumber = split[12];
                if (gpProviderNumber != "")
                {
                    var found = crm.FindOrAddGp(gpProviderNumber, out var gp);
                    if (found)
                    {
                        if (!string.Equals(gp.Name, gpName, StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine($"Warning: Inconsistent GP name '{gpName}' for number {gpProviderNumber}. '{gp.Name}' will be used.");
                        }
                    }
                    else
                    {
                        gp.Name = gpName;
                    }
                    customer.ReferingGP = gp;
                }
                if (DateOnly.TryParse(split[13], out var referringDate))
                {
                    customer.ReferringDate = referringDate;
                }
            }
            return crm;
        }

        static IEnumerable<DateOnly> GetDates(string input, int defaultYear=2020)
        {
            var rexDate = new Regex(@"\d+(\s*/\s*\d+){1,2}");
            var matches = rexDate.Matches(input);
            foreach (Match match in matches)
            {
                var split = match.Value.Split('/');
                if (split.Length == 3)
                {
                    if (DateOnly.TryParse(match.Value, out var date))
                    {
                        yield return date;
                    }
                }
                else
                {
                    // Date format dependent
                    var dayStr = split[0].Trim();
                    var monthStr = split[1].Trim();
                    if (int.TryParse(dayStr, out var day) && int.TryParse(monthStr, out var month))
                    {
                        yield return new DateOnly(defaultYear, month, day);
                    }
                }
            }
        }

        void ParseSeenOn(CommentedValue<int?> expectedServicesCount, CommentedValue<int?> currentCount, string seenOn, Customer customer)
        {
            var rexSeenOnKeyword = new Regex("(?i)seen[ ]*on");
            var rexClaimKeyword = new Regex("(?i)claim[ ]*on");

            var matchSeenOn = rexSeenOnKeyword.Match(seenOn);
            var matchClaim = rexClaimKeyword.Match(seenOn);

            int? indexSeenOn = null;
            int? indexClaim = null;
            if (matchSeenOn.Success)
            {
                indexSeenOn = matchSeenOn.Index + matchSeenOn.Length;
            }
            if (matchClaim.Success)
            {
                indexClaim = matchClaim.Index + matchClaim.Length;
            }

            var visitBatch = new VisitBatch
            {
                ExpectedVisits = expectedServicesCount.Value
            };

            if (indexSeenOn.HasValue && indexClaim.HasValue)
            {
                if (indexSeenOn.Value < indexClaim.Value)
                {
                   
                    GetDates(seenOn.Substring(indexSeenOn.Value, indexClaim.Value - indexSeenOn.Value)).ToList().ForEach(x => visitBatch.VisitsMade.Add(new CommentedValue<DateOnly>(x)));
                    GetDates(seenOn.Substring(indexClaim.Value)).ToList().ForEach(x => visitBatch.ClaimsMade.Add(new CommentedValue<DateOnly>(x)));
                }
                else
                {
                    GetDates(seenOn.Substring(indexClaim.Value, indexSeenOn.Value - indexClaim.Value)).ToList().ForEach(x => visitBatch.ClaimsMade.Add(new CommentedValue<DateOnly>(x)));
                    GetDates(seenOn.Substring(indexSeenOn.Value)).ToList().ForEach(x => visitBatch.VisitsMade.Add(new CommentedValue<DateOnly>(x)));
                }
            }
            else if (indexSeenOn.HasValue)
            {
                GetDates(seenOn.Substring(indexSeenOn.Value)).ToList().ForEach(x => visitBatch.VisitsMade.Add(new CommentedValue<DateOnly>(x)));
            }
            else if (indexClaim.HasValue)
            {
                GetDates(seenOn.Substring(indexClaim.Value)).ToList().ForEach(x => visitBatch.ClaimsMade.Add(new CommentedValue<DateOnly>(x)));
                GetDates(seenOn.Substring(0, indexClaim.Value)).ToList().ForEach(x => visitBatch.VisitsMade.Add(new CommentedValue<DateOnly>(x)));
            }
            else
            {
                GetDates(seenOn).ToList().ForEach(x => visitBatch.VisitsMade.Add(new CommentedValue<DateOnly>(x)));
            }
            customer.VisitBatches.Add(new CommentedValue<VisitBatch>(visitBatch));

            var sb = new StringBuilder("[2020 legacy data]");
            sb.Append("EV:(");
            if (expectedServicesCount.Value.HasValue)
            {
                sb.Append($"{expectedServicesCount.Value.Value}");
            }
            sb.Append("|");
            sb.Append($"{expectedServicesCount.Comments}");
            sb.Append(")AV:(");
            if (currentCount.Value.HasValue)
            {
                sb.Append($"{currentCount.Value.Value}");
            }
            sb.Append("|");
            sb.Append($"{currentCount.Comments}");
            sb.Append($")SeenOn:{seenOn}");
            customer.LegacyData = sb.ToString();
        }
    }
}

