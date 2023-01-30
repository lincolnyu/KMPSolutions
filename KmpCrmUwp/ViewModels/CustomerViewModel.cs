using KmpCrmCore;
using System.ComponentModel;

namespace KmpCrmUwp.ViewModels
{
    internal class CustomerViewModel : INotifyPropertyChanged
    {
        public bool CustomerAdded { get; private set; }

        public CustomerViewModel(Customer customer)
        {
            CustomerAdded = customer != null;
            if (customer == null)
            {
                customer = new Customer(); // tentative
                // todo
            }

            _model = customer;

            Gender.SelectOrAdd(_model.Gender);
            Gender.SelectedItemChanged += (selectedItem)=> { _model.Gender = selectedItem ?? ""; };
        }

        public string MedicareNumber { 
            get { return _model.MedicareNumber; } 
            set
            {
                if (!CustomerAdded)
                {
                    // todo check if the customer exists
                    if (CrmData.Instance.CrmRepo.Customers.TryGetValue(value, out var customer))
                    {
                        _model = customer;
                        CustomerAdded = true;
                        OnAllPropertiesChanged();
                        return;
                    }
                }
                // todo conflict management.
                _model.MedicareNumber = value; 
            } 
        }
        public string FirstName { get { return _model.FirstName; } set { _model.FirstName = value; } }
        public string Surname { get { return _model.Surname; } set { _model.Surname = value; } }
        // TODO Name?

        public string DateOfBirth
        {
            get { return _model.DateOfBirth?.DateToString(); }
            set { _model.DateOfBirth = value.StringToDate(); }
        }

        public string PhoneNumber { get { return _model.PhoneNumber; } set { _model.PhoneNumber = value; } }
        public string Address { get { return _model.Address; } set { _model.Address = value; } }


        public GenderViewModel Gender { get; private set; } = new GenderViewModel();

        public string GpName 
        {
            get { return _model.ReferingGP?.Name??""; } 
            set 
            { 
                // TODO warning to change existing gp name
                _model.ReferingGP.Name = value; 
            }
        }
        public string GpProviderNumber
        { 
            get { return _model.ReferingGP?.ProviderNumber??"n/a"; } 
            set 
            {
                throw new System.NotImplementedException("Add or pick existing gp");
            } 
        }

        private Customer _model;

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnAllPropertiesChanged()
        {
            // TODO make sure it includes all properties
            OnPropertyChanged("MedicareNumber");
            OnPropertyChanged("FirstName");
            OnPropertyChanged("Surname");
            OnPropertyChanged("Gender");
            OnPropertyChanged("GpName");
            OnPropertyChanged("GpProviderNumber");
        }
    }
}
