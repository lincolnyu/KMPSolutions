using System;
using KMPBookingCore.Database;

namespace KMPBookingCore.DbObjects
{
    [DbClass]
    public class Event : DbObject
    {
        private int _id;
        private Client _client;
        private DateTime? _eventDate;
        private string _type;

        [DbPrimaryKey]
        public int Id
        {
            get => _id; set
            {
                _id = value;
                RaiseEventChanged("Id");
            }
        }
        [DbField]
        public Client Client
        {
            get => _client; set
            {
                _client = value;
                RaiseEventChanged("Client");
            }
        }
        [DbField]
        public DateTime? EventDate
        {
            get => _eventDate; set
            {
                _eventDate = value;
                RaiseEventChanged("EventDate");
            }
        }
        [DbField]
        public string Type
        {
            get => _type; set
            {
                _type = value; RaiseEventChanged("Type");
            }
        }
    }
}
