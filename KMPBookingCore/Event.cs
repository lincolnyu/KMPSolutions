using System;

namespace KMPBookingCore
{
    public class Event
    {
        public int Id { get; set; }
        public string MedicareNumber { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Remarks { get; set; }
    }
}
