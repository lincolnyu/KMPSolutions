using KMPCommon;
using System.Text;

namespace KMPBusinessRelationship.ImportExport
{
    public class ExportGoogleContactsCsv
    {
        /// <summary>
        ///  Exports the clients list to a google contact csv with mainly clients' phone numbers included
        /// </summary>
        /// <param name="repo"></param>
        /// <param name="csvFile"></param>
        public void ExportSimple(BaseRepository repo, FileInfo csvFile)
        {
            using var sw = new StreamWriter(csvFile.OpenWrite());

            sw.WriteLine("Name,Given Name,Additional Name,Family Name,Yomi Name,Given Name Yomi,Additional Name Yomi,Family Name Yomi,Name Prefix,Name Suffix,Initials,Nickname,Short Name,Maiden Name,File As,Birthday,Gender,Location,Billing Information,Directory Server,Mileage,Occupation,Hobby,Sensitivity,Priority,Subject,Notes,Language,Photo,Group Membership,Phone 1 - Type,Phone 1 - Value");
            foreach (var client in repo.Clients)
            {
                var (givenName, surname) = StringUtility.ParseNameOfPerson(client.Name);
                var sb = new StringBuilder();
                sb.Append($"{givenName} {surname}");
                sb.Append(',');
                sb.Append(givenName);
                sb.Append(",,");
                sb.Append(surname);
                sb.Append(",,,,,,,,,,,,,,,,,,,,,,,,,,");
                sb.Append("* myContacts,Mobile,");
                sb.Append(client.PhoneNumber);
                sw.WriteLine(sb.ToString());
            }
        }
    }
}
