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
using static KMPBookingCore.UiUtils;

namespace KMPControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ClientsControl : UserControl
    {
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
        private Dictionary<string, List<Client>> _nameToClients;
        private Dictionary<string, List<Client>> _mediToClients;
        private Dictionary<string, List<Client>> _phoneToClients;
        private Dictionary<string, Client> _idToClient;
        private AutoResetSuppressor _suppressSearch = new AutoResetSuppressor();
        private AutoResetSuppressor _addEditSuprressor = new AutoResetSuppressor();
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

        private Client _activeClient;
        public Client ActiveClient
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

        public bool TrySetActiveClient(Client client)
        {
            if (_idToClient[client.MedicareNumber] == client)
            {
                SearchBy(new[] { client }, "");
                return true;
            }
            return false;
        }

        public delegate void ActiveClientChangedEventHandler();
        public event ActiveClientChangedEventHandler ActiveClientChanged;

        public ClientsControl()
        {
            InitializeComponent();
            InputMode = Mode.Simple;
            UpdateUIOnAddingStatusChanged();
            IsAdding.Checked += IsAddingCheckedUnchecked;
            IsAdding.Unchecked += IsAddingCheckedUnchecked;
            IsEditing.Checked += IsEditingCheckedUnchecked;
            IsEditing.Unchecked += IsEditingCheckedUnchecked;
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

        private void IsAddingCheckedUnchecked(object sender, RoutedEventArgs e)
        {
            _addEditSuprressor.Run(() =>
            {
                if (IsAdding.IsChecked == true)
                {
                    IsEditing.IsChecked = false;
                }
                UpdateUIOnAddingStatusChanged();
            });
        }

        private void IsEditingCheckedUnchecked(object sender, RoutedEventArgs e)
        {
            _addEditSuprressor.Run(() =>
            {
                if (IsEditing.IsChecked == true)
                {
                    IsAdding.IsChecked = false;
                }
                UpdateUIOnAddingStatusChanged();
            });
        }

        private void UpdateUIOnAddingStatusChanged()
        {
            var isUpdating = IsUpdating;
            var visible = isUpdating ? Visibility.Collapsed : Visibility.Visible;
            SearchByNameBtn.Visibility = visible;
            SearchByPhoneBtn.Visibility = visible;
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
                    ClientId.Text = ActiveClient?.MedicareNumber ?? "";
                    break;
                default:
                    SearchByIdBtn.Visibility = Visibility.Visible;
                    ClientId.IsEnabled = true;
                    UpdateBtn.Content = "Update";
                    ClientId.Text = ActiveClient?.MedicareNumber ?? "";
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
                var query = "select [Medicare Number], [First Name], [Surname], [DOB], [Gender], [Phone], [Address] from Client";
                _nameToClients = new Dictionary<string, List<Client>>();
                _mediToClients = new Dictionary<string, List<Client>>();
                _phoneToClients = new Dictionary<string, List<Client>>();
                _idToClient = new Dictionary<string, Client>();

                var clientNames = new List<string>();
                var clientIds = new List<string>();
                var clientNumbers = new List<string>();

                using (var r = Connection.RunReaderQuery(query))
                {
                    while (r.Read())
                    {
                        var medicareNumber = r.GetString(0);
                        var firstName = r.TryGetString(1).Trim();
                        var surname = r.TryGetString(2).Trim();
                        var name = $"{surname}, {firstName}";
                        var dob = r.TryGetDateTime(3);
                        var gender = Client.ParseGender(r.TryGetString(4));
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
                        if (!_idToClient.ContainsKey(medicareNumber))
                        {
                            _idToClient[medicareNumber] = cr;
                            clientIds.Add(medicareNumber);
                        }
                        else
                        {
                            //TODO it's an error
                        }
                        if (!_nameToClients.TryGetValue(name, out var namelist))
                        {
                            _nameToClients.Add(name, new List<Client> { cr });
                            clientNames.Add(name);
                        }
                        else
                        {
                            namelist.Add(cr);
                        }
                        if (!string.IsNullOrWhiteSpace(cr.PhoneNumber))
                        {
                            if (!_phoneToClients.TryGetValue(cr.PhoneNumber, out var phonelist))
                            {
                                _phoneToClients.Add(cr.PhoneNumber, new List<Client> { cr });
                                clientNumbers.Add(cr.PhoneNumber);
                            }
                            else
                            {
                                phonelist.Add(cr);
                            }
                        }
                    }

                    clientIds.Sort();
                    clientNames.Sort();
                    clientNumbers.Sort();
                    foreach (var n in clientNumbers)
                    {
                        ClientNumber.Items.Add(n);
                    }
                    foreach (var id in clientIds)
                    {
                        ClientId.Items.Add(id);
                    }
                    foreach (var n in clientNames)
                    {
                        ClientName.Items.Add(n);
                    }
                }
            }
        }

        private void ClientMediSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByMedicareNumber(e.AddedItems[0].ToString());
            }
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
                SearchByMedicareNumber(ClientId.Text);
            }
        }

        private void ClientIdSelChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByMedicareNumber(e.AddedItems[0].ToString());
            }
        }

        private void SearchByIdClick(object sender, RoutedEventArgs e)
        {
            SearchByMedicareNumber(ClientId.Text);
        }

        private IEnumerable<string> RecordsToStrings(IList<Client> clients)
        {
            foreach (var c in clients)
            {
                yield return $"#{c.MedicareNumber}: {c.ClientFormalName()} (Medicare#{c.MedicareNumber}, Phone#{c.PhoneNumber})";
            }
        }

        private void SearchBy(IList<Client> foundClients, string duplicateMessage)
        {
            _suppressSearch.Run(() =>
            {
                ActiveClient = null;
                if (foundClients.Count > 1)
                {
                    var dc = new DuplicateClients(duplicateMessage,
                        RecordsToStrings(foundClients))
                    {
                        //TODO set owner to the owner of this control
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    if (dc.ShowDialog() == true && dc.SelectedIndex >= 0)
                    {
                        ActiveClient = foundClients[dc.SelectedIndex];
                    }
                }
                else if (foundClients.Count == 1)
                {
                    ActiveClient = foundClients[0];
                }
                else
                {
                    ErrorReporter?.Invoke("Error: Client not found.");
                }
                if (ActiveClient != null)
                {
                    ClientId.Text = CurrentUpdateMode == UpdateMode.Adding ? "(Adding ...)" : ActiveClient.MedicareNumber;
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

        private void SearchByMedicareNumber(string medi)
        {
            SearchBy(_idToClient.Values.FindByMedicareNumberContaining(medi)
                    .OrderBy(x => x.MedicareNumber).ToList(),
                    $"Multiple clients found with medicare number containing '{medi}'");
        }

        private void SearchByName(string name)
        {
            SearchBy(_idToClient.Values.FindNameContaining(name)
                    .OrderBy(x => x.MedicareNumber).ToList(),
                    $"Multiple clients found with name containing '{name}'");
        }

        private void SearchByPhone(string phone)
        {   
            SearchBy(_idToClient.Values.FindByPhoneNumberContaining(phone)
                    .OrderBy(x => x.MedicareNumber).ToList(),
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
            ActiveClient = new Client();
            UpdateUiToActive();
            var sbSql = new StringBuilder("insert into Clients([Client Name], [DOB], [Gender], [Medicare], [Phone], [Address]) values(");
            sbSql.Append($"'{ActiveClient.ClientFormalName()}',");
            sbSql.Append($"{ActiveClient.DOB.ToDbDate()},");
            sbSql.Append($"'{Client.ToString(ActiveClient.Gender)}',");
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
            sbSql.Append($"[Gender] = '{Client.ToString(ActiveClient.Gender)}',");
            sbSql.Append($"[Phone] = '{ActiveClient.PhoneNumber}',");
            sbSql.Append($"[Address] = '{ActiveClient.Address}'");
            sbSql.Append($" where ID = {ActiveClient.MedicareNumber}");
            Connection.RunNonQuery(sbSql.ToString());

            //TODO Successful message
        }

        private void UpdateUiToActive()
        {
            ActiveClient.DOB = ClientDob.SelectedDate;
            ActiveClient.MedicareNumber = ClientId.Text.Trim();
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
                ClientNumber.Text = "";
                ClientGender.SelectedIndex = (int)Gender.Unspecified;
                ClientDob.SelectedDate = null;
                ClientAddress.Text = "";
            }
            else if (CurrentUpdateMode == UpdateMode.Editing && ActiveClient != null)
            {
                ClientName.Text = ActiveClient.ClientFormalName();
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
