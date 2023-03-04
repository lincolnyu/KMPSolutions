using KmpCrmCore;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KmpCrmUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListCustomersPage : Page
    {
        public ListCustomersPage()
        {
            this.InitializeComponent();
            DataContext = this;

            LoadList();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            CustomerList.SelectedItem = CrmData.FocusedCustomer;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            CrmData.FocusedCustomer = (Customer)CustomerList.SelectedItem;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (CustomerList.SelectedItem != null)
            {
                this.Frame.Navigate(typeof(EditCustomerPage));
            }
        }

        private void LoadList()
        {
            foreach (var customer in CrmData.Instance.CrmRepo.Customers)
            {
                CustomerList.Items.Add(customer.Value);
            }
        }

        private void CustomerList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CrmData.FocusedCustomer = (Customer)CustomerList.SelectedItem;
        }
    }
}
