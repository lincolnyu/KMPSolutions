using KMPBookingCore;
using KMPBookingPlus;
using System;
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
        public OleDbConnection _connection;
        private Dictionary<string, List<ClientRecord>> _nameToClients;
        private Dictionary<string, List<ClientRecord>> _mediToClients;
        private Dictionary<string, List<ClientRecord>> _phoneToClients;
        private Dictionary<int, ClientRecord> _idToClient;

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
                _idToClient = new Dictionary<int, ClientRecord>();

                using (var r = _connection.RunReaderQuery(query))
                {
                    while (r.Read())
                    {
                        var name = r.GetString(1);
                        var (fn, sn) = BookingIcs.SmartParseName(name);
                        var cr = new ClientRecord
                        {
                            FirstName = fn,
                            Surname = sn,
                            DOB = r.TryGetDateTime(2),
                            Gender = ClientRecord.ParseGender(r.TryGetString(3)),
                            MedicareNumber = r.TryGetString(4),
                            PhoneNumber = r.TryGetString(5),
                            Address = r.TryGetString(6)
                        };
                        var id = r.GetInt32(0);
                        if (!_idToClient.ContainsKey(id))
                        {
                            _idToClient[id] = cr;
                            ClientId.Items.Add(id.ClientIdToStr());
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

        private void SearchById(string text)
        {
            throw new NotImplementedException();
        }

        private void SearchByMedi(string v)
        {
            throw new NotImplementedException();
        }

        private void SearchByName(string v)
        {
            throw new NotImplementedException();
        }

        private void SearchByPhone(string v)
        {
            throw new NotImplementedException();
        }
    }
}
