using System.Text;

namespace KmpCrmCore
{
    public class CsvSerializerV1
    {
        public CrmRepository Deserialize(StreamReader sr)
        {
            var crm = new CrmRepository();
            while (!sr.EndOfStream)
            {
                var line = sr.ReadLine();
                if (line == null) continue;
                line = line.Trim();
                if (line.StartsWith("Medicare"))
                {
                    continue;
                }
                var split = line.BreakLine(true).ToArray();
                if (split.Length > 0)
                {
                    var medicareNumber = split[0];
                    if (crm.FindOrAddCustomer(medicareNumber, out var customer))
                    {
                        throw new ArgumentException($"Duplicate customer not supported, medicare number: {medicareNumber}.");
                    }
                    customer.MedicareId = medicareNumber;
                    customer.FirstName = split[1];
                    customer.Surname = split[2];
                    customer.Gender= split[3];
                    customer.DateOfBirth = DateOnly.Parse(split[4]);
                    customer.PhoneNumber = split[5];
                    customer.Address = split[6];
                    var expectedServicesCount = split[7].ParseInt();
                    var currentCount = split[8].ParseInt();
                    customer.InitialLetter = split[9].ParseYes();
                    ParseSeenOn(expectedServicesCount, currentCount, split[10], customer);
                    var gpName = split[11];
                    var gpProviderNumber = split[12];
                    var found = crm.FindOrAddGp(gpProviderNumber, out var gp);
                    if (found)
                    {
                        if (!string.Equals(gp.Name, gpName, StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ArgumentException($"Inconsistent GP name for provider {gpProviderNumber}.");
                        }
                    }
                    else
                    {
                        gp.Name = gpName;
                    }
                    customer.ReferringDate = DateOnly.Parse(split[13]);
                }
            }
            return crm;
        }

        void ParseSeenOn(CommentedValue<int?> expectedServicesCount, CommentedValue<int?> currentCount, string seenOn, Customer customer)
        {
            seenOn = seenOn.Replace('\n', ';');
            var visitBatch = new VisitBatch
            {
                ExpectedVisits = expectedServicesCount.Value
            };

            var sb = new StringBuilder("[2020 legacy data]");
            sb.Append("EV: (");
            if (expectedServicesCount.Value.HasValue)
            {
                sb.Append($"{expectedServicesCount.Value.Value}");
            }
            sb.Append(",");
            sb.Append($"{expectedServicesCount.Comments}");
            sb.Append("AV: (");
            if (currentCount.Value.HasValue)
            {
                sb.Append($"{currentCount.Value.Value}");
            }
            sb.Append(",");
            sb.Append($"{currentCount.Comments}");
            sb.Append($"SeenOn: {seenOn}");
            visitBatch.Remarks = sb.ToString();
            customer.VisitBatches.Add(visitBatch);
        }
    }
}
