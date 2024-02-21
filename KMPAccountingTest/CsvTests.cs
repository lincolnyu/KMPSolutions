using KMPAccounting.BookKeepingTabular;
using KMPAccounting.BookKeepingTabular.InstitutionSpecifics;
using KMPAccounting.InstitutionSpecifics;
using KMPAccounting.KMPSpecifics;
using KMPCommon;

namespace KMPAccountingTest
{
    public class CsvTests
    {
        private string TestConfig = Path.Combine(Utility.GetThisFolderPath(), "localtest.cfg");
        private string TestDir;

        const string WaveAccountNABBusiness = "NAB Business";
        const string WaveAccountCommbankPersonal = "Commbank Personal";
        const string WaveAccountCashOnHand = "Cash on Hand";
        const string WaveAccountTBC = "TBC";

        static CsvReader SharedCsvReader = new CsvReader();
        static WaveRawReader SharedWaveRawReader = new WaveRawReader();

        static CommbankCashRowDescriptor CbaCashDesc = new CommbankCashRowDescriptor();
        static CommbankCreditCardRowDescriptor CbaCcDesc = new CommbankCreditCardRowDescriptor();
        static NABCashRowDescriptor NabCashDesc = new NABCashRowDescriptor();
        static WaveRowDescriptor WaveDesc = new WaveRowDescriptor();

        static TransactionMatcher Matcher = new TransactionMatcher();

        void ResetCsvReader()
        {
            SharedCsvReader = new CsvReader();
        }

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
        public void TestLoadingCBACash()
        {
            var cbaCash = GetCbaCash("CSVData.csv", "CBA Cash").AssertChangeToAscendingInDate();

            Assert.Multiple(() =>
            {
                Assert.That(cbaCash.Count(), Is.EqualTo(179));
                Assert.That(SharedCsvReader.HasHeader, Is.EqualTo(false));
            });
            Assert.Pass();
        }

        [Test]
        public void TestLoadingCBAWithHeader()
        {
            var cbaCash = GetCbaCash("CSVData_withheader.csv", "CBA Cash").AssertChangeToAscendingInDate();

            Assert.Multiple(() =>
            {
                Assert.That(cbaCash.Count(), Is.EqualTo(179));
                Assert.That(SharedCsvReader.HasHeader, Is.EqualTo(true));
            });
            Assert.Pass();
        }

        [Test]
        public void TestLoadingNABCash()
        {
            var nabCash = GetNab("nabcash", "Transactions.csv", "NAB Cash").ChangeToAscendingInDate();

            Assert.Multiple(() =>
            {
                Assert.That(nabCash.Count(), Is.EqualTo(243));
                Assert.That(SharedCsvReader.HasHeader, Is.EqualTo(true));
            });
            Assert.Pass();
        }

        [Test]
        public void TestLoadingNABSaving()
        {
            var nabSaving = GetNab("nabsaving", "Transactions.csv", "NAB Saving").AssertChangeToAscendingInDate();

            Assert.Multiple(() =>
            {
                Assert.That(nabSaving.Count(), Is.EqualTo(64));
                Assert.That(SharedCsvReader.HasHeader, Is.EqualTo(true));
            });
            Assert.Pass();
        }

        [Test]
        public void TestCBACashGuess()
        {
            var rows = GetCbaCash("CSVData_withheader.csv", "CBA Cash").AssertChangeToAscendingInDate();

            var guesser = new CommbankCashCounterAccountPrefiller();
            var guessedRows = rows.Select(x => { guesser.PrefillPersonalBankTransaction(x, null, false); return x; });

            var emptyCount = 0;
            var filledCount = 0;

            {
                using var f = new StreamWriter(@"C:\temp\cbacash_guessed.csv");
                var headerChecked = false;
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
                        if (SharedCsvReader.HasHeader == true)
                        {
                            f.WriteLine(string.Join(',', SharedCsvReader.LoadedHeader!));
                        }
                        headerChecked = true;
                    }
                    f.WriteLine(row);
                }
            }

            var filledPercentage = (double)filledCount / (filledCount + emptyCount);
            Assert.Multiple(() =>
            {
                Assert.That(filledCount + emptyCount, Is.EqualTo(179));
                Assert.That(filledPercentage, Is.GreaterThan(0.88));    // TODO improve it
            });
            Assert.Pass();
        }

        [Test]
        public void TestCBACCGuess()
        {
            var rows = GetCbaCc("CSVData_withheader.csv", "CBA Credit Card").AssertChangeToAscendingInDate();

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = rows.Select(x => { guesser.PrefillPersonalBankTransaction(x, null, false); return x; });
            {
                using var f = new StreamWriter(@"C:\temp\cbacc_guessed.csv");
                if (SharedCsvReader.HasHeader == true)
                {
                    f.WriteLine(string.Join(',', SharedCsvReader.LoadedHeader!));
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
                        if (SharedCsvReader.HasHeader == true)
                        {
                            f.WriteLine(string.Join(',', SharedCsvReader.LoadedHeader!));
                        }
                        headerChecked = true;
                    }
                    f.WriteLine(row);
                }
                var filledPercentage = (double)filledCount / (filledCount + emptyCount);
                Assert.That(filledCount + emptyCount, Is.EqualTo(1435));
                Assert.That(filledPercentage, Is.GreaterThan(0.55));    // TODO improve it.
            }
            Assert.Pass();
        }

        [Test]
        public void TestWaveRawLoadingExcludingIncome()
        {
            var rows = GetWave("raw.txt", "Wave", false).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(SharedWaveRawReader.ReadRowCount, Is.EqualTo(135));
                Assert.That(rows, Has.Count.EqualTo(84));
            });

            {
                using var f = new StreamWriter(@"C:\temp\wave.csv");

                f.WriteLine(string.Join(',', SharedWaveRawReader.RowDescriptor!.Keys));
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
            var rows = GetWave("raw.txt", "Wave", true).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(SharedWaveRawReader.ReadRowCount, Is.EqualTo(135));
                Assert.That(rows, Has.Count.EqualTo(137));
            });

            {
                using var f = new StreamWriter(@"C:\temp\wave.csv");

                f.WriteLine(string.Join(',', SharedWaveRawReader.RowDescriptor!.Keys));
                foreach (var row in rows)
                {
                    f.WriteLine(row);
                }
            }

            Assert.Pass();
        }

        private IEnumerable<(int, ITransactionRow?, ITransactionRow?)> MatchTransactions()
        {

            var cbaCash = GetCbaCash("CSVData.csv", "CBA Cash").AssertChangeToAscendingInDate();

            var cbaCc = GetCbaCc("CSVData.csv", "CBA Credit Card").AssertChangeToAscendingInDate();

            var nabCash = GetNab("nabcash", "Transactions.csv", "NAB Cash").ChangeToAscendingInDate();

            var wave = GetWave("raw.txt", "Wave", false).AssertChangeToAscendingInDate();

            var items = Matcher.Match(new IEnumerable<ITransactionRow>[] { cbaCash, cbaCc, nabCash }, wave, inputRow =>
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

            return TransactionMatcher.OrderMatch(items);
        }

        [Test]
        public void TestTransactionMatching()
        {
            var items = MatchTransactions();

            var printer = TransactionRowCsvFormatter.CreateSimpleCombiningRowDescriptors(CbaCashDesc, CbaCcDesc, NabCashDesc, WaveDesc);

            printer.GetColumn("Date")!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            var cashInvoices = 0;
            var inconsistentAccountTypes = 0;
            {
                using var f = new StreamWriter(@"C:\temp\wave_combined.csv");
                f.WriteLine("Source,Message," + string.Join(',', printer.Columns.Select(x => x.TargetColumnName)));
                foreach (var (index, bankRow, invoiceRow) in items)
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
                            if ((invoiceRow[WaveDesc.WaveAccountKey] == WaveAccountCommbankPersonal && (index != 0 && index != 1)) || (invoiceRow[WaveDesc.WaveAccountKey] == WaveAccountNABBusiness && index != 2) || (invoiceRow[WaveDesc.WaveAccountKey] == WaveAccountCashOnHand))
                            {
                                message = "Error: Inconsistent account types. Invoice is with a bank account that differs.";
                                inconsistentAccountTypes++;
                            }
                            else if (invoiceRow[WaveDesc.WaveAccountKey] == WaveAccountTBC)
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
                Assert.That(Matcher.MatchedInvoices + cashInvoices, Is.EqualTo(Matcher.TotalInvoices));
            });
        }

        private string DateFormatter(string arg)
        {
            return CsvUtility.ParseDateTime(arg).ToShortDateOnlyString();
        }

        [Test]
        public void TestCBACCGuessWithInvoice()
        {
            var items = MatchTransactions();

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaccRows.Select(x => { guesser.PrefillPersonalBankTransaction((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2, false); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

            var printer = TransactionRowCsvFormatter.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

            printer.GetColumn("Date")!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            var emptyCount = 0;
            var filledCount = 0;
            {
                using var f = new StreamWriter(@"C:\temp\cbacc_joint_guessed.csv");
                f.WriteLine(string.Join(',', printer.Columns.Select(x => x.TargetColumnName)));
                
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    if (!string.IsNullOrWhiteSpace(bankRow[bankRow.OwnerTable.RowDescriptor.CounterAccountKey]))
                    {
                        filledCount++;
                    }
                    else
                    {
                        emptyCount++;
                    }
                    IEnumerable<string> line;
                    if (invoiceRow != null)
                    {
                        line = printer.FieldsToStrings(bankRow!, invoiceRow);
                    }
                    else
                    {
                        line = printer.FieldsToStrings(bankRow!);
                    }

                    f.WriteLine(string.Join(',', line));
                }

            }
            var filledPercentage = (double)filledCount / (filledCount + emptyCount);
            Assert.Multiple(() =>
            {
                Assert.That(filledCount + emptyCount, Is.EqualTo(1435));

                Assert.That(filledPercentage, Is.GreaterThanOrEqualTo(0.58));    // TODO improve it.
            });
            Assert.Pass();
        }

        [Test]
        public void TestCBACCGuessWithInvoiceAndFallback()
        {
            var items = MatchTransactions();

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaccRows.Select(x => { guesser.PrefillPersonalBankTransaction((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

            var printer = TransactionRowCsvFormatter.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

            printer.GetColumn("Date")!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            {
                using var f = new StreamWriter(@"C:\temp\cbacc_joint_guessed_fallback.csv");
                f.WriteLine(string.Join(',', printer.Columns.Select(x => x.TargetColumnName)));

                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    Assert.IsTrue(!string.IsNullOrWhiteSpace(bankRow[bankRow.OwnerTable.RowDescriptor.CounterAccountKey]));

                    IEnumerable<string> line;
                    if (invoiceRow != null)
                    {
                        line = printer.FieldsToStrings(bankRow!, invoiceRow);
                    }
                    else
                    {
                        line = printer.FieldsToStrings(bankRow!);
                    }

                    f.WriteLine(string.Join(',', line));
                }

            }

            Assert.Pass();
        }

        [Test]
        public void TestGuessAllWithInvoice()
        {
            var items = MatchTransactions().ToArray();

            {
                var cbaCashRows = items.Where(x => x.Item1 == 0).Select(x => (x.Item2, x.Item3));
                var guesser = new CommbankCreditCardCounterAccountPrefiller();
                foreach (var x in cbaCashRows)
                {
                    guesser.PrefillPersonalBankTransaction((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2, false, false);
                }
            }

            {
                var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));
                var guesser = new CommbankCreditCardCounterAccountPrefiller();
                foreach (var x in cbaccRows)
                {
                    guesser.PrefillPersonalBankTransaction((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2, false, false);
                }
            }

            var printer = TransactionRowCsvFormatter.CreateSimpleCombiningRowDescriptors(CbaCashDesc, CbaCcDesc, NabCashDesc, WaveDesc);

            printer.GetColumn("Date")!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            var cashInvoices = 0;
            var inconsistentAccountTypes = 0;

            {
                using var f = new StreamWriter(@"C:\temp\wave_combined_guessed.csv");
                f.WriteLine("Source,Message," + string.Join(',', printer.Columns.Select(x => x.TargetColumnName)));
                foreach (var (index, bankRow, invoiceRow) in items)
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
                            if ((invoiceRow[WaveDesc.WaveAccountKey] == WaveAccountCommbankPersonal && (index != 0 && index != 1)) || (invoiceRow[WaveDesc.WaveAccountKey] == WaveAccountNABBusiness && index != 2) || (invoiceRow[WaveDesc.WaveAccountKey] == WaveAccountCashOnHand))
                            {
                                message = "Error: Inconsistent account types. Invoice is with a bank account that differs.";
                                inconsistentAccountTypes++;
                            }
                            else if (invoiceRow[WaveDesc.WaveAccountKey] == WaveAccountTBC)
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
                Assert.That(Matcher.MatchedInvoices + cashInvoices, Is.EqualTo(Matcher.TotalInvoices));
            });

            Assert.Pass();
        }

        [Test]
        public void TestCBACashGuessWithInvoice()
        {
            var items = MatchTransactions();

            var cbaCashRows = items.Where(x => x.Item1 == 0).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCashCounterAccountPrefiller();
            var guessedRows = cbaCashRows.Select(x => { guesser.PrefillPersonalBankTransaction((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2, false); return ((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2); });

            var printer = TransactionRowCsvFormatter.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

            printer.GetColumn("Date")!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            var emptyCount = 0;
            var filledCount = 0;
            {
                using var f = new StreamWriter(@"C:\temp\cbacash_joint_guessed.csv");
                f.WriteLine(string.Join(',', printer.Columns.Select(x => x.TargetColumnName)));

                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    if (!string.IsNullOrWhiteSpace(bankRow[bankRow.OwnerTable.RowDescriptor.CounterAccountKey]))
                    {
                        filledCount++;
                    }
                    else
                    {
                        emptyCount++;
                    }
                    IEnumerable<string> line;
                    if (invoiceRow != null)
                    {
                        line = printer.FieldsToStrings(bankRow!, invoiceRow);
                    }
                    else
                    {
                        line = printer.FieldsToStrings(bankRow!);
                    }

                    f.WriteLine(string.Join(',', line));
                }

            }
            var filledPercentage = (double)filledCount / (filledCount + emptyCount);
            Assert.Multiple(() =>
            {
                Assert.That(filledCount + emptyCount, Is.EqualTo(179));
                Assert.That(filledPercentage, Is.GreaterThanOrEqualTo(0.88));    // TODO improve it.
            });
            Assert.Pass();
        }

        public void TestCbaCreditCardCorrelation()
        {
            var items = MatchTransactions();

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaccRows.Select(x => { guesser.PrefillPersonalBankTransaction((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

            {
                using var f = new StreamWriter(@"C:\temp\cbacc_correlation.txt");
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    var transaction = LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!);

                    f.WriteLine(transaction.ToString());
                    f.WriteLine("--------------------------------------------------------------------------------");
                }
            }
            Assert.Pass();
        }

        [Test]
        public void TestCbaCashCorrelation()
        {
            var items = MatchTransactions();

            var cbaCashRows = items.Where(x => x.Item1 == 0).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCashCounterAccountPrefiller();
            var guessedRows = cbaCashRows.Select(x => { guesser.PrefillPersonalBankTransaction((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2); });

            {
                using var f = new StreamWriter(@"C:\temp\cbacash_correlation.txt");
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    var transaction = LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!);

                    f.WriteLine(transaction.ToString());
                    f.WriteLine("--------------------------------------------------------------------------------");
                }
            }
            Assert.Pass();
        }

        private IEnumerable<TransactionRow<CommbankCashRowDescriptor>> GetCbaCash(string fileName, string tableName)
        {
            ResetCsvReader();
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == fileName);
            return SharedCsvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTable<CommbankCashRowDescriptor>(tableName) { BaseAccountName = AccountConstants.PersonalCashAccount } );
        }

        private IEnumerable<TransactionRow<CommbankCreditCardRowDescriptor>> GetCbaCc(string fileName, string tableName)
        {
            ResetCsvReader();
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacc"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == fileName);
            return SharedCsvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTable<CommbankCreditCardRowDescriptor>(tableName) { BaseAccountName = AccountConstants.PersonalCommbankCreditCardRepayment });
        }

        private IEnumerable<TransactionRow<NABCashRowDescriptor>> GetNab(string folder, string fileName, string tableName)
        {
            ResetCsvReader();
            var dir = new DirectoryInfo(Path.Combine(TestDir, folder));
            var nabCsv = dir.GetFiles().First(x => x.Name == fileName);
            return SharedCsvReader.GetRows(new StreamReader(nabCsv.FullName), new BankTransactionTable<NABCashRowDescriptor>(tableName));
        }

        private IEnumerable<TransactionRow<WaveRowDescriptor>> GetWave(string fileName, string tableName, bool includeIncome = false)
        {
            ResetCsvReader();
            var dir = new DirectoryInfo(Path.Combine(TestDir, "wavereceipt"));
            var rawTxtFile = dir.GetFiles().First(x => x.Name == fileName);
            var rowDescriptor = new WaveRowDescriptor();
            return SharedWaveRawReader.GetRows(new StreamReader(rawTxtFile.FullName), new TransactionTable<WaveRowDescriptor>(tableName, rowDescriptor), includeIncome);
        }
    }
}
