using System;
using System.Collections.Generic;
using KMPBookingCore.Database;

namespace KMPBookingCore.DbObjects
{
    [DBClass]
    public class Client : DbObject, IEquatable<Client>
    {
        public Client()
        {
        }

        [DBPrimaryKey]
        public string MedicareNumber
        {
            get => _medicareNumber; set
            {
                _medicareNumber = value;
                RaiseEventChanged(nameof(MedicareNumber));
            }
        } // Client ID

        [DBField]
        public string FirstName
        {
            get => _firstName; set
            {
                _firstName = value;
                RaiseEventChanged(nameof(FirstName));
            }
        }
        [DBField]
        public string Surname
        {
            get => _surname; set
            {
                _surname = value;
                RaiseEventChanged(nameof(Surname));
            }
        }
        [DBField]
        public DateTime? DOB
        {
            get => _dob; set
            {
                _dob = value;
                RaiseEventChanged(nameof(DOB));
            }
        }
        [DBField]
        public string Gender
        {
            get => _gender; set
            {
                _gender = value;
                RaiseEventChanged(nameof(Gender));
            }
        }
        [DBField]
        public string PhoneNumber
        {
            get => _phoneNumber; set
            {
                _phoneNumber = value;
                RaiseEventChanged(nameof(PhoneNumber));
            }
        }
        [DBField]
        public string Address
        {
            get => _address; set
            {
                _address = value;
                RaiseEventChanged(nameof(Address));
            }
        }

        [DBField]
        public GP ReferringGP
        {
            get => _referringGP; set
            {
                _referringGP = value;
                RaiseEventChanged(nameof(ReferringGP));
            }
        }
        [DBField]
        public DateTime? ReferringDate
        {
            get => _referringDate; set
            {
                _referringDate = value;
                RaiseEventChanged(nameof(ReferringDate));
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
        private GP _referringGP;
        private DateTime? _referringDate;
    }
}

