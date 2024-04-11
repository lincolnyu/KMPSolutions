using KMPBusinessRelationship;
using KMPBusinessRelationship.Objects;

namespace KMPBusinessRelationshipPersistence
{
    public class Repository : BaseRepository
    {
        public override IEnumerable<Person> Persons => DbContext != null? DbContext.Persons : persons_;

        public override IEnumerable<Event> Events => DbContext != null? DbContext.Events : events_;

        private HashSet<Person> persons_ = new HashSet<Person>();
        private List<Event> events_ = new List<Event>();

        Context? DbContext { get; }

        protected override void AddEvent(Event e)
        {
            if (DbContext != null)
            {
                DbContext.Events.Add(e);
            }
            else
            {
                events_.Add(e);
            }
        }

        protected override void AddPerson(Person person)
        {
            if (DbContext != null)
            {
                DbContext.Persons.Add(person);
            }
            else
            { 
                persons_.Add(person);
            }
        }
    }
}
