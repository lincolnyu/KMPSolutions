using System.ComponentModel;

namespace KmpCrmUwp.ViewModels
{
    internal class BaseViewModel<ModelType> : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected BaseViewModel(ModelType model)
        {
            Model = model;
        }

        public ModelType Model { get; protected set; }

        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
