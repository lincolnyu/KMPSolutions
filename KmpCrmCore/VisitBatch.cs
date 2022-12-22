namespace KmpCrmCore
{
    public class VisitBatch
    {
        public class Date
        {
            public DateOnly VisitDate;
            public string Remarks;
        }

        public List<Date> VisitsMade { get; private set; } = new List<Date>();
        public int? ExpectedVisits { get; set; }
        public string Remarks { get; set; }
    }
}
