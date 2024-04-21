using System;

namespace KMPBusinessRelationship.Objects
{
    public class Client : Person
    {
        public string Id { get => CareNumber; set => CareNumber = value; }


        /// <summary>
        ///  An ID that identifies the client. Usually a medicare number or a DVA number.
        ///  DVA number starts with prefix 'DVA'.
        /// </summary>
        public string CareNumber { get; set; }

        /// <summary>
        ///  Surname, Givn Name (including middle name separated by 'space').
        /// </summary>
        public string Name { get; set; } = "";

        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        ///  'M' and 'F' for male and female.
        /// </summary>
        public string Gender { get; set; } = "";

        /// <summary>
        ///  Current primary phone number.
        /// </summary>
        public string PhoneNumber { get; set; } = "";
        public string Address { get; set; } = "";
    }
}
