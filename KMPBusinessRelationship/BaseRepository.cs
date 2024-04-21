using KMPBusinessRelationship.Objects;
using System.Collections.Generic;
using System.Diagnostics;

namespace KMPBusinessRelationship
{
    public abstract class BaseRepository
    {
        #region Core (persisted) data

        public abstract IEnumerable<Referrer> Referrers { get; }
        public abstract IEnumerable<Client> Clients { get; }
        public abstract IEnumerable<Event> Events { get; }

        #endregion

        protected void SyncToEventList()
        {
            EventList.Clear();
            foreach (var ev in Events)
            {
                EventList.Add(ev);
            }
            CurrentEventIndex = 0;
        }

        protected abstract void AddEvent(Event e);
        protected abstract void AddReferrer(Referrer referrer);
        protected abstract void AddClient(Client client);

        public readonly Dictionary<string, Client> CareNumberToClientMap = new Dictionary<string, Client>();
        public readonly Dictionary<string, Referrer> ProviderNumberToReferrerMap = new Dictionary<string, Referrer>();

        public int CurrentEventIndex { get; set; } = 0;
        public List<Event> EventList { get; } = new List<Event>();

        public void AddAndExecuteEvent(Event e)
        {
            ExecuteToEnd();

            AddEvent(e);
            EventList.Add(e);

            e.Redo();
            CurrentEventIndex = EventList.Count;
        }

        public void ExecuteToEnd() => ExecuteTo(EventList.Count);

        public void ExecuteTo(int targetIndex)
        {
            if (targetIndex > CurrentEventIndex)
            {
                for (; CurrentEventIndex < targetIndex; CurrentEventIndex++)
                {
                    var e = EventList[CurrentEventIndex];
                    e.Redo();
                }
            }
            else if (targetIndex < CurrentEventIndex)
            {
                for (; CurrentEventIndex > targetIndex; CurrentEventIndex++)
                {
                    var e = EventList[CurrentEventIndex-1];
                    e.Undo();
                }
            }
        }

        /// <summary>
        ///  Add client assuming the entry is not existent
        /// </summary>
        /// <param name="referrer">The referrer to add</param>
        public void AddClientNoCheck(Client client)
        {
            Debug.Assert(!CareNumberToClientMap.ContainsKey(client.CareNumber));
            AddClient(client);
            CareNumberToClientMap[client.CareNumber] = client;
        }

        /// <summary>
        ///  Add referrer assuming the entry is not existent
        /// </summary>
        /// <param name="referrer">The referrer to add</param>
        public void AddReferrerNoCheck(Referrer referrer)
        {
            Debug.Assert(!ProviderNumberToReferrerMap.ContainsKey(referrer.ProviderNumber));
            AddReferrer(referrer);
            ProviderNumberToReferrerMap[referrer.ProviderNumber] = referrer;
        }

        public List<Client> ReCreateIdToClientMap()
        {
            var clientsWithDuplicateKey = new List<Client>();
            foreach (var client in Clients)
            {
                if (CareNumberToClientMap.ContainsKey(client.CareNumber))
                {
                    // Error: ID not unique.
                    clientsWithDuplicateKey.Add(client);
                }
                else
                {
                    CareNumberToClientMap[client.CareNumber] = client;
                }
            }
            return clientsWithDuplicateKey;
        }

        public List<Referrer> ReCreateIdToReferrerMap()
        {
            var referrersWithDuplicateKey = new List<Referrer>();
            foreach (var referrer in Referrers)
            {
                if (ProviderNumberToReferrerMap.ContainsKey(referrer.ProviderNumber))
                {
                    // Error: ID not unique.
                    referrersWithDuplicateKey.Add(referrer);
                }
                else
                {
                    ProviderNumberToReferrerMap[referrer.ProviderNumber] = referrer;
                }
            }
            return referrersWithDuplicateKey;
        }
    }
}
