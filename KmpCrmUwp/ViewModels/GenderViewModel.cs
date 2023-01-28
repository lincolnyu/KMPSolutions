using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace KmpCrmUwp.ViewModels
{
    internal class GenderViewModel
    {
        //https://stackoverflow.com/questions/3373239/wpf-editable-combobox
        private ObservableCollection<string> _items = new ObservableCollection<string>
        {
            "Female",
            "Male",
            "n/a",
            "Unspecified"
        };
        
        public static bool IsEmptyGenderString(string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public IEnumerable Items
        {
            get { return _items; }
        }

        public delegate void SelectedItemChangedDelegate(string selectedItem);

        public event PropertyChangedEventHandler PropertyChanged;

        public event SelectedItemChangedDelegate SelectedItemChanged;
   
        public string SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                SelectedItemChanged?.Invoke(value);
                OnPropertyChanged("SelectedItem");
            }
        }

        public string NewItem
        {
            set
            {
                if (SelectedItem != null)
                {
                    return;
                }
                if (!IsEmptyGenderString(value))
                {
                    _items.Add(value);
                    SelectedItem = value;
                }
            }
        }

        public void SelectOrAdd(string str)
        {
            if (IsEmptyGenderString(str))
            {
                return;
            }
            switch (str)
            {
                case "F": str = "Female"; break;
                case "M": str = "Male"; break;
                case "U": str = "Unspecified"; break;
            }
            var index = _items.IndexOf(str);
            if (index >= 0)
            {
                SelectedItem = _items[index];
            }
            else
            {
                _items.Add(str);
                SelectedItem = str;
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private string _selectedItem;
    }
}
