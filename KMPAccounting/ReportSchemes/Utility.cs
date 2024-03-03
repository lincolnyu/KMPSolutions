using KMPAccounting.Objects;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using KMPAccounting.Objects.Reports;
using System.Diagnostics;

namespace KMPAccounting.ReportSchemes
{
    public static class Utility
    {
        public class AccountsSetup
        {
            public string IncomeBaseNode { get; set; } = Constants.Income;
            public string DeductionBaseNode { get; set; } = Constants.Deduction;
            public string TaxWithheldBaseNode { get; set; } = Constants.TaxWithheld;
            public string TaxReturnBaseNode { get; set; } = Constants.TaxReturn;
            public string EquityMainNode { get; set; } = Constants.EquityMain;

            public static readonly AccountsSetup Default = new AccountsSetup();
        }

        public static void InitializeTaxPeriod(AccountsState state, string equityDivisionName, AccountsSetup? accountsSetup = null)
        {
            if (accountsSetup == null) 
            { 
                accountsSetup = AccountsSetup.Default;
            }

            var incomeAccount = state.GetAccount(accountsSetup.IncomeBaseNode + equityDivisionName);
            var income = incomeAccount?.Balance ?? 0m;

            var deductionAccount = state.GetAccount(accountsSetup.DeductionBaseNode + equityDivisionName);
            var deduction = deductionAccount?.Balance ?? 0m;

            var taxWithheldAccount = state.GetAccount(accountsSetup.TaxWithheldBaseNode + equityDivisionName);
            var taxWithheld = deductionAccount?.Balance ?? 0m;

            var taxReturnAccount = state.GetAccount(accountsSetup.TaxReturnBaseNode  + equityDivisionName);
            var taxReturn = deductionAccount?.Balance ?? 0m;

            // Debit
            incomeAccount?.ZeroOutBalanceOfTree();
            
            // Credit
            deductionAccount?.ZeroOutBalanceOfTree();

            // Credit
            taxWithheldAccount?.ZeroOutBalanceOfTree();

            // Debit
            taxReturnAccount?.ZeroOutBalanceOfTree();

            var deltaIncome = income - deduction + taxReturn - taxWithheld;

            Ledger? ledger = null;
            ledger.EnsureCreateAccount(state, accountsSetup.EquityMainNode + equityDivisionName, false);

            state.GetAccount(accountsSetup.EquityMainNode + equityDivisionName)!.Balance += deltaIncome;
        }

        public static void FinalizeTaxPeriodPreTaxCalculation(AccountsState state, string equityDivisionName, PnlReport pnlReport, AccountsSetup? accountsSetup = null)
        {
            if (accountsSetup == null)
            {
                accountsSetup = AccountsSetup.Default;
            }
            
            pnlReport.Income = state.GetAccount(accountsSetup.IncomeBaseNode + equityDivisionName)?.Balance ?? 0;
            pnlReport.Deduction = state.GetAccount(accountsSetup.DeductionBaseNode + equityDivisionName)?.Balance ?? 0;
            pnlReport.TaxWithheld = state.GetAccount(accountsSetup.TaxWithheldBaseNode + equityDivisionName)?.Balance ?? 0;
        }

        public static void FinalizeTaxPeriodPostTaxCalculation(AccountsState state, string equityDivisionName, string taxReturnCashAccount, PnlReport pnlReport, AccountsSetup? accountsSetup = null)
        {
            if (accountsSetup == null)
            {
                accountsSetup = AccountsSetup.Default;
            }

            Ledger? ledger = null;
            ledger.EnsureCreateAccount(state, accountsSetup.TaxReturnBaseNode + equityDivisionName, false);
            ledger.EnsureCreateAccount(state, taxReturnCashAccount, false);

            state.GetAccount(accountsSetup.TaxReturnBaseNode + equityDivisionName)!.Balance = pnlReport.TaxReturn;
            var cash = state.GetAccount(taxReturnCashAccount)!;
            cash.Balance += pnlReport.TaxReturn;

            Debug.Assert(state.Balance == 0);
        }
    }
}
