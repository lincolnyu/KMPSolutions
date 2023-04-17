using KmpCrmCore;
using System;

namespace KmpCrmUwp.ViewModels
{
    internal abstract class BaseVisitBatchViewModel : BaseViewModel<CommentedValue<VisitBatch>>
    {
        private bool _isReadOnly;

        public BaseVisitBatchViewModel(CommentedValue<VisitBatch> model) : base(model)
        {
        }

        public bool IsNotReadOnly
        {
            get { return !_isReadOnly; }
            set
            {
                _isReadOnly = !value;
                OnPropertyChanged("IsNotReadOnly");
                OnPropertyChanged("IsReadOnly");
                OnIsReadOnlyChanged();
            }
        }

        public bool IsReadOnly
        {
            get { return _isReadOnly; }
            set
            {
                _isReadOnly = value;
                OnPropertyChanged("IsNotReadOnly");
                OnPropertyChanged("IsReadOnly");
                OnIsReadOnlyChanged();
            }
        }

        protected virtual void OnIsReadOnlyChanged()
        {
        }
    }
}

