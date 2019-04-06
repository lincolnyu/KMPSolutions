using System.Collections.Generic;
using System.Windows;

namespace Booking
{
    /// <summary>
    /// Interaction logic for DuplicateClients.xaml
    /// </summary>
    public partial class DuplicateClients : Window
    {
        public DuplicateClients(string description, IEnumerable<string> items)
        {
            InitializeComponent();
            Description.Text = description;
            foreach (var item in items)
            {
                DupItemList.Items.Add(item);
            }
        }

        public int SelectedIndex => DupItemList.SelectedIndex;

        private void OKClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void DupItemListDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
