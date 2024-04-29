using KMPBusinessRelationship.ImportExport;
using KMPBusinessRelationship.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KMPBusinessRelationship.Test
{
    internal class ImportExportTests
    {
        [Test]
        public void TestImportAndPersist()
        {
            var builder = new DbContextOptionsBuilder();
            builder.UseSqlite(@"data source=C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\KMPBusiness.sqlite");

            using var context = new Context(builder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            var repo = new Repository(context);

            {
                var importExcel = new ImportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2401280034\KMPBusinessLatest.xlsx");
                var errors = importExcel.Import(file, repo).ToList();
                Assert.That(errors.Count, Is.Zero);
            }

            context.SaveChanges();

            Assert.Pass();

        }
    }
}
