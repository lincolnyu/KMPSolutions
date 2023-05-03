using System;
using KMPBookingCore.Database;

namespace KMPBookingCore.DbObjects
{
    [DBClass]
    public class Event : DbObject
    {
        private int _id;
        private Client _client;
        private DateTime? _eventDate;
        private string _type;

        [DBPrimaryKey]
        public int Id
        {
            get => _id; set
            {
                _id = value;
                RaiseEventChanged("Id");
            }
        }
        [DBField]
        public Client Client
        {
            get => _client; set
            {
                _client = value;
                RaiseEventChanged("Client");
            }
        }
        [DBField]
        public DateTime? EventDate
        {
            get => _eventDate; set
            {
                _eventDate = value;
                RaiseEventChanged("EventDate");
            }
        }
        [DBField]
        public string Type
        {
            get => _type; set
            {
                _type = value; RaiseEventChanged("Type");
            }
        }
    }
}
