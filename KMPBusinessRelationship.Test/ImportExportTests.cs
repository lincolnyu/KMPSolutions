using KMPBusinessRelationship.ImportExport;
using KMPBusinessRelationship.Persistence;

namespace KMPBusinessRelationship.Test
{
    internal class ImportExportTests
    {
        [Test]
        public void TestImport()
        {
            var importExcel = new ImportExcel();
            var repo = new Repository();
            var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2401280034\KMPBusinessLatest.xlsx");
            var errors = importExcel.Import(file, repo).ToList();
            Assert.That(errors.Count, Is.Zero);
            Assert.Pass();
        }
    }
}
