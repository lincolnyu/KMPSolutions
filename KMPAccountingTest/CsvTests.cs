using KMPAccounting.BookKeepingTabular;
using KMPAccounting.BookKeepingTabular.InstitutionSpecifics;
using KMPAccounting.InstitutionSpecifics;
using KMPAccounting.KMPSpecifics;
using KMPAccounting.Objects.BookKeeping;
using OU = KMPAccounting.Objects.Utility;
using KMPCommon;
using KMPAccounting.Objects.Accounts;

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
        static List<(int, ITransactionRow?, ITransactionRow?)>? MatchResult = null;

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
            var guessedRows = rows.Select(x => { guesser.Prefill(x, null, false); return x; });

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
            var guessedRows = rows.Select(x => { guesser.Prefill(x, null, false); return x; });
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

            var nabCash = GetNab("nabcash", "Transactions.csv", "NAB Cash").ChangeToAscendingInDate().ToList();

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

        private List<(int, ITransactionRow?, ITransactionRow?)> MatchTransactionsAndKeep()
        {
            if (MatchResult == null)
            {
                MatchResult = MatchTransactions().ToList();
            }
            return MatchResult!;
        }

        [Test]
        public void TestTransactionMatching()
        {
            var items = MatchTransactionsAndKeep();

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCashDesc, CbaCcDesc, NabCashDesc, WaveDesc);

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
            var items = MatchTransactionsAndKeep();

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaccRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

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
            var items = MatchTransactionsAndKeep();

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaccRows.Select(x => { guesser.Prefill((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

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
        public void TestCBACashGuessWithInvoice()
        {
            var items = MatchTransactionsAndKeep();

            var cbaCashRows = items.Where(x => x.Item1 == 0).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCashCounterAccountPrefiller();
            var guessedRows = cbaCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false); return ((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2); });

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

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

        [Test]
        public void TestNabGuessWithInvoiceAndCsvLoading()
        {
            var items = MatchTransactionsAndKeep();

            var nabCashRows = items.Where(x => x.Item1 == 2).Select(x => (x.Item2, x.Item3));

            var guesser = new NABCashCounterAccountPrefiller();
            var guessedRows = nabCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false); return ((TransactionRow<NABCashRowDescriptor>)x.Item1!, x.Item2); }).ToList();

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(NabCashDesc, WaveDesc);

            printer.GetColumn("Date")!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            var columnSelector = new NABCashRowDescriptor().Keys.Select(k => printer.GetColumnIndex(typeof(NABCashRowDescriptor), k)).ToList();

            var emptyCount = 0;
            var filledCount = 0;
            {
                using var f = new StreamWriter(@"C:\temp\nabcash_joint_guessed.csv");
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
                Assert.That(guessedRows.Count, Is.EqualTo(243));
                Assert.That(filledCount + emptyCount, Is.EqualTo(243));
                Assert.That(filledPercentage, Is.GreaterThanOrEqualTo(0.89));   // TODO: improve.
            });

            {
                ResetCsvReader();

                var srNabJointGuessed = new StreamReader(@"C:\temp\nabcash_joint_guessed.csv");
                var loadedNabRows = SharedCsvReader.GetRows(srNabJointGuessed, new BankTransactionTable<NABCashRowDescriptor>("NAB Cash Reviewed"), columnSelector, true).ToList();

                Assert.That(loadedNabRows.Count, Is.EqualTo(243));

                for (var i = 0; i < 243; i++)
                {
                    var row = guessedRows[i].Item1;
                    var loadedRow = loadedNabRows[i].Item1;
                    AssertRowsAreEqual(row, loadedRow);
                }
            }

            Assert.Pass();
        }

        [Test]
        public void TestCbaCreditCardCorrelation()
        {
            var items = MatchTransactionsAndKeep();

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaccRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

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
            AccountsState.Clear();

            var items = MatchTransactionsAndKeep();

            var cbaCashRows = items.Where(x => x.Item1 == 0).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCashCounterAccountPrefiller();
            var guessedRows = cbaCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2); }).ToList();

            var totalRows = guessedRows.Count;

            var ledger = new Ledger();

            AccountConstants.EnsureCreateAllPersonalAccounts(ledger);

            OU.AddAndExecuteTransaction(ledger, DateTime.Now, AccountConstants.Personal.Accounts.Cash, AccountConstants.Personal.Accounts.Equity, 8701.53m);

            Assert.That(OU.GetAccount(AccountConstants.Personal.Accounts.Cash)!.Balance, Is.EqualTo(8701.53m));

            {
                using var f = new StreamWriter(@"C:\temp\cbacash_correlation.txt");
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    var transaction = LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!);

                    f.WriteLine(transaction.ToString());
                    f.WriteLine("--------------------------------------------------------------------------------");

                    OU.AddAndExecute(ledger, transaction);

                    var actualBalance = OU.GetAccount(AccountConstants.Personal.Accounts.Cash)!.Balance;
                    var expectedBalance = bankRow.GetDecimalValue(bankRow.OwnerTable.RowDescriptor.BalanceKey);
                    Assert.That(actualBalance, Is.EqualTo(expectedBalance));
                }
            }
            Assert.Pass();
        }

        [Test]
        public void TestCbaCcCorrelation()
        {
            AccountsState.Clear();

            var items = MatchTransactionsAndKeep();

            var cbaCcRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaCcRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); }).ToList();

            var totalRows = guessedRows.Count;

            var ledger = new Ledger();

            AccountConstants.EnsureCreateAllPersonalAccounts(ledger);

            // TODO Add balance initial setup.

            {
                using var f = new StreamWriter(@"C:\temp\cbacc_correlation.txt");
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    var transaction = LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!);

                    f.WriteLine(transaction.ToString());
                    f.WriteLine("--------------------------------------------------------------------------------");

                    OU.AddAndExecute(ledger, transaction);

                    // TODO Add balance check
                }
            }
            Assert.Pass();
        }

        [Test]
        public void TestNabCashCorrelation()
        {
            AccountsState.Clear();

            var items = MatchTransactionsAndKeep();

            var nabCashRows = items.Where(x => x.Item1 == 2).Select(x => (x.Item2, x.Item3));

            var guesser = new NABCashCounterAccountPrefiller();
            var guessedRows = nabCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false, true); return ((TransactionRow<NABCashRowDescriptor>)x.Item1!, x.Item2); }).ToList();

            var totalRows = guessedRows.Count;

            var ledger = new Ledger();

            AccountConstants.EnsureCreateAllBusinessAccounts(ledger);

            OU.AddAndExecuteTransaction(ledger, DateTime.Now, AccountConstants.Business.Accounts.Cash, AccountConstants.Business.Accounts.Equity, 663.91m);

            Assert.That(OU.GetAccount(AccountConstants.Business.Accounts.Cash)!.Balance, Is.EqualTo(663.91m));

            {
                using var f = new StreamWriter(@"C:\temp\nabcash_correlation.txt");
                var processedRows = 0;
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    var amount = bankRow.GetDecimalValue(bankRow.OwnerTable.RowDescriptor.AmountKey);
                    if (amount == 0) continue;

                    var transaction = LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!);

                    f.WriteLine(transaction.ToString());
                    f.WriteLine("--------------------------------------------------------------------------------");

                    OU.AddAndExecute(ledger, transaction);

                    // Check the balance if it's the last few rows. (The original transaction list may be wrong on balance for a few rows so we don't check all of them)
                    if (processedRows + 10 >= totalRows)
                    {
                        var actualBalance = OU.GetAccount(AccountConstants.Business.Accounts.Cash)!.Balance;
                        var expectedBalance = bankRow.GetDecimalValue(bankRow.OwnerTable.RowDescriptor.BalanceKey);
                        Assert.That(actualBalance, Is.EqualTo(expectedBalance));
                    }

                    processedRows++;
                }
            }

            Assert.That(OU.GetAccount(AccountConstants.Business.Accounts.Cash)!.Balance, Is.EqualTo(1165.88));

            Assert.Pass();
        }

        [Test]
        public void TestGuessAllWithInvoice()
        {
            var items = MatchTransactionsAndKeep();

            var cbaCashRows = items.Where(x => x.Item1 == 0).Select(x => (x.Item2, x.Item3));
            {
                var guesser = new CommbankCashCounterAccountPrefiller();
                foreach (var x in cbaCashRows)
                {
                    guesser.Prefill(x.Item1!, x.Item2, false, false);
                }
            }

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));
            {
                var guesser = new CommbankCreditCardCounterAccountPrefiller();
                foreach (var x in cbaccRows)
                {
                    guesser.Prefill(x.Item1!, x.Item2, false, false);
                }
            }

            var nabRows = items.Where(x => x.Item1 == 2).Select(x => (x.Item2, x.Item3)).ToList();
            {
                var guesser = new NABCashCounterAccountPrefiller();
                foreach (var x in nabRows)
                {
                    guesser.Prefill(x.Item1!, x.Item2, false, false);
                }
            }

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCashDesc, CbaCcDesc, NabCashDesc, WaveDesc);

            printer.GetColumn("Date")!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;


            // +2 for the extra beginning columns.
            var nabColumnSelector = new NABCashRowDescriptor().Keys.Select(k => printer.GetColumnIndex(typeof(NABCashRowDescriptor), k)+2).ToList();


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

            {
                ResetCsvReader();

                var srNabJointGuessed = new StreamReader(@"C:\temp\wave_combined_guessed.csv");
                var loadedNabRows = SharedCsvReader.GetRows(srNabJointGuessed, new BankTransactionTable<NABCashRowDescriptor>("NAB Cash Reviewed"), nabColumnSelector, true).Where(x => x.Item2[0].Contains('N')).ToList();

                Assert.That(loadedNabRows.Count, Is.EqualTo(243));

                for (var i = 0; i < 243; i++)
                {
                    var row = nabRows[i].Item1;
                    var loadedRow = loadedNabRows[i];
                    AssertRowsAreEqual(row!, loadedRow.Item1);
                }
            }

            Assert.Pass();
        }

        void ResetCsvReader()
        {
            SharedCsvReader = new CsvReader();
        }

        private IEnumerable<TransactionRow<CommbankCashRowDescriptor>> GetCbaCash(string fileName, string tableName)
        {
            ResetCsvReader();
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacash"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == fileName);
            return SharedCsvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTable<CommbankCashRowDescriptor>(tableName) { BaseAccountName = AccountConstants.Personal.Accounts.Cash } );
        }

        private IEnumerable<TransactionRow<CommbankCreditCardRowDescriptor>> GetCbaCc(string fileName, string tableName)
        {
            ResetCsvReader();
            var dir = new DirectoryInfo(Path.Combine(TestDir, "cbacc"));
            var cbaCashCsv = dir.GetFiles().First(x => x.Name == fileName);
            return SharedCsvReader.GetRows(new StreamReader(cbaCashCsv.FullName), new BankTransactionTable<CommbankCreditCardRowDescriptor>(tableName) { BaseAccountName = AccountConstants.Personal.Accounts.CommbankCreditCard });
        }

        private IEnumerable<TransactionRow<NABCashRowDescriptor>> GetNab(string folder, string fileName, string tableName)
        {
            ResetCsvReader();
            var dir = new DirectoryInfo(Path.Combine(TestDir, folder));
            var nabCsv = dir.GetFiles().First(x => x.Name == fileName);
            return SharedCsvReader.GetRows(new StreamReader(nabCsv.FullName), new BankTransactionTable<NABCashRowDescriptor>(tableName) {  BaseAccountName = AccountConstants.Business.Accounts.Cash}).ToList();
        }

        private IEnumerable<TransactionRow<WaveRowDescriptor>> GetWave(string fileName, string tableName, bool includeIncome = false)
        {
            ResetCsvReader();
            var dir = new DirectoryInfo(Path.Combine(TestDir, "wavereceipt"));
            var rawTxtFile = dir.GetFiles().First(x => x.Name == fileName);
            var rowDescriptor = new WaveRowDescriptor();
            return SharedWaveRawReader.GetRows(new StreamReader(rawTxtFile.FullName), new TransactionTable<WaveRowDescriptor>(tableName, rowDescriptor), includeIncome);
        }

        void AssertRowsAreEqual(ITransactionRow row1, ITransactionRow row2)
        {
            var desc1 = row1.OwnerTable.RowDescriptor;
            var desc2 = row2.OwnerTable.RowDescriptor;
            Assert.That(desc1.Keys.Count, Is.EqualTo(desc2.Keys.Count));
            for (var i = 0; i < desc1.Keys.Count; i++)
            {
                Assert.That(desc1.Keys[i] == desc2.Keys[i]);
            }
            foreach (var key in desc1.Keys)
            {
                if (key == desc1.DateTimeKey)
                {
                    Assert.That(CsvUtility.ParseDateTime(row1[key]!), Is.EqualTo(CsvUtility.ParseDateTime(row2[key]!)));
                }
                else
                {
                    if ((row1[key] == null || string.IsNullOrEmpty(row1[key])) && (row2[key] == null || string.IsNullOrEmpty(row2[key])))
                    {
                        continue;
                    }
                    Assert.That(row1[key], Is.EqualTo(row2[key]));
                }
            }
        }
    }
}
