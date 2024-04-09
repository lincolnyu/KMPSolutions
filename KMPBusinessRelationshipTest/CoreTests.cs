using KMPBusinessRelationship.Objects;
using KMPBusinessRelationship;

namespace KMPBusinessRelationshipTest
{
    public class CoreTests
    {
        Repository repository_;

        [SetUp]
        public void Setup()
        {
            repository_ = new Repository();
        }

        [Test]
        public void TestGPAndClientCreationAndAssociation()
        {
            repository_.CreateSampleGPs();
            repository_.CreateSampleClients();

            Assert.Pass();
        }
    }
}