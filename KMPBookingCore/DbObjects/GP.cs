﻿using KMPBookingCore.Database;

namespace KMPBookingCore.DbObjects
{
    [DbClass]
    public class GP : DbObject
    {
        private string _id;
        private string _name;
        private string _address;
        private string _phone;
        private string _fax;

        [DbPrimaryKey]
        public string ProviderNumber
        {
            get => _id; set
            {
                _id = value;
                RaiseEventChanged(nameof(ProviderNumber));
            }
        }

        [DbField]
        public string Name
        {
            get => _name; set
            {
                _name = value;
                RaiseEventChanged(nameof(Name));
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
        public string Phone
        {
            get => _phone; set
            {
                _phone = value;
                RaiseEventChanged(nameof(Phone));
            }
        }
        [DbField]
        public string Fax
        {
            get => _fax; set
            {
                _fax = value;
                RaiseEventChanged(nameof(Fax));
            }
        }
    }
}
