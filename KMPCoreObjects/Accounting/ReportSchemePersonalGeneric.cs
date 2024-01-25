using KMPAccounting.Objects;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.Reports;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KMPAccounting.Accounting
{
    public class ReportSchemePersonalGeneric : ReportSchemeBase
    {
        public class PersonalDetails
        {
            /// <summary>
            ///  Constructing the object
            /// </summary>
            /// <param name="statesName">Name of the accounts state of the person</param>
            /// <param name="taxReturnCashAccount">Suffix following Assets.Cash in the name of the account for tax return, with leading dot.</param>
            public PersonalDetails(string statesName, string taxReturnCashAccount = "")
            {
                StatesName = statesName;
                TaxReturnCashAccount = taxReturnCashAccount;
            }

            public string StatesName { get; }
            public string TaxReturnCashAccount { get; }
        }

        public ReportSchemePersonalGeneric(PersonalDetails selfDetails, PersonalDetails? partnerDetails = null)
        {
            selfDetails_ = selfDetails;
            partnerDetails_ = partnerDetails;

            selfState_ = AccountsState.GetAccountsState(selfDetails.StatesName)!;
            partnerState_ = partnerDetails != null? AccountsState.GetAccountsState(partnerDetails.StatesName)! : null;
        }

        public override void Initialize()
        {
            // Debit
            selfState_.GetAccount("Equity.Income")?.ZeroOutBalanceOfTree();
            // Credit
            selfState_.GetAccount("Equity.Deduction")?.ZeroOutBalanceOfTree();
            // Credit
            selfState_.GetAccount("Liability.TaxWithheld")?.ZeroOutBalanceOfTree();

            if (partnerState_ != null)
            {
                partnerState_.GetAccount("Equity.Income")?.ZeroOutBalanceOfTree();
                partnerState_.GetAccount("Equity.Deduction")?.ZeroOutBalanceOfTree();
                partnerState_.GetAccount("Liability.TaxWithheld")?.ZeroOutBalanceOfTree();
            }
        }

        public override IEnumerable<PnlReport> Finalize()
        {
            var pnlReport = new PnlReport
            {
                Income = selfState_.GetAccount("Equity.Income")?.Balance ?? 0,
                Deduction = selfState_.GetAccount("Equity.Deduction")?.Balance ?? 0,
                TaxWithheld = selfState_.GetAccount("Liability.TaxWithheld")?.Balance ?? 0
            };

            var taxReturnCashAccount = "Assets.Cash" + selfDetails_.TaxReturnCashAccount;

            selfState_.EnsureCreateAccount("Liability.TaxReturn", false, null);
            selfState_.EnsureCreateAccount(taxReturnCashAccount, false, null);

            if (partnerState_ != null)
            {
                var partnerPnlReport = new PnlReport
                {
                    Income = partnerState_.GetAccount("Equity.Income")?.Balance ?? 0,
                    Deduction = partnerState_.GetAccount("Equity.Deduction")?.Balance ?? 0,
                    TaxWithheld = partnerState_.GetAccount("Liability.TaxWithheld")?.Balance ?? 0
                };

                var (tax, partnerTax) = GetFamilyTax(pnlReport.TaxableIncome, partnerPnlReport.TaxableIncome);
                pnlReport.Tax = tax;
                partnerPnlReport.Tax = partnerTax;

                var partnerTaxReturnCashAccount = "Assets.Cash" + partnerDetails_!.TaxReturnCashAccount;

                partnerState_.EnsureCreateAccount("Liability.TaxReturn", false, null);
                partnerState_.EnsureCreateAccount(partnerTaxReturnCashAccount, false, null);

                Debug.Assert(selfState_.Balance == 0);
                Debug.Assert(partnerState_.Balance == 0);

                // Simulate tax return
                selfState_.GetAccount("Liability.TaxReturn")!.Balance = pnlReport.TaxReturn;
                var cash = selfState_.GetAccount(taxReturnCashAccount)!;
                cash.Balance += pnlReport.TaxReturn;

                partnerState_.GetAccount("Liability.TaxReturn")!.Balance = pnlReport.TaxReturn;
                var partnerCash = partnerState_.GetAccount(partnerTaxReturnCashAccount)!;
                partnerCash.Balance += pnlReport.TaxReturn;

                Debug.Assert(selfState_.Balance == 0);
                Debug.Assert(partnerState_.Balance == 0);

                yield return (pnlReport);
                yield return (partnerPnlReport);
            }
            else
            {
                pnlReport.Tax = GetPersonalTax(pnlReport.TaxableIncome);

                Debug.Assert(selfState_.Balance == 0);

                // Simulate tax return
                selfState_.GetAccount("Liability.TaxReturn")!.Balance = pnlReport.TaxReturn;
                var cash = selfState_.GetAccount(taxReturnCashAccount)!;
                cash.Balance += pnlReport.TaxReturn;

                Debug.Assert(selfState_.Balance == 0);

                yield return (pnlReport);
            }
        }

        protected virtual decimal GetPersonalTax(decimal taxableIncome) => GetPersonalTaxDefault(taxableIncome);

        public static decimal GetPersonalTaxDefault(decimal taxableIncome)
        {
            const decimal Bracket0UpperLimit = 18200;
            const decimal Bracket1UpperLimit = 45000;
            const decimal Bracket2UpperLimit = 120000;
            const decimal Bracket3UpperLimit = 180000;
            if (taxableIncome <= Bracket0UpperLimit)
            {
                return 0m;
            }
            else if (taxableIncome <= Bracket1UpperLimit)
            {
                return (taxableIncome - Bracket0UpperLimit) * 0.19m;
            }
            else if (taxableIncome <= Bracket2UpperLimit)
            {
                return 5092m + (taxableIncome - Bracket1UpperLimit) * 0.325m;
            }
            else if (taxableIncome <= Bracket3UpperLimit)
            {
                return 29467m + (taxableIncome - Bracket2UpperLimit) * 0.37m;
            }
            else
            {
                return 51667m + (taxableIncome - Bracket3UpperLimit) * 0.45m;
            }
        }

        protected virtual (decimal, decimal) GetFamilyTax(decimal taxableIncome1, decimal taxableIncome2)
        {
            throw new NotImplementedException();
        }

        private PersonalDetails selfDetails_;
        private AccountsState selfState_;

        private PersonalDetails? partnerDetails_;
        private AccountsState? partnerState_;
    }
}
