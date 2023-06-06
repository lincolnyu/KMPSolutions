using KMPBookingCore.DbObjects;
using System;
using System.Collections.Generic;

namespace KMPControls.ViewModel
{
    internal class EventViewModel : BaseViewModel<Event>
    {
        public EventViewModel(Event model) : base(model)
        {
        }

        public int Id
        {
            get { return Model.Id; }
            set
            {
                Model.Id = value;
                RaisePropertyChanged("Id");
            }
        }

        public string Type
        {
            get { return Model.Type; }
            set
            {
                Model.Type = value;
                OnTypeChanged();
                RaisePropertyChanged("Type");
            }
        }

        public List<string> PredefinedTypes
        {
            get
            {
                return _predefinedTypes;
            }
        }

        private static List<string> _predefinedTypes = new List<string>
        {
            "Booking",
            "Claim",
            "Initial Referral",
            "Receipt",
            "Service"
        };

        public DateTime? Date
        {
            get { return Model.EventDate; }
            set
            {
                Model.EventDate = value;
                RaisePropertyChanged("Date");
            }
        }

        private void OnTypeChanged()
        {
            
            // TODO...
        }
    }
}
