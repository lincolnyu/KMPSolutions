namespace KMPBookingCore
{
    public static class BookingUtils
    {

        public static string BookingIdToStr(this int id)
        {
            //TODO ID rangecheck
            return $"{id:00000000}";
        }
    }
}
