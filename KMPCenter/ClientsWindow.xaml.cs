﻿using System.Windows;

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
            Clients.SetDataConnection(MainWindow.Connection);

            GPs.SetDataConnection(MainWindow.Connection);
        }
    }
}
