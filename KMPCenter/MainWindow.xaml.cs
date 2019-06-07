using System;
using System.Windows;
using System.Data.OleDb;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;
using KMPBookingCore;
using OfficeOpenXml;
using System.Text;
using System.Data;
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
        private OleDbConnection _connection;
        private string _connectionString;
        private BookingWindow _booking;
        private InvoicingWindow _invoicing;
        private readonly ClientRecords _clients = new ClientRecords();

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
            InvoiceTemplatePath.Text = Properties.Settings.Default.InvoiceTemplatePath;
            if (!string.IsNullOrWhiteSpace(ExcelPath.Text))
            {
                LoadExcel(false);
            }
            if (!string.IsNullOrWhiteSpace(DbPath.Text))
            {
                ConnectToDb(false);
            }
            if (!string.IsNullOrWhiteSpace(InvoiceTemplatePath.Text))
            {
                LoadInvoiceTemplate(false);
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

        void DropHandler(object sender, DragEventArgs e, TextBox tb, Action<bool> loader)
        {
            var data = (DataObject)e.Data;
            if (data.ContainsFileDropList())
            {
                string[] rawFiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (rawFiles.Length > 0)
                {
                    tb.Text = rawFiles[0];
                    loader(true);
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
            App.ShowMessage("Error: Unable to locate the database file.");
        }

        private DateTime? TryGetDateTime(OleDbDataReader r, int col)
        {
            try
            {
                return r.GetDateTime(col);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private string TryGetString(OleDbDataReader r, int col)
        {
            try
            {
                return r.GetString(col);
            }
            catch (Exception)
            {
                return "";
            }
        }

        private void ExcelSyncToDbClick(object sender, RoutedEventArgs e)
        {
            if (_connection == null)
            {
                return;
            }
            var excelRecords = _clients.Records();
            if (excelRecords.Count > 0)
            {
                App.ShowMessage($"{excelRecords.Count} client records found in Excel. Syncing to the database.");
                var dbClients = new Dictionary<string, ClientRecord>();
                using (var r = RunReaderQuery("select [Client Name], [DOB], [Gender], [Medicare], [Phone], [Address] from Clients"))
                {
                    var sb = new StringBuilder();
                    var prov= CultureInfo.InvariantCulture;
                    while (r.Read())
                    {
                        var name = r.GetString(0);
                        var (fn, sn) = BookingIcs.SmartParseName(name);
                        var cr = new ClientRecord
                        {
                            FirstName = fn,
                            Surname = sn,
                            DOB = TryGetDateTime(r, 1),
                            Gender = ClientRecord.ParseGender(TryGetString(r, 2)),
                            MedicareNumber = TryGetString(r, 3),
                            PhoneNumber = TryGetString(r, 4),
                            Address = TryGetString(r, 5)
                        };
                        dbClients[cr.MedicareNumber] = cr;
                    }
                    var modRecords = new List<ClientRecord>();
                    var newRecords = new List<ClientRecord>();
                    foreach (var er in excelRecords)
                    {
                        if (dbClients.TryGetValue(er.MedicareNumber, out var dr))
                        {
                            if(!er.Equals(dr))
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
                        var name = BookingIcs.FormCommaSeparateName(mr.FirstName, mr.Surname);
                        var sbSql = new StringBuilder($"update Clients set ");
                        sbSql.Append($"[Client Name] = \"{name}\",");
                        sbSql.Append($"[DOB] = {mr.DOB?.ToString("'dd/MM/yyyy'") ?? "NULL"},");
                        sbSql.Append($"[Gender] = '{ClientRecord.ToString(mr.Gender)}'");
                        sbSql.Append($"[Phone] = '{mr.PhoneNumber}'");
                        sbSql.Append($"[Address] = \"{mr.Address}\"");
                        sbSql.Append($" where [Medicare] = '{mr.MedicareNumber}'");
                        RunNonQuery(sbSql.ToString(), false);
                    }
                    foreach (var nr in newRecords)
                    {
                        var name = BookingIcs.FormCommaSeparateName(nr.FirstName, nr.Surname);
                        var sbSql = new StringBuilder("insert into Clients([Client Name], [DOB], [Gender], [Medicare], [Phone], [Address]) values(");
                        sbSql.Append($"\"{name}\",");
                        sbSql.Append($"{nr.DOB?.ToString("'dd/MM/yyyy'") ?? "NULL"},");
                        sbSql.Append($"'{ClientRecord.ToString(nr.Gender)}',");
                        sbSql.Append($"'{nr.MedicareNumber}',");
                        sbSql.Append($"'{nr.PhoneNumber}',");
                        sbSql.Append($"\"{nr.Address}\")");
                        RunNonQuery(sbSql.ToString(), false);
                    }
                    //TODO  delete non-existent clients?
                    CloseConnection();
                }
            }
        }

        private void CloseConnection()
        {
            _connection.Close();
        }

        private OleDbDataReader RunReaderQuery(string query, bool closeConnectionOnComplete = true)
        {
            using (var cmd = new OleDbCommand(query, _connection))
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                var cb = closeConnectionOnComplete? 
                    CommandBehavior.CloseConnection 
                    : CommandBehavior.Default;
                var reader = cmd.ExecuteReader(cb);
                return reader;
            }
        }

        private object RunScalarQuery(string query, bool closeConnectionOnComplete = true)
        {
            using (var cmd = new OleDbCommand(query, _connection))
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                var res = cmd.ExecuteScalar();
                if (closeConnectionOnComplete)
                {
                    _connection.Close();
                }
                return res;
            }
        }

        private void RunNonQuery(string cmdstr, bool closeConnectionOnComplete)
        {
            using (var cmd = new OleDbCommand(cmdstr, _connection))
            {
                if (_connection.State != ConnectionState.Open)
                {
                    _connection.Open();
                }
                cmd.ExecuteNonQuery();
                if (closeConnectionOnComplete)
                {
                    _connection.Close();
                }
            }
        }

        private void DbSyncToExcelClick(object sender, RoutedEventArgs e)
        {
            if (_connection == null)
            {
                return;
            }
            App.ShowMessage("Not implemented yet.");
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
            try
            {
                if (!File.Exists(excelPath))
                {
                    App.ShowMessage("Error: The data file does not exist. Please provide a valid one.\nIf this is the first time to run a new version, it may not be initially available.");
                    return false;
                }

                var fi = new FileInfo(excelPath);
                using (var p = new ExcelPackage(fi))
                {
                    var ws = p.Workbook.Worksheets["Details"];

                    _clients.Clear();
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
                        var client = new ClientRecord
                        {
                            FirstName = firstName,
                            Surname = surname,
                            MedicareNumber = medi,
                            PhoneNumber = phone,
                            Address = addr,
                            Gender = ClientRecord.ParseGender(gen)
                        };
                        if (DateTime.TryParse(dob, out var dt))
                        {
                            client.DOB = dt;
                        }
                        _clients.Add(client);
                    }
                }

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
            _connectionString = connstr;
            var successful = false;
            try
            {
                _connection = new OleDbConnection(_connectionString);
                _connection.Open();
                App.ShowMessage("Successfully connected to the database.");
                successful = true;
            }
            catch (Exception e)
            {
                App.ShowMessage($"Failed to connect to the database. Details:\n{e.Message}");
                _connection = null;
            }
            finally
            {
                if (_connection != null)
                {
                    _connection.Close();
                }
                if (!successful)
                {
                    _connection = null;
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
            DropHandler(sender, e, InvoiceTemplatePath, b => { LoadInvoiceTemplate(b); });
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
                LoadInvoiceTemplate(true);
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

        private void LoadInvoiceTemplate(bool saveSettings)
        {
            if (File.Exists(InvoiceTemplatePath.Text))
            {
                if (saveSettings)
                {
                    Properties.Settings.Default.InvoiceTemplatePath = InvoiceTemplatePath.Text;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void HomePageLinkRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {

        }

        private void InvoiceTemplateLostFocus(object sender, RoutedEventArgs e)
        {
            LoadInvoiceTemplate(true);
        }
    }
}
