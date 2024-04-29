using KMPBusinessRelationship.Objects;
using KMPCommon;
using OfficeOpenXml;

namespace KMPBusinessRelationship.ImportExport
{
    public class ImportExcel
    {
        public static class ReferrerColumns
        {
            public const int InternalID = 1;
            public const int ProviderNumber = 2;
            public const int Name = 3;
            public const int Phone = 4;
            public const int Fax = 5;
            public const int PracticeName = 6;
            public const int Address = 7;
            public const int PostalAddress = 8;
            public const int Remarks = 9;
        }

        public static class VisitsColumns
        {
            public const int InternalID = 1;
            public const int CareNumber = 2;
            public const int GivenName = 3;
            public const int Surname = 4;
            public const int DateOfBirth = 5;
            public const int Gender = 6;
            public const int Phone = 7;
            public const int Address = 8;
            public const int ReferrerID = 9;
            public const int ReferralDate = 10;
            public const int Visit = 11;
            public const int IfClaimed = 12;
            public const int Remarks = 13;
        }

        public IEnumerable<string> Import(FileInfo excelFile, BaseRepository repo)
        {
            using var excelPackage = new ExcelPackage(excelFile);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var visits = excelPackage.Workbook.Worksheets["Visits"];
            var referrers = excelPackage.Workbook.Worksheets["Referrers"];

            Referrer? lastReferrer = null;
            for (var i = 2; i <= referrers.Cells.Rows; i++ )
            {
                var entryId = referrers.Cells[i, ReferrerColumns.InternalID].Text.Trim(); ;

                var providerNumber = referrers.Cells[i, ReferrerColumns.ProviderNumber].Text.Trim();
                var referrerToAdd = new Referrer { ProviderNumber = providerNumber };

                if (!string.IsNullOrEmpty(entryId))
                {
                    repo.SearchOrAddReferrer(referrerToAdd, out var referrer, r =>
                    {
                        r.Name = referrers.Cells[i, ReferrerColumns.Name].Text.Trim();
                        r.Phone = referrers.Cells[i, ReferrerColumns.Phone].Text.Trim();
                        r.Fax = referrers.Cells[i, ReferrerColumns.Fax].Text.Trim();
                        r.Address = referrers.Cells[i, ReferrerColumns.Address].Text.Trim();
                        r.PostalAddress = referrers.Cells[i, ReferrerColumns.PostalAddress].Text.Trim();
                        r.Remarks = referrers.Cells[i, ReferrerColumns.Remarks].Text.Trim();
                    });
                    lastReferrer = referrer;
                }
                else if (lastReferrer != null && !string.IsNullOrEmpty(providerNumber))
                {
                    // Alternative provider number
                    repo.ReferrerAddOtherId(lastReferrer, providerNumber);
                }

                if (lastReferrer == null)
                {
                    yield return $"Row {i} has no preceding referrer to add additional information for.";
                }
            }

            Client? lastClient = null;
            for (var i = 2; i <= visits.Cells.Rows; i++)
            {
                var entryId = visits.Cells[i, VisitsColumns.InternalID].Text.Trim();   // per client
                var careNumber = visits.Cells[i, VisitsColumns.CareNumber].Text.Trim();

                var clientToAdd = new Client { CareNumber = careNumber };

                if (!string.IsNullOrEmpty(entryId))
                {
                    var found = repo.SearchOrAddClient(clientToAdd, out var client, c =>
                    {
                        var givenName = visits.Cells[i, VisitsColumns.GivenName].Text.Trim();
                        var surname = visits.Cells[i, VisitsColumns.Surname].Text.Trim();
                        c.Name = $"{surname}, {givenName}";
                        c.CareNumber = visits.Cells[i, VisitsColumns.CareNumber].Text.Trim();
                        var dob = visits.Cells[i, VisitsColumns.DateOfBirth].Text.Trim();
                        if (!string.IsNullOrEmpty(dob))
                        {
                            c.DateOfBirth = CsvUtility.ParseDateTime(dob);
                        }
                        c.Gender = visits.Cells[i, VisitsColumns.Gender].Text.Trim();
                        c.PhoneNumber = visits.Cells[i, VisitsColumns.Phone].Text.Trim();
                        c.Address = visits.Cells[i, VisitsColumns.Address].Text.Trim();
                    });
                    if (found)
                    {
                        yield return $"Row {i} contains a duplicate care number.";
                    }
                    lastClient = client;
                }

                if (lastClient != null)
                {
                    var referrerId = visits.Cells[i, VisitsColumns.ReferrerID].Text.Trim();
                    if (!string.IsNullOrEmpty(referrerId))
                    {
                        var referrerToSearch = new Referrer
                        {
                            ProviderNumber = referrerId
                        };
                        var referrer = repo.SearchReferrer(referrerToSearch);
                        if (referrer != null)
                        {
                            repo.AcceptReferral(referrer!, lastClient);
                        }
                        else
                        {
                            yield return $"Row {i} contains non-existent referrer with provider ID.";
                        }
                    }

                    var visit = visits.Cells[i, VisitsColumns.ReferralDate].Text.Trim();
                    var claimed = visits.Cells[i, VisitsColumns.ReferralDate].Text.Trim().StartsWith('Y');
                    if (!string.IsNullOrEmpty(visit))
                    {
                        var visitDate = CsvUtility.ParseDateTime(visit);
                        repo.AddClaimableService(visitDate, lastClient, claimed);
                    }
                }
                else
                {
                    yield return $"Row {i} has no preceding client to add additional information for.";
                }
            }
        }
    }
}
