using KMPBookingCore;
using KMPBookingPlus;
using static KMPBookingCore.BookingIcs;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KMPControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ClientsControl : UserControl
    {
        class AutoResetSuppressor
        {
            public bool Suppressing { get; private set; } = false;

            public void Run(Action proc)
            {
                if (!Suppressing)
                {
                    Suppressing = true;
                    proc();
                    Suppressing = false;
                }
            }
        }

        public OleDbConnection _connection;
        private Dictionary<string, List<ClientRecord>> _nameToClients;
        private Dictionary<string, List<ClientRecord>> _mediToClients;
        private Dictionary<string, List<ClientRecord>> _phoneToClients;
        private Dictionary<string, ClientRecord> _idToClient;
        AutoResetSuppressor _suppressSearch = new AutoResetSuppressor();
        public Action<string> ErrorReporter { private get;  set; }

        private ClientRecord _activeClient;
        public ClientRecord ActiveClient
        {
            get => _activeClient;
            private set
            {
                if (_activeClient != value)
                {
                    _activeClient = value;
                    ActiveClientChanged?.Invoke();
                }
            }
        }

        public delegate void ActiveClientChangedEventHandler();
        public event ActiveClientChangedEventHandler ActiveClientChanged;

        public ClientsControl()
        {
            InitializeComponent();
        }

        public void SetDataConnection(OleDbConnection connection)
        {
            _connection = connection;
            LoadData();
        }

        private void LoadData()
        {
            if (_connection != null)
            {
                var query = "select ID, [Client Name], [DOB], [Gender], [Medicare], [Phone], [Address] from Clients";
                _nameToClients = new Dictionary<string, List<ClientRecord>>();
                _mediToClients = new Dictionary<string, List<ClientRecord>>();
                _phoneToClients = new Dictionary<string, List<ClientRecord>>();
                _idToClient = new Dictionary<string, ClientRecord>();

                using (var r = _connection.RunReaderQuery(query))
                {
                    while (r.Read())
                    {
                        var name = r.GetString(1);
                        var (fn, sn) = BookingIcs.SmartParseName(name);
                        var id = r.GetInt32(0);
                        var idstr = id.ClientIdToStr();
                        var cr = new ClientRecord
                        {
                            Id = idstr,
                            FirstName = fn,
                            Surname = sn,
                            DOB = r.TryGetDateTime(2),
                            Gender = ClientRecord.ParseGender(r.TryGetString(3)),
                            MedicareNumber = r.TryGetString(4),
                            PhoneNumber = r.TryGetString(5),
                            Address = r.TryGetString(6)
                        };
                        if (!_idToClient.ContainsKey(idstr))
                        {
                            _idToClient[idstr] = cr;
                            ClientId.Items.Add(idstr);
                        }
                        else
                        {
                            //TODO it's an error
                        }
                        if (!_nameToClients.TryGetValue(name, out var namelist))
                        {
                            _nameToClients.Add(name, new List<ClientRecord> { cr });
                            ClientName.Items.Add(name);
                        }
                        else
                        {
                            namelist.Add(cr);
                        }
                        if (!string.IsNullOrWhiteSpace(cr.MedicareNumber))
                        {
                            if (!_mediToClients.TryGetValue(cr.MedicareNumber, out var medilist))
                            {
                                _mediToClients.Add(cr.MedicareNumber, new List<ClientRecord> { cr });
                                ClientMedicare.Items.Add(cr.MedicareNumber);
                            }
                            else
                            {
                                medilist.Add(cr);
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(cr.PhoneNumber))
                        {
                            if (!_phoneToClients.TryGetValue(cr.PhoneNumber, out var phonelist))
                            {
                                _phoneToClients.Add(cr.PhoneNumber, new List<ClientRecord> { cr });
                                ClientNumber.Items.Add(cr.PhoneNumber);
                            }
                            else
                            {
                                phonelist.Add(cr);
                            }
                        }
                    }
                }
            }
        }

        private void ClientMediKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByMedi(ClientMedicare.Text);
            }
        }

        private void ClientMediSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByMedi(e.AddedItems[0].ToString());
            }
        }

        private void SearchByMediClick(object sender, RoutedEventArgs e)
        {
            SearchByMedi(ClientMedicare.Text);
        }

        private void ClientNameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByName(ClientName.Text);
            }
        }

        private void ClientNameSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByName(e.AddedItems[0].ToString());
            }
        }

        private void SearchByNameClick(object sender, RoutedEventArgs e)
        {
            SearchByName(ClientName.Text);
        }

        private void ClientNumberKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByPhone(ClientNumber.Text);
            }
        }

        private void ClientNumberSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByPhone(e.AddedItems[0].ToString());
            }
        }

        private void SearchByPhoneClick(object sender, RoutedEventArgs e)
        {
            SearchByPhone(ClientNumber.Text);
        }

        private void ClientIdKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchById(ClientId.Text);
            }
        }

        private void ClientIdSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchById(e.AddedItems[0].ToString());
            }
        }

        private void SearchByIdClick(object sender, RoutedEventArgs e)
        {
            SearchById(ClientId.Text);
        }

        private IEnumerable<string> RecordsToStrings(IList<ClientRecord> clients)
        {
            foreach (var c in clients)
            {
                yield return $"#{c.Id}: {c.ClientFormalName()} (Medicare#{c.MedicareNumber}, Phone#{c.PhoneNumber})";
            }
        }

        private void SearchBy(IList<ClientRecord> finderResults, string duplicateMessage)
        {
            _suppressSearch.Run(() =>
            {
                var clients = finderResults;
                ActiveClient = null;
                if (clients.Count > 1)
                {
                    var dc = new DuplicateClients(duplicateMessage,
                        RecordsToStrings(clients))
                    {
                        //TODO set owner to the owner of this control
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    if (dc.ShowDialog() == true && dc.SelectedIndex >= 0)
                    {
                        ActiveClient = clients[dc.SelectedIndex];
                    }
                }
                else if (clients.Count == 1)
                {
                    ActiveClient = clients[0];
                }
                else
                {
                    ErrorReporter?.Invoke("Error: Client not found.");
                }
                if (ActiveClient != null)
                {
                    ClientId.Text = ActiveClient.Id;
                    ClientMedicare.Text = ActiveClient.MedicareNumber;
                    ClientName.Text = ActiveClient.ClientFormalName();
                    ClientNumber.Text = ActiveClient.PhoneNumber;
                }
                else
                {
                }
            });
        }

        private void SearchByMedi(string medi)
        {
            SearchBy(_idToClient.Values.FindByMedicareNumberContaining(medi)
                    .OrderBy(x => x.MedicareNumber).ToList(),
                    $"Multiple clients found with medicare number containing '{medi}'");
        }

        private void SearchById(string id)
        {
            SearchBy(_idToClient.Values.FindByIdContaining(id)
                    .OrderBy(x => x.Id).ToList(),
                    $"Multiple clients found with ID containing '{id}'");
        }

        private void SearchByName(string name)
        {
            SearchBy(_idToClient.Values.FindNameContaining(name)
                    .OrderBy(x => x.Id).ToList(),
                    $"Multiple clients found with name containing '{name}'");
        }

        private void SearchByPhone(string phone)
        {
            SearchBy(_idToClient.Values.FindByPhoneNumberContaining(phone)
                    .OrderBy(x => x.Id).ToList(),
                    $"Multiple clients found with phone number containing '{phone}'");
        }
    }
}
