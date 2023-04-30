using System;
using System.Collections.Generic;

namespace KMPBookingCore
{
    public class Client : DbObject, IEquatable<Client>
    {
        public Client()
        {
        }

        public string MedicareNumber
        {
            get => _medicareNumber; set
            {
                _medicareNumber = value;
                RaiseEventChanged(nameof(MedicareNumber));
            }
        } // Client ID
        public string FirstName
        {
            get => _firstName; set
            {
                _firstName = value;
                RaiseEventChanged(nameof(FirstName));
            }
        }
        public string Surname
        {
            get => _surname; set
            {
                _surname = value;
                RaiseEventChanged(nameof(Surname));
            }
        }
        public DateTime? DOB
        {
            get => _dob; set
            {
                _dob = value;
                RaiseEventChanged(nameof(DOB));
            }
        }
        public string Gender
        {
            get => _gender; set
            {
                _gender = value;
                RaiseEventChanged(nameof(Gender));
            }
        }
        public string PhoneNumber
        {
            get => _phoneNumber; set
            {
                _phoneNumber = value;
                RaiseEventChanged(nameof(PhoneNumber));
            }
        }
        public string Address
        {
            get => _address; set
            {
                _address = value;
                RaiseEventChanged(nameof(Address));
            }
        }

        public GP ReferringGP
        {
            get => referringGP; set
            {
                referringGP = value;
                RaiseEventChanged(nameof(ReferringGP));
            }
        }

        public bool Equals(Client other)
            => FirstName == other.FirstName
            && Surname == other.Surname
            && MedicareNumber == other.MedicareNumber
            && Equals(DOB, other.DOB)
            && Gender == other.Gender
            && PhoneNumber == other.PhoneNumber
            && Address == other.Address;

        public List<Event> Events = new List<Event>();

        private string _medicareNumber;
        private string _firstName;
        private string _surname;
        private DateTime? _dob;
        private string _gender;
        private string _phoneNumber;
        private string _address;
        private GP referringGP;
    }
}

