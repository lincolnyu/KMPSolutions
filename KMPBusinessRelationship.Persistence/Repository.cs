using KMPBusinessRelationship;
using KMPBusinessRelationship.Objects;

namespace KMPBusinessRelationship.Persistence
{
    public class Repository : BaseRepository
    {
        public override IEnumerable<Client> Clients => clients_;
        public override IEnumerable<Referrer> Referrers => referrers_;
        public override IEnumerable<Event> Events => events_;

        private HashSet<Client> clients_ = new HashSet<Client>();
        private HashSet<Referrer> referrers_ = new HashSet<Referrer>();
        private List<Event> events_ = new List<Event>();

        public Repository()
        {
        }

        public Repository(Context context)
        {
            DbContext = context;
            SyncCacheFromDatabase();
        }

        private void SyncCacheFromDatabase()
        {
            foreach (var e in DbContext!.Events)
            {
                events_.Add(e);
            }
            foreach (var client in DbContext!.Clients)
            {
                clients_.Add(client);
            }
            foreach (var referrer in DbContext!.Referrers)
            {
                referrers_.Add(referrer);
            }
        }

        public Context? DbContext { get; }

        protected override void AddEvent(Event e)
        {
            DbContext?.Events.Add(e);
            events_.Add(e);
        }

        protected override void AddReferrer(Referrer referrer)
        {
            DbContext?.Referrers.Add(referrer);
            referrers_.Add(referrer);
        }

        protected override void AddClient(Client client)
        {
            DbContext?.Clients.Add(client);
            clients_.Add(client);
        }
    }
}
