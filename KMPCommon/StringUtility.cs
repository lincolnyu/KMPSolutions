using System.Linq;
using System.Text;

namespace KMPCommon
{
    public static class StringUtility
    {
        public static bool GetNextWord(this string line, char sep, int start, out int end, out string? word) 
        {
            end = line.IndexOf(sep, start);
            if (end == -1) 
            { 
                word = null; 
                return false; 
            }
            word = line[start..end];
            return true;
        }

        public static bool StartsWithCaseIgnored(this string str, string substr)
        {
            return str.StartsWith(substr, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsCaseIgnored(this string str, string substr)
        {
            return str.Contains(substr, System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsWholeWord(this string str, string substr, bool ignoreCase = true)
        {
            for (var p = 0; p <= str.Length - substr.Length; p += substr.Length)
            {
                p = str.IndexOf(substr, p, ignoreCase ? System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal);
                if (p >= 0)
                {
                    if (p > 0 && char.IsLetter(str[p - 1])) continue;
                    if (p + substr.Length < str.Length && char.IsLetter(str[p + substr.Length])) continue;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        public static string TrimAndReplaceConsecutiveSpacesWithSingleSpace(this string input)
        {
            var segs = input.Split(' ').Where(x => x.Length > 0);
            var sb = new StringBuilder();
            foreach (var seg in segs)
            {
                sb.Append(seg);
                sb.Append(' ');
            }
            if (sb.Length > 1)
            {
                sb.Remove(sb.Length - 1, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        ///  Extract first name and surname from a name in the form 'surname,given-name' or 'given-name surname'
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static (string /* given name*/, string /* surname */) ParseNameOfPerson(this string name)
        {
            name = name.Trim();
            var segs = name.Split(',');
            if (segs.Length > 1)
            {
                var surname = segs[0].TrimAndReplaceConsecutiveSpacesWithSingleSpace();
                var firstName = segs[1].TrimAndReplaceConsecutiveSpacesWithSingleSpace();
                // subsequent segments ignored if any
                return (firstName, surname);
            }
            // Treated as 'FirstName LastName'
            var space = name.IndexOf(' ');
            if (space >= 0)
            {
                var fn = name.Substring(0, space).TrimAndReplaceConsecutiveSpacesWithSingleSpace();
                var sn = name.Substring(space + 1).TrimAndReplaceConsecutiveSpacesWithSingleSpace();
                return (fn, sn);
            }
            else
            {
                return (name, "");
            }
        }
    }
}
