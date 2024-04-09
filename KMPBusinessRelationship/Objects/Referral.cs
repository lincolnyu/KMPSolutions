namespace KMPBusinessRelationship.Objects
{
    public class Referral : Event
    {
        public Referral(GeneralPractitioner referringGP, Client client)
        {
            ReferringGP = referringGP;
            Client = client;
        }

        public GeneralPractitioner ReferringGP { get; set; }
        public Client Client { get; set; }
    }
}
