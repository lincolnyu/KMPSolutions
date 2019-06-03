using System.Windows;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static void ShowMessage(string message)
        {
            MessageBox.Show(message, App.Current.MainWindow.Title);
        }
    }
}
