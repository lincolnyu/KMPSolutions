using System;

namespace KMPBusinessRelationship.Objects
{
    public interface IService
    {
        public DateTime? Time { get; set; }

        public Booking? Booking { get; set; }

        public Client Client { get; set; }
    }
}
