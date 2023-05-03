using KMPBookingCore.Database;
using System;

namespace KMPBookingCore.DbObjects
{
    [DBClass]
    public class Booking : Event
    {
        private DateTime? _madeOn;
        private TimeSpan _duration;
        private DateTime? _reminderDate;

        public Booking()
        {
            Type = "Booking";
        }

        [DBField]
        public DateTime? MadeOn
        {
            get => _madeOn; set
            {
                _madeOn = value;
                RaiseEventChanged(nameof(MadeOn));
            }
        }
        [DBField]
        public TimeSpan Duration
        {
            get => _duration; set
            {
                _duration = value;
                RaiseEventChanged(nameof(Duration));
            }
        }
        [DBField]
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
