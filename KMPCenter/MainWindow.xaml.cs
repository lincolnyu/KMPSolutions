using System;
using System.Windows;
using System.Data.OleDb;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private OleDbConnection _conn;
        private BookingWindow _booking;
        private InvoicingWindow _invoicing;

        public MainWindow()
        {
            InitializeComponent();
            SetTitle();
            LoadSettings();
        }

        private void SetTitle()
        {
            //TODO implement it
        }

        private void LoadSettings()
        {
            ExcelPath.Text = Properties.Settings.Default.ExcelPath;
            DbPath.Text = Properties.Settings.Default.DbPath;
            if (!string.IsNullOrWhiteSpace(ExcelPath.Text))
            {
                LoadExcel(false);
            }
            if (!string.IsNullOrWhiteSpace(DbPath.Text))
            {
                ConnectToDb(false);
            }
        }

        private void ExcelPathDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void ExcelPathDrop(object sender, DragEventArgs e)
        {
            var data = (DataObject)e.Data;
            if (data.ContainsFileDropList())
            {
                string[] rawFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (rawFiles.Length > 0)
                {
                    ExcelPath.Text = rawFiles[0];
                    LoadExcel(true);
                }
            }
        }

        private void ExcelPathPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void ExcelBrowseClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Microsoft Excel files (*.xslx)|*.xslx|All files (*.*)|*.*"
            };
            if (File.Exists(ExcelPath.Text))
            {
                ofd.InitialDirectory = Path.GetDirectoryName(ExcelPath.Text);
            }
            if (ofd.ShowDialog() == true)
            {
                ExcelPath.Text = ofd.FileName;
                LoadExcel(true);
            }
        }

        private void ExcelShowInExplorerClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(ExcelPath.Text))
            {
                var dn = Path.GetDirectoryName(ExcelPath.Text);
                if (Directory.Exists(dn))
                {
                    Process.Start("explorer.exe", dn);
                    return;
                }
            }
            MessageBox.Show("Error: Unable to locate the Excel file.", Title);

        }

        private void DbPathDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void DbPathDrop(object sender, DragEventArgs e)
        {

        }

        private void DbPathPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void DbBrowseClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Microsoft Access database files (*.accdb)|*.accdb|All files (*.*)|*.*"
            };
            if (File.Exists(DbPath.Text))
            {
                ofd.InitialDirectory = Path.GetDirectoryName(DbPath.Text);
            }
            if (ofd.ShowDialog() == true)
            {
                DbPath.Text = ofd.FileName;
                ConnectToDb(true);
            }
        }

        private void DbShowInExplorerClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(DbPath.Text))
            {
                var dn = Path.GetDirectoryName(DbPath.Text);
                if (Directory.Exists(dn))
                {
                    Process.Start("explorer.exe", dn);
                    return;
                }
            }
            MessageBox.Show("Error: Unable to locate the database file.", Title);
        }

        private void ExcelSyncToDbClick(object sender, RoutedEventArgs e)
        {

        }

        private void DbSyncToExcelClick(object sender, RoutedEventArgs e)
        {

        }

        private void LoadExcel(bool saveSettings)
        {
            if (LoadClientsFromExcel(ExcelPath.Text))
            {
                if (saveSettings)
                {
                    Properties.Settings.Default.ExcelPath = ExcelPath.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void ConnectToDb(bool saveSettings)
        {
            if (ConnectToDb(DbPath.Text))
            {
                if (saveSettings)
                {
                    Properties.Settings.Default.DbPath = DbPath.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }
        
        bool LoadClientsFromExcel(string excelPath)
        {
            //throw new NotImplementedException();
            return true;
        }

        private bool ConnectToDb(string path)
        {
            var connstr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Persist Security Info=True";
            _conn = new OleDbConnection
            {
                ConnectionString = connstr
            };
            try
            {
                _conn.Open();
                MessageBox.Show("Successfully connected to the database.", Title);
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show($"Failed to connect to the database. Details:\n{e.Message}", Title);
                return false;
            }
        }

        private void BookingClick(object sender, RoutedEventArgs e)
        {
            if (_booking == null)
            {
                _booking = new BookingWindow();
                _booking.Closed += (s1, e1) =>
                {
                    _booking = null;
                };
            }
            _booking.Show();
            _booking.Activate();
        }

        private void InvoicingClick(object sender, RoutedEventArgs e)
        {
            if (_invoicing == null)
            {
                _invoicing = new InvoicingWindow();
                _invoicing.Closed += (s1, e1) =>
                {
                    _invoicing = null;
                };
            }
            _invoicing.Show();
            _invoicing.Activate();
        }
    }
}
