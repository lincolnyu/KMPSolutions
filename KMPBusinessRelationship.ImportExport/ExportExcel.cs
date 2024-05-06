using KMPBusinessRelationship.Objects;
using OfficeOpenXml;
using static KMPBusinessRelationship.ImportExport.ExcelCommon;
using static KMPCommon.CsvUtility;

namespace KMPBusinessRelationship.ImportExport
{
    public class ExportExcel
    {
        public void Export(BaseRepository repo, FileInfo excelFile, DateTime? referralStartDate = null)
        {
            using var excelPackage = new ExcelPackage(excelFile);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var visits = excelPackage.Workbook.Worksheets.Add("Visits");
            var referrers = excelPackage.Workbook.Worksheets.Add("Referrers");

            var col = 1;
            foreach (var columnName in ReferrersColumns.GetAllColumns())
            {
                referrers.Cells[1, col].Value = columnName;
                referrers.Cells[1, col].Style.Font.Bold = true;
                col++;
            }
            col = 1;
            foreach (var columnName in VisitsColumns.GetAllColumns())
            {
                visits.Cells[1, col].Value = columnName;
                visits.Cells[1, col].Style.Font.Bold = true;
                col++;
            }

            var internalId = 1;
            var row = 2;
            foreach (var referrer in repo.Referrers)
            {
                var cells = referrers.Cells;
                cells[row, ReferrersColumns.Index].Value = internalId++;
                cells[row, ReferrersColumns.ProviderNumber].Value = referrer.ProviderNumber;
                cells[row, ReferrersColumns.Name].Value = referrer.Name;
                cells[row, ReferrersColumns.Phone].Value = referrer.Phone;
                cells[row, ReferrersColumns.Fax].Value = referrer.Fax;
                cells[row, ReferrersColumns.PracticeName].Value = referrer.PracticeName;
                cells[row, ReferrersColumns.Address].Value = referrer.Address;
                cells[row, ReferrersColumns.PostalAddress].Value = referrer.PostalAddress;
                cells[row, ReferrersColumns.Remarks].Value = referrer.Remarks;

                row++;
                foreach (var pn in referrer.OtherProviderNumbers)
                {
                    cells[row, ReferrersColumns.ProviderNumber].Value = pn;
                    row++;
                }
            }

            internalId = 1;
            row = 2;
            foreach (var client in repo.Clients)
            {
                var cells = visits.Cells;
                

                var allEvents = repo.Events.Where(x => 
                    (x is Referral r) && r.Client == client
                    || (x is IService s) && s.Client == client);

                if (referralStartDate != null)
                {
                    allEvents = allEvents.Where(x =>
                    {
                        if (x is Referral r)
                        {
                            return r.Time >= referralStartDate;
                        }
                        else
                        {
                            var s = (IService)x;
                            return s.Time >= referralStartDate;
                        }
                    });
                }

                var allEventList = allEvents.OrderBy(x => x.Time).ToList();

                if (referralStartDate == null || allEventList.Count > 0)
                {
                    // When events are all filtered out the client is made invisible.
                    cells[row, VisitsColumns.Index].Value = internalId++;
                    cells[row, VisitsColumns.CareNumber].Value = client.CareNumber;
                    var (surname, givenName) = Utility.SplitNameToSurnameAndGivenName(client.Name);
                    cells[row, VisitsColumns.Surname].Value = surname;
                    cells[row, VisitsColumns.GivenName].Value = givenName;
                    cells[row, VisitsColumns.DateOfBirth].Value = client.DateOfBirth?.ToShortDateOnlyString();
                    cells[row, VisitsColumns.Gender].Value = client.Gender;
                    cells[row, VisitsColumns.Phone].Value = client.PhoneNumber;
                    cells[row, VisitsColumns.Address].Value = client.Address;

                    row++;

                    // 0 - just added the client
                    // 1 - just added new referral
                    // 2 - filled a visit
                    int state = 0;

                    foreach (var e in allEventList)
                    {
                        if (e is Referral r)
                        {
                            if (state == 0)
                            {
                                row--;
                                state = 1;
                            }
                            cells[row, VisitsColumns.ReferrerID].Value = r.ReferrerId;
                            cells[row, VisitsColumns.ReferralDate].Value = r.Time?.ToShortDateOnlyString();
                        }
                        else
                        {
                            if (state == 0 || state == 1)
                            {
                                row--;
                            }
                            if (e is ClaimableService s)
                            {
                                cells[row, VisitsColumns.Visit].Value = s.Time?.ToShortDateOnlyString();
                                cells[row, VisitsColumns.IfClaimed].Value = s.Claimed ? "Y" : "";
                            }
                            else if (e is ChargedService h)
                            {
                                cells[row, VisitsColumns.Visit].Value = h.Time?.ToShortDateOnlyString();
                                cells[row, VisitsColumns.IfClaimed].Value = "NOT Claimable";
                            }
                            state = 2;
                        }
                        row++;
                    }
                }
            }

            excelPackage.Save();
        }
    }
}
