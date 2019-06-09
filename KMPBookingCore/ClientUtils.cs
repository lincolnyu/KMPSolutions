namespace KMPBookingCore
{
    public static class ClientUtils
    {
        public static string ClientIdToStr(this int id)
        {
            //TODO ID rangecheck
            return $"{id:00000000}";
        }
    }
}
