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

        /// <summary>
        ///  Search the client in the repo and return it or add it to the repo and return.
        /// </summary>
        /// <param name="repo">The repository</param>
        /// <param name="clientToSearchOrAdd">The client to search which may have partial information.</param>
        /// <param name="clientInRepo">The client in the repo if found or the client  queried which has now been added to the repo.</param>
        /// <returns>True if an existing client has been found</returns>
        public static bool SearchOrAddClient(this Repository repo, Client clientToSearchOrAdd, out Client clientInRepo)
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

            if (!string.IsNullOrEmpty(clientToSearchOrAdd.MedicareNumber))
            {
                if(repo.MedicareNumberToClientMap.TryGetValue(clientToSearchOrAdd.MedicareNumber, out var client))
                {
                    clientInRepo = client;
                    return true;
                }
            }
            else
            {
                foreach (var client in repo.Persons.OfType<Client>())
                {
                    if (Matched(clientToSearchOrAdd, client))
                    {
                        clientInRepo = client;
                        return true;
                    }
                }
            }
            repo.AddPersonNoCheck(clientToSearchOrAdd);
            clientInRepo = clientToSearchOrAdd;
            return false;
        }

        public static bool SearchOrAddGeneralPractitioner(this Repository repo, GeneralPractitioner gpToSearchOrAdd, out GeneralPractitioner gpInRepo)
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

            if (!string.IsNullOrEmpty(gpToSearchOrAdd.ProviderNumber))
            {
                if (repo.ProviderNumberToGPMap.TryGetValue(gpToSearchOrAdd.ProviderNumber, out var gp))
                {
                    gpInRepo = gp;
                    return true;
                }
            }
            else
            {
                foreach (var gp in repo.Persons.OfType<GeneralPractitioner>())
                {
                    if (Matched(gpToSearchOrAdd, gp))
                    {
                        gpInRepo = gp;
                        return true;
                    }
                }
            }
            repo.AddPersonNoCheck(gpToSearchOrAdd);
            gpInRepo = gpToSearchOrAdd;
            return false;
        }
    }
}
