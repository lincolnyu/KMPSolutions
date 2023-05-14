using KMPBookingCore.Database;
using KMPBookingPlus;
using System.Windows;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for ClientsWindow.xaml
    /// </summary>
    public partial class ClientsWindow : Window
    {
        private MainWindow MainWindow => (MainWindow) Owner;

        public ClientsWindow(MainWindow mw)
        {
            Owner = mw;
            InitializeComponent();
            Clients.InputMode = KMPControls.ClientsControl.Mode.Input;
            Clients.ActiveClientChanged += Clients_ActiveClientChanged;
            Clients.SetDataConnection(MainWindow.Connection);
            GPs.SetDataConnection(MainWindow.Connection);
            Query.CorrelateClientAndGP(MainWindow.Connection, Clients.ClientData, GPs.GPData);
        }

        private void Clients_ActiveClientChanged()
        {
            var activeClient = Clients.ActiveClient;
            if (activeClient != null)
            {
                GPs.ActiveGP = activeClient.ReferringGP;
            }
        }
    }
}
