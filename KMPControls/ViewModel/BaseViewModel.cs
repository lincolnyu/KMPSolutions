using KMPBookingCore.Database;
using System.ComponentModel;

namespace KMPControls.ViewModel
{
    internal class BaseViewModel<T> : INotifyPropertyChanged where T : DbObject
    {
        public BaseViewModel(T model)
        {
            Model = model;
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public T Model { get; }
    
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
