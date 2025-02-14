using System;

namespace KMPBusinessRelationship.Objects
{
    public class Booking : Event<Booking>
    {
        private DateTime? AppointmentTime { get; set; }
        private TimeSpan Duration { get; set; }
        private DateTime? ReminderTime { get; set; }

        public Client Client { get; set; }

        public override bool Equals(Booking other)
        {
            if (AppointmentTime != other.AppointmentTime) return false;
            if (Duration != other.Duration) return false;
            if (ReminderTime != other.ReminderTime) return false;
            if (Client.Id != other.Client.Id) return false;
            return true;
        }
    }
}
