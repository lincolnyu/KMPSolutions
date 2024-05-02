namespace KMPBusinessRelationship.Objects
{
    public interface IService
    {
        public Booking? Booking { get; set; }

        public Client Client { get; set; }
    }
}
