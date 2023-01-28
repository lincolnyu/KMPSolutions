using System;

namespace KmpCrmUwp.ViewModels
{
    internal static class Util
    {
        public static string DateToString(this DateTime? dateTime)
        {
            // The value parameter is the data from the source object.
            if (dateTime == null)
            {
                return "n/a";
            }
            DateTime theDate = (DateTime)dateTime;
            return theDate.ToString("d/MMM/yyyy");
        }

        public static DateTime? StringToDate(this string str)
        {
            if (DateTime.TryParse(str, out var result))
            {
                return result;
            }
            return null;
        }

        public static void StringToGenderViewModel(this string str, GenderViewModel genderViewModel)
        {
            genderViewModel.SelectOrAdd(str);
        }

        public static string GenderViewModelToString(this GenderViewModel gender)
        {
            return gender.SelectedItem;
        }
    }
}
