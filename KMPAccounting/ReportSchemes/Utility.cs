using KMPAccounting.Objects;
using KMPAccounting.Objects.BookKeeping;
using KMPAccounting.Objects.Reports;
using KOU = KMPAccounting.Objects.Utility;
using System.Diagnostics;
using System;
using KMPAccounting.Objects.AccountCreation;

namespace KMPAccounting.ReportSchemes
{
    public static class Utility
    {
        public class AccountsSetup
        {
            public string? TaxReturnCashAccount { get; set; }
            public string? Income { get; set; }
            public string? Expense { get; set; }
            public string? Deduction { get; set; }
            public string? TaxWithheld { get; set; }
            public string? TaxReturn { get; set; }

            /// <summary>
            ///  The equity account that attracts income and deduction
            /// </summary>
            public string? EquityMain { get; set; }

            public static AccountsSetup CreateDefault(string bookName, string divisionName = "")
            {
                var state = (AccountPath)bookName;
                return new AccountsSetup
                {
                    TaxReturnCashAccount = StandardAccounts.GetAccountFullName(state, StandardAccounts.Cash),
                    Income = StandardAccounts.GetAccountFullName(state, StandardAccounts.Income, divisionName),
                    Expense = StandardAccounts.GetAccountFullName(state, StandardAccounts.Expense, divisionName),
                    Deduction = StandardAccounts.GetAccountFullName(state, StandardAccounts.Deduction, divisionName),
                    TaxWithheld = StandardAccounts.GetAccountFullName(state, StandardAccounts.TaxWithheld, divisionName),
                    TaxReturn = StandardAccounts.GetAccountFullName(state, StandardAccounts.TaxReturn, divisionName),
                    EquityMain = StandardAccounts.GetAccountFullName(state, StandardAccounts.EquityMain),    // Assuming equity is equally shared.
                };
            }
        }

        public static void InitializeTaxPeriod(this AccountsSetup accountsSetup)
        {
            // Make sure income and deduction etc. are cleared into equity balance.

            // Debit
            var incomeAccount = KOU.GetAccount(accountsSetup.Income);
            var income = incomeAccount?.Balance ?? 0m;
            incomeAccount?.ZeroOutBalanceOfTree();

            // Tax return usually may be under income, so it may already be zeroed.
            // Debit
            var taxReturnAccount = KOU.GetAccount(accountsSetup.TaxReturn);
            var taxReturn = taxReturnAccount?.Balance ?? 0m;
            taxReturnAccount?.ZeroOutBalanceOfTree();

            // Credit
            var deductionAccount = KOU.GetAccount(accountsSetup.Deduction);
            var deduction = deductionAccount?.Balance ?? 0m;
            deductionAccount?.ZeroOutBalanceOfTree();

            // Credit
            decimal expense = 0;
            if (accountsSetup.Expense != null)
            {
                var expenseAccount = KOU.GetAccount(accountsSetup.Expense);
                expense = expenseAccount?.Balance ?? 0m;
                expenseAccount?.ZeroOutBalanceOfTree();
            }

            // Credit
            var taxWithheldAccount = KOU.GetAccount(accountsSetup.TaxWithheld);
            var taxWithheld = taxWithheldAccount?.Balance ?? 0m;
            taxWithheldAccount?.ZeroOutBalanceOfTree();

            var deltaEquity = income + taxReturn  - expense - deduction  - taxWithheld;

            Ledger? ledger = null;
            ledger.EnsureCreateAccount(accountsSetup.EquityMain, false);

            KOU.GetAccount(accountsSetup.EquityMain)!.Balance += deltaEquity;
        }

        public static void FinalizeTaxPeriodPreTaxCalculation(this AccountsSetup accountsSetup, PnlReport pnlReport)
        {
            pnlReport.Income = KOU.GetAccount(accountsSetup.Income)?.Balance ?? 0;
            pnlReport.Deduction = KOU.GetAccount(accountsSetup.Deduction)?.Balance ?? 0;
            pnlReport.TaxWithheld = KOU.GetAccount(accountsSetup.TaxWithheld)?.Balance ?? 0;
        }

        public static void FinalizeTaxPeriodPostTaxCalculation(this AccountsSetup accountsSetup, PnlReport pnlReport)
        {
            Ledger? ledger = null;
            ledger.EnsureCreateAccount(accountsSetup.TaxReturn, false);
            ledger.EnsureCreateAccount(accountsSetup.TaxReturnCashAccount, false);

            KOU.AddAndExecuteTransaction(ledger, DateTime.Now, accountsSetup.TaxReturnCashAccount, accountsSetup.TaxReturn, pnlReport.TaxReturn);

            Debug.Assert(KOU.GetStateNode(accountsSetup.TaxReturnCashAccount).Balance == 0);
        }
    }
}
