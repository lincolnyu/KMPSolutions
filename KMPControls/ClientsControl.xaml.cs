using KMPBookingCore;
using KMPBookingPlus;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text;

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

        public enum Mode
        {
            Init,
            Simple,
            Input
        }

        private Mode _inputMode = Mode.Init;
        public Mode InputMode
        {
            get => _inputMode;
            set
            {
                if (_inputMode != value && value != Mode.Init)
                {
                    _inputMode = value;
                    switch (_inputMode)
                    {
                        case Mode.Simple:
                            ClientGenderSection.Visibility = Visibility.Collapsed;
                            ClientDobSection.Visibility = Visibility.Collapsed;
                            ClientAddressSection.Visibility = Visibility.Collapsed;
                            AddClientControl.Visibility = Visibility.Collapsed;
                            IsEditing.IsChecked = false;
                            break;
                        case Mode.Input:
                            ClientGenderSection.Visibility = Visibility.Visible;
                            ClientDobSection.Visibility = Visibility.Visible;
                            ClientAddressSection.Visibility = Visibility.Visible;
                            AddClientControl.Visibility = Visibility.Visible;
                            IsAdding.IsChecked = true;
                            IsAdding.IsChecked = false;
                            break;
                    }
                }
            }
        }
        public bool IsUpdating => CurrentUpdateMode == UpdateMode.Adding || CurrentUpdateMode == UpdateMode.Editing;

        public OleDbConnection Connection { get; private set; }
        private Dictionary<string, List<ClientRecord>> _nameToClients;
        private Dictionary<string, List<ClientRecord>> _mediToClients;
        private Dictionary<string, List<ClientRecord>> _phoneToClients;
        private Dictionary<string, ClientRecord> _idToClient;
        AutoResetSuppressor _suppressSearch = new AutoResetSuppressor();
        public Action<string> ErrorReporter { private get; set; }

        public enum UpdateMode
        {
            Reading,
            Editing,
            Adding
        }

        public UpdateMode CurrentUpdateMode
        {
            get
            {
                if (IsAdding.IsVisible && IsAdding.IsChecked == true)
                {
                    return UpdateMode.Adding;
                }
                else if (IsEditing.IsVisible && IsEditing.IsChecked == true)
                {
                    return UpdateMode.Editing;
                }
                else
                {
                    return UpdateMode.Reading;
                }
            }
        }

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
            InputMode = Mode.Simple;
            UpdateUIOnAddingStatusChanged();
            IsAdding.Checked += IsAddingEditingCheckedUnchecked;
            IsAdding.Unchecked += IsAddingEditingCheckedUnchecked;
            IsEditing.Checked += IsAddingEditingCheckedUnchecked;
            IsEditing.Unchecked += IsAddingEditingCheckedUnchecked;
            ClientMedicare.DropDownOpened += ClientFieldDropDownOpened;
            ClientName.DropDownOpened += ClientFieldDropDownOpened;
            ClientNumber.DropDownOpened += ClientFieldDropDownOpened;
        }

        private void ClientFieldDropDownOpened(object sender, EventArgs e)
        {
            if (CurrentUpdateMode == UpdateMode.Adding
                || CurrentUpdateMode == UpdateMode.Editing)
            {
                ((ComboBox)sender).IsDropDownOpen = false;
            }
        }

        private void IsAddingEditingCheckedUnchecked(object sender, RoutedEventArgs e)
        {
            UpdateUIOnAddingStatusChanged();
        }

        private void UpdateUIOnAddingStatusChanged()
        {
            var isUpdating = IsUpdating;
            var visible = isUpdating ? Visibility.Collapsed : Visibility.Visible;
            SearchByNameBtn.Visibility = visible;
            SearchByMediBtn.Visibility = visible;
            SearchByPhoneBtn.Visibility = visible;
            ClientMedicare.IsTextSearchEnabled = !isUpdating;
            ClientName.IsTextSearchEnabled = !isUpdating;
            ClientNumber.IsTextSearchEnabled = !isUpdating;
            ClientAddress.IsReadOnly = !isUpdating;
            ClientGender.IsReadOnly = !isUpdating;
            ClientDob.IsEnabled = isUpdating;
            UpdateBtn.IsEnabled = isUpdating;
            ResetBtn.IsEnabled = isUpdating;
            ResetDob.IsEnabled = isUpdating;

            switch (CurrentUpdateMode)
            {
                case UpdateMode.Adding:
                    SearchByIdBtn.Visibility = Visibility.Collapsed;
                    ClientId.IsEnabled = false;
                    UpdateBtn.Content = "Add";
                    ClientId.Text = "(Adding ...)";
                    break;
                case UpdateMode.Editing:
                    SearchByIdBtn.Visibility = Visibility.Visible;
                    ClientId.IsEnabled = true;
                    UpdateBtn.Content = "Update";
                    ClientId.Text = ActiveClient?.Id ?? "";
                    break;
                default:
                    SearchByIdBtn.Visibility = Visibility.Visible;
                    ClientId.IsEnabled = true;
                    UpdateBtn.Content = "Update";
                    ClientId.Text = ActiveClient?.Id ?? "";
                    break;
            }
        }

        public void SetDataConnection(OleDbConnection connection)
        {
            Connection = connection;
            LoadData();
        }

        private void LoadData()
        {
            if (Connection != null)
            {
                var query = "select ID, [Client Name], [DOB], [Gender], [Medicare], [Phone], [Address] from Clients";
                _nameToClients = new Dictionary<string, List<ClientRecord>>();
                _mediToClients = new Dictionary<string, List<ClientRecord>>();
                _phoneToClients = new Dictionary<string, List<ClientRecord>>();
                _idToClient = new Dictionary<string, ClientRecord>();

                using (var r = Connection.RunReaderQuery(query))
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
                    ClientId.Text = CurrentUpdateMode == UpdateMode.Adding ?
                        "(Adding ...)" : ActiveClient.Id;
                    ClientMedicare.Text = ActiveClient.MedicareNumber;
                    ClientName.Text = ActiveClient.ClientFormalName();
                    ClientNumber.Text = ActiveClient.PhoneNumber;
                    if (InputMode == Mode.Input)
                    {
                        ClientGender.SelectedIndex = (int)ActiveClient.Gender;
                        if (ActiveClient.DOB.HasValue)
                        {
                            ClientDob.SelectedDate = ActiveClient.DOB.Value;
                        }
                        ClientAddress.Text = ActiveClient.Address;
                    }
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

        private void UpdateClientClick(object sender, RoutedEventArgs e)
        {
            if (CurrentUpdateMode == UpdateMode.Adding)
            {
                Add();
            }
            else if (CurrentUpdateMode == UpdateMode.Editing)
            {
                Edit();
            }
        }

        private void Add()
        {
            ActiveClient = new ClientRecord();
            UpdateUiToActive();
            var sbSql = new StringBuilder("insert into Clients([Client Name], [DOB], [Gender], [Medicare], [Phone], [Address]) values(");
            sbSql.Append($"'{ActiveClient.ClientFormalName()}',");
            sbSql.Append($"{ActiveClient.DOB.ToDbDate()},");
            sbSql.Append($"'{ClientRecord.ToString(ActiveClient.Gender)}',");
            sbSql.Append($"'{ActiveClient.MedicareNumber}',");
            sbSql.Append($"'{ActiveClient.PhoneNumber}',");
            sbSql.Append($"'{ActiveClient.Address}')");
            Connection.RunNonQuery(sbSql.ToString(), false);
        }

        private void Edit()
        {
            if (ActiveClient == null)
            {
                //TODO Error message...
                return;
            }

            UpdateUiToActive();
            var sbSql = new StringBuilder($"update Clients set ");
            sbSql.Append($"[Client Name] = '{ActiveClient.ClientFormalName()}',");
            sbSql.Append($"[DOB] = {ActiveClient.DOB.ToDbDate()},");
            sbSql.Append($"[Medicare] = {ActiveClient.MedicareNumber}, ");
            sbSql.Append($"[Gender] = '{ClientRecord.ToString(ActiveClient.Gender)}',");
            sbSql.Append($"[Phone] = '{ActiveClient.PhoneNumber}',");
            sbSql.Append($"[Address] = '{ActiveClient.Address}'");
            sbSql.Append($" where ID = {ActiveClient.Id.ClientIdFromStr()}");
            Connection.RunNonQuery(sbSql.ToString());

            //TODO Successful message
        }

        private void UpdateUiToActive()
        {
            ActiveClient.DOB = ClientDob.SelectedDate;
            ActiveClient.MedicareNumber = ClientMedicare.Text.Trim();
            ActiveClient.Gender = (Gender)ClientGender.SelectedIndex;
            ActiveClient.PhoneNumber = ClientNumber.Text;
            ActiveClient.Address = ClientAddress.Text;
            var name = ClientName.Text;
            (ActiveClient.FirstName, ActiveClient.Surname) = BookingIcs.SmartParseName(name);
        }

        private void ResetClick(object sender, RoutedEventArgs e)
        {
            if (CurrentUpdateMode == UpdateMode.Adding)
            {
                ClientName.Text = "";
                ClientMedicare.Text = "";
                ClientNumber.Text = "";
                ClientGender.SelectedIndex = (int)Gender.Unspecified;
                ClientDob.SelectedDate = null;
                ClientAddress.Text = "";
            }
            else if (CurrentUpdateMode == UpdateMode.Editing && ActiveClient != null)
            {
                ClientName.Text = ActiveClient.ClientFormalName();
                ClientMedicare.Text = ActiveClient.MedicareNumber;
                ClientNumber.Text = ActiveClient.PhoneNumber;
                ClientGender.SelectedIndex = (int)ActiveClient.Gender;
                ClientDob.SelectedDate = ActiveClient.DOB;
                ClientAddress.Text = ActiveClient.Address;
            }
        }

        private void ClientDobReset(object sender, RoutedEventArgs e)
        {
            ClientDob.SelectedDate = null;
        }
    }
}
