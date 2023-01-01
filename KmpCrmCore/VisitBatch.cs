namespace KmpCrmCore
{
    public class VisitBatch
    {
        public List<CommentedValue<DateOnly>> VisitsMade { get; set; } = new List<CommentedValue<DateOnly>>();
        public int? ExpectedVisits { get; set; }
    }
}
