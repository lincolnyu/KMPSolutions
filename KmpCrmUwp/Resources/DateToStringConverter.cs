using System;
using Windows.UI.Xaml.Data;

namespace KmpCrmUwp.Resources
{
    internal class DateToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // The value parameter is the data from the source object.
            if (value == null)
            {
                return "n/a";
            }
            DateTime theDate = (DateTime)value;
            return theDate.ToString("d/MMM/yyyy");
        }


        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
