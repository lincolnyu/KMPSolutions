namespace KMPBusinessRelationship.Objects
{
    public class Referral : Event
    {
        public GeneralPractitioner ReferringGP { get; set; }
        public Client Client { get; set; }
    }
}
