namespace KmpCrmCore
{
    public class CrmRepository
    {
        public Dictionary<string, Customer> Customers { get; private set; } = new Dictionary<string, Customer>();
        public Dictionary<string, Gp> Gps { get; private set; } = new Dictionary<string, Gp>();

        public bool FindOrAddCustomer(string medicareNumber, out Customer customer)
        {
            if (Customers.TryGetValue(medicareNumber, out customer))
            {
                return true;
            }
            customer = new Customer
            {
                MedicareId = medicareNumber
            };
            Customers[medicareNumber] = customer;
            return false;
        }

        public bool FindOrAddGp(string providerNumber, out Gp gp)
        {
            if (Gps.TryGetValue(providerNumber, out gp))
            {
                return true;
            }
            gp = new Gp
            {
                ProviderId = providerNumber
            };
            Gps[providerNumber] = gp;
            return false;
        }
    }
}
