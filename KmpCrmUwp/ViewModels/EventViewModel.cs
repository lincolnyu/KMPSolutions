using KmpCrmCore;
using System;
using System.Collections.Generic;

namespace KmpCrmUwp.ViewModels
{
    internal class EventViewModel : BaseEventViewModel
    {
        private EventType _type;

        public EventViewModel(CommentedValue<DateTime> model, CommentedVisitBatchViewModel parent) : base(model)
        {
            Parent = parent;
        }

        public enum EventType
        {
            Visit,
            Claim
        }

        public CommentedVisitBatchViewModel Parent { get; }

        public EventType Type
        {
            get
            {
                return _type;
            }
            set
            {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        public DateTimeOffset? Date
        {
            get { return Model.Value != null ? new DateTimeOffset(Model.Value) : (DateTimeOffset?)null; }
            set { Model.Value = value?.Date??DateTime.UtcNow; }
        }

        public List<EventType> TypeOptions { get; set; } = new List<EventType> { EventType.Visit, EventType.Claim };

        public string Comments
        {
            get { return Model.Comments; }
            set { Model.Comments = value; }
        }

        internal void RemoveSelf()
        {
            Parent.RemoveEvent(this);
        }
    }
}
