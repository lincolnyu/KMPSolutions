using KMPBookingCore.DbObjects;
using KMPBookingPlus;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static KMPBookingCore.GPUtils;
using System.Linq;
using static KMPBookingCore.UiUtils;
using static KMPBookingPlus.Query;
using KMPBookingCore;

namespace KMPControls
{
    /// <summary>
    /// Interaction logic for GPControl.xaml
    /// </summary>
    public partial class GPControl : UserControl
    {
        public OleDbConnection Connection { get; private set; }

        public delegate void ActiveGPChangedEventHandler();
        public event ActiveGPChangedEventHandler ActiveGPChanged;

        private AutoResetSuppressor _suppressSearch = new AutoResetSuppressor();
        private AutoResetSuppressor _addEditSuprressor = new AutoResetSuppressor();
        private GP _activeGP;

        public GP ActiveGP
        {
            get => _activeGP;
            set
            {
                if (_activeGP != value)
                {
                    _activeGP = value;
                    UpdateUiFromActive();
                    ActiveGPChanged?.Invoke();
                }
            }
        }

        public GPData GPData { get; private set; }

        public Dictionary<string, GP> IdToGp => GPData.IdToEntry;

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

        public bool IsUpdating => CurrentUpdateMode == UpdateMode.Adding || CurrentUpdateMode == UpdateMode.Editing;

        public GPControl()
        {
            InitializeComponent();

            UpdateUIOnAddingStatusChanged();
            IsAdding.Checked += IsAddingCheckedUnchecked;
            IsAdding.Unchecked += IsAddingCheckedUnchecked;
            IsEditing.Checked += IsEditingCheckedUnchecked;
            IsEditing.Unchecked += IsEditingCheckedUnchecked;

            this.Unloaded += GPControl_Unloaded;
        }

        private void GPControl_Unloaded(object sender, RoutedEventArgs e)
        {
            SaveData();
        }

        private void UpdateUIOnAddingStatusChanged()
        {
            var isUpdating = IsUpdating;
            var visible = isUpdating ? Visibility.Collapsed : Visibility.Visible;
            SearchByNameBtn.Visibility = visible;
            SearchByPhoneBtn.Visibility = visible;
            GPId.IsTextSearchEnabled = !isUpdating;
            GPName.IsTextSearchEnabled = !isUpdating;
            GPPhoneNumber.IsTextSearchEnabled = !isUpdating;
            GPAddress.IsReadOnly = !isUpdating;
            UpdateBtn.IsEnabled = isUpdating;
            ResetBtn.IsEnabled = isUpdating;

            switch (CurrentUpdateMode)
            {
                case UpdateMode.Adding:
                    SearchByIdBtn.Visibility = Visibility.Collapsed;
                    GPId.IsEnabled = true;
                    UpdateBtn.Content = "Add";
                    GPId.Text = "";
                    break;
                case UpdateMode.Editing:
                    SearchByIdBtn.Visibility = Visibility.Visible;
                    GPId.IsEnabled = true;
                    UpdateBtn.Content = "Update";
                    GPId.Text = ActiveGP?.ProviderNumber ?? "";
                    break;
                default:
                    SearchByIdBtn.Visibility = Visibility.Visible;
                    GPId.IsEnabled = true;
                    UpdateBtn.Content = "Update";
                    GPId.Text = ActiveGP?.ProviderNumber ?? "";
                    break;
            }
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

        private void GPId_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByProviderNumber(GPPhoneNumber.Text);
            }
        }

        private void GPId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByProviderNumber(e.AddedItems[0].ToString());
            }
        }

        private void SearchByProviderNumber(string providerNumber)
        {
            SearchBy(IdToGp.Values.FindByProviderNumberContaining(providerNumber)
                    .OrderBy(x => x.ProviderNumber).ToList(),
                    $"Multiple GPs found with provider number containing '{providerNumber}'");
        }

        private IEnumerable<string> RecordsToStrings(IList<GP> gpList)
        {
            foreach (var gp in gpList)
            {
                yield return $"#{gp.ProviderNumber}: {gp.Name} (Medicare#{gp.ProviderNumber}, Phone#{gp.Phone})";
            }
        }

        private void SearchBy(IList<GP> foundGPs, string duplicateMessage)
        {
            _suppressSearch.Run(() =>
            {
                ActiveGP = null;
                if (foundGPs.Count > 1)
                {
                    var dc = new DuplicateResolverDialog("Duplicate GPs", duplicateMessage,
                                          RecordsToStrings(foundGPs))
                    {
                        //TODO set owner to the owner of this control
                        WindowStartupLocation = WindowStartupLocation.CenterOwner
                    };
                    if (dc.ShowDialog() == true && dc.SelectedIndex >= 0)
                    {
                        ActiveGP = foundGPs[dc.SelectedIndex];
                    }
                }
                else if (foundGPs.Count == 1)
                {
                    ActiveGP = foundGPs[0];
                }
                else
                {
                    //ErrorReporter?.Invoke("Error: Client not found.");
                }
            });
        }

        private void SearchByIdBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchByProviderNumber(GPId.Text);
        }

        private void GPName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByName(GPName.Text);
            }
        }

        private void GPName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByName(e.AddedItems[0].ToString());
            }
        }

        private void SearchByName(string name)
        {
            SearchBy(IdToGp.Values.FindNameContaining(name)
                    .OrderBy(x => x.ProviderNumber).ToList(),
                    $"Multiple GP found with name containing '{name}'");
        }

        private void SearchByNameBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchByName(GPName.Text);
        }

        private void GPPhoneNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchByPhone(GPPhoneNumber.Text);
            }
        }

        private void GPPhoneNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SearchByPhone(e.AddedItems[0].ToString());
            }
        }

        private void SearchByPhoneBtn_Click(object sender, RoutedEventArgs e)
        {
            SearchByPhone(GPPhoneNumber.Text);
        }

        private void SearchByPhone(string phone)
        {
            SearchBy(IdToGp.Values.FindPhoneContaining(phone).OrderBy(x=>x.ProviderNumber).ToList(), $"Multiple GP found with phone number containing '{phone}'");
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUpdateMode == UpdateMode.Adding)
            {
                GPId.Text = "";
                GPName.Text = "";
                GPPhoneNumber.Text = "";
                GPAddress.Text = "";
            }
            else if (CurrentUpdateMode == UpdateMode.Editing && ActiveGP != null)
            {
                GPId.Text = ActiveGP.ProviderNumber;
                GPName.Text = ActiveGP.Name;
                GPPhoneNumber.Text = ActiveGP.Phone;
                GPAddress.Text = ActiveGP.Address;
            }
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
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

        private void Edit()
        {
            if (ActiveGP == null)
            {
                //TODO Error message...
                return;
            }

            UpdateUiToActive();

            //TODO Successful message
        }

        private void Add()
        {
            ActiveGP = new GP();
            UpdateUiToActive();
            if (!GPData.TryAdd(ActiveGP))
            {
                // todo...
                return;
            }
            //TODO Successful message
        }

        private void UpdateUiToActive()
        {
            ActiveGP.ProviderNumber = GPId.Text.Trim();
            ActiveGP.Name = GPName.Text;
            ActiveGP.Phone = GPPhoneNumber.Text;
            ActiveGP.Address = GPAddress.Text;
        }

        private void UpdateUiFromActive()
        {
            if (ActiveGP != null)
            {
                GPId.Text = ActiveGP.ProviderNumber;
                GPName.Text = ActiveGP.Name;
                GPPhoneNumber.Text = ActiveGP.Phone;
                GPAddress.Text = ActiveGP.Address;
            }
            else
            {
                GPId.Text = "";
                GPName.Text = "";
                GPPhoneNumber.Text = "";
                GPAddress.Text = "";
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
                GPData = Query.LoadGPData(Connection);

                foreach (var n in GPData.PhoneNumbers)
                {
                    GPPhoneNumber.Items.Add(n);
                }
                foreach (var id in GPData.Ids)
                {
                    GPId.Items.Add(id);
                }
                foreach (var n in GPData.Names)
                {
                    GPName.Items.Add(n);
                }
            }
        }

        private void SaveData()
        {
            if (Connection != null)
            {
                Update.SaveGPData(Connection, GPData);
            }
        }
    }
}
