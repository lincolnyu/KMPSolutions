using KmpCrmUwp.ViewModels;
using System;
using Windows.UI.Xaml.Data;

namespace KmpCrmUwp.Resources
{
    internal class EventTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var type = (EventViewModel.EventType)value;
            switch (type)
            {
                case EventViewModel.EventType.Visit:
                    return "Visit";
                case EventViewModel.EventType.Claim:
                    return "Claim";
            }
            throw new ArgumentException("Unexpected event type");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotSupportedException("This conversion is not expected to be used");
        }
    }
}
