using KMPBookingCore;
using KMPBookingPlus;
using System;
using System.Windows;
using System.Data.OleDb;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using OfficeOpenXml;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;

namespace KMPCenter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public OleDbConnection Connection { get; private set; }
        private string _connectionString;
        private BookingWindow _booking;
        private InvoicingWindow _invoicing;
        private ClientsWindow _clientsWindow;
        private string _loadedExcel;
        private readonly List<Client> _excelRecords = new List<Client>();

        public MainWindow()
        {
            InitializeComponent();

            this.Closed += MainWindowClosed;
            this.Closing += MainWindowClosing; ;

            SetTitle();
            LoadSettings();
        }

        private void MainWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //TODO ask
        }

        private void MainWindowClosed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SetTitle()
        {
            //TODO implement it
        }

        private void LoadSettings()
        {
            ExcelPath.Text = Properties.Settings.Default.ExcelPath;
            DbPath.Text = Properties.Settings.Default.DbPath;
            InvoiceTemplatePath.Text = Properties.Settings.Default.InvoiceTemplatePath;
            if (!string.IsNullOrWhiteSpace(ExcelPath.Text))
            {
                LoadExcel();
            }
            if (!string.IsNullOrWhiteSpace(DbPath.Text))
            {
                ConnectToDb();
            }
            if (!string.IsNullOrWhiteSpace(InvoiceTemplatePath.Text))
            {
                LoadInvoiceTemplate();
            }
        }

        private void ExcelPathDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void ExcelPathDrop(object sender, DragEventArgs e)
        {
            DropHandler(sender, e, ExcelPath, LoadExcel);
        }

        void DropHandler(object sender, DragEventArgs e, TextBox tb, Action loader)
        {
            var data = (DataObject)e.Data;
            if (data.ContainsFileDropList())
            {
                string[] rawFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (rawFiles.Length > 0)
                {
                    tb.Text = rawFiles[0];
                    loader();
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
                LoadExcel();
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
            App.ShowMessage("Error: Unable to locate the Excel file.");
        }

        private void DbPathDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void DbPathDrop(object sender, DragEventArgs e)
        {
            DropHandler(sender, e, DbPath, ConnectToDb);
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
                ConnectToDb();
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
            App.ShowMessage("Error: Unable to locate the database file.");
        }

        private void ExcelSyncToDbClick(object sender, RoutedEventArgs e)
        {
            if (Connection == null)
            {
                return;
            }
            if (_excelRecords.Count > 0)
            {
                string errorMsg = null;
                try
                {
                    App.ShowMessage($"{_excelRecords.Count} client records found in Excel. Syncing to the database.");
                    var dbClients = new Dictionary<string, Client>();
                    using (var r = Connection.RunReaderQuery("select [Client Name], [DOB], [Gender], [Medicare], [Phone], [Address] from Clients"))
                    {
                        var sb = new StringBuilder();
                        while (r.Read())
                        {
                            var name = r.GetString(0);
                            var (fn, sn) = BookingIcs.SmartParseName(name);
                            var cr = new Client
                            {
                                FirstName = fn,
                                Surname = sn,
                                DOB = r.TryGetDateTime(1),
                                Gender = r.TryGetString(2),
                                MedicareNumber = r.TryGetString(3),
                                PhoneNumber = r.TryGetString(4),
                                Address = r.TryGetString(5)
                            };
                            dbClients[cr.MedicareNumber] = cr;
                        }
                        var modRecords = new List<Client>();
                        var newRecords = new List<Client>();
                        foreach (var er in _excelRecords)
                        {
                            if (dbClients.TryGetValue(er.MedicareNumber, out var dr))
                            {
                                if (!er.Equals(dr))
                                {
                                    modRecords.Add(er);
                                }
                            }
                            else
                            {
                                newRecords.Add(er);
                            }
                        }
                        foreach (var mr in modRecords)
                        {
                            var sbSql = new StringBuilder($"update Clients set ");
                            sbSql.Append($"[Client Name] = '{mr.ClientFormalName()}',");
                            sbSql.Append($"[DOB] = {mr.DOB.ToDbDate()},");
                            sbSql.Append($"[Gender] = '{mr.Gender}',");
                            sbSql.Append($"[Phone] = '{mr.PhoneNumber}',");
                            sbSql.Append($"[Address] = '{mr.Address}'");
                            sbSql.Append($" where [Medicare] = '{mr.MedicareNumber}'");
                            Connection.RunNonQuery(sbSql.ToString(), false);
                        }
                        foreach (var nr in newRecords)
                        {
                            var sbSql = new StringBuilder("insert into Clients([Client Name], [DOB], [Gender], [Medicare], [Phone], [Address]) values(");
                            sbSql.Append($"'{nr.ClientFormalName()}',");
                            sbSql.Append($"{nr.DOB.ToDbDate()},");
                            sbSql.Append($"'{nr.Gender}',");
                            sbSql.Append($"'{nr.MedicareNumber}',");
                            sbSql.Append($"'{nr.PhoneNumber}',");
                            sbSql.Append($"'{nr.Address}')");
                            Connection.RunNonQuery(sbSql.ToString(), false);
                        }
                        //TODO  delete non-existent clients?
                    }
                }
                catch (Exception ex)
                {
                    errorMsg = ex.Message;
                }
                finally
                {
                    CloseConnection();
                }
                if (errorMsg != null)
                {
                    App.ShowMessage($"Sync Excel to DB failed:\n{errorMsg}");
                }
                else
                {
                    App.ShowMessage("Sync Excel to DB succeeded.");
                }
            }
        }

        private void CloseConnection()
        {
            Connection.Close();
        }

        private void DbSyncToExcelClick(object sender, RoutedEventArgs e)
        {
            if (Connection == null)
            {
                return;
            }
            App.ShowMessage("Not implemented yet.");
        }

        private void LoadExcel()
        {
            if (LoadClientsFromExcel(ExcelPath.Text))
            {
                Properties.Settings.Default.ExcelPath = ExcelPath.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void ConnectToDb()
        {
            if (ConnectToDb(DbPath.Text))
            {
                Properties.Settings.Default.DbPath = DbPath.Text;
                Properties.Settings.Default.Save();
            }
        }
        
        bool LoadClientsFromExcel(string excelPath)
        {
            try
            {
                if (_loadedExcel == excelPath)
                {
                    return true;
                }

                if (!File.Exists(excelPath))
                {
                    App.ShowMessage("Error: The data file does not exist. Please provide a valid one.\nIf this is the first time to run a new version, it may not be initially available.");
                    return false;
                }

                var fi = new FileInfo(excelPath);
                using (var p = new ExcelPackage(fi))
                {
                    var ws = p.Workbook.Worksheets["Details"];

                    _excelRecords.Clear();
                    for (var i = 2; i <= ws.Cells.Rows; i++)
                    {
                        var medi = ws.Cells[i, 1].Text.Trim();
                        if (medi.Length == 0) break;
                        var firstName = ws.Cells[i, 2].Text.LaunderSpaceSeparateString();
                        var surname = ws.Cells[i, 3].Text.LaunderSpaceSeparateString();
                        var phone = ws.Cells[i, 6].Text;                        
                        var dob = ws.Cells[i, 5].Text.Trim();
                        var gen = ws.Cells[i, 4].Text.Trim();
                        var addr = ws.Cells[i, 7].Text.Trim();
                        var client = new Client
                        {
                            FirstName = firstName,
                            Surname = surname,
                            MedicareNumber = medi,
                            PhoneNumber = phone,
                            Address = addr,
                            Gender = gen
                        };
                        CultureInfo cultureinfo = new CultureInfo("en-AU");
                        if (DateTime.TryParse(dob, cultureinfo, DateTimeStyles.AssumeLocal, out var dt))
                        {
                            client.DOB = dt;
                        }
                        _excelRecords.Add(client);
                    }
                }

                _loadedExcel = excelPath;

                return true;
            }
            catch (Exception e)
            {
                App.ShowMessage($"Error: Failed to load the data file. Details:\n{e.Message}.");
                return false;
            }
        }

        private bool ConnectToDb(string path)
        {
            var connstr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={path};Persist Security Info=True";
            if (Connection?.ConnectionString == connstr)
            {
                return true;
            }
            _connectionString = connstr;
            var successful = false;
            try
            {
                Connection = new OleDbConnection(_connectionString);
                Connection.Open();
                App.ShowMessage("Successfully connected to the database.");
                successful = true;
            }
            catch (Exception e)
            {
                App.ShowMessage($"Failed to connect to the database. Details:\n{e.Message}");
                Connection = null;
            }
            finally
            {
                if (Connection != null)
                {
                    Connection.Close();
                }
                if (!successful)
                {
                    Connection = null;
                }
            }
            return successful;
        }

        private void BookingClick(object sender, RoutedEventArgs e)
        {
            if (_booking == null)
            {
                _booking = new BookingWindow(this);
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
                _invoicing = new InvoicingWindow(this);
                _invoicing.Closed += (s1, e1) =>
                {
                    _invoicing = null;
                };
            }
            _invoicing.Show();
            _invoicing.Activate();
        }

        private void InvoiceTemplatePathDrop(object sender, DragEventArgs e)
        {
            DropHandler(sender, e, InvoiceTemplatePath, LoadInvoiceTemplate);
        }

        private void InvoiceTemplatePathPreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        private void InvoiceTemplateBrowseClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog
            {
                Filter = "Microsoft Word documents (*.docx)|*.docx|All files (*.*)|*.*"
            };
            if (File.Exists(InvoiceTemplatePath.Text))
            {
                ofd.InitialDirectory = Path.GetDirectoryName(InvoiceTemplatePath.Text);
            }
            if (ofd.ShowDialog() == true)
            {
                InvoiceTemplatePath.Text = ofd.FileName;
                LoadInvoiceTemplate();
            }
        }

        private void InvoiceTemplateShowInExplorerClick(object sender, RoutedEventArgs e)
        {
            if (File.Exists(InvoiceTemplatePath.Text))
            {
                var dn = Path.GetDirectoryName(InvoiceTemplatePath.Text);
                if (Directory.Exists(dn))
                {
                    Process.Start("explorer.exe", dn);
                    return;
                }
            }
            App.ShowMessage("Error: Unable to locate the invoice template.");
        }

        private void LoadInvoiceTemplate()
        {
            if (File.Exists(InvoiceTemplatePath.Text))
            {
                Properties.Settings.Default.InvoiceTemplatePath = InvoiceTemplatePath.Text;
                Properties.Settings.Default.Save();
            }
        }

        private void HomePageLinkRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        private void InvoiceTemplatePathLostFocus(object sender, RoutedEventArgs e)
        {
            LoadInvoiceTemplate();
        }

        private void ExcelPathLostFocus(object sender, RoutedEventArgs e)
        {
            LoadExcel();
        }

        private void DbPathLostFocus(object sender, RoutedEventArgs e)
        {
            ConnectToDb();
        }

        private void ClientsClick(object sender, RoutedEventArgs e)
        {
            if (_clientsWindow == null)
            {
                _clientsWindow = new ClientsWindow(this);
                _clientsWindow.Closed += (s1, e1) =>
                {
                    _clientsWindow = null;
                };
            }
            _clientsWindow.Show();
            _clientsWindow.Activate();
        }
    }
}
