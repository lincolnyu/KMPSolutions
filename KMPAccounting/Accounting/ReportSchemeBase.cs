using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.Reports;
using System.Collections.Generic;

namespace KMPAccounting.Accounting
{
    public abstract class ReportSchemeBase
    {
        private AccountsState state_;
        public abstract void Initialize();
        public abstract IEnumerable<PnlReport> Finalize();
    }
}
