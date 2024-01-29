using KMPAccounting.Objects.BookKeeping;
using KMPAccounting.Objects.Reports;
using System.Collections.Generic;

namespace KMPAccounting.Accounting
{
    public static class ReportsGenerator
    {
        public static IEnumerable<PnlReport> Run(ICollection<ReportSchemeBase> reportSchemes, Ledger ledger, int startingIndexInclusive, int endingIndexExclusive)
        {
            foreach (var reportScheme in reportSchemes)
            {
                reportScheme.Initialize();
            }

            ledger.Execute(startingIndexInclusive, endingIndexExclusive);

            foreach (var reportScheme in reportSchemes)
            {
                foreach (var reportPair in reportScheme.Finalize())
                {
                    yield return reportPair;
                }
            }
        }
    }
}
