﻿using KMPAccounting.Accounting;
using KMPAccounting.Objects.Reports;
using System.Collections.Generic;
using static KMPAccounting.ReportSchemes.Utility;

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
            public PersonalDetails(AccountsSetup accountsSetup)
            {
                AccountsSetup = accountsSetup;
            }

            public AccountsSetup AccountsSetup { get; }

            public decimal NetIncomeAdjustment { get; set; } = 0m;

            /// <summary>
            ///  Post income tax calculation adjustent, such as medicare levy etc.
            /// </summary>
            public decimal TaxAdjustment { get; internal set; } = 0m;
        }

        public ReportSchemePersonalGeneric(PersonalDetails selfDetails, PersonalDetails? partnerDetails = null)
        {
            SelfDetails = selfDetails;
            PartnerDetails = partnerDetails;
        }

        public override void Initialize()
        {
            SelfDetails.AccountsSetup.InitializeTaxPeriod();

            if (PartnerDetails != null)
            {
                PartnerDetails.AccountsSetup.InitializeTaxPeriod();
            }
        }

        public override IEnumerable<PnlReport> Finalize()
        {
            var pnlReport = new PnlReport();
            PnlReport? partnerPnlReport = null;

            SelfDetails.AccountsSetup.FinalizeTaxPeriodPreTaxCalculation(pnlReport, SelfDetails.NetIncomeAdjustment);

            if (PartnerDetails != null)
            {
                partnerPnlReport = new PnlReport();

                PartnerDetails.AccountsSetup.FinalizeTaxPeriodPreTaxCalculation(partnerPnlReport, PartnerDetails.NetIncomeAdjustment);

                var (tax, partnerTax) = GetFamilyTax(pnlReport.TaxableIncome, partnerPnlReport.TaxableIncome);

                pnlReport.Tax = tax;
                partnerPnlReport.Tax = partnerTax;

                pnlReport.Tax += SelfDetails.TaxAdjustment;
                partnerPnlReport.Tax += PartnerDetails.TaxAdjustment;
            }
            else
            {
                pnlReport.Tax = GetPersonalTax(pnlReport.TaxableIncome);
                pnlReport.Tax += SelfDetails.TaxAdjustment;
            }

            SelfDetails.AccountsSetup.FinalizeTaxPeriodPostTaxCalculation(pnlReport);

            yield return pnlReport;

            if (partnerPnlReport != null)
            {
                PartnerDetails!.AccountsSetup.FinalizeTaxPeriodPostTaxCalculation(partnerPnlReport);
               
                yield return partnerPnlReport;
            }    
        }

        protected virtual decimal GetPersonalTax(decimal taxableIncome) => GetPersonalTaxDefault(taxableIncome);

        protected virtual (decimal, decimal) GetFamilyTax(decimal taxableIncome1, decimal taxableIncome2)
        {
            // TODO Find out the real family tax policy
            var tax1 = GetPersonalTax(taxableIncome1);
            var tax2 = GetPersonalTax(taxableIncome2);
            return (tax1, tax2);
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

        /// <summary>
        ///  Stage 3 Tax cut
        /// </summary>
        /// <param name="taxableIncome">Taxable income</param>
        /// <returns>Tax paiable</returns>
        /// <remarks>
        /// https://www.etax.com.au/stage-3-tax-cuts-explained/?utm_source=taxtipsmar2024&utm_medium=email&utm_campaign=taxtips0324-seg2-5397002&sc_src=email_5397002&sc_lid=372240647&sc_uid=YP8Nz3SJbL&sc_llid=655882&sc_eh=99de70f40385c2701
        /// </remarks>
        public static decimal GetPersonalTax_FY2024Stage3Cut(decimal taxableIncome)
        {
            const decimal Bracket0UpperLimit = 18200;
            const decimal Bracket1UpperLimit = 45000;
            const decimal Bracket2UpperLimit = 135000;  // changed
            const decimal Bracket3UpperLimit = 180000;

            const decimal Bracket1Rate = 0.16m; // changed
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

        public PersonalDetails SelfDetails { get; }

        public PersonalDetails? PartnerDetails { get; }
    }
}
