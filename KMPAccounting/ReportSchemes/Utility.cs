using KMPAccounting.Objects;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.Reports;
using System.Diagnostics;

namespace KMPAccounting.ReportSchemes
{
    public static class Utility
    {
        public static void InitializeTaxPeriod(AccountsState state, string equityDivisionName)
        {
            var incomeAccount = state.GetAccount("Equity.Income" + equityDivisionName);
            var income = incomeAccount?.Balance ?? 0m;

            var deductionAccount = state.GetAccount("Equity.Deduction" + equityDivisionName);
            var deduction = deductionAccount?.Balance ?? 0m;

            var taxWithheldAccount = state.GetAccount("Liability.TaxWithheld" + equityDivisionName);
            var taxWithheld = deductionAccount?.Balance ?? 0m;

            var taxReturnAccount = state.GetAccount("Liability.TaxReturn" + equityDivisionName);
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

            Objects.Utility.EnsureCreateAccount(state, "Equity.Base" + equityDivisionName, false, null);
            state.GetAccount("Equity.Base" + equityDivisionName)!.Balance += deltaIncome;
        }

        public static void FinalizeTaxPeriodPreTaxCalculation(AccountsState state, string equityDivisionName, PnlReport pnlReport)
        {
            pnlReport.Income = state.GetAccount("Equity.Income" + equityDivisionName)?.Balance ?? 0;
            pnlReport.Deduction = state.GetAccount("Equity.Deduction" + equityDivisionName)?.Balance ?? 0;
            pnlReport.TaxWithheld = state.GetAccount("Liability.TaxWithheld" + equityDivisionName)?.Balance ?? 0;
        }

        public static void FinalizeTaxPeriodPostTaxCalculation(AccountsState state, string equityDivisionName, string taxReturnCashAccount, PnlReport pnlReport)
        {
            state.EnsureCreateAccount("Liability.TaxReturn" + equityDivisionName, false, null);
            state.EnsureCreateAccount(taxReturnCashAccount, false, null);

            state.GetAccount("Liability.TaxReturn" + equityDivisionName)!.Balance = pnlReport.TaxReturn;
            var cash = state.GetAccount(taxReturnCashAccount)!;
            cash.Balance += pnlReport.TaxReturn;

            Debug.Assert(state.Balance == 0);
        }
    }
}
