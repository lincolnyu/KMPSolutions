using KMPBusinessRelationship;
using KMPBusinessRelationshipPersistence;

namespace KMPBusinessRelationshipTest
{
    public class CoreTests
    {
        BaseRepository repository_;

        [SetUp]
        public void Setup()
        {
            repository_ = new Repository();
        }

        [Test]
        public void TestGPAndClientCreationAndAssociation()
        {
            var gps = repository_.CreateSampleGPs().ToList();
            var clients = repository_.CreateSampleClients().ToList();

            repository_.AcceptReferral(gps[0], clients[0]);
            repository_.AcceptReferral(gps[0], clients[1]);

            {
                var clientRefs = repository_.GetAllReferrals(clients[0]).ToArray();
                Assert.That(clientRefs.Length, Is.EqualTo(1));
                Assert.That(clientRefs[0].ReferringGP, Is.EqualTo(gps[0]));
            }
            {
                var clientRefs = repository_.GetAllReferrals(clients[1]).ToArray();
                Assert.That(clientRefs.Length, Is.EqualTo(1));
                Assert.That(clientRefs[0].ReferringGP, Is.EqualTo(gps[0]));
            }
            {
                var gpRefs = repository_.GetAllReferrals(gps[0]).ToArray();
                Assert.That(gpRefs.Length, Is.EqualTo(2));
                Assert.That(gpRefs[0].Client, Is.EqualTo(clients[0]));
                Assert.That(gpRefs[1].Client, Is.EqualTo(clients[1]));
            }
            {
                var gpRefs = repository_.GetAllReferrals(gps[1]).ToArray();
                Assert.That(gpRefs.Length, Is.Zero);
            }

            {
                Assert.That(repository_.GetInitialReferringGP(clients[0]), Is.EqualTo(gps[0]));
                Assert.That(repository_.GetCurrentReferringGP(clients[0]), Is.EqualTo(gps[0]));
                Assert.That(repository_.GetInitialReferringGP(clients[1]), Is.EqualTo(gps[0]));
                Assert.That(repository_.GetCurrentReferringGP(clients[1]), Is.EqualTo(gps[0]));
            }

            Assert.Pass();
        }
    }
}