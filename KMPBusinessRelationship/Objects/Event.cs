using System;

namespace KMPBusinessRelationship.Objects
{
    public abstract class Event
    {
        public int Index { get; set; }
        public DateTime? Time;  // Time this event occurs.
        public string? Remarks {  get; set; }

        public virtual void Redo() { }
        public virtual void Undo() { }
    }
}
