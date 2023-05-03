using KMPBookingCore.DbObjects;
using KMPBookingPlus;
using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KMPControls
{
    /// <summary>
    /// Interaction logic for GPControl.xaml
    /// </summary>
    public partial class GPControl : UserControl
    {
        private Dictionary<string, GP> _idToGP;

        public OleDbConnection Connection { get; private set; }

        public GPControl()
        {
            InitializeComponent();
        }

        private void GPId_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void GPId_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchByIdBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GPName_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void GPName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchByNameBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GPPhoneNumber_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void GPPhoneNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void SearchByPhoneBtn_Click(object sender, RoutedEventArgs e)
        {

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
                _idToGP = gpData.IdToEntry;

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
