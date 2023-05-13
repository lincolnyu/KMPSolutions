using KMPBookingCore.DbObjects;
using System.Collections.Generic;
using System.Linq;

namespace KMPBookingCore
{
    public static class GPUtils
    {
        public static IEnumerable<GP> FindByProviderNumberContaining(
            this IEnumerable<GP> gpEnumerable, string providerNumberSubStr)
            => gpEnumerable.Where(x => x.ProviderNumber.Contains(providerNumberSubStr));

        public static IEnumerable<GP> FindByName(this IEnumerable<GP> gpEnumerable, string name)
            => gpEnumerable.Where(x => x.Name == name);

        public static IEnumerable<GP> FindNameContaining(this IEnumerable<GP> gpEnumerable, string nameSubstr)
        {
            return gpEnumerable.Where(x => x.Name.Contains(nameSubstr));
        }

        public static IEnumerable<GP> FindPhoneContaining(this IEnumerable<GP> gpEnumerable, string phoneSubstr)
        {
            return gpEnumerable.Where(x => x.Phone.Contains(phoneSubstr));
        }
    }
}
