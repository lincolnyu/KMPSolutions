using System;

namespace KMPBusinessRelationship.Objects
{
    public class Client : Person
    {
        public string MedicareNumber { get; set; } = "";    // Current Medicare number including ref number separated by slash

        public string Name { get; set; } = "";  // Surname, Givn Name (including middle name separated by 'space')
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = "";        // 'M' and 'F' for male and female
        public string PhoneNumber { get; set; } = "";   // Current primary phone number
        public string Address { get; set; } = "";
    }
}
