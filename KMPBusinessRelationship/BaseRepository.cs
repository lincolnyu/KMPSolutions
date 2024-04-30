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

        protected abstract void AddEvent(Event e);
        protected abstract void AddReferrer(Referrer referrer);
        protected abstract void AddClient(Client client);

        public readonly Dictionary<string, Client> IdToClientMap = new Dictionary<string, Client>();
        public readonly Dictionary<string, Referrer> IdToReferrerMap = new Dictionary<string, Referrer>();

        public int CurrentEventIndex { get; set; } = 0;
        public List<Event> EventList { get; } = new List<Event>();

        public void AddAndExecuteEvent(Event e)
        {
            ExecuteToEnd();

            e.Id = EventList.Count + 1;
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
            Debug.Assert(!IdToClientMap.ContainsKey(client.Id));
            AddClient(client);
            IdToClientMap[client.Id] = client;
        }

        /// <summary>
        ///  Add referrer assuming the entry is not existent
        /// </summary>
        /// <param name="referrer">The referrer to add</param>
        public void AddReferrerNoCheck(Referrer referrer)
        {
            foreach (var id in referrer.Ids)
            {
                Debug.Assert(!IdToReferrerMap.ContainsKey(id));
                AddReferrer(referrer);
                IdToReferrerMap[id] = referrer;
            }
        }

        protected void ReCreateEventList()
        {
            EventList.Clear();
            foreach (var ev in Events)
            {
                EventList.Add(ev);
            }
            CurrentEventIndex = 0;
        }

        protected List<Client> ReCreateIdToClientMap()
        {
            IdToClientMap.Clear();
            var clientsWithDuplicateKey = new List<Client>();
            foreach (var client in Clients)
            {
                if (IdToClientMap.ContainsKey(client.Id))
                {
                    // Error: ID not unique.
                    clientsWithDuplicateKey.Add(client);
                }
                else
                {
                    IdToClientMap[client.Id] = client;
                }
            }
            return clientsWithDuplicateKey;
        }

        protected List<Referrer> ReCreateIdToReferrerMap()
        {
            IdToReferrerMap.Clear();
            var referrersWithDuplicateKey = new List<Referrer>();
            foreach (var referrer in Referrers)
            {
                foreach (var id in referrer.Ids)
                {
                    if (IdToReferrerMap.ContainsKey(id))
                    {
                        // Error: ID not unique.
                        referrersWithDuplicateKey.Add(referrer);
                    }
                    else
                    {
                        IdToReferrerMap[id] = referrer;
                    }
                }
            }
            return referrersWithDuplicateKey;
        }
    }
}
