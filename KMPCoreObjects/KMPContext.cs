using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace KMPCoreObjects
{
    public class KMPContext : DbContext
    {
        public KMPContext(string connectionString) : base(connectionString)
        {

        }

        public DbSet<Client>? Clients { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
    }
}
