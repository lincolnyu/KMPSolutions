namespace KMPBusinessRelationship.Objects
{
    public class Referral : Event
    {
        public Referrer Referrer { get; set; }
        public Client Client { get; set; }
    }
}
