using KMPAccounting.Accounting;
using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.Reports;
using System.Collections.Generic;

namespace KMPAccounting.ReportSchemes
{
    public class ReportSchemeBusinessGeneric : ReportSchemeBase
    {
        public class BusinessDetails
        {
            /// <summary>
            ///  Constructing the object
            /// </summary>
            /// <param name="stateName">Name of the accounts state of the business.</param>
            /// <param name="taxReturnCashAccount">Suffix following Assets.Cash in the name of the account for tax return, with leading dot.</param>
            public BusinessDetails(string stateName, string taxReturnCashAccount = "")
            {
                StateName = stateName;
                TaxReturnCashAccount = taxReturnCashAccount;
            }

            public string StateName { get; }
            public string TaxReturnCashAccount { get; }
        }

        public ReportSchemeBusinessGeneric(BusinessDetails details)
        {
            details_ = details;
            state_ = AccountsState.GetAccountsState(details.StateName)!;
        }

        public override void Initialize()
        {
            Utility.InitializeTaxPeriod(state_, "");
        }

        public override IEnumerable<PnlReport> Finalize()
        {
            var pnlReport = new PnlReport();
            Utility.FinalizeTaxPeriodPreTaxCalculation(state_, "", pnlReport);

            pnlReport.Tax = GetBusinessTax(pnlReport.TaxableIncome);

            Utility.FinalizeTaxPeriodPostTaxCalculation(state_, "", details_.TaxReturnCashAccount, pnlReport);
            yield return pnlReport;
        }

        protected virtual decimal GetBusinessTax(decimal taxableIncome) => GetBusinessTaxDefault(taxableIncome);

        private decimal GetBusinessTaxDefault(decimal taxableIncome)
        {
            const decimal FullRate = 0.3m;
            const decimal LowerRate = 0.275m;
            // TODO Update this to the correct.
            return taxableIncome * LowerRate;
        }

        private readonly BusinessDetails details_;
        private readonly AccountsState state_;
    }
}
