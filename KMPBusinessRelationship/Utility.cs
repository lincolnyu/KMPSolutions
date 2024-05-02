using KMPBusinessRelationship.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace KMPBusinessRelationship
{
    public static class Utility
    {
        public static IEnumerable<T> QueryEvents<T>(this BaseRepository repo, Func<T, bool> criteria) where T : Event => repo.Events.OfType<T>().Where(criteria);

        public static IEnumerable<T> QueryPersons<T>(this BaseRepository repo, Func<T, bool> criteria) where T : Person => repo.Events.OfType<T>().Where(criteria);

        public static IEnumerable<Referral> GetAllReferrals(this BaseRepository repo, Client client) => repo.QueryEvents<Referral>(referral => referral.Client == client);

        public static IEnumerable<Referral> GetAllReferrals(this BaseRepository repo, Referrer referrer) => repo.QueryEvents<Referral>(referral => referral.GetReferrer(repo) == referrer);

        public static Referrer? GetInitialReferrer(this BaseRepository repo, Client client)
        {
            for (var i = 0; i < repo.EventList.Count; i++)
            {
                if (i >= repo.CurrentEventIndex)
                {
                    return null;
                }
                var e = repo.EventList[i];
                if (e is Referral referral)
                {
                    return referral.GetReferrer(repo);
                }
            }
            return null;
        }

        public static Referrer? GetCurrentReferrer(this BaseRepository repo, Client client)
        {
            for (var i = repo.CurrentEventIndex-1; i >= 0; i--)
            {
                var e = repo.EventList[i];
                if (e is Referral referral)
                {
                    return referral.GetReferrer(repo);
                }
            }
            return null;
        }

        public static void RunEventsTo(this BaseRepository repo, int newIndex)
        {
            if (newIndex > repo.CurrentEventIndex)
            {
                for (var i = repo.CurrentEventIndex; i < newIndex; i++)
                {
                    var e = repo.EventList[i];
                    e.Redo();
                }
                repo.CurrentEventIndex = newIndex;
            }
            else if (newIndex < repo.CurrentEventIndex)
            {
                for (var i = repo.CurrentEventIndex - 1; i >= newIndex; i--)
                {
                    var e = repo.EventList[i];
                    e.Undo();
                }
                repo.CurrentEventIndex = newIndex;
            }
        }

        public static Client? SearchClient(this BaseRepository repo, Client clientToSearch)
        {
            bool Matched(Client clientToSearch, Client target)
            {
                if (!string.IsNullOrEmpty(clientToSearch.CareNumber))
                {
                    if (clientToSearch.CareNumber == target.CareNumber)
                    {
                        return true;
                    }
                }

                if (!string.IsNullOrEmpty(clientToSearch.Name) && clientToSearch.DateOfBirth != null && clientToSearch.Gender != null)
                {
                    if (clientToSearch.Name == target.Name && clientToSearch.DateOfBirth == target.DateOfBirth && clientToSearch.Gender == target.Gender)
                    {
                        return true;
                    }
                }

                return false;
            }

            if (!string.IsNullOrEmpty(clientToSearch.CareNumber))
            {
                if (repo.IdToClientMap.TryGetValue(clientToSearch.CareNumber, out var client))
                {
                    return client;
                }
            }
            else
            {
                foreach (var client in repo.Clients)
                {
                    if (Matched(clientToSearch, client))
                    {
                        return client;
                    }
                }
            }

            return null;
        }

        public static Referrer? SearchReferrer(this BaseRepository repo, Referrer referrerToSearch)
        {
            bool Matched(Referrer referrerToSearch, Referrer target)
            {
                if (!string.IsNullOrEmpty(referrerToSearch.ProviderNumber))
                {
                    if (referrerToSearch.Id == target.Id)
                    {
                        return true;
                    }
                }

                // TODO Other fallback criteria?

                return false;
            }

            if (!string.IsNullOrEmpty(referrerToSearch.Id))
            {
                if (repo.IdToReferrerMap.TryGetValue(referrerToSearch.Id, out var referrer))
                {
                    return referrer;
                }
            }
            else
            {
                foreach (var referrer in repo.Referrers)
                {
                    if (Matched(referrerToSearch, referrer))
                    {
                        return referrer;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Search the client in the repo and return it or add it to the repo and return.
        /// </summary>
        /// <param name="repo">The repository</param>
        /// <param name="clientToSearchOrAdd">The client to search which may have ONLY partial information.</param>
        /// <param name="clientInRepo">The client in the repo if found or the client  queried which has now been added to the repo.</param>
        /// <param name="fillMoreDetails">Provide details to the client to add pre-adding. Note as client <paramref name="clientToSearchOrAdd"/> may not contain essential info such as medicare number which is required by the AddClientNoCheck() function, it is this function responsibility to ensure it.</param>
        /// <returns>True if an existing client has been found</returns>
        public static bool SearchOrAddClient(this BaseRepository repo, Client clientToSearchOrAdd, out Client clientInRepo, Action<Client>? fillMoreDetails = null)
        {
            var clientFound = repo.SearchClient(clientToSearchOrAdd);
            if (clientFound != null)
            {
                clientInRepo = clientFound;
                return true;
            }

            fillMoreDetails?.Invoke(clientToSearchOrAdd);
            repo.AddClientNoCheck(clientToSearchOrAdd);
            clientInRepo = clientToSearchOrAdd;
            return false;
        }

        /// <summary>
        ///  Search the referrer in the repo and return it or add it to the repo and return.
        /// </summary>
        /// <param name="repo">The repository</param>
        /// <param name="referrerToSearchOrAdd">The referrer to search which may have only partial information.</param>
        /// <param name="referrerInRepo">The referrer in the repo if found or the referrer queried which has now been added to the repo.</param>
        /// <param name="fillMoreDetails">Provide details to the referrer to add pre-adding. Note as client <paramref name="referrerToSearchOrAdd"/> may not contain essential info such as provider number which is required by the AddSearchReferrerNoCheck() function, it is this function responsibility to ensure it.</param>
        /// <returns>True if an existing referrer has been found</returns>
        public static bool SearchOrAddReferrer(this BaseRepository repo, Referrer referrerToSearchOrAdd, out Referrer referrerInRepo, Action<Referrer>? fillMoreDetails = null)
        {
            var referrerFound = repo.SearchReferrer(referrerToSearchOrAdd);
            if (referrerFound != null)
            {
                referrerInRepo = referrerFound;
                return true;
            }

            fillMoreDetails?.Invoke(referrerToSearchOrAdd);
            repo.AddReferrerNoCheck(referrerToSearchOrAdd);
            referrerInRepo = referrerToSearchOrAdd;
            return false;
        }

        public static bool ReferrerAddOtherIdIfNonExistent(this BaseRepository repo, Referrer referrer, string otherId)
        {
            if (!repo.IdToReferrerMap.ContainsKey(otherId))
            {
                referrer.OtherProviderNumbers.Add(otherId);
                repo.IdToReferrerMap[otherId] = referrer;
                return true;
            }
            return false;
        }

        public static void AcceptReferral(this BaseRepository repository, DateTime? time, string referrerId, Client client)
        {
            repository.AddAndExecuteEvent(new Referral
            {
                Time = time,
                ReferrerId = referrerId,
                Client = client
            });
        }

        public static void AddClaimableService(this BaseRepository repository, DateTime time, Client client, bool claimed)
        {
            repository.AddAndExecuteEvent(new ClaimableService
            {
                Time = time,
                Client = client,
                Claimed = claimed
            });
        }

        public static (string, string) SplitNameToSurnameAndGivenName(string name)
        {
            var split = name.Split(',');
            var surname = split[0];
            if (split.Length < 2)
            {
                return (surname, "");
            }
            var givenName = split[1];
            return (surname, givenName);
        }
    }
}
