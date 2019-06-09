using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KMPControls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ClientsControl : UserControl
    {
        public OleDbConnection _connection;

        public ClientsControl()
        {
            InitializeComponent();
        }

        public void SetDataConnection(OleDbConnection connection)
        {
            _connection = connection;
        }

        private void LoadData()
        {

        }

        private void ClientMediKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ClientMediSelChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchByMediClick(object sender, RoutedEventArgs e)
        {

        }

        private void ClientNameKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ClientNameSelChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchByNameClick(object sender, RoutedEventArgs e)
        {

        }

        private void ClientNumberKeyDown(object sender, KeyEventArgs e)
        {

        }

        private void ClientNumberSelChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchByPhoneClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
