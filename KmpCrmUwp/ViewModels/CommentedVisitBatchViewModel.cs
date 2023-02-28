using KmpCrmCore;
using System.Collections.ObjectModel;
using System.Linq;

namespace KmpCrmUwp.ViewModels
{
    internal class CommentedVisitBatchViewModel : BaseViewModel<CommentedValue<VisitBatch>>
    {
        public CommentedVisitBatchViewModel(CommentedValue<VisitBatch> model) : base(model)
        {
            Events = new ObservableCollection<EventViewModel>();
            var events = Model.Value.VisitsMade.Select(x => new EventViewModel(x) { Type = EventViewModel.EventType.Visit }).Concat(Model.Value.ClaimsMade.Select(x => new EventViewModel(x) { Type = EventViewModel.EventType.Claim })).OrderBy(x=>x.Model.Value.Date);
            foreach (var e in events)
            {
                Events.Add(e);
            }
            Events.CollectionChanged += Events_CollectionChanged;
        }

        public string Comments
        {
            get { return Model.Comments; }
            set { Model.Comments = value; }
        }

        public ObservableCollection<EventViewModel> Events { get; set; }

        private void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch(e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    break;
            }
        }
    }
}

