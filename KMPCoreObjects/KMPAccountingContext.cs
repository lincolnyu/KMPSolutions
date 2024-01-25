using KMPAccounting.Objects.Accounts;
using KMPAccounting.Objects.BookKeeping;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace KMPAccounting
{
    public class KMPAccountingContext : DbContext
    {
        public KMPAccountingContext(string connectionString) : base(connectionString)
        {

        }

        public DbSet<Transaction>? Entries { get; set; }
        public DbSet<AccountNode>? Accounts { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
