using System;

namespace KMPBookingCore
{
    public class Event : DbObject
    {
        private int _id;
        private Client _client;
        private DateTime? _eventDate;
        private string _type;

        public int Id { 
            get => _id; set { 
                _id = value;
                RaiseEventChanged("Id"); 
            }
        }
        public Client Client { 
            get => _client; set { 
                _client = value;
                RaiseEventChanged("Client"); 
            }
        }
        public DateTime? EventDate {
            get => _eventDate; set {
                _eventDate = value;
                RaiseEventChanged("EventDate"); 
            } 
        }
        public string Type { 
            get => _type; set { 
                _type = value; RaiseEventChanged("Type"); 
            } 
        }
    }
}
