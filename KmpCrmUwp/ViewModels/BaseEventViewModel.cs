using KmpCrmCore;
using System;

namespace KmpCrmUwp.ViewModels
{
    internal class BaseEventViewModel : BaseViewModel<CommentedValue<DateTime>>
    {
        public BaseEventViewModel(CommentedValue<DateTime> model) : base(model)
        {
        }
    }
}
