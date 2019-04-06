using System;
using System.Linq;
using System.Collections.Generic;

namespace BookingCore
{
    public class ClientRecords
    {
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

        public ClientRecord FindByName(string firstName, string surname)
            => _records.FirstOrDefault(x => x.FirstName == firstName && x.Surname == surname);
 
        public ClientRecord FindByMedicareNumber(string medicareNumber)
            => _records.FirstOrDefault(x => x.MedicareNumber == medicareNumber);

        public ClientRecord FindByPhoneNumber(string phoneNumber)
            => _records.FirstOrDefault(x => x.PhoneNumber == phoneNumber);

        List<ClientRecord> _records = new List<ClientRecord>();
    }
}
