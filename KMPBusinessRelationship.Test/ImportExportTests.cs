using KMPBusinessRelationship.ImportExport;
using KMPBusinessRelationship.Objects;
using KMPBusinessRelationship.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

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

        [Test]
        public void DoubleImportTest()
        {
            var repo = new Repository();

            {
                var importExcel = new ImportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\input\base.xlsx");
                var errors = importExcel.Import(file, repo).ToList();
                Assert.That(errors.Count, Is.Zero);
            }

            var repoToDoubleImpoort = new Repository();
            {
                var importExcel = new ImportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\input\base.xlsx");
                var errors = importExcel.Import(file, repoToDoubleImpoort, false).ToList();
                Assert.That(errors.Count, Is.Zero);
                errors = importExcel.Import(file, repoToDoubleImpoort, true).ToList();
                Assert.That(errors.Count, Is.Zero);
            }
            Utility.AssertsEqual(repo, repoToDoubleImpoort);
        }

        [Test]
        public void ImportMergeTest()
        {
            var repo = new Repository();
            {
                var importExcel = new ImportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\input\base.xlsx");
                var errors = importExcel.Import(file, repo).ToList();
                Assert.That(errors.Count, Is.Zero);
            }

            var repoRef = new Repository();
            {
                var importExcel = new ImportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\input\base.xlsx");
                var errors = importExcel.Import(file, repoRef).ToList();
                Assert.That(errors.Count, Is.Zero);
            }

            // events to add
            {
                var testClient = repo.Clients.First();
                var lastReferral = repo.GetAllReferrals(testClient).ToList().Last();
                var newReferral = new Referral
                {
                    Time = lastReferral.Time.Value.AddYears(1),
                    Client = testClient,
                    ReferrerId = lastReferral.ReferrerId,
                };
                var newVisit = new ClaimableService
                {
                    Time = newReferral.Time.Value.AddMonths(1),
                    Client = testClient,
                    Claimed = false
                };

                // export the change to excel
                var repoToExport = new Repository();
                repoToExport.AddClientNoCheck(testClient);
                repoToExport.AddReferrerNoCheck(lastReferral.GetReferrer(repo));
                repoToExport.AddAndExecuteEvent(newReferral);
                repoToExport.AddAndExecuteEvent(newVisit);
                var exportExcel = new ExportExcel();
                var fileEvents = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\events.xlsx");
                fileEvents.Delete();
                exportExcel.Export(repoToExport, fileEvents);

                repoRef.AddAndExecuteEvent(newReferral);
                repoRef.AddAndExecuteEvent(newVisit);
            }

            // import merge
            {
                var importExcel = new ImportExcel();
                var file = new FileInfo(@"C:\Users\quanb\OneDrive\tagged\store\2402012306\br-tests\output\events.xlsx");
                var errors = importExcel.Import(file, repo, true).ToList();
                Assert.That(errors.Count, Is.Zero);
            }

            Utility.AssertsEqual(repo, repoRef);
        }
    }
}
