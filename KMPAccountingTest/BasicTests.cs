using KMPAccounting.Objects;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using KMPAccounting.Accounting;
using KMPAccounting.ReportSchemes;
using OU = KMPAccounting.Objects.Utility;

namespace KMPCoreObjectsTest
{
    public class BasicTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void BachelorTest()
        {
            var ledger = new Ledger();

            OU.EnsureCreateAccount(ledger, "Tom.Assets", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity", true);
            OU.EnsureCreateAccount(ledger, "Tom.Liability", true);

            var tomState = AccountsState.GetAccountsState("Tom")!;

            const decimal BaseEquity = 300m;

            // Initial balance setup
            OU.EnsureCreateAccount(ledger, "Tom.Assets.Cash", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Base", false);
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets.Cash", "Tom.Equity.Base", BaseEquity);

            var indexInitial = ledger.Entries.Count;
            Assert.Multiple(() =>
            {
                Assert.That(tomState.Balance, Is.EqualTo(0));
                Assert.That(OU.GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity));
                Assert.That(OU.GetAccount("Tom.Equity.Base")!.Balance, Is.EqualTo(BaseEquity));
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

            ledger.AddAndExecuteTransaction(DateTime.Now, new[] { ("Tom.Assets.Cash", Salary-TaxWithheld), ("Tom.Liability.TaxWithheld", TaxWithheld) },
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
                Assert.That(OU.GetAccount("Tom.Equity.Base")!.Balance, Is.EqualTo(BaseEquity));
            });

            tomInitialBalanceSnapshot.CopyTo(tomState, true); // load snapshot at indexInitial.
            Assert.That(OU.GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity));

            var pnlReports = ReportsGenerator.Run(new[] { new ReportSchemePersonalGeneric(new ReportSchemePersonalGeneric.PersonalDetails("Tom")) }, ledger, indexInitial, indexAfterRealTransactions).ToArray();
            
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
        public void LedgerSerializationTest()
        {
            var ledger = new Ledger();

            OU.EnsureCreateAccount(ledger, "Tom.Assets", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity", true);
            OU.EnsureCreateAccount(ledger, "Tom.Liability", true);

            const decimal BaseEquity = 300m;

            // Initial balance setup
            OU.EnsureCreateAccount(ledger, "Tom.Assets.Cash", false);
            OU.EnsureCreateAccount(ledger, "Tom.Equity.Base", false);
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets.Cash", "Tom.Equity.Base", BaseEquity);

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
                ledger.WriteToStream(sw);
            }
            {
                using var sr = new StreamReader("test.txt");
                var loadedLedger = new Ledger();
                loadedLedger.LoadFromStream(sr);

                Assert.That(loadedLedger, Is.EqualTo(ledger));
            }

            Assert.Pass();
        }
    }
}