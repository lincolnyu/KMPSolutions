namespace KmpCrmUwp.ViewModels
{
    internal class AddEventViewModel : BaseEventViewModel
    {
        public CommentedVisitBatchViewModel Parent { get; }

        public AddEventViewModel(CommentedVisitBatchViewModel parent) : base(null)
        {
            Parent = parent;
        }
    }
}
