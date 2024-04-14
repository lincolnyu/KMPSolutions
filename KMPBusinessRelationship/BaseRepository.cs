using KMPBusinessRelationship.Objects;
using System.Collections.Generic;
using System.Linq;

namespace KMPBusinessRelationship
{
    public abstract class BaseRepository
    {
        #region Core (persisted) data

        public abstract IEnumerable<Person> Persons { get; }
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
        protected abstract void AddPerson(Person person);

        public readonly Dictionary<string, Client> MedicareNumberToClientMap = new Dictionary<string, Client>();
        public readonly Dictionary<string, GeneralPractitioner> ProviderNumberToGPMap = new Dictionary<string, GeneralPractitioner>();

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

        public void AddPersonNoCheck(Person person)
        {
            AddPerson(person);
            if (person is Client client)
            {
                MedicareNumberToClientMap[client.MedicareNumber] = client;
            }
            else if (person is GeneralPractitioner gp)
            {
                ProviderNumberToGPMap[gp.ProviderNumber] = gp;
            }
        }

        public List<Client> ReCreateMedicareNumberToClientMap()
        {
            var clientsWithDuplicateKey = new List<Client>();
            foreach (var client in Persons.OfType<Client>())
            {
                if (MedicareNumberToClientMap.ContainsKey(client.MedicareNumber))
                {
                    // Error: medicare number not unique.
                    clientsWithDuplicateKey.Add(client);
                }
                else
                {
                    MedicareNumberToClientMap[client.MedicareNumber] = client;
                }
            }
            return clientsWithDuplicateKey;
        }

        public List<GeneralPractitioner> ReCreateProviderNumberToGPMap()
        {
            var gpsWithDuplicateKey = new List<GeneralPractitioner>();
            foreach (var gp in Persons.OfType<GeneralPractitioner>())
            {
                if (ProviderNumberToGPMap.ContainsKey(gp.ProviderNumber))
                {
                    // Error: provider number not unique.
                    gpsWithDuplicateKey.Add(gp);
                }
                else
                {
                    ProviderNumberToGPMap[gp.ProviderNumber] = gp;
                }
            }
            return gpsWithDuplicateKey;
        }
    }
}
