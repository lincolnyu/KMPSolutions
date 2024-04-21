using KMPBusinessRelationship.Objects;
using Microsoft.EntityFrameworkCore;

namespace KMPBusinessRelationship.Persistence
{
    public class Context : DbContext
    {
        public Context()
        {
        }

        public Context(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Referrer> Referrers { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>().HasKey(e => e.Id);
            
            modelBuilder.Entity<Event>().HasDiscriminator<string>("event_type")
                .HasValue<Referral>("event_referral")
                .HasValue<ChangeOfDetails<Client>>("event_change_of_client_details")
                .HasValue<ChangeOfDetails<Referrer>>("event_change_of_referrer_details")
                .HasValue<Invoice>("event_invoice")
                .HasValue<ClaimableService>("event_claimable_service")
                .HasValue<ChargedService>("event_charged_service")
                .HasValue<Booking>("event_booking");

            modelBuilder.Entity<Client>().Ignore("Id").HasKey(e => e.CareNumber);
            modelBuilder.Entity<Referrer>().Ignore("Id").HasKey(e => e.ProviderNumber);

            base.OnModelCreating(modelBuilder);
        }
    }
}
