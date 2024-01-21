using KMPBookingCore.DbObjects;
using System.Collections.ObjectModel;
using System.Linq;

namespace KMPControls.ViewModel
{
    internal class ClientViewModel : BaseViewModel<Client>
    {
        public ClientViewModel(Client model) : base(model)
        {
            PopulateEvents();
            Events.CollectionChanged += Events_CollectionChanged;
        }

        private void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Model != null)
            {
                Model.Events.Clear();
                foreach (var ev in Events)
                {
                    Model.Events.Add(ev.Model);
                }
            }
        }

        public ObservableCollection<EventViewModel> Events { get; } = new ObservableCollection<EventViewModel>();

        private void PopulateEvents()
        {
            Events.Clear();
            if (Model != null)
            {
                foreach (var e in Model.Events)
                {
                    Events.Add(new EventViewModel(e));
                }
            }
        }

        public void Replace(EventViewModel toBeReplaced, EventViewModel replacement)
        {
            for (var i = 0; i < Events.Count; i++)
            {
                var ev = Events[i];
                if (ev ==  toBeReplaced)
                {
                    Events.RemoveAt(i);
                    Events.Insert(i, replacement);
                    break;
                }
            }
        }
    }
}
