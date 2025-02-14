namespace KMPBusinessRelationship.Objects
{
    public class Referral : Event<Referral>
    {
        public string ReferrerId { get; set; }
        public Client Client { get; set; }

        public Referrer GetReferrer(BaseRepository repo)
        {
            return repo.IdToReferrerMap[ReferrerId];
        }

        public override bool Equals(Referral other)
        {
            if (ReferrerId != other.ReferrerId) return false;
            if (Client.Id != other.Client.Id) return false;
            return true;
        }
    }
}
