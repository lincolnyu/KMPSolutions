using System.Windows;
using System.Collections.Generic;

namespace KMPControls
{
    /// <summary>
    /// Interaction logic for DuplicateClients.xaml
    /// </summary>
    public partial class DuplicateResolverDialog : Window
    {
        public DuplicateResolverDialog(string title, string description, IEnumerable<string> items)
        {
            InitializeComponent();
            this.Title = Title;
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
