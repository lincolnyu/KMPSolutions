using System;
using System.Linq;
using System.Collections.Generic;

namespace KMPBookingCore
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

        public IEnumerable<ClientRecord> FindByName(string firstName, string surname)
            => _records.Where(x => x.FirstName == firstName && x.Surname == surname);

        public IEnumerable<ClientRecord> FindNameContaining(string nameSubstr)
        {
            if (nameSubstr.Contains(','))
            {
                (var fn, var sn) = nameSubstr.SmartParseName();
                return FindByName(fn, sn);
            }
            else
            {
                var segs = nameSubstr.Split(' ').Select(x => x.Trim().ToLower()).Where(x => x.Length > 0);
                return _records.Where(x => segs.All(y=>x.FirstName.ToLower().Contains(y) 
                    || x.Surname.ToLower().Contains(y)));
            }
        }

        public ClientRecord FindByMedicareNumber(string medicareNumber)
            => _records.FirstOrDefault(x => x.MedicareNumber == medicareNumber);

        public IEnumerable<ClientRecord> FindByMedicareNumberContaining(string medSubstr)
            => _records.Where(x => x.MedicareNumber.Contains(medSubstr));

        public IEnumerable<ClientRecord> FindByPhoneNumber(string phoneNumber)
            => _records.Where(x => x.PhoneNumber == phoneNumber);

        public IEnumerable<ClientRecord> FindByPhoneNumberContaining(string numberSubstr)
            => _records.Where(x => x.PhoneNumber.Contains(numberSubstr));

        List<ClientRecord> _records = new List<ClientRecord>();
    }
}
