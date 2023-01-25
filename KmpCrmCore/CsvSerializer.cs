using System;
using System.IO;
using System.Linq;
using System.Text;

namespace KmpCrmCore
{
    using DateOnly = DateTime;

    public class CsvSerializer
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
                customer.Gender = split[3];
                if (DateOnly.TryParse(split[4], out var dob))
                {
                    customer.DateOfBirth = dob;
                }
                customer.PhoneNumber = split[5];
                customer.Address = split[6];
                customer.InitialLetter = split[7].ParseYes(true);

                foreach (var vb in split[8].CsvFieldToVisits())
                {
                    customer.VisitBatches.Add(vb);
                }

                var gpName = split[9];
                var gpProviderNumber = split[10];
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
                if (DateOnly.TryParse(split[11], out var referringDate))
                {
                    customer.ReferringDate = referringDate;
                }

                customer.LegacyData = split[12];
            }
            return crm;
        }

        public void Serialize(CrmRepository crm, StreamWriter sw)
        {
            sw.WriteLine("Medicare Number,First Name,Surname,Gender,DOB,Phone,Address,Initial Letter,Visits,GP Name,GP Provider Number,Referring Date,Legacy Data");
            var sb = new StringBuilder();
            foreach (var kvp in crm.Customers)
            {
                var customer = kvp.Value;
                sb.Append(customer.MedicareNumber); sb.Append(","); 
                sb.Append(customer.FirstName); sb.Append(",");
                sb.Append(customer.Surname); sb.Append(",");
                sb.Append(customer.Gender); sb.Append(",");
                sb.Append(customer.DateOfBirth?.DateToString()?? ""); sb.Append(",");
                sb.Append(customer.PhoneNumber); sb.Append(",");
                sb.Append(customer.Address.CsvEscape()); sb.Append(",");
                sb.Append(customer.InitialLetter.ToCsvField(false)); sb.Append(",");
                sb.Append(customer.VisitBatches.VisitsToCsvField()); sb.Append(",");
                sb.Append(customer.ReferingGP?.Name?? ""); sb.Append(",");
                sb.Append(customer.ReferingGP?.ProviderNumber?? ""); sb.Append(",");
                sb.Append(customer.ReferringDate?.DateToString()?? ""); sb.Append(",");
                sb.Append(customer.LegacyData?.CsvEscape()?? ""); sb.Append(",");
                sw.WriteLine(sb.ToString());
                sb.Clear();
            }
        }
    }
}
