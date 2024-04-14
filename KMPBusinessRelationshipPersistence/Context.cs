using KMPBusinessRelationship.Objects;
using Microsoft.EntityFrameworkCore;

namespace KMPBusinessRelationshipPersistence
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

        public DbSet<Person> Persons { get; set; }
        public DbSet<Event> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>().HasKey(e => e.Id);
            
            modelBuilder.Entity<Event>().HasDiscriminator<string>("event_type").HasValue<Referral>("event_referal").HasValue<ChangeOfDetails>("event_change_of_details").HasValue<Service>("event_service").HasValue<Booking>("booking");

            modelBuilder.Entity<Person>().HasKey(e => e.Id);

            modelBuilder.Entity<Person>().HasDiscriminator<string>("person_type").HasValue<GeneralPractitioner>("person_gp").HasValue<Client>("person_client");

            base.OnModelCreating(modelBuilder);
        }
    }
}
