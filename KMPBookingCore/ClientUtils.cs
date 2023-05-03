using System.Collections.Generic;
using System.Linq;
using KMPBookingCore.DbObjects;

namespace KMPBookingCore
{
    public static class ClientUtils
    {
        public static string ClientIdToStr(this int id)
        {
            //TODO ID rangecheck
            return $"{id:00000000}";
        }

        public static string ClientFormalName(this Client client)
            => BookingIcs.FormCommaSeparateName(client.FirstName, client.Surname);

        public static IEnumerable<Client> FindByMedicareNumberContaining(
            this IEnumerable<Client> clients, string medSubstr)
            => clients.Where(x => x.MedicareNumber.Contains(medSubstr));

        public static IEnumerable<Client> FindByName(this IEnumerable<Client> clients, string firstName, string surname)
            => clients.Where(x => x.FirstName == firstName && x.Surname == surname);

        public static IEnumerable<Client> FindNameContaining(this IEnumerable<Client> clients, string nameSubstr)
        {
            if (nameSubstr.Contains(','))
            {
                (var fn, var sn) = nameSubstr.SmartParseName();
                return clients.FindByName(fn, sn);
            }
            else
            {
                var segs = nameSubstr.Split(' ').Select(x => x.Trim().ToLower()).Where(x => x.Length > 0);
                return clients.Where(x => segs.All(y => x.FirstName.ToLower().Contains(y)
                    || x.Surname.ToLower().Contains(y)));
            }
        }

        public static IEnumerable<Client> FindByPhoneNumberContaining(this IEnumerable<Client> clients, string numberSubstr)
            => clients.Where(x => x.Phone.Contains(numberSubstr));

    }
}
