using KMPAccounting.Accounting;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.Reports;
using System;
using System.Collections.Generic;

namespace KMPAccounting.ReportSchemes
{
    public class ReportSchemePersonalGeneric : ReportSchemeBase
    {
        public class PersonalDetails
        {
            /// <summary>
            ///  Constructing the object
            /// </summary>
            /// <param name="stateName">Name of the accounts state of the person.</param>
            /// <param name="taxReturnCashAccount">Suffix following Assets.Cash in the name of the account for tax return, with leading dot.</param>
            /// <param name="equityDivisionName">A nonempty substring in the equity account for the person of a couple share the same accounts state, with leading dot.</param>
            public PersonalDetails(string stateName, string taxReturnCashAccount = "", string equityDivisionName = "")
            {
                StateName = stateName;
                TaxReturnCashAccount = taxReturnCashAccount;
                EquityDivisionName = equityDivisionName;
            }

            public string StateName { get; }
            public string TaxReturnCashAccount { get; }
            public string EquityDivisionName { get; }
        }

        public ReportSchemePersonalGeneric(PersonalDetails selfDetails, PersonalDetails? partnerDetails = null)
        {
            selfDetails_ = selfDetails;
            partnerDetails_ = partnerDetails;

            selfState_ = AccountsState.GetAccountsState(selfDetails.StateName)!;
            partnerState_ = partnerDetails != null ? AccountsState.GetAccountsState(partnerDetails.StateName)! : null;
        }

        public override void Initialize()
        {
            Utility.InitializeTaxPeriod(selfState_, selfDetails_.EquityDivisionName);

            if (partnerState_ != null)
            {
                Utility.InitializeTaxPeriod(partnerState_, partnerDetails_!.EquityDivisionName);
            }
        }

        public override IEnumerable<PnlReport> Finalize()
        {
            var pnlReport = new PnlReport();
            PnlReport? partnerPnlReport = null;

            Utility.FinalizeTaxPeriodPreTaxCalculation(selfState_, selfDetails_.EquityDivisionName, pnlReport);

            var taxReturnCashAccount = "Assets.Cash" + selfDetails_.TaxReturnCashAccount;

            if (partnerState_ != null)
            {
                partnerPnlReport = new PnlReport();

                Utility.FinalizeTaxPeriodPreTaxCalculation(partnerState_, partnerDetails_!.EquityDivisionName, partnerPnlReport);

                var (tax, partnerTax) = GetFamilyTax(pnlReport.TaxableIncome, partnerPnlReport.TaxableIncome);

                pnlReport.Tax = tax;
                partnerPnlReport.Tax = partnerTax;
            }
            else
            {
                pnlReport.Tax = GetPersonalTax(pnlReport.TaxableIncome);
            }

            Utility.FinalizeTaxPeriodPostTaxCalculation(selfState_, selfDetails_.EquityDivisionName, taxReturnCashAccount, pnlReport);

            yield return pnlReport;

            if (partnerPnlReport != null)
            {
                var partnerTaxReturnCashAccount = "Assets.Cash" + partnerDetails_!.TaxReturnCashAccount;

                Utility.FinalizeTaxPeriodPostTaxCalculation(partnerState_!, partnerDetails_.EquityDivisionName, partnerTaxReturnCashAccount, partnerPnlReport);

                yield return partnerPnlReport;
            }    
        }

        protected virtual decimal GetPersonalTax(decimal taxableIncome) => GetPersonalTaxDefault(taxableIncome);

        protected virtual (decimal, decimal) GetFamilyTax(decimal taxableIncome1, decimal taxableIncome2)
        {
            throw new NotImplementedException();
        }

        public static decimal GetPersonalTaxDefault(decimal taxableIncome)
        {
            const decimal Bracket0UpperLimit = 18200;
            const decimal Bracket1UpperLimit = 45000;
            const decimal Bracket2UpperLimit = 120000;
            const decimal Bracket3UpperLimit = 180000;

            const decimal Bracket1Rate = 0.19m;
            const decimal Bracket2Rate = 0.325m;
            const decimal Bracket3Rate = 0.37m;
            const decimal Bracket4Rate = 0.45m;

            if (taxableIncome <= Bracket0UpperLimit)
            {
                return 0m;
            }
            else if (taxableIncome <= Bracket1UpperLimit)
            {
                return (taxableIncome - Bracket0UpperLimit) * Bracket1Rate;
            }
            else if (taxableIncome <= Bracket2UpperLimit)
            {
                return 5092m + (taxableIncome - Bracket1UpperLimit) * Bracket2Rate;
            }
            else if (taxableIncome <= Bracket3UpperLimit)
            {
                return 29467m + (taxableIncome - Bracket2UpperLimit) * Bracket3Rate;
            }
            else
            {
                return 51667m + (taxableIncome - Bracket3UpperLimit) * Bracket4Rate;
            }
        }

        private readonly PersonalDetails selfDetails_;
        private readonly AccountsState selfState_;

        private readonly PersonalDetails? partnerDetails_;
        private readonly AccountsState? partnerState_;
    }
}
