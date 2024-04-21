using KMPBusinessRelationship;
using KMPBusinessRelationship.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KMPBusinessRelationship.Test
{
    public class CoreTests
    {
        [Test]
        public void TestReferrerAndClientCreationAndAssociation()
        {
            var repo = new Repository();

            var gps = repo.CreateSampleReferrers().ToList();
            var clients = repo.CreateSampleClients().ToList();

            repo.AcceptReferral(gps[0], clients[0]);
            repo.AcceptReferral(gps[0], clients[1]);

            {
                var clientRefs = repo.GetAllReferrals(clients[0]).ToArray();
                Assert.That(clientRefs.Length, Is.EqualTo(1));
                Assert.That(clientRefs[0].Referrer, Is.EqualTo(gps[0]));
            }
            {
                var clientRefs = repo.GetAllReferrals(clients[1]).ToArray();
                Assert.That(clientRefs.Length, Is.EqualTo(1));
                Assert.That(clientRefs[0].Referrer, Is.EqualTo(gps[0]));
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
                Assert.That(repo.GetInitialReferrer(clients[0]), Is.EqualTo(gps[0]));
                Assert.That(repo.GetCurrentReferrer(clients[0]), Is.EqualTo(gps[0]));
                Assert.That(repo.GetInitialReferrer(clients[1]), Is.EqualTo(gps[0]));
                Assert.That(repo.GetCurrentReferrer(clients[1]), Is.EqualTo(gps[0]));
            }

            Assert.Pass();
        }

        [Test]
        public void TestSimplePersistence()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlite(@"data source=C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\br.sqlite");

            using var context = new Context(builder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var repo = new Repository(context);
            var referrers = repo.CreateSampleReferrers().ToList();
            var clients = repo.CreateSampleClients().ToList();

            repo.AcceptReferral(referrers[0], clients[0]);
            repo.AcceptReferral(referrers[0], clients[1]);

            context.SaveChanges();
        }
    }
}
