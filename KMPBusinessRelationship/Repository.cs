using KMPBusinessRelationship.Objects;
using System.Collections.Generic;
using System.Linq;

namespace KMPBusinessRelationship
{
    public class Repository
    {
        #region Core (persisted) data

        public List<Person> Persons { get; private set; } = new List<Person>();
        public List<Event> Events { get; private set; } = new List<Event>();
       
        #endregion

        public readonly Dictionary<string, Client> MedicareNumberToClientMap = new Dictionary<string, Client>();
        public readonly Dictionary<string, GeneralPractitioner> ProviderNumberToGPMap = new Dictionary<string, GeneralPractitioner>();

        public int CurrentEventIndex { get; set; }

        public void AddPersonNoCheck(Person person)
        {
            Persons.Add(person);
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
