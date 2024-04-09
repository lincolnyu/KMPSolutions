using System;

namespace KMPBusinessRelationship.Objects
{
    public abstract class Event
    {
        public DateTime? Time;  // Time this event occurs.
        public string? Remarks {  get; set; }
    }
}
