using KmpCrmUwp.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
            DataEntryGrid.DataContext = new CustomerViewModel(CrmData.FocusedCustomer);
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        private void AddVisitBatch_Click(object sender, RoutedEventArgs e)
        {
            var vm = (CustomerViewModel)DataEntryGrid.DataContext;
            vm.AddEmptyVisitBatch();
        }
    }
}
