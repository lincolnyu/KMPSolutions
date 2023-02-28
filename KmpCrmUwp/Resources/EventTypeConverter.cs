using KmpCrmUwp.ViewModels;
using System;
using Windows.UI.Xaml.Data;

namespace KmpCrmUwp.Resources
{
    internal class EventTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            EventViewModel.EventType type = (EventViewModel.EventType)value;
            switch (type)
            {
                case EventViewModel.EventType.Visit:
                    return "Visit";
                case EventViewModel.EventType.Claim:
                    return "Claim";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
