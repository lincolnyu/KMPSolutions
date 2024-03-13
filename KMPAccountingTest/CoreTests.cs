using KMPAccounting.Objects;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using KMPAccounting.Accounting;
using KMPAccounting.ReportSchemes;
using OU = KMPAccounting.Objects.Utility;
using System.Linq;

namespace KMPCoreObjectsTest
{
    public class CoreTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void BachelorTest()
        {
            AccountsState.Clear();

            var ledger = new Ledger();

            OU.EnsureCreateAccount(ledger, "Tom.Assets", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity", true);
            OU.EnsureCreateAccount(ledger, "Tom.Liability", true);

            var tomState = AccountsState.GetAccountsState("Tom")!;

            const decimal BaseEquity = 300m;

            // Initial balance setup
            OU.EnsureCreateAccount(ledger, "Tom.Assets.Cash", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Main", false);
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets.Cash", "Tom.Equity.Main", BaseEquity);

            var indexInitial = ledger.Entries.Count;
            Assert.Multiple(() =>
            {
                Assert.That(tomState.Balance, Is.EqualTo(0));
                Assert.That(OU.GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity));
                Assert.That(OU.GetAccount("Tom.Equity.Main")!.Balance, Is.EqualTo(BaseEquity));
            });

            var tomInitialBalanceSnapshot = new AccountsState("Tom");
            tomState.CopyTo(tomInitialBalanceSnapshot, true);

            // Real transactions
            OU.EnsureCreateAccount(ledger, "Tom.Liability.TaxWithheld", true);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Income.Salary", false);

            const decimal Salary = 25000m;
            const decimal TaxWithheld = 2000m;
            const decimal Expense = 8000m;
            const decimal Deduction = 6000m;

            ledger.AddAndExecuteTransaction(DateTime.Now, new[] { ("Tom.Assets.Cash", Salary - TaxWithheld), ("Tom.Liability.TaxWithheld", TaxWithheld) },
                new[] { ("Tom.Equity.Income.Salary", Salary) });

            OU.EnsureCreateAccount(ledger, "Tom.Equity.Expense", true);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Deduction", true);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Expense", "Tom.Assets.Cash", Expense);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Deduction", "Tom.Assets.Cash", Deduction);

            var indexAfterRealTransactions = ledger.Entries.Count;

            Assert.Multiple(() =>
            {
                Assert.That(tomState.Balance, Is.EqualTo(0m));

                Assert.That(OU.GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity + Salary - Expense - Deduction - TaxWithheld));
                Assert.That(OU.GetAccount("Tom.Liability.TaxWithheld")!.Balance, Is.EqualTo(TaxWithheld));

                Assert.That(OU.GetAccount("Tom.Equity.Income.Salary")!.Balance, Is.EqualTo(Salary));
                Assert.That(OU.GetAccount("Tom.Equity.Main")!.Balance, Is.EqualTo(BaseEquity));
            });

            tomInitialBalanceSnapshot.CopyTo(tomState, true); // load snapshot at indexInitial.

            Assert.That(OU.GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity));

            var reportScheme = new ReportSchemePersonalGeneric(new ReportSchemePersonalGeneric.PersonalDetails("Tom"));

            reportScheme.Initialize();

            ledger.Execute(indexInitial, indexAfterRealTransactions);

            var pnlReports= reportScheme.Finalize().ToArray();

            Assert.Multiple(() =>
            {
                Assert.That(tomState.Balance, Is.EqualTo(0));
                Assert.That(pnlReports.Count, Is.EqualTo(1));
            });

            var tomPnlReport = pnlReports[0];
            Assert.Multiple(() =>
            {
                Assert.That(tomPnlReport.Income, Is.EqualTo(Salary));
                Assert.That(tomPnlReport.Deduction, Is.EqualTo(Deduction));
                Assert.That(tomPnlReport.TaxableIncome, Is.EqualTo(Salary - Deduction));

                var ExpectedTax = ReportSchemePersonalGeneric.GetPersonalTaxDefault(tomPnlReport.TaxableIncome);
                Assert.That(tomPnlReport.Tax, Is.EqualTo(ExpectedTax));
                Assert.That(tomPnlReport.TaxWithheld, Is.EqualTo(TaxWithheld));
                Assert.That(tomPnlReport.TaxReturn, Is.EqualTo(TaxWithheld - ExpectedTax));
                Assert.That(tomPnlReport.PostTaxIncome, Is.EqualTo(Salary - Deduction - ExpectedTax));
            });

            Assert.Pass();
        }

        [Test]
        public void TestUndoAndRedo()
        {
            AccountsState.Clear();

            var ledger = new Ledger();

            OU.EnsureCreateAccount(ledger, "Tom.Assets", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity", true);
            OU.EnsureCreateAccount(ledger, "Tom.Liability", true);

            const decimal BaseEquity = 300m;

            // Initial balance setup
            OU.EnsureCreateAccount(ledger, "Tom.Assets.Cash", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Main", false);
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets.Cash", "Tom.Equity.Main", BaseEquity, null);

            // Real transactions
            OU.EnsureCreateAccount(ledger, "Tom.Liability.TaxWithheld", true);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Income.Salary", false);

            const decimal Salary = 25000m;
            const decimal TaxWithheld = 2000m;
            const decimal Expense = 8000m;
            const decimal Deduction = 6000m;

            ledger.AddAndExecuteTransaction(DateTime.Now, new[] { ("Tom.Assets.Cash", Salary - TaxWithheld), ("Tom.Liability.TaxWithheld", TaxWithheld) },
                new[] { ("Tom.Equity.Income.Salary", Salary) }, null);

            OU.EnsureCreateAccount(ledger, "Tom.Equity.Expense", true);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Deduction", true);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Expense", "Tom.Assets.Cash", Expense, null);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Deduction", "Tom.Assets.Cash", Deduction, null);

            Assert.Multiple(() =>
            {
                Assert.That(OU.GetAccount("Tom")!.Balance, Is.EqualTo(0m));

                Assert.That(OU.GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity + Salary - Expense - Deduction - TaxWithheld));
                Assert.That(OU.GetAccount("Tom.Liability.TaxWithheld")!.Balance, Is.EqualTo(TaxWithheld));

                Assert.That(OU.GetAccount("Tom.Equity.Income.Salary")!.Balance, Is.EqualTo(Salary));
                Assert.That(OU.GetAccount("Tom.Equity.Main")!.Balance, Is.EqualTo(BaseEquity));
            });

            foreach (var entry in ledger.Entries.Reverse<Entry>())
            {
                entry.Undo();
            }

            Assert.That(OU.GetAccount("Tom"), Is.EqualTo(null));

            foreach (var entry in ledger.Entries)
            {
                entry.Redo();
            }

            Assert.Multiple(() =>
            {
                Assert.That(OU.GetAccount("Tom")!.Balance, Is.EqualTo(0m));

                Assert.That(OU.GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity + Salary - Expense - Deduction - TaxWithheld));
                Assert.That(OU.GetAccount("Tom.Liability.TaxWithheld")!.Balance, Is.EqualTo(TaxWithheld));

                Assert.That(OU.GetAccount("Tom.Equity.Income.Salary")!.Balance, Is.EqualTo(Salary));
                Assert.That(OU.GetAccount("Tom.Equity.Main")!.Balance, Is.EqualTo(BaseEquity));
            });

            Assert.Pass();
        }

        [Test]
        public void TestReckoningInstantly()
        {
            AccountsState.Clear();

            var ledger = new Ledger();

            OU.EnsureCreateAccount(ledger, "Tom.Assets", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity", true);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Main", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity2", false);
            
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_1", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_1.Equity1_1_1", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_1.Equity1_1_2", true);

            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_2", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_2.Equity1_2_1", true);
            
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets", "Tom.Equity.Equity1.Equity1_1.Equity1_1_1", 300);
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets", "Tom.Equity.Equity1.Equity1_1.Equity1_1_2", 200);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Equity2", "Tom.Assets", 100);

            var tomState = AccountsState.GetAccountsState("Tom")!;
            Assert.Multiple(() =>
            {
                Assert.That(tomState.Balance, Is.EqualTo(0));
                Assert.That(OU.GetAccount("Tom.Assets")!.Balance, Is.EqualTo(400));
                Assert.That(OU.GetAccount("Tom.Equity")!.Balance, Is.EqualTo(400));
                Assert.That(OU.GetAccount("Tom.Equity.Main")!.Balance, Is.EqualTo(0));

                Assert.That(OU.GetAccount("Tom.Equity.Equity1.Equity1_1.Equity1_1_1")!.Balance, Is.EqualTo(300));
                Assert.That(OU.GetAccount("Tom.Equity.Equity1.Equity1_1.Equity1_1_2")!.Balance, Is.EqualTo(-200));
                Assert.That(OU.GetAccount("Tom.Equity.Equity2")!.Balance, Is.EqualTo(-100));
            });

            OU.GetAccount("Tom.Equity")!.ReckonInstantly();
            
            Assert.Multiple(() =>
            {
                Assert.That(OU.GetAccount("Tom.Assets")!.Balance, Is.EqualTo(400));
                Assert.That(OU.GetAccount("Tom.Equity")!.Balance, Is.EqualTo(400));
                Assert.That(OU.GetAccount("Tom.Equity.Main")!.Balance, Is.EqualTo(400));

                Assert.That(OU.GetAccount("Tom.Equity.Equity1.Equity1_1.Equity1_1_1")!.Balance, Is.EqualTo(0));
                Assert.That(OU.GetAccount("Tom.Equity.Equity1.Equity1_1.Equity1_1_2")!.Balance, Is.EqualTo(0));
                Assert.That(OU.GetAccount("Tom.Equity.Equity2")!.Balance, Is.EqualTo(0));
            });
            Assert.Pass();
        }

        [Test]
        public void TestReckoningByTransactions()
        {
            AccountsState.Clear();

            var ledger = new Ledger();

            OU.EnsureCreateAccount(ledger, "Tom.Assets", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity", true);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Main", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity2", false);

            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_1", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_1.Equity1_1_1", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_1.Equity1_1_2", true);

            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_2", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Equity1.Equity1_2.Equity1_2_1", true);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets", "Tom.Equity.Equity1.Equity1_1.Equity1_1_1", 300);
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets", "Tom.Equity.Equity1.Equity1_1.Equity1_1_2", 200);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Equity2", "Tom.Assets", 100);

            OU.GetAccount("Tom.Equity")!.ReckonAccountByTransactions(out var toDebit, out var toCredit);

            Assert.Multiple(() =>
            {
                Assert.That(toDebit, Has.Count.EqualTo(2));
                Assert.That(toDebit[0], Is.EqualTo(("Tom.Equity.Equity1.Equity1_1.Equity1_1_1", 300)));
                Assert.That(toDebit[1], Is.EqualTo(("Tom.Equity.Equity1.Equity1_1.Equity1_1_2", 200)));

                Assert.That(toCredit, Has.Count.EqualTo(2));
                Assert.That(toCredit[0], Is.EqualTo(("Tom.Equity.Equity2", 100)));
                Assert.That(toCredit[1], Is.EqualTo(("Tom.Equity.Main", 400)));
            });

            Assert.Pass();
        }

        [Test]
        public void TestLedgerSerialization()
        {
            AccountsState.Clear();

            var ledger = new Ledger();

            OU.EnsureCreateAccount(ledger, "Tom.Assets", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity", true);
            OU.EnsureCreateAccount(ledger, "Tom.Liability", true);

            const decimal BaseEquity = 300m;

            // Initial balance setup
            OU.EnsureCreateAccount(ledger, "Tom.Assets.Cash", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Main", false);
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets.Cash", "Tom.Equity.Main", BaseEquity);

            // Real transactions
            OU.EnsureCreateAccount(ledger, "Tom.Liability.TaxWithheld", true);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Income.Salary", false);

            const decimal Salary = 25000m;
            const decimal TaxWithheld = 2000m;
            const decimal Expense = 8000m;
            const decimal Deduction = 6000m;

            ledger.AddAndExecuteTransaction(DateTime.Now, new[] { ("Tom.Assets.Cash", Salary - TaxWithheld), ("Tom.Liability.TaxWithheld", TaxWithheld) },
                new[] { ("Tom.Equity.Income.Salary", Salary) });


            OU.EnsureCreateAccount(ledger, "Tom.Equity.Expense", true);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Deduction", true);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Expense", "Tom.Assets.Cash", Expense);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Deduction", "Tom.Assets.Cash", Deduction);

            {
                using var sw = new StreamWriter("test.txt");
                ledger.SerializeToStream(sw);
            }
            {
                using var sr = new StreamReader("test.txt");
                var loadedLedger = new Ledger();
                loadedLedger.DeserializeFromStream(sr);

                Assert.That(loadedLedger, Is.EqualTo(ledger));
            }

            Assert.Pass();
        }
    }
}