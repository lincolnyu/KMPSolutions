﻿using KmpCrmUwp.ViewModels;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace KmpCrmUwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EditCustomerPage : Page
    {
        public EditCustomerPage()
        {
            this.InitializeComponent();
            this.PopulateDataIfFocused();
        }

        private CustomerViewModel _focusedCustomerVm;

        private void PopulateDataIfFocused()
        {
            DataEntryGrid.DataContext = new CustomerViewModel(CrmData.FocusedCustomer);
            var vm = (CustomerViewModel)DataEntryGrid.DataContext;
            
            GenderComboBox.IsEnabled = vm.IsNotReadOnly;
            vm.PropertyChanged += Vm_PropertyChanged;
            IsNotReadOnlyChanged();
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsNotReadOnly")
            {
                IsNotReadOnlyChanged();
            }
        }

        private void IsNotReadOnlyChanged()
        {
            var vm = (CustomerViewModel)DataEntryGrid.DataContext;
            GenderComboBox.IsEnabled = vm.IsNotReadOnly;
        }

        private void Home_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            rootFrame.GoBack();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var vm = (CustomerViewModel)DataEntryGrid.DataContext;
            vm.IsReadOnly = true;
            base.OnNavigatedTo(e);
        }

        private void AddVisitBatch_Click(object sender, RoutedEventArgs e)
        {
            var vm = (CustomerViewModel)DataEntryGrid.DataContext;
            vm.AddEmptyVisitBatch();
        }

        private void AddVisit_Click(object sender, RoutedEventArgs e)
        {
            var vm = ((AddEventViewModel)((Button)sender).DataContext).Parent;
            vm.AddEmptyVisit();
        }

        private void AddClaim_Click(object sender, RoutedEventArgs e)
        {
            var vm = ((AddEventViewModel)((Button)sender).DataContext).Parent;
            vm.AddEmptyClaim();
        }

        private void RemoveEvent_Click(object sender, RoutedEventArgs e)
        {
            var vm = (EventViewModel)((MenuFlyoutItem)sender).DataContext;
            vm.RemoveSelf();
        }

        private void RemoveVisitBatch_Click(object sender, RoutedEventArgs e)
        {
            var vm = (CommentedVisitBatchViewModel)((MenuFlyoutItem)sender).DataContext;
            vm.RemoveSelf();
        }
    }
}
