using System.Collections.Generic;
using System.Linq;

namespace KMPBookingCore
{
    public static class ClientUtils
    {
        public static string ClientIdToStr(this int id)
        {
            //TODO ID rangecheck
            return $"{id:00000000}";
        }

        public static int ClientIdFromStr(this string idstr)
        {
            return int.Parse(idstr);
        }

        public static string ClientFormalName(this ClientRecord client)
            => BookingIcs.FormCommaSeparateName(client.FirstName, client.Surname);

        public static IEnumerable<ClientRecord> FindByMedicareNumberContaining(
            this IEnumerable<ClientRecord> clients, string medSubstr)
            => clients.Where(x => x.MedicareNumber.Contains(medSubstr));

        public static IEnumerable<ClientRecord> FindByIdContaining(
            this IEnumerable<ClientRecord> clients, string idSubstr)
            => clients.Where(x => x.Id.Contains(idSubstr));

        public static IEnumerable<ClientRecord> FindByName(this IEnumerable<ClientRecord> clients, string firstName, string surname)
            => clients.Where(x => x.FirstName == firstName && x.Surname == surname);

        public static IEnumerable<ClientRecord> FindNameContaining(this IEnumerable<ClientRecord> clients, string nameSubstr)
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

        public static IEnumerable<ClientRecord> FindByPhoneNumberContaining(this IEnumerable<ClientRecord> clients, string numberSubstr)
            => clients.Where(x => x.PhoneNumber.Contains(numberSubstr));

    }
}
