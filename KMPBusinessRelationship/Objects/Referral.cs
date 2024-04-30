namespace KMPBusinessRelationship.Objects
{
    public class Referral : Event<Referral>
    {
        public Referrer Referrer { get; set; }
        public Client Client { get; set; }

        public override bool Equals(Referral other)
        {
            if (Referrer.Id != other.Referrer.Id) return false;
            if (Client.Id != other.Client.Id) return false;
            return true;
        }
    }
}
