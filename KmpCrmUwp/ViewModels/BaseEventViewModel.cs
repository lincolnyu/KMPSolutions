using KmpCrmCore;
using System;

namespace KmpCrmUwp.ViewModels
{
    internal class BaseEventViewModel : BaseViewModel<CommentedValue<DateTime>>
    {
        private bool _isReadOnly;

        public BaseEventViewModel(CommentedValue<DateTime> model) : base(model)
        {
        }

        public bool IsNotReadOnly
        { 
            get
            {
                return !_isReadOnly;
            }
            internal set
            {
                _isReadOnly = !value;
                OnPropertyChanged("IsNotReadOnly");
                OnPropertyChanged("IsReadOnly");
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return _isReadOnly;
            }
            internal set 
            {
                _isReadOnly = value;
                OnPropertyChanged("IsNotReadOnly");
                OnPropertyChanged("IsReadOnly");
            }
        }
    }
}
