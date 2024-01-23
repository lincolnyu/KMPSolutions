namespace KMPCoreObjectsTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public void Test1()
        {
            using var context = new KMPCoreObjects.KMPContext("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\\Users\\quanb\\OneDrive\\cibo\\kmp\\it\\testing\\KMPCRM.accdb;\r\nPersist Security Info=False;\r\n");

            Assert.AreEqual(context.Clients.Count(), 132);
            Assert.Pass();
        }
    }
}