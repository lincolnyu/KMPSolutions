namespace KMPBusinessRelationship.Objects
{
    public class Service : Event
    {
        public Booking? Booking { get; set; }

        public Client Client { get; set; }
    }
}
