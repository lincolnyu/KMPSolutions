using System.Linq;
using System.Collections.Generic;
using KMPBookingCore.DbObjects;

namespace KMPBookingCore
{
    public class ClientRecords
    {
        private readonly List<Client> _records = new List<Client>();

        public ClientRecords()
        {
        }

        public void Clear()
        {
            _records.Clear();
        }

        public void Add(Client record)
        {
            _records.Add(record);
        }

        public IEnumerable<Client> FindByName(string firstName, string surname)
            => _records.FindByName(firstName, surname);

        public IEnumerable<Client> FindNameContaining(string nameSubstr)
            => _records.FindNameContaining(nameSubstr);

        public Client FindByMedicareNumber(string medicareNumber)
            => _records.FirstOrDefault(x => x.MedicareNumber == medicareNumber);

        public IEnumerable<Client> FindByMedicareNumberContaining(string medSubstr)
            => _records.FindByMedicareNumberContaining(medSubstr);

        public IEnumerable<Client> FindByPhoneNumber(string phoneNumber)
            => _records.Where(x => x.PhoneNumber == phoneNumber);

        public IEnumerable<Client> FindByPhoneNumberContaining(string numberSubstr)
            => _records.Where(x => x.PhoneNumber.Contains(numberSubstr));

        public IReadOnlyList<Client> Records()
            => _records;
    }
}
