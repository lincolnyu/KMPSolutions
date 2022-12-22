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
            foreach (var (_,customer) in crm.Customers)
            {
                var sb = new StringBuilder();
                sb.Append(customer.MedicareId);
                sb.Append(",");
                sb.Append(customer.FirstName); sb.Append(",");

            }
        }
    }
}
