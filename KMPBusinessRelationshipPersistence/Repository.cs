using KMPBusinessRelationship;
using KMPBusinessRelationship.Objects;

namespace KMPBusinessRelationshipPersistence
{
    public class Repository : BaseRepository
    {
        public override IEnumerable<Person> Persons => persons_;

        public override IEnumerable<Event> Events => events_;

        private HashSet<Person> persons_ = new HashSet<Person>();
        private List<Event> events_ = new List<Event>();

        public Repository()
        {
        }

        public Repository(Context context)
        {
            DbContext = context;
            SyncDbToCache();
        }

        private void SyncDbToCache()
        {
            foreach (var e in DbContext!.Events)
            {
                events_.Add(e);
            }
            foreach (var p in DbContext!.Persons)
            {
                persons_.Add(p);
            }
        }

        public Context? DbContext { get; }

        protected override void AddEvent(Event e)
        {
            if (DbContext != null)
            {
                DbContext.Events.Add(e);
            }
            events_.Add(e);
        }

        protected override void AddPerson(Person person)
        {
            if (DbContext != null)
            {
                DbContext.Persons.Add(person);
            }
            persons_.Add(person);
        }
    }
}
