using KMPAccounting.BookKeepingTabular;
using KMPAccounting.BookKeepingTabular.InstitutionSpecifics;
using KMPAccounting.InstitutionSpecifics;
using KMPAccounting.KMPSpecifics;
using KMPAccounting.Objects.BookKeeping;
using OU = KMPAccounting.Objects.Utility;
using TabularConstants = KMPAccounting.BookKeepingTabular.Constants;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.KMPSpecifics.Scripts;
using KMPAccounting.Accounting;
using KMPCommon;


namespace KMPAccounting.Test
{
    public class CsvTests
    {
        private class TestAccountingExecutor : AccountingExecutor
        {
            public TestAccountingExecutor(string statementsFolder) : base(statementsFolder)
            {
            }
        }

        private string TestConfig = Path.Combine(Utility.GetThisFolderPath(), "localtest.cfg");

        private TestAccountingExecutor SharedAccountingExecutor;

        const string WaveAccountNABBusiness = "NAB Business";
        const string WaveAccountCommbankPersonal = "Commbank Personal";
        const string WaveAccountCashOnHand = "Cash on Hand";
        const string WaveAccountTBC = "TBC";

        TransactionsReader SharedTransactionReader => SharedAccountingExecutor.TransactionsReader;

        CsvReader? SharedCsvReader => SharedTransactionReader.CsvReader;
        WaveRawReader? SharedWaveRawReader => SharedTransactionReader.WaveReader;
        TransactionMatcher Matcher => SharedAccountingExecutor.Matcher;

        static CommbankCashRowDescriptor CbaCashDesc = new CommbankCashRowDescriptor();
        static CommbankCreditCardRowDescriptor CbaCcDesc = new CommbankCreditCardRowDescriptor();
        static NABRowDescriptor NabCashDesc = new NABRowDescriptor();
        static WaveRowDescriptor WaveDesc = new WaveRowDescriptor();

        static List<(int, ITransactionRow?, ITransactionRow?)>? MatchResult = null;

        public string StatementsDir { get; private set; }

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
            StatementsDir = elems[1];
            SharedAccountingExecutor = new TestAccountingExecutor(StatementsDir);
        }

        [Test]
        public void TestLoadingCBACash()
        {
            var cbaCash = SharedTransactionReader.GetCbaCash("CSVData.csv", "CBA Cash").AssertChangeToAscendingInDate();

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
            var cbaCash = SharedTransactionReader.GetCbaCash("CSVData_withheader.csv", "CBA Cash").AssertChangeToAscendingInDate();

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
            var nabCash = SharedTransactionReader.GetNab("nabcash", "Transactions.csv", "NAB Cash", AccountConstants.Business.Accounts.Cash).ChangeToAscendingInDate();

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
            var nabSaving = SharedTransactionReader.GetNab("nabsaving", "Transactions.csv", "NAB Saving", AccountConstants.Business.Accounts.Saving).AssertChangeToAscendingInDate();

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
            var rows = SharedTransactionReader.GetCbaCash("CSVData_withheader.csv", "CBA Cash").AssertChangeToAscendingInDate();

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
            var rows = SharedTransactionReader.GetCbaCc("CSVData_withheader.csv", "CBA Credit Card").AssertChangeToAscendingInDate();

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
            var rows = SharedTransactionReader.GetWave("raw.txt", "Wave", false).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(SharedWaveRawReader.ReadRowCount, Is.EqualTo(154));
                Assert.That(rows, Has.Count.EqualTo(89));
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
            var rows = SharedTransactionReader.GetWave("raw.txt", "Wave", true).ToList();

            Assert.Multiple(() =>
            {
                Assert.That(SharedWaveRawReader.ReadRowCount, Is.EqualTo(154));
                Assert.That(rows, Has.Count.EqualTo(156));
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
        public void TestTransactionMatching()
        {
            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCashDesc, CbaCcDesc, NabCashDesc, WaveDesc);

            printer.GetColumn(TabularConstants.DateTimeKey)!.Formatter = DateFormatter;
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

            Assert.Pass();
        }

        private string DateFormatter(string arg)
        {
            return CsvUtility.ParseDateTime(arg).ToShortDateOnlyString();
        }

        [Test]
        public void TestCBACCGuessWithInvoice()
        {
            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaccRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

            printer.GetColumn(TabularConstants.DateTimeKey)!.Formatter = DateFormatter;
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
            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var cbaccRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaccRows.Select(x => { guesser.Prefill((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

            printer.GetColumn(TabularConstants.DateTimeKey)!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            {
                using var f = new StreamWriter(@"C:\temp\cbacc_joint_guessed_fallback.csv");
                f.WriteLine(string.Join(',', printer.Columns.Select(x => x.TargetColumnName)));

                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    Assert.That(string.IsNullOrWhiteSpace(bankRow[bankRow.OwnerTable.RowDescriptor.CounterAccountKey]), Is.False);

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
            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var cbaCashRows = items.Where(x => x.Item1 == 0).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCashCounterAccountPrefiller();
            var guessedRows = cbaCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false); return ((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2); });

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

            printer.GetColumn(TabularConstants.DateTimeKey)!.Formatter = DateFormatter;
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
        public void TestAdbCashGuessWithInvoice()
        {
            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var cbaccRows = items.Where(x => x.Item1 == 3).Select(x => (x.Item2, x.Item3));

            var guesser = new AdbCashCounterAccountPrefiller(false);
            var guessedRows = cbaccRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false); return ((TransactionRow<AdbRowDescriptor>)x.Item1!, x.Item2); });

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(CbaCcDesc, WaveDesc);

            printer.GetColumn(TabularConstants.DateTimeKey)!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            var emptyCount = 0;
            var filledCount = 0;
            {
                using var f = new StreamWriter(@"C:\temp\adbcash_joint_guessed.csv");
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
                Assert.That(filledCount + emptyCount, Is.EqualTo(93));
                Assert.That(filledPercentage, Is.EqualTo(1));
            });
            Assert.Pass();
        }

        [Test]
        public void TestNabGuessWithInvoiceAndCsvLoading()
        {
            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var nabCashRows = items.Where(x => x.Item1 == 2).Select(x => (x.Item2, x.Item3));

            var guesser = new NABCashCounterAccountPrefiller();
            var guessedRows = nabCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false); return ((TransactionRow<NABRowDescriptor>)x.Item1!, x.Item2); }).ToList();

            var printer = MultiTransactionRowSourceCsvCombiner.CreateSimpleCombiningRowDescriptors(NabCashDesc, WaveDesc);

            printer.GetColumn(TabularConstants.DateTimeKey)!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            var columnSelector = new NABRowDescriptor().Keys.Select(k => printer.GetColumnIndex(typeof(NABRowDescriptor), k)).ToList();

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
                var loadedNabRows = SharedCsvReader.GetRows(srNabJointGuessed, new BankTransactionTable<NABRowDescriptor>("NAB Cash Reviewed"), columnSelector, true).ToList();

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
        public void TestCbaCashGuessAndCorrelation()
        {
            AccountsState.Clear();

            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var cbaCashRows = items.Where(x => x.Item1 == 0).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCashCounterAccountPrefiller();
            var guessedRows = cbaCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCashRowDescriptor>)x.Item1!, x.Item2); }).ToList();

            var ledger = new Ledger();

            AccountConstants.EnsureCreateAllPersonalAccounts(ledger, DateTime.Now);

            OU.AddAndExecuteTransaction(ledger, DateTime.Now, AccountConstants.Personal.Accounts.CashCba, AccountConstants.Personal.Accounts.Equity, 8701.53m);

            Assert.That(OU.GetAccount(AccountConstants.Personal.Accounts.CashCba)!.Balance, Is.EqualTo(8701.53m));

            {
                using var f = new StreamWriter(@"C:\temp\cbacash_correlation.txt");
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    foreach (var transaction in LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!))
                    {
                        f.Write(transaction.ToString());
                        f.WriteLine("--------------------------------------------------------------------------------");

                        OU.AddAndExecute(ledger, transaction);
                    }

                    var actualBalance = OU.GetAccount(AccountConstants.Personal.Accounts.CashCba)!.Balance;
                    var expectedBalance = bankRow.GetDecimalValue(bankRow.OwnerTable.RowDescriptor.BalanceKey);
                    Assert.That(actualBalance, Is.EqualTo(expectedBalance));
                }
            }

            Assert.That(AccountsState.GetAccountsState("Family")!.Balance, Is.EqualTo(0m));

            Assert.Pass();
        }

        [Test]
        public void TestCbaCreditCardGuessAndCorrelation()
        {
            AccountsState.Clear();

            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var cbaCcRows = items.Where(x => x.Item1 == 1).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = cbaCcRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false, true); return ((TransactionRow<CommbankCreditCardRowDescriptor>)x.Item1!, x.Item2); });

            var ledger = new Ledger();

            AccountConstants.EnsureCreateAllPersonalAccounts(ledger, DateTime.Now);

            OU.AddAndExecuteTransaction(ledger, DateTime.Now, AccountConstants.Personal.Accounts.ExpenseMain, AccountConstants.Personal.Accounts.CommbankCreditCard, 6984.37m);

            {
                using var f = new StreamWriter(@"C:\temp\cbacc_correlation.txt");
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    foreach (var transaction in LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!))
                    {
                        f.WriteLine(transaction.ToString());
                        f.WriteLine("--------------------------------------------------------------------------------");

                        OU.AddAndExecute(ledger, transaction);
                    }

                    var expectedBalance = bankRow.GetDecimalValue(bankRow.OwnerTable.RowDescriptor.BalanceKey);
                    if (expectedBalance.HasValue)
                    {
                        var bal = OU.GetAccount(AccountConstants.Personal.Accounts.CommbankCreditCard)!.Balance;
                        Assert.That(bal, Is.EqualTo(-expectedBalance.Value));
                    }
                }
            }

            Assert.That(AccountsState.GetAccountsState("Family")!.Balance, Is.EqualTo(0m));

            Assert.Pass();
        }

        [Test]
        public void TestNabCashGuessAndCorrelation()
        {
            AccountsState.Clear();

            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var nabCashRows = items.Where(x => x.Item1 == 2).Select(x => (x.Item2, x.Item3));

            var guesser = new NABCashCounterAccountPrefiller();
            var guessedRows = nabCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false, true); return ((TransactionRow<NABRowDescriptor>)x.Item1!, x.Item2); }).ToList();

            var totalRows = guessedRows.Count;

            var ledger = new Ledger();

            AccountConstants.EnsureCreateAllBusinessAccounts(ledger, DateTime.Now);

            OU.AddAndExecuteTransaction(ledger, DateTime.Now, AccountConstants.Business.Accounts.Cash, AccountConstants.Business.Accounts.Equity, 663.91m);

            Assert.That(OU.GetAccount(AccountConstants.Business.Accounts.Cash)!.Balance, Is.EqualTo(663.91m));

            {
                using var f = new StreamWriter(@"C:\temp\nabcash_correlation.txt");
                var processedRows = 0;
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    var amount = bankRow.GetDecimalValue(bankRow.OwnerTable.RowDescriptor.AmountKey);
                    if (amount == 0) continue;

                    foreach (var transaction in LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!))
                    {
                        f.WriteLine(transaction.ToString());
                        f.WriteLine("--------------------------------------------------------------------------------");

                        OU.AddAndExecute(ledger, transaction);
                    }

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
            Assert.That(AccountsState.GetAccountsState("KMP")!.Balance, Is.EqualTo(0m));

            Assert.Pass();
        }

        [Test]
        public void TestAdbCashGuessAndCorrelation()
        {
            AccountsState.Clear();

            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

            var adbCashRows = items.Where(x => x.Item1 == 3).Select(x => (x.Item2, x.Item3));

            var guesser = new CommbankCreditCardCounterAccountPrefiller();
            var guessedRows = adbCashRows.Select(x => { guesser.Prefill(x.Item1!, x.Item2, false, true); return ((TransactionRow<AdbRowDescriptor>)x.Item1!, x.Item2); }).ToList();

            var ledger = new Ledger();

            AccountConstants.EnsureCreateAllPersonalAccounts(ledger, DateTime.Now);

            OU.AddAndExecuteTransaction(ledger, DateTime.Now, AccountConstants.Personal.Accounts.CashAdb, AccountConstants.Personal.Accounts.Equity, 279188.86m);

            Assert.That(OU.GetAccount(AccountConstants.Personal.Accounts.CashAdb)!.Balance, Is.EqualTo(279188.86m));

            {
                using var f = new StreamWriter(@"C:\temp\adbcash_correlation.txt");
                var processedRows = 0;
                foreach (var (bankRow, invoiceRow) in guessedRows)
                {
                    var amount = bankRow.GetDecimalValue(bankRow.OwnerTable.RowDescriptor.AmountKey);
                    if (amount == 0) continue;

                    foreach (var transaction in LedgerBankTransactionRowCorrelator.CorrelateToSingleTransaction(bankRow!))
                    {
                        f.WriteLine(transaction.ToString());
                        f.WriteLine("--------------------------------------------------------------------------------");

                        OU.AddAndExecute(ledger, transaction);
                    }

                    var expectedBalance = bankRow.GetDecimalValue(bankRow.OwnerTable.RowDescriptor.BalanceKey);
                    if (expectedBalance.HasValue)
                    {
                        var actualBalance = OU.GetAccount(AccountConstants.Personal.Accounts.CashAdb)!.Balance;
                        Assert.That(actualBalance, Is.EqualTo(expectedBalance.Value));
                    }

                    processedRows++;
                }
            }
        }

        [Test]
        public void TestAdbLoansGuessAndCorrelation()
        {
            {
                var adbLoan1 = SharedTransactionReader.GetAdbLoan("adbloan1", "adbloan1_balance.csv", "ADB Loan1", AccountConstants.Personal.Accounts.LoanLiveIn).AssertChangeToAscendingInDate();
                var guesser1 = new AdbLoanCounterAccountPrefillier($"{AccountConstants.Personal.Accounts.LoanInterestProeprty1}", $"{AccountConstants.Personal.Accounts.LoanLiveInRepayment}", $"{AccountConstants.Personal.Accounts.LoanFeesProperty1}");
                var guessedRows1 = adbLoan1.Select(x => { guesser1.Prefill(x); return x; });

                using var f = new StreamWriter(@"C:\temp\adbloan1_guessed.csv");
                var headerChecked = false;
                var filledCount = 0;
                var emptyCount = 0;
                foreach (var row in guessedRows1)
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
                Assert.Multiple(() =>
                {
                    Assert.That(filledCount + emptyCount, Is.EqualTo(36));
                    Assert.That(filledPercentage, Is.EqualTo(1));
                });
            }

            {
                var adbLoan2 = SharedTransactionReader.GetAdbLoan("adbloan2", "adbloan2_balance.csv", "ADB Loan2", AccountConstants.Personal.Accounts.LoanProperty2).AssertChangeToAscendingInDate();

                var guesser2 = new AdbLoanCounterAccountPrefillier($"{AccountConstants.Personal.Accounts.LoanInterestProperty2}", $"{AccountConstants.Personal.Accounts.LoanRepaymentProperty2}", $"{AccountConstants.Personal.Accounts.LoanFeesProperty2}");

                var guessedRows2 = adbLoan2.Select(x => { guesser2.Prefill(x); return x; });

                using var f = new StreamWriter(@"C:\temp\adbloan2_guessed.csv");
                var headerChecked = false;
                var filledCount = 0;
                var emptyCount = 0;
                foreach (var row in guessedRows2)
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
                Assert.Multiple(() =>
                {
                    Assert.That(filledCount + emptyCount, Is.EqualTo(24));
                    Assert.That(filledPercentage, Is.EqualTo(1));
                });
            }
        }


        [Test]
        public void TestGuessAllWithInvoice()
        {
            var items = SharedAccountingExecutor.MatchTransactionsAndKeep();

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

            printer.GetColumn(TabularConstants.DateTimeKey)!.Formatter = DateFormatter;
            printer.GetColumn("Invoice Date")!.Formatter = DateFormatter;

            // +2 for the extra beginning columns.
            var nabColumnSelector = new NABRowDescriptor().Keys.Select(k => printer.GetColumnIndex(typeof(NABRowDescriptor), k)+2).ToList();

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
                var loadedNabRows = SharedCsvReader.GetRows(srNabJointGuessed, new BankTransactionTable<NABRowDescriptor>("NAB Cash Reviewed"), nabColumnSelector, true).Where(x => x.Item2[0].Contains('N')).ToList();

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

        [Test]
        public void TestFY23()
        {
            var fy23 = new FinancialYear23();
            fy23.Initialize();
            fy23.Step1_MatchTransactionsAndPrint(@"c:\temp\fy23_step1_matched.csv");
            fy23.Step2_CorrelateToLedger(@"c:\temp\fy23_step2_bal_personal.txt", @"c:\temp\fy23_step2_bal_kmp.txt");
            fy23.Step3_OffsetLiabilities(@"c:\temp\fy23_step3_bal_personal_liabilitiesOffset.txt", @"c:\temp\fy23_step3_bal_kmp_liabilitiesOffset.txt");
            fy23.Step4_ClaimAndTrialCalculatePnLAndTax(@"c:\temp\fy23_step4_bal_personal_taxPrep.txt", @"c:\temp\fy23_step4_bal_kmp_taxPrep.txt", @"c:\temp\fy23_step4_pnl_personal.txt", @"c:\temp\fy23_step4_pnl_business.txt");
            fy23.Step5_ClearEquity(@"c:\temp\fy23_step5_bal_personal_cleanedup.txt", @"c:\temp\fy23_step5_bal_kmp_cleanedup.txt");
            fy23.PrintLedger(@"c:\temp\fy23_ledger.txt");

            var error = Utility.CompareTextFiles(@"c:\temp\", new DirectoryInfo(Path.Combine(StatementsDir,@"results\v2")));

            Assert.That(error, Is.Null);

            Assert.Pass();
        }

        [Test]
        public void TestFY24()
        {

        }

        [Test]
        public void TestSuperCsvLoading()
        {
            var myNorthCashFileName = Path.Combine(StatementsDir, @"mynorth\Cash.csv");
            var myNorthCashRows = SuperTracker.GetMyNorthRows<MyNorthCashDescriptor>(myNorthCashFileName, "MyNorth Cash").AssertChangeToAscendingInDate().ToList();

            var myNorthNonCashFileName = Path.Combine(StatementsDir, @"mynorth\Invest.csv");
            var myNorthNonCashRows = SuperTracker.GetMyNorthRows<MyNorthNonCashDescriptor>(myNorthNonCashFileName, "MyNorth Investment").AssertChangeToAscendingInDate().ToList();

            Assert.That(myNorthCashRows.Count(), Is.EqualTo(304));
            Assert.That(myNorthNonCashRows.Count(), Is.EqualTo(51));

            Assert.Pass();
        }

        [Test]
        public void TestSuper()
        {
            var myNorthCashFileName = Path.Combine(StatementsDir, @"mynorth\Cash.csv");
            var myNorthCashRows = SuperTracker.GetMyNorthRows<MyNorthCashDescriptor>(myNorthCashFileName, "MyNorth Cash").AssertChangeToAscendingInDate().ToList();

            var myNorthNonCashFileName = Path.Combine(StatementsDir, @"mynorth\Invest.csv");
            var myNorthNonCashRows = SuperTracker.GetMyNorthRows<MyNorthNonCashDescriptor>(myNorthNonCashFileName, "MyNorth Investment").AssertChangeToAscendingInDate().ToList();

            var st = new SuperTracker("MyNorth");
            var entries = st.TrackMyNorth(myNorthCashRows, myNorthNonCashRows).ToList();

            Assert.That(st.Error, Is.Null);
            var ledger = new Ledger();
            st.Setup(ledger, DateTime.Now);

            var start = ledger.Entries.Count;
            ledger.Entries.AddRange(entries);
            ledger.Execute(start);

            Assert.That(AccountsState.GetAccountsState("MyNorth")!.Balance, Is.Zero);

            {
                var north = AccountsState.GetAccountsState("MyNorth");
                using var sw = new StreamWriter(@"c:\temp\north_bal.txt");
                sw.Write(north!.ToString(2));
            }

            {
                using var sw = new StreamWriter(@"c:\temp\north_ledger.txt");
                ledger.PrintLedger(sw);
            }

            Assert.Pass();
        }

        void ResetCsvReader()
        {
            SharedTransactionReader.RenewCsvReader();
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
