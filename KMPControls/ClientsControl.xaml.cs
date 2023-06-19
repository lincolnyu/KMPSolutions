using KMPBookingCore;
using KMPBookingPlus;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static KMPBookingCore.UiUtils;
using KMPBookingCore.DbObjects;
using KMPControls.ViewModel;

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
                            GPReferralDate.Visibility = Visibility.Collapsed;
                            ClientAddressSection.Visibility = Visibility.Collapsed;
                            AddClientControl.Visibility = Visibility.Collapsed;
                            IsEditing.IsChecked = false;
                            break;
                        case Mode.Input:
                            ClientGenderSection.Visibility = Visibility.Visible;
                            ClientDobSection.Visibility = Visibility.Visible;
                            GPReferralDate.Visibility = Visibility.Visible;
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

        public Query.ClientData ClientData { get; private set; }

        private AutoResetSuppressor _suppressSearch = new AutoResetSuppressor();
        private AutoResetSuppressor _addEditSuprressor = new AutoResetSuppressor();
        public Action<string> ErrorReporter { private get; set; }
        public Dictionary<string, Client> IdToClient => ClientData.IdToEntry;

        public GPControl LinkedGPControl
        {
            get
            {
                return _linkedGPControl;
            }
            set
            {
                if (_linkedGPControl != value)
                {
                    var oldGpControl = _linkedGPControl;
                    _linkedGPControl = value;
                    SetupLinkageWithGPControl(oldGpControl);
                }
            }
        }

        private void SetupLinkageWithGPControl(GPControl oldGpControl)
        {
            if (oldGpControl != null)
            {
                oldGpControl.ActiveGPChanged -= GpControl_ActiveGPChanged;
            }
            if (_linkedGPControl != null)
            {
                _linkedGPControl.ActiveGPChanged += GpControl_ActiveGPChanged;
            }
        }

        private void GpControl_ActiveGPChanged()
        {
            if (CurrentUpdateMode != UpdateMode.Reading)
            {
                UpdateActiveClientReferringGP();
            }
        }

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
                    UpdateActiveClientReferringGP();
                    return UpdateMode.Adding;
                }
                else if (IsEditing.IsVisible && IsEditing.IsChecked == true)
                {
                    UpdateActiveClientReferringGP();
                    return UpdateMode.Editing;
                }
                else
                {
                    return UpdateMode.Reading;
                }
            }
        }

        private void UpdateActiveClientReferringGP()
        {
            if (ActiveClient != null)
            {
                ActiveClient.ReferringGP = _linkedGPControl.ActiveGP;
            }
        }

        private Client _activeClient;
        private GPControl _linkedGPControl;
        private GP _originalReferringGP;

        public Client ActiveClient
        {
            get => _activeClient;
            private set
            {
                if (_activeClient != value)
                {
                    var oldActiveClient = _activeClient;
                    _activeClient = value;
                    OnActiveClientChanged(oldActiveClient);
                    ActiveClientChanged?.Invoke();
                }
            }
        }

        internal ClientViewModel ActiveClientViewModel { get; private set; }

        private void OnActiveClientChanged(Client oldActiveClient)
        {
            _originalReferringGP = null;
            if (LinkedGPControl != null)
            {
                if (ActiveClient != null)
                {
                    LinkedGPControl.ActiveGP = ActiveClient.ReferringGP;
                }
                _originalReferringGP = LinkedGPControl.ActiveGP;
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
                    if (ActiveClient.ReferringDate.HasValue)
                    {
                        GPReferralDate.SelectedDate = ActiveClient.ReferringDate.Value;
                    }
                    ClientAddress.Text = ActiveClient.Address;
                }
            }
            else
            {
                ClientId.Text = "";
                ClientName.Text = "";
                ClientNumber.Text = "";
                ClientDob.SelectedDate = null;
                GPReferralDate.SelectedDate = null;
                ClientAddress.Text = "";
            }

            ActiveClientViewModel = new ClientViewModel(ActiveClient);
            this.DataContext = ActiveClientViewModel;
        }

        public bool TrySetActiveClient(Client client)
        {
            if (IdToClient[client.MedicareNumber] == client)
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
            this.Unloaded += ClientsControl_Unloaded;
        }

        private void ClientsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveData();
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
            ClientGender.IsEnabled = isUpdating;
            ClientDob.IsEnabled = isUpdating;
            GPReferralDate.IsEnabled = isUpdating;
            UpdateBtn.IsEnabled = isUpdating;
            ResetBtn.IsEnabled = isUpdating;
            ResetDob.IsEnabled = isUpdating;
            ResetGPReferralDate.IsEnabled = isUpdating;

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
                ClientData = Query.LoadClientData(Connection);

                foreach (var n in ClientData.PhoneNumbers)
                {
                    ClientNumber.Items.Add(n);
                }
                foreach (var id in ClientData.Ids)
                {
                    ClientId.Items.Add(id);
                }
                foreach (var n in ClientData.Names)
                {
                    ClientName.Items.Add(n);
                }

                // TODO Load data for specific event types.
                Query.LoadClientEventData(Connection, ClientData, null, null, null);
            }
        }

        public void SaveData()
        {
            if (Connection != null)
            {
                Update.SaveClientData(Connection, ClientData);
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
            });
        }

        private void SearchByMedicareNumber(string medi)
        {
            SearchBy(IdToClient.Values.FindByMedicareNumberContaining(medi)
                    .OrderBy(x => x.MedicareNumber).ToList(),
                    $"Multiple clients found with medicare number containing '{medi}'");
        }

        private void SearchByName(string name)
        {
            SearchBy(IdToClient.Values.FindNameContaining(name)
                    .OrderBy(x => x.MedicareNumber).ToList(),
                    $"Multiple clients found with name containing '{name}'");
        }

        private void SearchByPhone(string phone)
        {   
            SearchBy(IdToClient.Values.FindByPhoneNumberContaining(phone)
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
            if (!ClientData.TryAdd(ActiveClient))
            {
                // todo...
                return;
            }
            //TODO Successful message
        }

        private void Edit()
        {
            if (ActiveClient == null)
            {
                //TODO Error message...
                return;
            }

            UpdateUiToActive();
            
            //TODO Successful message
        }

        private void UpdateUiToActive()
        {
            ActiveClient.DOB = ClientDob.SelectedDate;
            ActiveClient.ReferringDate = GPReferralDate.SelectedDate;
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
                GPReferralDate.SelectedDate = null;
                ClientAddress.Text = "";
                _linkedGPControl.ActiveGP = null;
            }
            else if (CurrentUpdateMode == UpdateMode.Editing && ActiveClient != null)
            {
                ClientName.Text = ActiveClient.ClientFormalName();
                ClientNumber.Text = ActiveClient.Phone;
                ConvertGenderToUi(ActiveClient.Gender);
                ClientDob.SelectedDate = ActiveClient.DOB;
                GPReferralDate.SelectedDate = ActiveClient.ReferringDate;
                ClientAddress.Text = ActiveClient.Address;
                _linkedGPControl.ActiveGP = _originalReferringGP;
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

        private void ResetGPReferralDate_Click(object sender, RoutedEventArgs e)
        {
            GPReferralDate.SelectedDate = null;
        }

        private void EventItemDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var obj = button.DataContext;
            ActiveClientViewModel.Events.Remove((EventViewModel)obj);
        }
    }
}
