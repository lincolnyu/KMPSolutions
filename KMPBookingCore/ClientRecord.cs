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

    public class ClientRecord
    {
        public ClientRecord()
        {
        }

        public string FirstName { get; set; }
        public string Surname { get; set; }
        public string MedicareNumber { get; set; }
        public DateTime? DOB { get; set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }

        public static Gender ParseGender(string g)
        {
            if (g.StartsWith("M")) return Gender.Male;
            if (g.StartsWith("F")) return Gender.Female;
            if (g.StartsWith("N")) return Gender.Nonbinary;
            return Gender.Unspecified;
        }
    }
}

