using System;
using System.IO;
using System.Linq;
using System.Text;

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

        void ParseSeenOn(CommentedValue<int?> expectedServicesCount, CommentedValue<int?> currentCount, string seenOn, Customer customer)
        {
            //seenOn = seenOn.Replace('\n', ';');
            //var visitBatch = new VisitBatch
            //{
            //    ExpectedVisits = expectedServicesCount.Value
            //};

            //// Would put all info in comments for convenience
            //var sb = new StringBuilder("[2020 legacy data]");
            //sb.Append("EV:(");
            //if (expectedServicesCount.Value.HasValue)
            //{
            //    sb.Append($"{expectedServicesCount.Value.Value}");
            //}
            //sb.Append("|");
            //sb.Append($"{expectedServicesCount.Comments}");
            //sb.Append(")AV:(");
            //if (currentCount.Value.HasValue)
            //{
            //    sb.Append($"{currentCount.Value.Value}");
            //}
            //sb.Append("|");
            //sb.Append($"{currentCount.Comments}");
            //sb.Append($")SeenOn:{seenOn}");
            //customer.VisitBatches.Add(new CommentedValue<VisitBatch>(visitBatch, sb.ToString()));

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
