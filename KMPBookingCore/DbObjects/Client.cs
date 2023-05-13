﻿using System;
using System.Collections.Generic;
using KMPBookingCore.Database;

namespace KMPBookingCore.DbObjects
{
    [DbClass]
    public class Client : DbObject, IEquatable<Client>
    {
        public Client()
        {
        }

        [DbPrimaryKey]
        public string MedicareNumber
        {
            get => _medicareNumber; set
            {
                _medicareNumber = value;
                RaiseEventChanged(nameof(MedicareNumber));
            }
        } // Client ID

        [DbField]
        public string FirstName
        {
            get => _firstName; set
            {
                _firstName = value;
                RaiseEventChanged(nameof(FirstName));
            }
        }
        [DbField]
        public string Surname
        {
            get => _surname; set
            {
                _surname = value;
                RaiseEventChanged(nameof(Surname));
            }
        }
        [DbField]
        public DateTime? DOB
        {
            get => _dob; set
            {
                _dob = value;
                RaiseEventChanged(nameof(DOB));
            }
        }
        [DbField]
        public string Gender
        {
            get => _gender; set
            {
                _gender = value;
                RaiseEventChanged(nameof(Gender));
            }
        }
        [DbField]
        public string Phone
        {
            get => _phone; set
            {
                _phone = value;
                RaiseEventChanged(nameof(Phone));
            }
        }
        [DbField]
        public string Address
        {
            get => _address; set
            {
                _address = value;
                RaiseEventChanged(nameof(Address));
            }
        }

        [DbField]
        public GP ReferringGP
        {
            get => _referringGP; set
            {
                _referringGP = value;
                RaiseEventChanged(nameof(ReferringGP));
            }
        }
        [DbField]
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
            && Phone == other.Phone
            && Address == other.Address;

        public List<Event> Events = new List<Event>();

        private string _medicareNumber;
        private string _firstName;
        private string _surname;
        private DateTime? _dob;
        private string _gender;
        private string _phone;
        private string _address;
        private GP _referringGP;
        private DateTime? _referringDate;
    }
}

