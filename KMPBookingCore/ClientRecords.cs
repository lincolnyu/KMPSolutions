using System;
using System.Linq;
using System.Collections.Generic;

namespace KMPBookingCore
{
    public class ClientRecords
    {
        private readonly List<ClientRecord> _records = new List<ClientRecord>();

        public ClientRecords()
        {
        }

        public void Clear()
        {
            _records.Clear();
        }

        public void Add(ClientRecord record)
        {
            _records.Add(record);
        }

        public IEnumerable<ClientRecord> FindByName(string firstName, string surname)
            => _records.FindByName(firstName, surname);

        public IEnumerable<ClientRecord> FindNameContaining(string nameSubstr)
            => _records.FindNameContaining(nameSubstr);

        public ClientRecord FindByMedicareNumber(string medicareNumber)
            => _records.FirstOrDefault(x => x.MedicareNumber == medicareNumber);

        public IEnumerable<ClientRecord> FindByMedicareNumberContaining(string medSubstr)
            => _records.FindByMedicareNumberContaining(medSubstr);

        public IEnumerable<ClientRecord> FindByPhoneNumber(string phoneNumber)
            => _records.Where(x => x.PhoneNumber == phoneNumber);

        public IEnumerable<ClientRecord> FindByPhoneNumberContaining(string numberSubstr)
            => _records.Where(x => x.PhoneNumber.Contains(numberSubstr));

        public IReadOnlyList<ClientRecord> Records()
            => _records;
    }
}
