using System;
using System.Windows;
using static BookingCore.BookingIcs;
using static BookingCore.DateTimeUtils;

namespace Booking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }


        private void BookClicked(object sender, RoutedEventArgs e)
        {
            var duration = TimeSpan.FromMinutes(30);
            if (int.TryParse(Duration.Text, out var dur))
            {
                duration = TimeSpan.FromMinutes(dur);
            }

            if (!BookingDate.SelectedDate.HasValue)
            {
                //TODO SelectedDate bad
                return;
            }

            var bookingDate = BookingDate.SelectedDate.Value.Date;
            var bookingTime = LaunderTime(BookingTime.Text);
            if (!bookingTime.HasValue)
            {
                //TODO booking time bad
                return;
            }
            var bookingDateTime = new DateTime(bookingDate.Year, bookingDate.Month, bookingDate.Day,
                bookingTime.Value.Hour, bookingTime.Value.Minute, bookingTime.Value.Second);
            var ics = GenerateBookingIcs(ClientName.Text,
                ClientNumber.Text, bookingDateTime, duration);
            if (ics != null)
            {
                LaunchBookingIcs(ics);
            }
            else
            {
                //TODO SelectedDate bad
            }
        }
    }
}
