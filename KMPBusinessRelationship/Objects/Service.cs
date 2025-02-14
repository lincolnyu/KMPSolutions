namespace KMPBusinessRelationship.Objects
{
    public abstract class Service<T> : Event<T>, IService where T : Service<T>
    {
        public Booking? Booking { get; set; }

        public Client Client { get; set; }

        protected bool Equals(Service<T> other)
        {
            if (Booking?.Id != other.Booking?.Id) return false;
            if (Client.Id != other.Client.Id) return false;
            return true;
        }
    }
}
