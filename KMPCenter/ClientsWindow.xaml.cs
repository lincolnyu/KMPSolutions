using System.Windows;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for ClientsWindow.xaml
    /// </summary>
    public partial class ClientsWindow : Window
    {
        private MainWindow _mainwindow;

        public ClientsWindow(MainWindow mw)
        {
            _mainwindow = mw;
            InitializeComponent();
        }
    }
}
