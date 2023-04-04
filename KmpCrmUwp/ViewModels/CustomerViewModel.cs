using KmpCrmCore;
using System;
using System.Collections.ObjectModel;

namespace KmpCrmUwp.ViewModels
{
    using DateOnly = DateTime;

    internal class CustomerViewModel : BaseViewModel<Customer>
    {
        public bool CustomerAdded { get; private set; }
        public static readonly DateOnly DefaultDate = new DateOnly(1923, 1, 1);

        public CustomerViewModel(Customer customer) : base(customer)
        {
            CustomerAdded = customer != null;
            if (customer == null)
            {
                customer = new Customer(); // tentative
                // todo
            }

            Model = customer;

            Gender.SelectOrAdd(Model.Gender);
            Gender.SelectedItemChanged += (selectedItem)=> { Model.Gender = selectedItem ?? ""; };

            LoadVisitBatchViewModels();
            VisitBatches.CollectionChanged += VisitBatches_CollectionChanged;
        }

        public string MedicareNumber { 
            get { return Model.MedicareNumber; } 
            set
            {
                if (!CustomerAdded)
                {
                    // todo check if the customer exists
                    if (CrmData.Instance.CrmRepo.Customers.TryGetValue(value, out var customer))
                    {
                        Model = customer;
                        CustomerAdded = true;
                        OnAllPropertiesChanged();
                        return;
                    }
                }
                // todo conflict management.
                Model.MedicareNumber = value; 
            } 
        }
        public string FirstName { get { return Model.FirstName; } set { Model.FirstName = value; } }
        public string Surname { get { return Model.Surname; } set { Model.Surname = value; } }
        // TODO Name?

        private DateTimeOffset? _cachedDateOfBirth;
        private DateTimeOffset? _cachedReferringDate;

        public DateTimeOffset? DateOfBirth
        {
            get { return Model.DateOfBirth != null ? new DateTimeOffset(Model.DateOfBirth.Value) : (DateTimeOffset?)null; }
            set
            {
                Model.DateOfBirth = value?.Date;
                if (value != null)
                {
                    _cachedDateOfBirth = value.Value;
                }
                OnPropertyChanged("DateOfBirth");
            }
        }

        public bool? HasDateOfBirth
        {
            get { return DateOfBirth != null; }
            set
            {
                if (value == true)
                {
                    if (_cachedDateOfBirth != null)
                    {
                        DateOfBirth = _cachedDateOfBirth;
                    }
                    else
                    {
                        DateOfBirth = DefaultDate;
                    }
                }
                else
                {
                    if (DateOfBirth != null)
                    {
                        _cachedDateOfBirth = DateOfBirth;
                    }
                    DateOfBirth = null;
                }
                OnPropertyChanged("HasDateOfBirth");
            }
        }

        public DateTimeOffset? ReferringDate
        {
            get { return Model.ReferringDate != null ? new DateTimeOffset(Model.ReferringDate.Value) : (DateTimeOffset?)null; }
            set 
            { 
                Model.ReferringDate = value?.Date;
                OnPropertyChanged("ReferringDate");
            }
        }

        public bool? HasReferringDate
        {
            get { return ReferringDate != null; }
            set
            {
                if (value == true)
                {
                    if (_cachedReferringDate != null)
                    {
                        ReferringDate = _cachedReferringDate;
                    }
                    else
                    {
                        ReferringDate = DefaultDate;
                    }
                }
                else
                {
                    if (ReferringDate != null)
                    {
                        _cachedReferringDate = ReferringDate;
                    }
                    ReferringDate = null;
                }
                OnPropertyChanged("HasReferringDate");
            }
        }

        public string PhoneNumber { get { return Model.PhoneNumber; } set { Model.PhoneNumber = value; } }
        public string Address { get { return Model.Address; } set { Model.Address = value; } }

        public GenderViewModel Gender { get; private set; } = new GenderViewModel();

        public string GpName 
        {
            get { return Model.ReferingGP?.Name??""; } 
            set 
            { 
                // TODO warning to change existing gp name
                Model.ReferingGP.Name = value; 
            }
        }
        public string GpProviderNumber
        { 
            get { return Model.ReferingGP?.ProviderNumber??"n/a"; } 
            set 
            {
                throw new NotImplementedException("Add or pick existing gp");
            } 
        }

        public bool HasInitialLetter
        {
            get { return Model.InitialLetter.Value; }
            set
            {
                Model.InitialLetter.Value = value;
            }
        }

        public string InitialLetterComments
        {
            get { return Model.InitialLetter.Comments; }
            set
            {
                Model.InitialLetter.Comments = value;
            }
        }

        public ObservableCollection<BaseVisitBatchViewModel> VisitBatches { get; set; }

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

        private void LoadVisitBatchViewModels()
        {
            if (VisitBatches == null)
            {
                VisitBatches = new ObservableCollection<BaseVisitBatchViewModel>();
            }
            else
            {
                VisitBatches.Clear();
            }
            foreach (var vb in Model.VisitBatches)
            {
                VisitBatches.Add(new CommentedVisitBatchViewModel(vb, this));
            }
            VisitBatches.Add(new AddVisitBatchViewModel());
        }

        private void VisitBatches_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        public void AddEmptyVisitBatch()
        {
            Model.VisitBatches.Add(new CommentedValue<VisitBatch>(new VisitBatch()));
            LoadVisitBatchViewModels();
        }

        internal void RemoveVisitBatch(CommentedVisitBatchViewModel commentedVisitBatchViewModel)
        {
            Model.VisitBatches.Remove(commentedVisitBatchViewModel.Model);
            LoadVisitBatchViewModels();
        }
    }
}
