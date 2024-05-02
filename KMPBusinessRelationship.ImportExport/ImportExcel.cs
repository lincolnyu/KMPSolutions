using KMPBusinessRelationship.Objects;
using KMPCommon;
using OfficeOpenXml;
using static KMPBusinessRelationship.ImportExport.ExcelCommon;

namespace KMPBusinessRelationship.ImportExport
{
    public class ImportExcel
    {
        public IEnumerable<string> Import(FileInfo excelFile, BaseRepository repo, bool merge = false)
        {
            using var excelPackage = new ExcelPackage(excelFile);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var visits = excelPackage.Workbook.Worksheets["Visits"];
            var referrers = excelPackage.Workbook.Worksheets["Referrers"];

            Referrer? lastReferrer = null;
            for (var i = 2; i <= referrers.Cells.Rows; i++ )
            {
                var entryId = referrers.Cells[i, ReferrersColumns.Index].Text.Trim(); ;

                var providerNumber = referrers.Cells[i, ReferrersColumns.ProviderNumber].Text.Trim();

                var referrerToAdd = new Referrer { ProviderNumber = providerNumber };

                if (!string.IsNullOrEmpty(entryId))
                {
                    if (string.IsNullOrEmpty(providerNumber))
                    {
                        yield return $"Row {i} introduces a referrer with no provider number.";
                    }

                    var found = repo.SearchOrAddReferrer(referrerToAdd, out var referrer, r =>
                    {
                        r.Name = referrers.Cells[i, ReferrersColumns.Name].Text.Trim();
                        r.Phone = referrers.Cells[i, ReferrersColumns.Phone].Text.Trim();
                        r.Fax = referrers.Cells[i, ReferrersColumns.Fax].Text.Trim();
                        r.Address = referrers.Cells[i, ReferrersColumns.Address].Text.Trim();
                        r.PostalAddress = referrers.Cells[i, ReferrersColumns.PostalAddress].Text.Trim();
                        r.Remarks = referrers.Cells[i, ReferrersColumns.Remarks].Text.Trim();
                    });

                    if (found && !merge)
                    {
                        yield return $"Row {i} introduces a referrer with duplicate provider number.";
                    }

                    lastReferrer = referrer;
                }
                else if (lastReferrer != null && !string.IsNullOrEmpty(providerNumber))
                {
                    // Alternative provider number
                    repo.ReferrerAddOtherIdIfNonExistent(lastReferrer, providerNumber);
                }

                if (lastReferrer == null)
                {
                    yield return $"Row {i} has no preceding referrer to add additional information for.";
                }
            }

            Client? lastClient = null;
            for (var i = 2; i <= visits.Cells.Rows; i++)
            {
                var entryId = visits.Cells[i, VisitsColumns.Index].Text.Trim();   // per client
                var careNumber = visits.Cells[i, VisitsColumns.CareNumber].Text.Trim();

                var clientToAdd = new Client { CareNumber = careNumber };

                if (!string.IsNullOrEmpty(entryId))
                {
                    if (string.IsNullOrEmpty(careNumber))
                    {
                        yield return $"Row {i} introduces a client with no care number.";
                    }

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
                    if (found && !merge)
                    {
                        yield return $"Row {i} introduces a client with a duplicate care number.";
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
                            var referralDateStr = visits.Cells[i, VisitsColumns.ReferralDate].Text.Trim();
                            DateTime? referralDate = null;
                            if (!string.IsNullOrEmpty(referralDateStr))
                            {
                                referralDate = CsvUtility.ParseDateTime(referralDateStr);
                            }
                            repo.AcceptReferral(referralDate, referrerId, lastClient);
                        }
                        else
                        {
                            yield return $"Row {i} contains non-existent referrer with provider ID.";
                        }
                    }

                    var visit = visits.Cells[i, VisitsColumns.ReferralDate].Text.Trim();
                    var claimed = visits.Cells[i, VisitsColumns.IfClaimed].Text.Trim().StartsWith('Y');
                    if (!string.IsNullOrEmpty(visit))
                    {
                        var visitDate = CsvUtility.ParseDateTime(visit);
                        repo.AddClaimableService(visitDate, lastClient, claimed);
                    }

                    // TODO other changes.
                }
                else
                {
                    yield return $"Row {i} has no preceding client to add additional information for.";
                }
            }
        }
    }
}
