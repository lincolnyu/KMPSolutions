using KmpCrmCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
            LoadList();
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            // todo implement it..
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
