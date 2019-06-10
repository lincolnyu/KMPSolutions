using System;

namespace KMPBookingCore
{
    public enum Gender
    {
        Unspecified,
        Male,
        Female,
        Nonbinary
    }

    public class ClientRecord : IEquatable<ClientRecord>
    {
        public ClientRecord()
        {
        }
        public string Id { get; set; }

        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string MedicareNumber { get; set; }
        public DateTime? DOB { get; set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }

        public static Gender ParseGender(string g)
        {
            if (g.StartsWith("M")) return Gender.Male;
            if (g.StartsWith("F")) return Gender.Female;
            if (g.StartsWith("N")) return Gender.Nonbinary;
            return Gender.Unspecified;
        }

        public static string ToString(Gender g)
        {
            switch (g)
            {
                case Gender.Male: return "M";
                case Gender.Female: return "F";
                case Gender.Nonbinary: return "N";
                default: return "U";
            }
        }

        public bool Equals(ClientRecord other)
            => FirstName == other.FirstName
            && Surname == other.Surname
            && MedicareNumber == other.MedicareNumber
            && Equals(DOB, other.DOB)
            && Gender == other.Gender
            && PhoneNumber == other.PhoneNumber
            && Address == other.Address;
    }
}

