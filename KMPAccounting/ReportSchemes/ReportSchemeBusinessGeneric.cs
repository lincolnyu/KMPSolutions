using KMPAccounting.Accounting;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.Reports;
using System;
using System.Collections.Generic;
using static KMPAccounting.ReportSchemes.Utility;

namespace KMPAccounting.ReportSchemes
{
    public class ReportSchemeBusinessGeneric : ReportSchemeBase
    {
        public class BusinessDetails
        {
            /// <summary>
            ///  Constructing the object
            /// </summary>
            /// <param name="accountsSetup">Name of the accounts state of the business.</param>
            public BusinessDetails(AccountsSetup accountsSetup)
            {
                AccountsSetup = accountsSetup;
            }

            public AccountsSetup AccountsSetup { get; }
        }

        public ReportSchemeBusinessGeneric(BusinessDetails details)
        {
            details_ = details;
        }

        public override void Initialize()
        {
            details_.AccountsSetup.InitializeTaxPeriod();
        }

        public override IEnumerable<PnlReport> Finalize()
        {
            var pnlReport = new PnlReport();

            details_.AccountsSetup.FinalizeTaxPeriodPreTaxCalculation(pnlReport);

            pnlReport.Tax = GetBusinessTax(pnlReport.TaxableIncome);

            details_.AccountsSetup.FinalizeTaxPeriodPostTaxCalculation(pnlReport);

            yield return pnlReport;
        }

        protected virtual decimal GetBusinessTax(decimal taxableIncome) => GetBusinessTaxDefault(taxableIncome);

        private decimal GetBusinessTaxDefault(decimal taxableIncome)
        {
            if (taxableIncome < 0) return 0;
            //const decimal FullRate = 0.3m;
            const decimal LowerRate = 0.275m;
            // TODO Update this to the correct.
            return taxableIncome * LowerRate;
        }

        private readonly BusinessDetails details_;
        private readonly AccountsState state_;
    }
}
