using System;

namespace KMPBusinessRelationship.Objects
{
    public abstract class Event
    {
        public int Id { get; set; }
        public DateTime? Time { get; set; }  // Time this event occurs.
        public string? Remarks {  get; set; }

        public virtual void Redo() { }
        public virtual void Undo() { }

        public abstract bool Equals(Event other);
    }

    public abstract class Event<T> : Event, IEquatable<Event> where T : Event
    {
        public override bool Equals(Event other)
        {
            return (other is T t) && Equals(t);
        }

        public abstract bool Equals(T other);
    }
}
