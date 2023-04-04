using KmpCrmCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace KmpCrmUwp.ViewModels
{
    internal class CommentedVisitBatchViewModel : BaseVisitBatchViewModel
    {
        public CommentedVisitBatchViewModel(CommentedValue<VisitBatch> model, CustomerViewModel parent) : base(model)
        {
            Parent = parent;
            Events = new ObservableCollection<BaseEventViewModel>();
            ReloadEventsFromSource();
            Events.CollectionChanged += Events_CollectionChanged;
        }

        public CommentedVisitBatchViewModel(CustomerViewModel parent) : this(new CommentedValue<VisitBatch>(new VisitBatch()), parent)
        {
        }

        private void Event_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Type")
            {
                // Just do a reload
                UpdateEventsToSource();
            }
        }

        public CustomerViewModel Parent { get; }

        public string Comments
        {
            get { return Model.Comments; }
            set { Model.Comments = value; }
        }

        public ObservableCollection<BaseEventViewModel> Events { get; set; }

        private void ReloadEventsFromSource()
        {
            Events.Clear();
            var events = Model.Value.VisitsMade.Select(x => new EventViewModel(x, this) { Type = EventViewModel.EventType.Visit }).Concat(Model.Value.ClaimsMade.Select(x => new EventViewModel(x, this) { Type = EventViewModel.EventType.Claim })).OrderBy(x => x.Model.Value.Date);
            foreach (var e in events)
            {
                Events.Add(e);
                e.PropertyChanged += Event_PropertyChanged;
            }
            Events.Add(new AddEventViewModel(this));
        }

        private void Events_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private void UpdateEventsToSource()
        {
            Model.Value.VisitsMade.Clear();
            Model.Value.ClaimsMade.Clear();
            var visits = Model.Value.VisitsMade;
            var claims = Model.Value.ClaimsMade;
            foreach (var re in Events)
            {
                if (re is EventViewModel e)
                {
                    switch (e.Type)
                    {
                        case EventViewModel.EventType.Visit:
                            visits.Add(e.Model);
                            break;
                        case EventViewModel.EventType.Claim:
                            claims.Add(e.Model);
                            break;
                        default:
                            throw new ArgumentException("Unexpected event type");
                    }
                }
            }
            var comp = new Comparison<CommentedValue<DateTime>>((x, y) => { return x.Value.Date.CompareTo(y.Value.Date); });
            visits.Sort(comp);
            claims.Sort(comp);
        }

        internal void AddEmptyVisit()
        {
            Model.Value.VisitsMade.Add(new CommentedValue<DateTime>(DateTime.Now));
            ReloadEventsFromSource();
        }

        internal void AddEmptyClaim()
        {
            Model.Value.ClaimsMade.Add(new CommentedValue<DateTime>(DateTime.Now));
            ReloadEventsFromSource();
        }

        internal void RemoveEvent(EventViewModel eventVm)
        {
            if (eventVm.Type == EventViewModel.EventType.Visit)
            {
                Model.Value.VisitsMade.Remove(eventVm.Model);
            }
            else
            {
                Model.Value.ClaimsMade.Remove(eventVm.Model);
            }
            ReloadEventsFromSource();
        }

        internal void RemoveSelf()
        {
            Parent.RemoveVisitBatch(this);
        }
    }
}

