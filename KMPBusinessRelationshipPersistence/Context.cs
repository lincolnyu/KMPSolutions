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
    }
}
