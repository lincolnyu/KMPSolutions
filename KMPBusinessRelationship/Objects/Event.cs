using System;

namespace KMPBusinessRelationship.Objects
{
    public class Event
    {
        public int Id { get; set; }
        public DateTime? Time { get; set; }  // Time this event occurs.
        public string? Remarks {  get; set; }

        public virtual void Redo() { }
        public virtual void Undo() { }
    }
}
