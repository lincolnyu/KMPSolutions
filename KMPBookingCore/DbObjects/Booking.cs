using KMPBookingCore.Database;
using System;

namespace KMPBookingCore.DbObjects
{
    [DbClass]
    public class Booking : Event
    {
        private DateTime? _madeOn;
        private TimeSpan _duration;
        private DateTime? _reminderDate;

        public Booking()
        {
            Type = "Booking";
        }

        [DbField]
        public DateTime? MadeOn
        {
            get => _madeOn; set
            {
                _madeOn = value;
                RaiseEventChanged(nameof(MadeOn));
            }
        }
        [DbField]
        public TimeSpan Duration
        {
            get => _duration; set
            {
                _duration = value;
                RaiseEventChanged(nameof(Duration));
            }
        }
        [DbField]
        public DateTime? ReminderDate
        {
            get => _reminderDate; set
            {
                _reminderDate = value;
                RaiseEventChanged(nameof(ReminderDate));
            }
        }
    }
}
