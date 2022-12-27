using System.Text;

namespace KmpCrmCore
{
    public class CsvSerializer
    {
        public CrmRepository Deserialize(StreamReader sr)
        {
            throw new NotImplementedException();
        }

        public void Serialize(CrmRepository crm, StreamWriter sw)
        {
            sw.WriteLine("Medicare Id,First Name,Surname,Gender,DOB,Phone,Address,Initial Letter,Visits,GP Name,GP Provider Id,Referring Date,");
            var sb = new StringBuilder();
            foreach (var (_,customer) in crm.Customers)
            {
                sb.Append(customer.MedicareId); sb.Append(","); 
                sb.Append(customer.FirstName); sb.Append(",");
                sb.Append(customer.Surname); sb.Append(",");
                sb.Append(customer.Gender); sb.Append(",");
                sb.Append(customer.DateOfBirth?.DateToString()?? ""); sb.Append(",");
                sb.Append(customer.PhoneNumber); sb.Append(",");
                sb.Append(customer.Address.CsvEscape()); sb.Append(",");
                sb.Append(customer.InitialLetter.ToCsvField(false)); sb.Append(",");
                sb.Append(customer.VisitBatches.VisitsToCsvField()); sb.Append(",");
                sb.Append(customer.ReferingGP?.Name?? ""); sb.Append(",");
                sb.Append(customer.ReferingGP?.ProviderId?? ""); sb.Append(",");
                sb.Append(customer.ReferringDate?.DateToString()?? ""); sb.Append(",");
                sw.WriteLine(sb.ToString());
                sb.Clear();
            }
        }
    }
}


