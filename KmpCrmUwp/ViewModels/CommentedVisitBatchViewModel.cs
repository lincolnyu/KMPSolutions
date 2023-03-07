using KmpCrmCore;
using System.Collections.Generic;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

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
                e.PropertyChanged += Event_PropertyChanged;
            }
            Events.CollectionChanged += Events_CollectionChanged;
        }

        private void Event_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Type")
            {
                // Just do a reload
                UpdateAllToSource();
            }
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

        private void UpdateAllToSource()
        {
            Model.Value.VisitsMade.Clear();
            Model.Value.ClaimsMade.Clear();
            var visits = Model.Value.VisitsMade;
            var claims = Model.Value.ClaimsMade;
            foreach (var e in Events)
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
            var comp = new Comparison<CommentedValue<DateTime>>((x, y) => { return x.Value.Date.CompareTo(y.Value.Date); });
            visits.Sort(comp);
            claims.Sort(comp);
        }
    }
}

