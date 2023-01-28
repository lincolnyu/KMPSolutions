using KmpCrmUwp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KmpCrmUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditCustomerPage : Page
    {
        public EditCustomerPage()
        {
            this.InitializeComponent();
            this.PopulateDataIfFocused();
        }

        private void PopulateDataIfFocused()
        {
            if (!CrmData.HasFocusedCustomer)
            {
                DataEntryGrid.DataContext = null;
                return;
            }
            DataEntryGrid.DataContext = new CustomerViewModel(CrmData.FocusedCustomer);
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
