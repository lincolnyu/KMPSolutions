using KMPBookingCore.DbObjects;
using KMPBookingPlus;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static KMPBookingPlus.Query;
using static KMPBookingCore.GPUtils;
using System.Linq;
using static KMPBookingCore.UiUtils;
using static KMPControls.ClientsControl;
using System.Xml.Linq;

namespace KMPControls
{
    /// <summary>
    /// Interaction logic for GPControl.xaml
    /// </summary>
    public partial class GPControl : UserControl
    {
        private Dictionary<string, GP> _idToGP;
        private Dictionary<string, List<GP>> _nameToGP;
        private Dictionary<string, List<GP>> _phoneToGP;

        public OleDbConnection Connection { get; private set; }

        public delegate void ActiveGPChangedEventHandler();
        public event ActiveGPChangedEventHandler ActiveGPChanged;

        private AutoResetSuppressor _suppressSearch = new AutoResetSuppressor();
        private GP _activeGP;

        public GP ActiveGP
        {
            get => _activeGP;
            private set
            {
                if (_activeGP != value)
                {
                    _activeGP = value;
                    ActiveGPChanged?.Invoke();
                }
            }
        }

        public GPControl()
        {
            InitializeComponent();
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
            SearchBy(_idToGP.Values.FindByProviderNumberContaining(providerNumber)
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
                if (ActiveGP != null)
                {
                    GPId.Text = ActiveGP.ProviderNumber;
                    GPName.Text = ActiveGP.Name;
                    GPPhoneNumber.Text = ActiveGP.Phone;
                    GPAddress.Text = ActiveGP.Address;
                }
                else
                {
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
            SearchBy(_idToGP.Values.FindNameContaining(name)
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
            SearchBy(_idToGP.Values.FindPhoneContaining(phone).OrderBy(x=>x.ProviderNumber).ToList(), $"Multiple GP found with phone number containing '{phone}'");
        }

        private void ResetBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {

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
                var gpData = Query.LoadGPData(Connection);

                _nameToGP = gpData.NameToEntry;
                _idToGP = gpData.IdToEntry;
                _phoneToGP = gpData.PhoneToEntry;

                foreach (var n in gpData.PhoneNumbers)
                {
                    GPPhoneNumber.Items.Add(n);
                }
                foreach (var id in gpData.Ids)
                {
                    GPId.Items.Add(id);
                }
                foreach (var n in gpData.Names)
                {
                    GPName.Items.Add(n);
                }
            }
        }
    }
}
