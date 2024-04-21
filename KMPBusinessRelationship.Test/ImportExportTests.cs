using KMPBusinessRelationship.ImportExport;
using KMPBusinessRelationship.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            importExcel.Import(file, repo);
        }
    }
}
