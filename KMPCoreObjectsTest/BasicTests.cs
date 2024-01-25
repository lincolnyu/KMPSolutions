using KMPAccounting.Objects;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using KMPAccounting.Accounting;
using static KMPAccounting.Objects.Utility;

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

            Utility.EnsureCreateAccount(ledger, "Tom.Assets", false);
            Utility.EnsureCreateAccount(ledger, "Tom.Equity", true);
            Utility.EnsureCreateAccount(ledger, "Tom.Liability", true);

            var tomState = AccountsState.GetAccountsState("Tom")!;

            const decimal BaseEquity = 300m;

            // Initial balance setup
            Utility.EnsureCreateAccount(ledger, "Tom.Assets.Cash", false);
            Utility.EnsureCreateAccount(ledger, "Tom.Equity.Base", false);
            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Assets.Cash", "Tom.Equity.Base", BaseEquity);

            var indexInitial = ledger.Entries.Count;

            Assert.That(tomState.Balance, Is.EqualTo(0));
            Assert.That(GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity));
            Assert.That(GetAccount("Tom.Equity.Base")!.Balance, Is.EqualTo(BaseEquity));

            var tomInitialBalanceSnapshot = new AccountsState("Tom");
            tomState.CopyTo(tomInitialBalanceSnapshot, true);

            // Real transactions
            Utility.EnsureCreateAccount(ledger, "Tom.Liability.TaxWithheld", true);
            Utility.EnsureCreateAccount(ledger, "Tom.Equity.Income.Salary", false);

            const decimal Salary = 25000m;
            const decimal TaxWithheld = 2000m;
            const decimal Expense = 8000m;
            const decimal Deduction = 6000m;

            ledger.AddAndExecuteTransaction(DateTime.Now, new[] { ("Tom.Assets.Cash", Salary-TaxWithheld), ("Tom.Liability.TaxWithheld", TaxWithheld) },
                new[] { ("Tom.Equity.Income.Salary", Salary) });

            Utility.EnsureCreateAccount(ledger, "Tom.Equity.Expense", true);
            Utility.EnsureCreateAccount(ledger, "Tom.Equity.Deduction", true);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Expense", "Tom.Assets.Cash", Expense);

            ledger.AddAndExecuteTransaction(DateTime.Now, "Tom.Equity.Deduction", "Tom.Assets.Cash", Deduction);

            var indexAfterRealTransactions = ledger.Entries.Count;

            Assert.Multiple(() =>
            {
                Assert.That(tomState.Balance, Is.EqualTo(0m));

                Assert.That(GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity + Salary - Expense - Deduction - TaxWithheld));
                Assert.That(GetAccount("Tom.Liability.TaxWithheld")!.Balance, Is.EqualTo(TaxWithheld));

                Assert.That(GetAccount("Tom.Equity.Income.Salary")!.Balance, Is.EqualTo(Salary));
                Assert.That(GetAccount("Tom.Equity.Base")!.Balance, Is.EqualTo(BaseEquity));
            });

            tomInitialBalanceSnapshot.CopyTo(tomState, true); // load snapshot at indexInitial.
            Assert.That(GetAccount("Tom.Assets.Cash")!.Balance, Is.EqualTo(BaseEquity));

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
    }
}