using KMPBookingCore;
using System.Collections.Generic;
using System.Data.OleDb;

namespace KMPBookingPlus
{
    public static class Query
    {
        public class ClientData
        {
            public Dictionary<string, List<Client>> NameToClients { get; } = new Dictionary<string, List<Client>> { };
            public Dictionary<string, List<Client>> PhoneToClients { get; } = new Dictionary<string, List<Client>> { };
            public Dictionary<string, Client> IdToClient { get; } = new Dictionary<string, Client> { };

            public List<string> Names { get; } = new List<string> { };
            public List<string> Ids { get; } = new List<string> { };
            public List<string> PhoneNumbers { get; } = new List<string> { };
        }

        public static ClientData LoadClientData(this OleDbConnection connection)
        {
            var query = "select [Medicare Number], [First Name], [Surname], [DOB], [Gender], [Phone], [Address] from Client";

            var clientData = new ClientData();

            using (var r = connection.RunReaderQuery(query))
            {
                while (r.Read())
                {
                    var medicareNumber = r.GetString(0);
                    var firstName = r.TryGetString(1).Trim();
                    var surname = r.TryGetString(2).Trim();
                    var name = $"{surname}, {firstName}";
                    var dob = r.TryGetDateTime(3);
                    var gender = r.TryGetString(4);
                    var phone = r.TryGetString(5);
                    var address = r.TryGetString(6);
                    var cr = new Client
                    {
                        MedicareNumber = medicareNumber,
                        FirstName = firstName,
                        Surname = surname,
                        DOB = dob,
                        Gender = gender,
                        PhoneNumber = phone,
                        Address = address
                    };
                    if (!clientData.IdToClient.ContainsKey(medicareNumber))
                    {
                        clientData.IdToClient[medicareNumber] = cr;
                        clientData.Ids.Add(medicareNumber);
                    }
                    else
                    {
                        //TODO it's an error
                    }
                    if (!clientData.NameToClients.TryGetValue(name, out var namelist))
                    {
                        clientData.NameToClients.Add(name, new List<Client> { cr });
                        clientData.Names.Add(name);
                    }
                    else
                    {
                        namelist.Add(cr);
                    }
                    if (!string.IsNullOrWhiteSpace(cr.PhoneNumber))
                    {
                        if (!clientData.PhoneToClients.TryGetValue(cr.PhoneNumber, out var phonelist))
                        {
                            clientData.PhoneToClients.Add(cr.PhoneNumber, new List<Client> { cr });
                            clientData.PhoneNumbers.Add(cr.PhoneNumber);
                        }
                        else
                        {
                            phonelist.Add(cr);
                        }
                    }
                }

                clientData.Ids.Sort();
                clientData.Names.Sort();
                clientData.PhoneNumbers.Sort();

                return clientData;
            }
        }
        public static void LoadClientEventData(this OleDbConnection connection, ClientData clientData)
        {
            var query = "select [Medicare Number] from Client, Event where Client.[Medicare Number] = Event.[Client Id]";
        }
    }
}
