using KMPBusinessRelationship.Objects;

namespace KMPBusinessRelationship
{
    public class ServiceManager
    {
        ServiceManager(Repository repo)
        {
            Repository = repo;
        }

        Repository Repository { get; }

        public void AcceptReferral(Client client, GeneralPractitioner referringGP)
        {
            Repository.SearchOrAddClient(client, out var clientInRepo);
            Repository.Events.Add(new Referral(referringGP, client));
        }
    }
}
