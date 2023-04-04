using KmpCrmCore;

namespace KmpCrmUwp.ViewModels
{
    internal abstract class BaseVisitBatchViewModel : BaseViewModel<CommentedValue<VisitBatch>>
    {
        public BaseVisitBatchViewModel(CommentedValue<VisitBatch> model) : base(model)
        {
        }
    }
}
