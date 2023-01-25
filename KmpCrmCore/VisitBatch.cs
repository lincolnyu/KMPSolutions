using System;
using System.Collections.Generic;

namespace KmpCrmCore
{
    using DateOnly = DateTime;

    public class VisitBatch
    {
        public List<CommentedValue<DateOnly>> VisitsMade { get; private set; } = new List<CommentedValue<DateOnly>>();
        public List<CommentedValue<DateOnly>> ClaimsMade { get; private set; } = new List<CommentedValue<DateOnly>>();
        public int? ExpectedVisits { get; set; }
    }
}
