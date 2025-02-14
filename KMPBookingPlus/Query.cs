using KMPBookingCore;
using KMPBookingCore.Database;
using KMPBookingCore.DbObjects;
using System;
using System.Collections.Generic;
using System.Data.OleDb;

namespace KMPBookingPlus
{
    // TODO Abstract the object-dbtable relations
    public static class Query
    {
        public static bool IsNullId(int id) => id != 0;

        public class EntriesWithID<EntryType, IdType> where EntryType:DbObject
        {
            public Dictionary<IdType, EntryType> IdToEntry { get;  } = new Dictionary<IdType, EntryType>();
            public List<IdType> Ids { get; } =  new List<IdType>();
            public EntriesWithID()
            {
            }

            public virtual void SortCollections()
            {
                Ids.Sort();
            }
        }

        public static T LoadData<T, EntryType, IdType>(OleDbConnection connection, string query, Func<OleDbDataReader, (EntryType, IdType)> createEntry, Action<T, EntryType> postCreate=null, Action<T> postAdd=null) where T : EntriesWithID<EntryType, IdType>, new() where EntryType: DbObject
        {
            DbObject.PushLoadingFromDb(true);
            var data = new T();
            using (var r = connection.RunReaderQuery(query))
            {
                while (r.Read())
                {
                    try
                    {
                        var (e, id) = createEntry(r);
                        if (!data.IdToEntry.ContainsKey(id))
                        {
                            data.Ids.Add(id);
                            data.IdToEntry.Add(id, e);
                            postCreate?.Invoke(data, e);
                        }
                        else
                        {
                            // TODO ntry already added
                        }
                    }
                    catch (Exception ex)
                    {
                        // TODO report error
                        continue;
                    }
                }
            }
            data.SortCollections();
            postAdd?.Invoke(data);
            DbObject.PopLoadingFromDb();
            return data;
        }

        public class ClientData : EntriesWithID<Client, string>
        {
            public Dictionary<string, List<Client>> NameToEntry { get; } = new Dictionary<string, List<Client>> { };
            public Dictionary<string, List<Client>> PhoneToEntry { get; } = new Dictionary<string, List<Client>> { };

            public List<string> Names { get; } = new List<string> { };
            public List<string> PhoneNumbers { get; } = new List<string> { };

            public override void SortCollections()
            {
                base.SortCollections();
                Names.Sort();
                PhoneNumbers.Sort();
            }

            public bool TryAdd(Client client)
            {
                if (IdToEntry.ContainsKey(client.MedicareNumber))
                {
                    return false;
                }
                IdToEntry.Add(client.MedicareNumber, client);
                var name = client.ClientFormalName();
                var index = Names.IndexOf(name);
                if (index < 0)
                {
                    index = -index - 1;
                    Names.Insert(index, name);
                }
                var phone = client.Phone;
                index = Names.IndexOf(phone);
                if (index < 0)
                {
                    index = -index - 1;
                    PhoneNumbers.Insert(index, phone);
                }
                return true;
            }
        }

        public class GPData : EntriesWithID<GP, string>
        {
            public Dictionary<string, List<GP>> NameToEntry { get; } = new Dictionary<string, List<GP>> { };
            public Dictionary<string, List<GP>> PhoneToEntry { get; } = new Dictionary<string, List<GP>> { };

            public List<string> Names { get; } = new List<string> { };
            public List<string> PhoneNumbers { get; } = new List<string> { };

            public override void SortCollections()
            {
                base.SortCollections();
                Names.Sort();
                PhoneNumbers.Sort();
            }

            public bool TryAdd(GP gp)
            {
                if (IdToEntry.ContainsKey(gp.ProviderNumber))
                {
                    return false;
                }
                IdToEntry.Add(gp.ProviderNumber, gp);
                var name = gp.Name;
                var index = Names.IndexOf(name);
                if (index < 0)
                {
                    index = -index - 1;
                    Names.Insert(index, name);
                }
                var phone = gp.Phone;
                index = PhoneNumbers.IndexOf(phone);
                if (index < 0)
                {
                    index = -index - 1;
                    PhoneNumbers.Insert(index, phone);
                }
                return true;
            }
        }

        public class EventData : EntriesWithID<Event, int>
        {
        }

        public class ServiceData : EntriesWithID<Service, int>
        {
        }

        public class ReceiptData : EntriesWithID<Receipt, int>
        {
        }

        public class BookingData : EntriesWithID<Booking, int>
        {
        }

        public static ClientData LoadClientData(OleDbConnection connection)
        {
            var query = "select [Medicare Number], [First Name], Surname, DOB, Gender, Phone, Address, [Referring Date] from Client";

            return LoadData<ClientData, Client, string>(connection, query, r => {
                var medicareNumber = r.GetString(0);
                var firstName = r.TryGetString(1).Trim();
                var surname = r.TryGetString(2).Trim();
                
                var dob = r.TryGetDateTime(3);
                var gender = r.TryGetString(4);
                var phone = r.TryGetString(5);
                var address = r.TryGetString(6);
                var referringDate = r.TryGetDateTime(7);
                var cr = new Client
                {
                    MedicareNumber = medicareNumber,
                    FirstName = firstName,
                    Surname = surname,
                    DOB = dob,
                    Gender = gender,
                    Phone = phone,
                    Address = address,
                    ReferringDate = referringDate
                };
                return (cr, medicareNumber);
            }, (clientData, cr)=> {
                var name = $"{cr.Surname}, {cr.FirstName}";
                if (!clientData.NameToEntry.TryGetValue(name, out var namelist))
                {
                    clientData.NameToEntry.Add(name, new List<Client> { cr });
                    clientData.Names.Add(cr.ClientFormalName());
                }
                else
                {
                    namelist.Add(cr);
                }
                if (!string.IsNullOrWhiteSpace(cr.Phone))
                {
                    if (!clientData.PhoneToEntry.TryGetValue(cr.Phone, out var phonelist))
                    {
                        clientData.PhoneToEntry.Add(cr.Phone, new List<Client> { cr });
                        clientData.PhoneNumbers.Add(cr.Phone);
                    }
                    else
                    {
                        phonelist.Add(cr);
                    }
                }
            }, clientData=> {
                clientData.Names.Sort();
                clientData.PhoneNumbers.Sort();
            });
        }

        public static GPData LoadGPData(OleDbConnection connection)
        {
            var query = "select [Provider Number], Name, Phone, Fax, Address from GP";

            return LoadData<GPData, GP, string>(connection, query, r => {
                var providerNumber = r.GetString(0);
                var name = r.GetString(1);
                var phoneNumber = r.TryGetString(2);
                var fax = r.TryGetString(3);
                var address = r.TryGetString(4);
                var gp = new GP
                {
                    ProviderNumber = providerNumber,
                    Name = name,
                    Phone = phoneNumber,
                    Fax = fax,
                    Address = address,
                };

                return (gp, providerNumber);
            }, (gpData, gp) => {
                if (!gpData.NameToEntry.TryGetValue(gp.Name, out var namelist))
                {
                    gpData.NameToEntry.Add(gp.Name, new List<GP> { gp });
                    gpData.Names.Add(gp.Name);
                }
                else
                {
                    namelist.Add(gp);
                }
                if (!string.IsNullOrWhiteSpace(gp.Phone))
                {
                    if (!gpData.PhoneToEntry.TryGetValue(gp.Phone, out var phonelist))
                    {
                        gpData.PhoneToEntry.Add(gp.Phone, new List<GP> { gp });
                        gpData.PhoneNumbers.Add(gp.Phone);
                    }
                    else
                    {
                        phonelist.Add(gp);
                    }
                }
            }, clientData => {
                clientData.Names.Sort();
                clientData.PhoneNumbers.Sort();
            });
        }

        public static void CorrelateClientAndGP(OleDbConnection connection, ClientData clientData, GPData gpData)
        {
            var query = "select Client.[Medicare Number], GP.[Provider Number] from Client, GP where Client.[Referring GP ID]=GP.[Provider Number]";

            DbObject.PushLoadingFromDb(true);

            using (var r = connection.RunReaderQuery(query))
            {
                while (r.Read())
                {
                    var clientId = r.GetString(0);
                    var gpId = r.GetString(1);
                    if (!clientData.IdToEntry.TryGetValue(clientId, out var client))
                    {
                        throw new KeyNotFoundException($"Client ID {clientId} not found for Client-GP relations.");
                    }
                    if (!gpData.IdToEntry.TryGetValue(gpId, out var gp))
                    {
                        throw new KeyNotFoundException($"GP ID {gpId} not found for Client-GP relations.");
                    }
                    client.ReferringGP = gp;
                }
            }

            DbObject.PopLoadingFromDb();
        }

        public static BookingData LoadBookingData(OleDbConnection connection)
        {
            var query = "select ID, [Made On], Duration, [Reminder Date]";

            return LoadData<BookingData, Booking, int>(connection, query, r => {
                var bookingId = r.GetInt32(0);
                var madeOn = r.GetDateTime(1);
                var durationMins = r.GetInt32(2);
                var reminderDate = r.GetDateTime(3);

                var booking = new Booking
                {
                    Id = bookingId,
                    MadeOn = madeOn,
                };

                return (booking, bookingId);
            });
        }

        public static ReceiptData LoadReceiptData(OleDbConnection connection)
        {
            var query = "select ID, Diagnosis, [Claim Number], [Health Fund], [Membership Number], [Total Due], [Payment Received], Discount, Balance from Receipt";

            return LoadData<ReceiptData, Receipt, int>(connection, query, r => {
                var id = r.GetInt32(0);
                var diagnosis = r.GetString(1);
                var claimNumber = r.GetString(2);
                var healthFund = r.GetString(3);
                var membershipNumber = r.GetString(4);
                var totalDue = r.GetDecimal(5);
                var paymentReceived = r.GetDecimal(6);
                var discount = r.GetDouble(7);
                var balance = r.GetDecimal(8);

                var receipt = new Receipt
                {
                    Id = id,
                    Diagnosis = diagnosis,
                    ClaimNumber = claimNumber,
                    HealthFund = healthFund,
                    MembershipNumber = membershipNumber,
                    TotalDue = totalDue,
                    PaymentReceived = paymentReceived,
                    Discount = discount
                };
                receipt.Calculate();

                return (receipt, id);
            });
        }

        public static ServiceData LoadServiceData(OleDbConnection connection, EventData eventData, ReceiptData receiptData, BookingData bookingData)
        {
            var query = "select ID, Service, [Receipt ID], [Booking ID], [Total Fee], Owing, Benefit, Gap, Discount select from Service";

            return LoadData<ServiceData, Service, int>(connection, query, r => {
                var id = r.GetInt32(0);
                var serviceConetnt = r.GetString(1);
                var receiptId = r.GetInt32(2);
                var bookingId = r.GetInt32(3);
                var totalFee = r.GetDecimal(4);
                var owing = r.GetDecimal(5);
                var benefit = r.GetDecimal(6);
                var gap = r.GetDecimal(7);
                var discount = r.GetDouble(8);

                Receipt receipt = null;
                if (!IsNullId(receiptId) && !receiptData.IdToEntry.TryGetValue(receiptId, out receipt))
                {
                    throw new KeyNotFoundException($"Receipt ID {receiptId} not found for service {id}.");
                }
                Booking booking = null;
                if (!IsNullId(bookingId) && !bookingData.IdToEntry.TryGetValue(bookingId, out booking))
                {
                    throw new KeyNotFoundException($"Receipt ID {bookingId} not found for service {id}.");
                }

                var service = new Service
                {
                    Id = id,
                    ServiceContent = serviceConetnt,
                    Booking = booking,
                    Receipt = receipt,
                    TotalFee = totalFee,
                    Owing = owing,
                    Benefit = benefit,
                    Gap = gap,
                    Discount = (decimal)discount
                };
                service.Calculate();

                return (service, id);
            });
        }

        public static EventData LoadClientEventData(OleDbConnection connection, ClientData clientData, ServiceData serviceData, BookingData bookingData, ReceiptData receiptData)
        {
            var query = "select Client.[Medicare Number], Event.ID, Event.[Event Date], Event.Type from Client, Event where Client.[Medicare Number]=Event.[Client ID]";

            return LoadData<EventData, Event, int>(connection, query, r =>
            {
                var clientId = r.GetString(0);
                var eventId = r.GetInt32(1);
                var date = r.GetDateTime(2);
                var type = r.TryGetString(3);

                if (!clientData.IdToEntry.TryGetValue(clientId, out var client))
                {
                    throw new KeyNotFoundException($"Client ID {clientId} not found for event {eventId}.");
                }

                Event e = null;
                switch (type)
                {
                    case "Service":
                        if (serviceData != null)
                        {
                            if (serviceData.IdToEntry.TryGetValue(eventId, out var service))
                            {
                                e = service;
                            }
                            else
                            {
                                throw new KeyNotFoundException($"Service not found for service event {eventId}");
                            }
                        }
                        break;
                    case "Booking":
                        if (bookingData != null)
                        {
                            if (bookingData.IdToEntry.TryGetValue(eventId, out var booking))
                            {
                                e = booking;
                            }
                            else
                            {
                                throw new KeyNotFoundException($"Service not found for service event {eventId}");
                            }
                        }
                        break;
                    case "Receipt":
                        if (receiptData != null)
                        {
                            if (receiptData.IdToEntry.TryGetValue(eventId, out var receipt))
                            {
                                e = receipt;
                            }
                            else
                            {
                                throw new KeyNotFoundException($"Service not found for service event {eventId}");
                            }
                        }
                        break;
                    default:
                        if (e == null && serviceData != null)
                        {
                            if (serviceData.IdToEntry.TryGetValue(eventId, out var service))
                            {
                                e = service;
                            }
                        }
                        if (e == null && bookingData != null)
                        {
                            if (bookingData.IdToEntry.TryGetValue(eventId, out var booking))
                            {
                                e = booking;
                            }
                        }
                        if (e == null && receiptData != null)
                        {
                            if (receiptData.IdToEntry.TryGetValue(eventId, out var receipt))
                            {
                                e = receipt;
                            }
                        }
                        break;
                }

                if (e == null)
                {
                    e = new Event
                    {
                        Id = eventId,
                        EventDate = date,
                        Type = type,
                    };
                }

                e.Client = client;
                client.Events.Add(e);

                return (e, eventId);
            });
        }
    }
}
