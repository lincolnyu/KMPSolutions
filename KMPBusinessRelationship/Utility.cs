using KMPBusinessRelationship.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KMPBusinessRelationship
{
    public static class Utility
    {
        public static IEnumerable<T> QueryEvents<T>(this Repository repo, Func<T, bool> criteria) where T : Event => repo.Events.OfType<T>().Where(criteria);

        public static IEnumerable<T> QueryPersons<T>(this Repository repo, Func<T, bool> criteria) where T : Person => repo.Events.OfType<T>().Where(criteria);

        public static IEnumerable<Referral> GetAllReferrals(this Repository repo, Client client) => repo.QueryEvents<Referral>(referral => referral.Client == client);

        public static IEnumerable<Referral> GetAllReferrals(this Repository repo, GeneralPractitioner gp) => repo.QueryEvents<Referral>(referral => referral.ReferringGP == gp);

        public static GeneralPractitioner? GetInitialReferringGP(this Repository repo, Client client)
        {
            var referral = repo.GetAllReferrals(client).FirstOrDefault();
            if (referral == null || referral.Index >= repo.CurrentEventIndex)
            {
                return null;
            }
            return referral.ReferringGP;
        }

        public static GeneralPractitioner? GetCurrentReferringGP(this Repository repo, Client client)
        {
            for (var i = repo.CurrentEventIndex-1; i >= 0; i--)
            {
                var e = repo.Events[i];
                if (e is Referral referral)
                {
                    return referral.ReferringGP;
                }
            }
            return null;
        }

        public static void RunEventsTo(this Repository repo, int newIndex)
        {
            if (newIndex > repo.CurrentEventIndex)
            {
                for (var i = repo.CurrentEventIndex; i < newIndex; i++)
                {
                    var e = repo.Events[i];
                    if (e is ChangeOfDetails cod)
                    {
                        cod.Redo();
                    }
                }
                repo.CurrentEventIndex = newIndex;
            }
            else if (newIndex < repo.CurrentEventIndex)
            {
                for (var i = repo.CurrentEventIndex - 1; i >= newIndex; i--)
                {
                    var e = repo.Events[i];
                    if (e is ChangeOfDetails cod)
                    {
                        cod.Undo();
                    }
                }
                repo.CurrentEventIndex = newIndex;
            }
        }

        public static Client? SearchClient(this Repository repo, Client clientToSearch)
        {
            bool Matched(Client clientToSearch, Client target)
            {
                if (!string.IsNullOrEmpty(clientToSearch.MedicareNumber))
                {
                    if (clientToSearch.MedicareNumber == target.MedicareNumber)
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

            if (!string.IsNullOrEmpty(clientToSearch.MedicareNumber))
            {
                if (repo.MedicareNumberToClientMap.TryGetValue(clientToSearch.MedicareNumber, out var client))
                {
                    return client;
                }
            }
            else
            {
                foreach (var client in repo.Persons.OfType<Client>())
                {
                    if (Matched(clientToSearch, client))
                    {
                        return client;
                    }
                }
            }

            return null;
        }

        public static GeneralPractitioner? SearchGeneralPractioner(this Repository repo, GeneralPractitioner gpToSearch)
        {
            bool Matched(GeneralPractitioner gpToSearch, GeneralPractitioner target)
            {
                if (!string.IsNullOrEmpty(gpToSearch.ProviderNumber))
                {
                    if (gpToSearch.ProviderNumber == target.ProviderNumber)
                    {
                        return true;
                    }
                }

                // TODO Other fallback criteria?

                return false;
            }

            if (!string.IsNullOrEmpty(gpToSearch.ProviderNumber))
            {
                if (repo.ProviderNumberToGPMap.TryGetValue(gpToSearch.ProviderNumber, out var gp))
                {
                    return gp;
                }
            }
            else
            {
                foreach (var gp in repo.Persons.OfType<GeneralPractitioner>())
                {
                    if (Matched(gpToSearch, gp))
                    {
                        return gp;
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
        /// <param name="fillMoreDetails">Provide details to the client to add pre-adding. Note as client <paramref name="clientToSearchOrAdd"/> may not contain essential info such as medicare number which is required by the AddPersonNoCheck() function, it is this function responsibility to ensure it.</param>
        /// <returns>True if an existing client has been found</returns>
        public static bool SearchOrAddClient(this Repository repo, Client clientToSearchOrAdd, out Client clientInRepo, Action<Client>? fillMoreDetails = null)
        {
            var clientFound = repo.SearchClient(clientToSearchOrAdd);
            if (clientFound != null)
            {
                clientInRepo = clientFound;
                return true;
            }

            fillMoreDetails?.Invoke(clientToSearchOrAdd);
            repo.AddPersonNoCheck(clientToSearchOrAdd);
            clientInRepo = clientToSearchOrAdd;
            return false;
        }

        /// <summary>
        ///  Search the GP in the repo and return it or add it to the repo and return.
        /// </summary>
        /// <param name="repo">The repository</param>
        /// <param name="gpToSearchOrAdd">The GP to search which may have only partial information.</param>
        /// <param name="gpInRepo">The GP in the repo if found or the GP queried which has now been added to the repo.</param>
        /// <param name="fillMoreDetails">Provide details to the GP to add pre-adding. Note as client <paramref name="gpToSearchOrAdd"/> may not contain essential info such as provider number which is required by the AddPersonNoCheck() function, it is this function responsibility to ensure it.</param>
        /// <returns>True if an existing GP has been found</returns>
        public static bool SearchOrAddGeneralPractitioner(this Repository repo, GeneralPractitioner gpToSearchOrAdd, out GeneralPractitioner gpInRepo, Action<GeneralPractitioner>? fillMoreDetails = null)
        {
            var gpFound = repo.SearchGeneralPractioner(gpToSearchOrAdd);
            if (gpFound != null)
            {
                gpInRepo = gpFound;
                return true;
            }

            fillMoreDetails?.Invoke(gpToSearchOrAdd);
            repo.AddPersonNoCheck(gpToSearchOrAdd);
            gpInRepo = gpToSearchOrAdd;
            return false;
        }

        public static void AcceptReferral(this Repository repository, GeneralPractitioner referringGP, Client client)
        {
            repository.AddAndExecuteEvent(new Referral(referringGP, client));
        }
    }
}
