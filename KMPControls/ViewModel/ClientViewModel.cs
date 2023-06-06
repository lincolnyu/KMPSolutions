using KMPBookingCore.DbObjects;
using System.Collections.ObjectModel;

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
    }
}
