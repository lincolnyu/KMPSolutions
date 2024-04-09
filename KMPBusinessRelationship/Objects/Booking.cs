using System;

namespace KMPBusinessRelationship.Objects
{
    public class Booking : Event
    {
        private DateTime? AppointmentTime { get; set; }
        private TimeSpan Duration { get; set; }
        private DateTime? ReminderTime { get; set; }
    }
}
