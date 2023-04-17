using System;

namespace KMPBookingCore
{
    public class Client : IEquatable<Client>
    {
        public Client()
        {
        }

        public string MedicareNumber { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public DateTime? DOB { get; set; }
        public string Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public bool Equals(Client other)
            => FirstName == other.FirstName
            && Surname == other.Surname
            && MedicareNumber == other.MedicareNumber
            && Equals(DOB, other.DOB)
            && Gender == other.Gender
            && PhoneNumber == other.PhoneNumber
            && Address == other.Address;
    }
}

