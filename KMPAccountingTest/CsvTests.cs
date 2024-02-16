using KMPAccounting.BookKeepingTabular;
using KMPAccounting.BookKeepingTabular.InstitutionSpecifics;
using KMPAccounting.InstitutionSpecifics;
using KMPAccounting.KMPSpecifics;

namespace KMPAccountingTest
{
    public class CsvTests
    {
        private string TestConfig = Path.Combine(Utility.GetThisFolderPath(),  "localtest.cfg");
        private string TestDir;

        [SetUp]
        public void Setup()
        {
            if (!File.Exists(TestConfig))
            {
                Assert.Ignore();
            }
            using var cfg = new StreamReader(TestConfig);
            var cfgContent = cfg.ReadToEnd();
            var lines = cfgContent.Split('\n');
            var line = lines.First(x => x.StartsWith("csvtestdir="));
            var elems = line.Split('=');
            TestDir = elems[1];
        }

        [Test]
        public void TestLoadingCBA()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x=>x.Name == "CSVData.csv");
            var csvReader = new CsvReader();
            Assert.Multiple(() =>
            {
                Assert.That(csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash")).Count, Is.EqualTo(179));
                Assert.That(csvReader.HasHeader, Is.EqualTo(false));
            });
            Assert.Pass();
        }

        [Test]
        public void TestLoadingCBAWithHeader()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "CSVData_withheader.csv");
            var csvReader = new CsvReader();
            Assert.Multiple(() =>
            {
                Assert.That(csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash")).Count, Is.EqualTo(179));
                Assert.That(csvReader.HasHeader, Is.EqualTo(true));
            });
            Assert.Pass();
        }

        [Test]
        public void TestLoadingNABCash()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "nabcash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "Transactions.csv");
            var csvReader = new CsvReader();
            Assert.Multiple(() =>
            {
                Assert.That(csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<NABCashRowDescriptor>("NAB Cash")).Count, Is.EqualTo(243));
                Assert.That(csvReader.HasHeader, Is.EqualTo(true));
            });
            Assert.Pass();
        }

        [Test]
        public void TestLoadingNABSaving()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "nabsaving"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "Transactions.csv");
            var csvReader = new CsvReader();
            Assert.Multiple(() =>
            {
                Assert.That(csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<NABCashRowDescriptor>("NAB Saving")).Count, Is.EqualTo(64));
                Assert.That(csvReader.HasHeader, Is.EqualTo(true));
            });
            Assert.Pass();
        }

        [Test]
        public void TestCBACashGuess()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "CSVData_withheader.csv");
            var csvReader = new CsvReader();
            var rows = csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash"));

            var guessedRows = rows.Select(x => { CommbankCashCounterAccountPrefiller.Prefill(x, null, false); return x; });

            {
                using var f = new StreamWriter(@"C:\temp\cbacash_guessed.csv");
                var headerChecked = false;
                var emptyCount = 0;
                var filledCount = 0;
                foreach(var row in guessedRows)
                {
                    if (!string.IsNullOrWhiteSpace(row[row.OwnerTable.RowDescriptor.CounterAccountKey]))
                    {
                        filledCount++;
                    }
                    else
                    {
                        emptyCount++;
                    }
                    if (!headerChecked)
                    {
                        if (csvReader.HasHeader == true)
                        {
                            f.WriteLine(string.Join(',', csvReader.LoadedHeader!));
                        }
                        headerChecked = true;
                    }
                    f.WriteLine(row);
                }
                var filledPercentage = (double)filledCount / (filledCount + emptyCount);
                Assert.That(filledPercentage, Is.GreaterThan(0.88));
            }
            Assert.Pass();
        }

        [Test]
        public void TestCBACCGuess()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacc"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "CSVData_withheader.csv");

            var csvReader = new CsvReader();
            var rows = csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCreditCardRowDescriptor>("CBA CreditCard"));
            var guessedRows = rows.Select(x => {CommbankCreditCardCounterAccountPrefiller.Guess(x, null, false); return x; });
            {
                using var f = new StreamWriter(@"C:\temp\cbacc_guessed.csv");
                if (csvReader.HasHeader == true)
                {
                    f.WriteLine(string.Join(',', csvReader.LoadedHeader!));
                }
                var headerChecked = false;
                var emptyCount = 0;
                var filledCount = 0;
                foreach (var row in guessedRows)
                {
                    if (!string.IsNullOrWhiteSpace(row[row.OwnerTable.RowDescriptor.CounterAccountKey]))
                    {
                        filledCount++;
                    }
                    else
                    {
                        emptyCount++;
                    }
                    if (!headerChecked)
                    {
                        if (csvReader.HasHeader == true)
                        {
                            f.WriteLine(string.Join(',', csvReader.LoadedHeader!));
                        }
                        headerChecked = true;
                    }
                    f.WriteLine(row);
                }
                var filledPercentage = (double)filledCount / (filledCount + emptyCount);
                Assert.That(filledPercentage, Is.GreaterThan(0.88));
            }
            Assert.Pass();
        }

        [Test]
        public void TestWaveRawLoadingExcludingIncome()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "wavereceipt"));
            var rawTxtFile = dir.GetFiles().First(x => x.Name == "raw.txt");
            var waveReader = new WaveRawReader();
            var rows = waveReader.GetRows(new StreamReader(rawTxtFile.FullName), new BaseTransactionTableDescriptor<WaveRowDescriptor>("Wave", new WaveRowDescriptor()), false).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(waveReader.ReadRowCount, Is.EqualTo(134));
                Assert.That(rows, Has.Count.EqualTo(83));
            });

            {
                using var f = new StreamWriter(@"C:\temp\wave.csv");

                f.WriteLine(string.Join(',', waveReader.RowDescriptor!.Keys));
                foreach (var row in rows)
                {
                    f.WriteLine(row);
                }
            }

            Assert.Pass();
        }

        [Test]
        public void TestWaveRawLoadingIncludingIncome()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "wavereceipt"));
            var rawTxtFile = dir.GetFiles().First(x => x.Name == "raw.txt");
            var waveReader = new WaveRawReader();
            var rows = waveReader.GetRows(new StreamReader(rawTxtFile.FullName), new BaseTransactionTableDescriptor<WaveRowDescriptor>("Wave", new WaveRowDescriptor()), true).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(waveReader.ReadRowCount, Is.EqualTo(134));
                Assert.That(rows, Has.Count.EqualTo(136));
            });

            {
                using var f = new StreamWriter(@"C:\temp\wave.csv");

                f.WriteLine(string.Join(',', waveReader.RowDescriptor!.Keys));
                foreach (var row in rows)
                {
                    f.WriteLine(row);
                }
            }

            Assert.Pass();
        }

        [Test]
        public void TestTransactionMatching()
        {
            const string WaveAccountNABBusiness = "NAB Business";
            const string WaveAccountCommbankPersonal = "Commbank Personal";
            const string WaveAccountCashOnHand = "Cash on Hand";
            const string WaveAccountTBC = "TBC";

            var cbaCash = GetCbaCash().OrderBy(x => x);
            var cbaCc = GetCbaCc().OrderBy(x => x);
            var nabCash = GetNabCash().OrderBy(x => x);

            var wave = GetWave().OrderBy(x => x);

            var cbaCashDesc = new CommbankCashRowDescriptor();
            var cbaCcDesc = new CommbankCreditCardRowDescriptor();
            var nabCashDesc = new NABCashRowDescriptor();
            var waveDesc = new WaveRowDescriptor();

            var printer = TransactionRowCsvFormatter.CreateSimpleCombiningRowDescriptors(cbaCashDesc, cbaCcDesc, nabCashDesc, waveDesc);
            printer.UnifyDateTimeColumnFormatIntoDateOnly = true;

            var matcher = new TransactionMatcher();
            var items = matcher.Match(new IEnumerable<ITransactionRow>[] { cbaCash, cbaCc, nabCash }, wave, inputRow =>
            {
                var invoice = (TransactionRow<WaveRowDescriptor>)inputRow;
                var account = invoice[invoice.OwnerTable.RowDescriptor.WaveAccountKey];
                if (account == WaveAccountNABBusiness)
                {
                    return new[] { 2, 1, 0 };
                }
                else if (account == WaveAccountCommbankPersonal)
                {
                    return new[] { 1, 0, 2 };
                }
                return new[] { 1, 2, 0 };
            }, true, new (int?, int?)?[] { null, null, (null, 10) });

            var cashInvoices = 0;
            var inconsistentAccountTypes = 0;
            {
                using var f = new StreamWriter(@"C:\temp\wave_combined.csv");
                f.WriteLine("Source,Message," + string.Join(',', printer.Columns.Select(x => x.TargetColumnName)));
                foreach (var (index, bankRow, invoiceRow) in items.OrderByDescending(x=> x.Item2?? x.Item3))
                {
                    string message = "";
                    string head = "";
                    IEnumerable<string> line;
                    if (bankRow != null)
                    {
                        if (invoiceRow != null)
                        {
                            head += "W";
                            line = printer.FieldsToStrings(bankRow, invoiceRow);
                            if ((invoiceRow[waveDesc.WaveAccountKey] == WaveAccountCommbankPersonal && (index != 0 && index != 1)) || (invoiceRow[waveDesc.WaveAccountKey] == WaveAccountNABBusiness && index != 2) || (invoiceRow[waveDesc.WaveAccountKey] == WaveAccountCashOnHand))
                            {
                                message = "Error: Inconsistent account types. Invoice is with a bank account that differs.";
                                inconsistentAccountTypes++;
                            }
                            else if (invoiceRow[waveDesc.WaveAccountKey] == WaveAccountTBC)
                            {
                                message = "Info: Invoice account type can be updated.";
                            }
                        }
                        else
                        {
                            line = printer.FieldsToStrings(bankRow);
                        }
                        switch (index)
                        {
                            case 0:
                                head += "C";
                                break;
                            case 1:
                                head += "D";
                                break;
                            case 2:
                                head += "N";
                                break;
                        }
                    }
                    else
                    {
                        head += "W";
                        line = printer.FieldsToStrings(invoiceRow!);
                        var waveRow = (TransactionRow<WaveRowDescriptor>)invoiceRow!;
                        if (waveRow[waveRow.OwnerTable.RowDescriptor.WaveAccountKey] == WaveAccountCashOnHand)
                        {
                            head += "H";
                            cashInvoices++;
                        }
                        else if (waveRow[waveRow.OwnerTable.RowDescriptor.WaveAccountKey] == WaveAccountTBC)
                        {
                            message = "Warning: Account type could be 'Cash on Hand'.";
                        }
                        else
                        {
                            message = "Error: Unmatched account.";
                        }
                    }

                    f.WriteLine(head + ',' + message + ',' + string.Join(',', line));
                }
            }

            Assert.Multiple(() =>
            {
                Assert.That(inconsistentAccountTypes, Is.Zero);
                Assert.That(matcher.MatchedInvoices + cashInvoices, Is.EqualTo(matcher.TotalInvoices));
            });
        }

        private IEnumerable<TransactionRow<CommbankCashRowDescriptor>> GetCbaCash()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "CSVData.csv");
            var csvReader = new CsvReader();
            return csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCashRowDescriptor>("CBA Cash"));
        }

        private IEnumerable<TransactionRow<CommbankCreditCardRowDescriptor>> GetCbaCc()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacc"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "CSVData.csv");

            var csvReader = new CsvReader();
            return csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<CommbankCreditCardRowDescriptor>("CBA CreditCard"));
        }

        private IEnumerable<TransactionRow<NABCashRowDescriptor>> GetNabCash()
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "nabcash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == "Transactions.csv");
            var csvReader = new CsvReader();
            return csvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTableDescriptor<NABCashRowDescriptor>("NAB Cash"));
        }

        private IEnumerable<TransactionRow<WaveRowDescriptor>> GetWave(bool includeIncome = false)
        {
            var dir = new DirectoryInfo(Path.Combine(TestDir, "wavereceipt"));
            var rawTxtFile = dir.GetFiles().First(x => x.Name == "raw.txt");
            var rowDescriptor = new WaveRowDescriptor();
            return new WaveRawReader().GetRows(new StreamReader(rawTxtFile.FullName), new BaseTransactionTableDescriptor<WaveRowDescriptor>("Wave", rowDescriptor), includeIncome);
        }
    }
}
