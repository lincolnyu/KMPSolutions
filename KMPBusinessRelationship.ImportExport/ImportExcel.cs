using KMPBusinessRelationship.Objects;
using OfficeOpenXml;

namespace KMPBusinessRelationship.ImportExport
{
    public class ImportExcel
    {
        public IEnumerable<string> Import(FileInfo excelFile, BaseRepository repo)
        {
            using var excelPackage = new ExcelPackage(excelFile);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var visits = excelPackage.Workbook.Worksheets["Visits"];
            var referrers = excelPackage.Workbook.Worksheets["Referrers"];

            Referrer? lastReferrer = null;
            for (var i = 2; i <= referrers.Cells.Rows; i++ )
            {
                var entryId = referrers.Cells[i, 1].Text.Trim(); ;

                var providerNumber = referrers.Cells[i, 2].Text.Trim();
                var referrerToAdd = new Referrer { ProviderNumber = providerNumber };

                var name = referrers.Cells[i, 3].Text.Trim();

                if (string.IsNullOrEmpty(entryId))
                {
                    // Alternative provider number
                    if (lastReferrer != null)
                    {
                        lastReferrer.OtherProviderNumbers.Add(providerNumber);
                    }
                    else
                    {
                        yield return $"Row {i} has no internal ID but has no preceding referrer to add additional provider number for.";
                    }
                }
                else
                {
                    repo.SearchOrAddReferrer(referrerToAdd, out var referrer, r =>
                    {
                        r.Name = name;
                        r.Phone = referrers.Cells[i, 4].Text.Trim();
                        r.Fax = referrers.Cells[i, 5].Text.Trim();
                        r.Address = referrers.Cells[i, 7].Text.Trim();
                        r.PostalAddress = referrers.Cells[i, 8].Text.Trim();
                        r.Remarks = referrers.Cells[i, 9].Text.Trim();
                    });
                    lastReferrer = referrer;
                }
            }

            for (var i = 2; i <= visits.Cells.Rows; i++)
            {
                var entryId = visits.Cells[i, 1].Text.Trim();   // per client
                var careNumber = visits.Cells[i, 2].Text.Trim();

                if (string.IsNullOrEmpty(entryId))
                {

                }
                else
                {

                }
            }
        }
    }
}
