using KMPBusinessRelationship.Objects;
using KMPCommon;
using OfficeOpenXml;
using static KMPBusinessRelationship.ImportExport.ExcelCommon;

namespace KMPBusinessRelationship.ImportExport
{
    public class ImportExcel
    {
        class EventComparerByDate : IComparer<Event>
        {
            public static EventComparerByDate Instance { get; } = new EventComparerByDate();
            public int Compare(Event x, Event y)
            {
                if (x.Time != null && y.Time != null)
                {
                    return x.Time!.Value.CompareTo(y.Time!.Value);
                }
                else if (x.Time != null)
                {
                    return 1;
                }
                else if (y.Time != null)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public IEnumerable<string> Import(FileInfo excelFile, BaseRepository repo, bool merge = false)
        {
            using var excelPackage = new ExcelPackage(excelFile);
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var visits = excelPackage.Workbook.Worksheets["Visits"];
            var referrers = excelPackage.Workbook.Worksheets["Referrers"];

            if (referrers != null)
            {
                Referrer? lastReferrer = null;
                for (var i = 2; i <= referrers.Cells.Rows; i++)
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
            }
            else if (!merge)
            {
                yield return $"Referrers sheet is missing.";
            }

            var existingEvents = repo.EventList.OrderBy(x => x.Time).ToList();
            bool containsEvent(Event e)
            {
                var index = existingEvents.BinarySearch(e, EventComparerByDate.Instance);
                if (index < 0) return false;
                for (var j = index; j >= 0; j--)
                {
                    var ee = existingEvents[j];
                    if (ee.Equals(e)) return true;
                }
                for (var j = index + 1; j < existingEvents.Count; j++)
                {
                    var ee = existingEvents[j];
                    if (ee.Equals(e)) return true;
                }
                return false;
            }

            var eventList = new List<Event>();
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

                            var newReferral = new Referral
                            {
                                Time = referralDate,
                                ReferrerId = referrerId,
                                Client = lastClient
                            };

                            if (!containsEvent(newReferral))
                            {
                                eventList.Add(newReferral);
                            }
                            else if (!merge)    // In merge duplicates are allowed and ignored
                            {
                                yield return $"Row {i} contains a duplicate referral";
                            }
                        }
                        else
                        {
                            yield return $"Row {i} contains non-existent referrer with provider ID.";
                        }
                    }

                    var visitDateStr = visits.Cells[i, VisitsColumns.Visit].Text.Trim();
                    var claimed = visits.Cells[i, VisitsColumns.IfClaimed].Text.Trim().StartsWith('Y');
                    if (!string.IsNullOrEmpty(visitDateStr))
                    {
                        var visitDate = CsvUtility.ParseDateTime(visitDateStr);

                        var newService = new ClaimableService
                        {
                            Time = visitDate,
                            Client = lastClient,
                            Claimed = claimed
                        };

                        if (!containsEvent(newService))
                        {
                            eventList.Add(newService);
                        }
                        else if (!merge)    // In merge duplicates are allowed and ignored
                        {
                            yield return $"Row {i} contains a duplicate service";
                        }
                    }

                    // TODO other changes.
                }
                else
                {
                    yield return $"Row {i} has no preceding client to add additional information for.";
                }
            }

            eventList.Sort((a, b) =>
            {
                if (a.Time.HasValue && b.Time.HasValue)
                {
                    var c = a.Time.Value.CompareTo(b.Time.Value);
                    if (c != 0) return c;
                }
                else if (a.Time.HasValue)
                {
                    return 1;
                }
                else if (b.Time.HasValue)
                {
                    return -1;
                }

                Client? clientA = null;
                if (a is Referral ra) clientA = ra.Client;
                else if (a is IService sa) clientA = sa.Client;

                Client? clientB = null;
                if (b is Referral rb) clientB = rb.Client;
                else if (b is IService sb) clientB = sb.Client;

                if (clientA != null && clientB != null)
                {
                    return clientA.CareNumber.CompareTo(clientB.CareNumber);
                }
                else if (clientA != null)
                {
                    return 1;
                }
                else if (clientB != null)
                {
                    return -1;
                }

                return 0; // Not to order the unknown types.
            });

            // Always append to the end even if the time is earlier.
            foreach (var e in eventList)
            {
                repo.AddAndExecuteEvent(e);
            }
        }
    }
}
