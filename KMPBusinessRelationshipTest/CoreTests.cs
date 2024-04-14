using KMPBusinessRelationship;
using KMPBusinessRelationshipPersistence;
using Microsoft.EntityFrameworkCore;

namespace KMPBusinessRelationshipTest
{
    public class CoreTests
    {
        [Test]
        public void TestGPAndClientCreationAndAssociation()
        {
            var repo = new Repository();

            var gps = repo.CreateSampleGPs().ToList();
            var clients = repo.CreateSampleClients().ToList();

            repo.AcceptReferral(gps[0], clients[0]);
            repo.AcceptReferral(gps[0], clients[1]);

            {
                var clientRefs = repo.GetAllReferrals(clients[0]).ToArray();
                Assert.That(clientRefs.Length, Is.EqualTo(1));
                Assert.That(clientRefs[0].ReferringGP, Is.EqualTo(gps[0]));
            }
            {
                var clientRefs = repo.GetAllReferrals(clients[1]).ToArray();
                Assert.That(clientRefs.Length, Is.EqualTo(1));
                Assert.That(clientRefs[0].ReferringGP, Is.EqualTo(gps[0]));
            }
            {
                var gpRefs = repo.GetAllReferrals(gps[0]).ToArray();
                Assert.That(gpRefs.Length, Is.EqualTo(2));
                Assert.That(gpRefs[0].Client, Is.EqualTo(clients[0]));
                Assert.That(gpRefs[1].Client, Is.EqualTo(clients[1]));
            }
            {
                var gpRefs = repo.GetAllReferrals(gps[1]).ToArray();
                Assert.That(gpRefs.Length, Is.Zero);
            }

            {
                Assert.That(repo.GetInitialReferringGP(clients[0]), Is.EqualTo(gps[0]));
                Assert.That(repo.GetCurrentReferringGP(clients[0]), Is.EqualTo(gps[0]));
                Assert.That(repo.GetInitialReferringGP(clients[1]), Is.EqualTo(gps[0]));
                Assert.That(repo.GetCurrentReferringGP(clients[1]), Is.EqualTo(gps[0]));
            }

            Assert.Pass();
        }

        [Test]
        public void TestSimplePersistence()
        {
            {
                var builder = new DbContextOptionsBuilder();
                builder.UseSqlite(@"data source=C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\br.sqlite");

                using var context = new Context(builder.Options);
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var repo = new Repository(context);
                var gps = repo.CreateSampleGPs().ToList();
                var clients = repo.CreateSampleClients().ToList();
                
                repo.AcceptReferral(gps[0], clients[0]);
                repo.AcceptReferral(gps[0], clients[1]);

                context.SaveChanges();
            }
        }
    }
}
