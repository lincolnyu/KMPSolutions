using KmpCrmCore;
using System;
using System.Collections.Generic;

namespace KmpCrmUwp.ViewModels
{
    internal class EventViewModel : BaseViewModel<CommentedValue<DateTime>>
    {
        private EventType _type;

        public EventViewModel(CommentedValue<DateTime> model) : base(model)
        {
        }

        public enum EventType
        {
            Visit,
            Claim
        }

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
    }
}
