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

        public GenderViewModel Gender { get; private set; } = new GenderViewModel();

        private Customer _model;
    }
}
