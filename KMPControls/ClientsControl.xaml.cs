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
using KMPBookingCore.DbObjects;

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
        private Dictionary<string, List<Client>> _nameToClient;
        private Dictionary<string, List<Client>> _phoneToClient;
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
            InitializeGenderUi();
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
            ClientId.IsTextSearchEnabled = !isUpdating;
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
                    ClientId.IsEnabled = true;
                    UpdateBtn.Content = "Add";
                    ClientId.Text = "";
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
                var clientData = Query.LoadClientData(Connection);
                _nameToClient = clientData.NameToEntry;
                _idToClient = clientData.IdToEntry;
                _phoneToClient = clientData.PhoneToEntry;

                foreach (var n in clientData.PhoneNumbers)
                {
                    ClientNumber.Items.Add(n);
                }
                foreach (var id in clientData.Ids)
                {
                    ClientId.Items.Add(id);
                }
                foreach (var n in clientData.Names)
                {
                    ClientName.Items.Add(n);
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
                yield return $"#{c.MedicareNumber}: {c.ClientFormalName()} (Medicare#{c.MedicareNumber}, Phone#{c.Phone})";
            }
        }

        private void SearchBy(IList<Client> foundClients, string duplicateMessage)
        {
            _suppressSearch.Run(() =>
            {
                ActiveClient = null;
                if (foundClients.Count > 1)
                {
                    var dc = new DuplicateResolverDialog("Duplicate Clients", duplicateMessage,
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
                    ClientId.Text = ActiveClient.MedicareNumber;
                    ClientName.Text = ActiveClient.ClientFormalName();
                    ClientNumber.Text = ActiveClient.Phone;
                    if (InputMode == Mode.Input)
                    {
                        ConvertGenderToUi(ActiveClient.Gender);
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
            sbSql.Append($"'{ActiveClient.Gender}',");
            sbSql.Append($"'{ActiveClient.MedicareNumber}',");
            sbSql.Append($"'{ActiveClient.Phone}',");
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
            sbSql.Append($"[Gender] = '{ActiveClient.Gender}',");
            sbSql.Append($"[Phone] = '{ActiveClient.Phone}',");
            sbSql.Append($"[Address] = '{ActiveClient.Address}'");
            sbSql.Append($" where ID = {ActiveClient.MedicareNumber}");
            Connection.RunNonQuery(sbSql.ToString());

            //TODO Successful message
        }

        private void UpdateUiToActive()
        {
            ActiveClient.DOB = ClientDob.SelectedDate;
            ActiveClient.MedicareNumber = ClientId.Text.Trim();
            ActiveClient.Gender = ConvertUiToGender();
            ActiveClient.Phone = ClientNumber.Text;
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
                ClientGender.SelectedItem = "";
                ClientDob.SelectedDate = null;
                ClientAddress.Text = "";
            }
            else if (CurrentUpdateMode == UpdateMode.Editing && ActiveClient != null)
            {
                ClientName.Text = ActiveClient.ClientFormalName();
                ClientNumber.Text = ActiveClient.Phone;
                ConvertGenderToUi(ActiveClient.Gender);
                ClientDob.SelectedDate = ActiveClient.DOB;
                ClientAddress.Text = ActiveClient.Address;
            }
        }

        private void InitializeGenderUi()
        {
            ClientGender.Items.Add("Unspecified");
            ClientGender.Items.Add("Female");
            ClientGender.Items.Add("Male");
        }

        string ConvertUiToGender()
        {
            var s = (string)ClientGender.SelectedItem;
            if (s == "Male")
            {
                s = "M";
            }
            else if (s == "Female")
            {
                s = "F";
            }
            else if (s == "Unspecified")
            {
                s = "U";
            }
            return s;
        }

        void ConvertGenderToUi(string gender)
        {
            if (gender == "M")
            {
                gender = "Male";
            }
            else if (gender == "F")
            {
                gender = "Female";
            }
            else if (gender == "U")
            {
                gender = "Unspecified";
            }
            var i = ClientGender.Items.IndexOf(gender);
            if (i >= 0)
            {
                ClientGender.SelectedIndex = i;
            }
            else
            {
                ClientGender.Items.Add(gender);
                ClientGender.SelectedIndex = ClientGender.Items.Count - 1;
            }
        }

        private void ClientDobReset(object sender, RoutedEventArgs e)
        {
            ClientDob.SelectedDate = null;
        }
    }
}
