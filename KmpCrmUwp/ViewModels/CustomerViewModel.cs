using KmpCrmCore;

namespace KmpCrmUwp.ViewModels
{
    internal class CustomerViewModel
    {
        public CustomerViewModel(Customer customer)
        {
            _model = customer;

            Gender.SelectOrAdd(_model.Gender);
            Gender.SelectedItemChanged += (selectedItem)=> { _model.Gender = selectedItem ?? ""; };
        }

        public string MedicareNumber { get { return _model.MedicareNumber; } set { _model.MedicareNumber = value; } }
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

        public string GpName { get { return _model.ReferingGP.Name; } set { _model.ReferingGP.Name = value; } }
        public string GpProviderNumber { get { return _model.ReferingGP.ProviderNumber; } set { _model.ReferingGP.ProviderNumber = value; } }

        public GenderViewModel Gender { get; private set; } = new GenderViewModel();

        private Customer _model;
    }
}
