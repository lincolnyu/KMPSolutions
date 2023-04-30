namespace KMPBookingCore
{
    public class GP : DbObject
    {
        private string _id;
        private string _name;
        private string _address;
        private string _phone;
        private string _fax;

        public string Id
        {
            get => _id; set
            {
                _id = value;
                RaiseEventChanged(nameof(Id));
            }
        }
        public string Name
        {
            get => _name; set
            {
                _name = value;
                RaiseEventChanged(nameof(Name));
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
        public string Phone
        {
            get => _phone; set
            {
                _phone = value;
                RaiseEventChanged(nameof(Phone));
            }
        }
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
