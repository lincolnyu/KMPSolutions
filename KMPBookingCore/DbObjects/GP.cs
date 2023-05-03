using KMPBookingCore.Database;

namespace KMPBookingCore.DbObjects
{
    [DBClass]
    public class GP : DbObject
    {
        private string _id;
        private string _name;
        private string _address;
        private string _phone;
        private string _fax;

        [DBPrimaryKey]
        public string Id
        {
            get => _id; set
            {
                _id = value;
                RaiseEventChanged(nameof(Id));
            }
        }

        [DBField]
        public string Name
        {
            get => _name; set
            {
                _name = value;
                RaiseEventChanged(nameof(Name));
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
        public string Phone
        {
            get => _phone; set
            {
                _phone = value;
                RaiseEventChanged(nameof(Phone));
            }
        }
        [DBField]
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
