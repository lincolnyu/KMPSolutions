using KMPBusinessRelationship.ImportExport;
using KMPBusinessRelationship.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KMPBusinessRelationship.Test
{
    internal class ImportExportTests
    {
        [Test]
        public void TestImportPersistAndExport()
        {
            Repository? savedRepo = null;
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

                savedRepo = repo;
            }
            
            Assert.That(savedRepo, Is.Not.Null);

            Repository? loadedRepo = null;
            {
                var builder = new DbContextOptionsBuilder();
                builder.UseSqlite(@"data source=C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\KMPBusiness.sqlite");

                using var context = new Context(builder.Options);
                var repo = new Repository(context);
                Utility.AssertsEqual(repo, savedRepo);
                loadedRepo = repo;
            }

            {
                var exportExcel = new ExportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\KMPBusinessExported.xlsx");
                file.Delete();
                exportExcel.Export(loadedRepo, file);
            }

            {
                var exportExcel = new ExportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\KMPBusinessExportedThisYear.xlsx");
                file.Delete();
                exportExcel.Export(loadedRepo, file, new DateTime(2022,1,1));
            }

            {
                var exportedRepo = new Repository();
                var importExcel = new ImportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\KMPBusinessExported.xlsx");
                var errors = importExcel.Import(file, exportedRepo).ToList();
                Assert.That(errors.Count, Is.Zero);

                Utility.AssertsEqual(exportedRepo, loadedRepo);
            }

            Assert.Pass();
        }
    }
}
